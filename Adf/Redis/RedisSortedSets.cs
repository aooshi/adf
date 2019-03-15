using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace Adf
{
    /// <summary>
    /// Redis 有序集合
    /// </summary>
    public class RedisSortedSets
    {
        RedisClient client;
        RedisConnection connection;

        internal RedisSortedSets(RedisClient client, RedisConnection connection)
        {
            this.connection = connection;
            this.client = client;
        }

        /// <summary>
        /// 添加一个元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="score"></param>
        /// <param name="member"></param>
        /// <returns>排序</returns>
        public int ZAdd(string key, double score, string member)
        {
            //ZADD myzset 1 "one"

            if (member == null)
                throw new ArgumentNullException("member");

            using (var w = new RedisWriter(this.client, 4, "ZADD"))
            {
                w.WriteArgument(key);
                w.WriteArgument(score.ToString());
                w.WriteArgument(member);

                this.connection.SendCommand(w);
            }

            //Integer reply, specifically:
            //The number of elements added to the sorted sets, not including elements already existing for which the score was updated.

            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 返回集合数
        /// key存在的时候，返回有序集的元素个数，否则返回0。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int ZCARD(string key)
        {
            //Integer reply: the cardinality (number of elements) of the set, or 0 if key does not exist

            using (var w = new RedisWriter(this.client, 2, "ZCARD"))
            {
                w.WriteArgument(key);
                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 指定分数范围的元素个数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public int ZCOUNT(string key, double min, double max)
        {
            //ZCOUNT key min max

            //Integer reply: the number of elements in the specified score range.
            using (var w = new RedisWriter(this.client, 4, "ZCOUNT"))
            {
                w.WriteArgument(key);
                w.WriteArgument(min.ToString());
                w.WriteArgument(max.ToString());

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 给指定元素添加自增值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public double ZINCRBY(string key, double increment, string member)
        {
            //ZINCRBY key increment member

            //Bulk string reply: the new score of member (a double precision floating point number), represented as string.
            using (var w = new RedisWriter(this.client, 4, "ZINCRBY"))
            {
                w.WriteArgument(key);
                w.WriteArgument(increment.ToString());
                w.WriteArgument(member);

                this.connection.SendCommand(w);
            }
            return this.connection._ExpectToDouble();
        }

        /// <summary>
        /// 计算给定的一个或多个有序集的并集，其中给定 key 的数量必须以 numkeys 参数指定，并将该并集(结果集)储存到 destination 。
        /// 默认情况下，结果集中某个成员的 score 值是所有给定集下该成员 score 值之 和 。
        /// </summary>
        /// <param name="destinationKey">目标键</param>
        /// <param name="keyWeights">键与乘法因子</param>
        /// <param name="aggregate">聚合方式</param>
        /// <returns>保存到 destination 的结果集的基数</returns>
        public int ZUNIONSTORE(string destinationKey, IDictionary<string, double> keyWeights, RedisAggregate aggregate)
        {
            //ZUNIONSTORE destination numkeys key [key ...] [WEIGHTS weight [weight ...]] [AGGREGATE SUM|MIN|MAX]

            //Integer reply: the number of elements in the resulting sorted set at destination.

            if (string.IsNullOrEmpty(destinationKey))
                throw new ArgumentNullException("destinationKey");

            if (keyWeights == null || keyWeights.Count == 0)
                throw new ArgumentNullException("keyWeights");
            //
            var keys = new string[keyWeights.Count];
            var weights = new double[keyWeights.Count];
            var i = 0;
            var e = keyWeights.GetEnumerator();
            while (e.MoveNext())
            {
                keys[i] = e.Current.Key;
                weights[i] = e.Current.Value;
                i++;
            }

            //ZUNIONSTORE destination numkeys key [key ...] [WEIGHTS weight [weight ...]] [AGGREGATE SUM|MIN|MAX]
            using (var w = new RedisWriter(this.client, 6 + keyWeights.Count * 2, "ZUNIONSTORE"))
            {
                w.WriteArgument(destinationKey);
                w.WriteArgument(keyWeights.Count.ToString());
                foreach (var key in keys)
                    w.WriteArgument(key);
                w.WriteArgument("WEIGHTS");
                foreach (var weight in weights)
                    w.WriteArgument(weight.ToString());
                w.WriteArgument("AGGREGATE");
                w.WriteArgument(aggregate.ToString());

                this.connection.SendCommand(w);
            }

            return this.connection.ExpectInt();

            //示例
            //            redis> ZRANGE programmer 0 -1 WITHSCORES
            //1) "peter"
            //2) "2000"
            //3) "jack"
            //4) "3500"
            //5) "tom"
            //6) "5000"

            //redis> ZRANGE manager 0 -1 WITHSCORES
            //1) "herry"
            //2) "2000"
            //3) "mary"
            //4) "3500"
            //5) "bob"
            //6) "4000"

            //redis> ZUNIONSTORE salary 2 programmer manager WEIGHTS 1 3
            //(integer) 6

            //redis> ZRANGE salary 0 -1 WITHSCORES
            //1) "peter"
            //2) "2000"
            //3) "jack"
            //4) "3500"
            //5) "tom"
            //6) "5000"
            //7) "herry"
            //8) "6000"
            //9) "mary"
            //10) "10500"
            //11) "bob"
            //12) "12000"
        }

        /// <summary>
        /// 计算给定的一个或多个有序集的交集，其中给定 key 的数量必须以 numkeys 参数指定，并将该交集(结果集)储存到 destination 。
        /// 默认情况下，结果集中某个成员的 score 值是所有给定集下该成员 score 值之和.
        /// 关于 WEIGHTS 和 AGGREGATE 选项的描述，参见 ZUNIONSTORE 命令
        /// </summary>
        /// <param name="destinationKey">目标键</param>
        /// <param name="keyWeights">键与乘法因子</param>
        /// <param name="aggregate">聚合方式</param>
        /// <returns>保存到 destination 的结果集的基数</returns>
        public int ZINTERSTORE(string destinationKey, IDictionary<string, double> keyWeights, RedisAggregate aggregate)
        {
            //ZINTERSTORE destination numkeys key [key ...] [WEIGHTS weight [weight ...]] [AGGREGATE SUM|MIN|MAX]

            //Integer reply: the number of elements in the resulting sorted set at destination.

            if (string.IsNullOrEmpty(destinationKey))
                throw new ArgumentNullException("destinationKey");

            if (keyWeights == null || keyWeights.Count == 0)
                throw new ArgumentNullException("keyWeights");
            //
            var keys = new string[keyWeights.Count];
            var weights = new double[keyWeights.Count];
            var i = 0;
            var e = keyWeights.GetEnumerator();
            while (e.MoveNext())
            {
                keys[i] = e.Current.Key;
                weights[i] = e.Current.Value;
                i++;
            }

            //ZINTERSTORE destination numkeys key [key ...] [WEIGHTS weight [weight ...]] [AGGREGATE SUM|MIN|MAX]
            using (var w = new RedisWriter(this.client, 6 + keyWeights.Count * 2, "ZINTERSTORE"))
            {
                w.WriteArgument(destinationKey);
                w.WriteArgument(keyWeights.Count.ToString());
                foreach (var key in keys)
                    w.WriteArgument(key);
                w.WriteArgument("WEIGHTS");
                foreach (var weight in weights)
                    w.WriteArgument(weight.ToString());
                w.WriteArgument("AGGREGATE");
                w.WriteArgument(aggregate.ToString());

                this.connection.SendCommand(w);
            }

            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 返回给定的分值范围元素内容列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public string[] ZRANGEValues(string key, double start, double stop)
        {
            var b = this.ZRANGEByte(key, start, stop, false);
            return this.connection.ExpectToStringArray(b);
        }

        /// <summary>
        /// 返回给定的分值范围元素内容列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="withscores"></param>
        /// <returns></returns>
        public SortedList<double, string> ZRANGE(string key, double start, double stop, bool withscores)
        {
            byte[][] b = null;
            if (withscores)
                b = this.ZRANGEByte(key, start, stop, true);
            else
                b = this.ZRANGEByte(key, start, stop, false);
            if (b.Length < 2)
                return new SortedList<double, string>(0);

            var r = new SortedList<double, string>(b.Length / 2);
            var d = 0d;
            for (int i = 0, l = b.Length; i < l; i += 2)
            {
                double.TryParse(this.client.Encoding.GetString(b[i + 1]), out d);
                r[d] = this.client.Encoding.GetString(b[i]);
            }
            return r;
        }

        /// <summary>
        /// 返回给定的分值范围元素列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="withscores">是否返回分值</param>
        /// <returns></returns>
        private byte[][] ZRANGEByte(string key, double start, double stop, bool withscores)
        {
            //ZRANGE key start stop [WITHSCORES]

            if (key == null)
                throw new ArgumentNullException("key");

            //Array reply: list of elements in the specified range (optionally with their scores).

            if (withscores)
            {
                using (var w = new RedisWriter(this.client, 5, "ZRANGE"))
                {
                    w.WriteArgument(key);
                    w.WriteArgument(start.ToString());
                    w.WriteArgument(stop.ToString());
                    w.WriteArgument("WITHSCORES");

                    this.connection.SendCommand(w);
                }
            }
            else
            {
                using (var w = new RedisWriter(this.client, 4, "ZRANGE"))
                {
                    w.WriteArgument(key);
                    w.WriteArgument(start.ToString());
                    w.WriteArgument(stop.ToString());

                    this.connection.SendCommand(w);
                }
            }

            return this.connection.ExpectMultiBulkReply();
        }

        /// <summary>
        /// 返回给定的分值范围元素列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="offset">取出偏移量,需设置count有效</param>
        /// <param name="count">设置数量则offset有产效</param>
        /// <returns></returns>
        public string[] ZRANGEBYSCOREValues(string key, double min, double max, int offset, int count)
        {
            //ZRANGEBYSCORE key min max [WITHSCORES] [LIMIT offset count]

            if (key == null)
                throw new ArgumentNullException("key");

            //Array reply: list of elements in the specified range (optionally with their scores).
            var size = 4;
            if (count > 0)
                size += 3;

            using (var w = new RedisWriter(this.client, size, "ZRANGEBYSCORE"))
            {
                w.WriteArgument(key);
                w.WriteArgument(min.ToString());
                w.WriteArgument(max.ToString());
                if (count > 0)
                {
                    w.WriteArgument("LIMIT");
                    w.WriteArgument(offset.ToString());
                    w.WriteArgument(count.ToString());
                }

                this.connection.SendCommand(w);
            }

            return this.connection.ExpectToStringArray();
        }

        /// <summary>
        /// 返回给定的分值范围元素列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="offset">取出偏移量,需设置count有效</param>
        /// <param name="count">设置数量则offset有产效</param>
        /// <returns></returns>
        public SortedList<double, string> ZRANGEBYSCORE(string key, double min, double max, int offset, int count)
        {
            //ZRANGEBYSCORE key min max [WITHSCORES] [LIMIT offset count]
            var size = 5;
            if (count > 0)
                size += 3;

            using (var w = new RedisWriter(this.client, size, "ZRANGEBYSCORE"))
            {
                w.WriteArgument(key);
                w.WriteArgument(min.ToString());
                w.WriteArgument(max.ToString());
                w.WriteArgument("WITHSCORES");
                if (count > 0)
                {
                    w.WriteArgument("LIMIT");
                    w.WriteArgument(offset.ToString());
                    w.WriteArgument(count.ToString());
                }

                this.connection.SendCommand(w);
            }

            var b = this.connection.ExpectMultiBulkReply();
            if (b.Length < 2)
                return new SortedList<double, string>(0);

            var r = new SortedList<double, string>(b.Length / 2);
            var d = 0d;
            for (int i = 0, l = b.Length; i < l; i += 2)
            {
                double.TryParse(this.client.Encoding.GetString(b[i + 1]), out d);
                r[d] = this.client.Encoding.GetString(b[i]);
            }
            return r;
        }

        /// <summary>
        /// 获取元素排名
        /// </summary>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public int? ZRANK(string key, string member)
        {
            //ZRANK key member
            if (key == null)
                throw new ArgumentNullException("key");

            if (member == null)
                throw new ArgumentNullException("member");

            //If member exists in the sorted set, Integer reply: the rank of member.
            //If member does not exist in the sorted set or key does not exist, Bulk string reply: nil.
            using (var w = new RedisWriter(this.client, 3, "ZRANK"))
            {
                w.WriteArgument(key);
                w.WriteArgument(member);

                this.connection.SendCommand(w);
            }

            return this.connection.ExpectIntOrNil();
        }

         /// <summary>
        /// 删除元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns>返回的是从有序集合中删除的成员个数，不包括不存在的成员。</returns>
        public int ZREM(string key, string member)
        {
            var param = new[] { member};
            return ZREM(key, param);
        }

        /// <summary>
        /// 删除元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="members"></param>
        /// <returns>返回的是从有序集合中删除的成员个数，不包括不存在的成员。</returns>
        public int ZREM(string key, params string[] members)
        {
            //ZREM key member [member ...]
            if (key == null)
                throw new ArgumentNullException("key");

            if (members == null || members.Length == 0)
                throw new ArgumentNullException("member");

            using (var w = new RedisWriter(this.client, members.Length + 2, "ZREM"))
            {
                w.WriteArgument(key);
                foreach (var member in members)
                    w.WriteArgument(member);

                this.connection.SendCommand(w);
            }

            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 删除一个范围元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public int ZREMRANGEBYRANK(string key, double start, double stop)
        {
            //ZREMRANGEBYRANK key start stop

            if (key == null)
                throw new ArgumentNullException("key");

            //Integer reply: the number of elements removed.
            using (var w = new RedisWriter(this.client, 4, "ZREMRANGEBYRANK"))
            {
                w.WriteArgument(key);
                w.WriteArgument(start.ToString());
                w.WriteArgument(stop.ToString());

                this.connection.SendCommand(w);
            }

            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 删除一个积分范围元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int ZREMRANGEBYSCORE(string key, double min, double max)
        {
            //ZREMRANGEBYSCORE key min max

            if (key == null)
                throw new ArgumentNullException("key");

            //Integer reply: the number of elements removed.
            using (var w = new RedisWriter(this.client, 4, "ZREMRANGEBYRANK"))
            {
                w.WriteArgument(key);
                w.WriteArgument(min.ToString());
                w.WriteArgument(max.ToString());

                this.connection.SendCommand(w);
            }

            return this.connection.ExpectInt();
        }

        /// <summary>
        /// 返回给定的区间元素列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public string[] ZREVRANGEValues(string key, int start, int stop)
        {
            var b = this.ZREVRANGEByte(key, start, stop, false);
            return this.connection.ExpectToStringArray(b);
        }

        /// <summary>
        /// 返回给定的区间元素列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public SortedList<double, string> ZREVRANGE(string key, int start, int stop)
        {
            var b = this.ZREVRANGEByte(key, start, stop, true);
            if (b.Length < 2)
                return new SortedList<double, string>(0);

            var r = new SortedList<double, string>(b.Length / 2);
            var d = 0d;
            for (int i = 0, l = b.Length; i < l; i += 2)
            {
                double.TryParse(this.client.Encoding.GetString(b[i + 1]), out d);
                r[d] = this.client.Encoding.GetString(b[i]);
            }
            return r;
        }

        /// <summary>
        /// 返回给定的区间元素列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="withscores">是否返回分值</param>
        /// <returns></returns>
        private byte[][] ZREVRANGEByte(string key, int start, int stop, bool withscores)
        {
            //ZREVRANGE key start stop [WITHSCORES]

            if (key == null)
                throw new ArgumentNullException("key");

            //Array reply: list of elements in the specified range (optionally with their scores).

            if (withscores)
            {
                using (var w = new RedisWriter(this.client, 5, "ZREVRANGE"))
                {
                    w.WriteArgument(key);
                    w.WriteArgument(start.ToString());
                    w.WriteArgument(stop.ToString());
                    w.WriteArgument("WITHSCORES");

                    this.connection.SendCommand(w);
                }
            }
            else
            {
                using (var w = new RedisWriter(this.client, 4, "ZREVRANGE"))
                {
                    w.WriteArgument(key);
                    w.WriteArgument(start.ToString());
                    w.WriteArgument(stop.ToString());

                    this.connection.SendCommand(w);
                }
            }

            return this.connection.ExpectMultiBulkReply();
        }

        /// <summary>
        /// 返回有序集 key 中， score 值介于 max 和 min 之间(默认包括等于 max 或 min )的所有的成员。有序集成员按 score 值递减(从大到小)的次序排列。
        /// 具有相同 score 值的成员按字典序的逆序(reverse lexicographical order )排列。
        /// 除了成员按 score 值递减的次序排列这一点外， ZREVRANGEBYSCORE 命令的其他方面和 ZRANGEBYSCORE 命令一样
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="count">设置数量则offset有产效</param>
        /// <param name="offset">取出偏移量,需设置count有效</param>
        /// <returns></returns>
        public string[] ZREVRANGEBYSCOREValues(string key, double min, double max, int offset, int count)
        {
            var b = this.ZREVRANGEBYSCOREByte(key, min, max, false, offset, count);
            return this.connection.ExpectToStringArray(b);
        }

        /// <summary>
        /// 返回有序集 key 中， score 值介于 max 和 min 之间(默认包括等于 max 或 min )的所有的成员。有序集成员按 score 值递减(从大到小)的次序排列。
        /// 具有相同 score 值的成员按字典序的逆序(reverse lexicographical order )排列。
        /// 除了成员按 score 值递减的次序排列这一点外， ZREVRANGEBYSCORE 命令的其他方面和 ZRANGEBYSCORE 命令一样
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="count">设置数量则offset有产效</param>
        /// <param name="offset">取出偏移量,需设置count有效</param>
        /// <returns></returns>
        public SortedList<double, string> ZREVRANGEBYSCORE(string key, double min, double max, int offset, int count)
        {
            var b = this.ZREVRANGEBYSCOREByte(key, min, max, true, offset, count);
            if (b.Length < 2)
                return new SortedList<double, string>(0);

            var r = new SortedList<double, string>(b.Length / 2);
            var d = 0d;
            for (int i = 0, l = b.Length; i < l; i += 2)
            {
                double.TryParse(this.client.Encoding.GetString(b[i + 1]), out d);
                r[d] = this.client.Encoding.GetString(b[i]);
            }
            return r;
        }


        /// <summary>
        /// 返回有序集 key 中， score 值介于 max 和 min 之间(默认包括等于 max 或 min )的所有的成员。有序集成员按 score 值递减(从大到小)的次序排列。
        /// 具有相同 score 值的成员按字典序的逆序(reverse lexicographical order )排列。
        /// 除了成员按 score 值递减的次序排列这一点外， ZREVRANGEBYSCORE 命令的其他方面和 ZRANGEBYSCORE 命令一样
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="withscores">是否返回分值</param>
        /// <param name="count">设置数量则offset有产效</param>
        /// <param name="offset">取出偏移量,需设置count有效</param>
        /// <returns></returns>
        private byte[][] ZREVRANGEBYSCOREByte(string key, double min, double max, bool withscores, int offset, int count)
        {
            //ZREVRANGEBYSCORE key max min [WITHSCORES] [LIMIT offset count]

            if (key == null)
                throw new ArgumentNullException("key");

            //Array reply: list of elements in the specified score range (optionally with their scores).

            var size = 4;
            if (withscores)
                size++;
            if (count > 0)
                size += 3;

            using (var w = new RedisWriter(this.client, size, "ZREVRANGEBYSCORE"))
            {
                w.WriteArgument(key);
                w.WriteArgument(max.ToString());
                w.WriteArgument(min.ToString());
                if (withscores)
                    w.WriteArgument("WITHSCORES");
                if (count > 0)
                {
                    w.WriteArgument("LIMIT");
                    w.WriteArgument(offset.ToString());
                    w.WriteArgument(count.ToString());
                }

                this.connection.SendCommand(w);
            }

            return this.connection.ExpectMultiBulkReply();
        }

        /// <summary>
        /// 返回有序集 key 中成员 member 的排名。其中有序集成员按 score 值递减(从大到小)排序。
        /// 排名以 0 为底，也就是说， score 值最大的成员排名为 0 。
        /// 使用 ZRANK 命令可以获得成员按 score 值递增(从小到大)排列的排名。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public int? ZREVRANK(string key, string member)
        {
            //ZREVRANK key member
            if (key == null)
                throw new ArgumentNullException("key");

            if (member == null)
                throw new ArgumentNullException("member");

            //If member exists in the sorted set, Integer reply: the rank of member.
            //If member does not exist in the sorted set or key does not exist, Bulk string reply: nil.
            using (var w = new RedisWriter(this.client, 3, "ZREVRANK"))
            {
                w.WriteArgument(key);
                w.WriteArgument(member);

                this.connection.SendCommand(w);
            }
            return this.connection.ExpectIntOrNil();
        }

        /// <summary>
        /// 返回有序集 key 中，成员 member 的 score 值。如果 member 元素不是有序集 key 的成员，或 key 不存在，返回 nil 。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public double ZSCORE(string key, string member)
        {
            //ZSCORE key member
            if (key == null)
                throw new ArgumentNullException("key");

            if (member == null || member.Length == 0)
                throw new ArgumentNullException("member");

            //If member exists in the sorted set, Integer reply: the rank of member.
            //If member does not exist in the sorted set or key does not exist, Bulk string reply: nil.
            using (var w = new RedisWriter(this.client, 3, "ZSCORE"))
            {
                w.WriteArgument(key);
                w.WriteArgument(member);

                this.connection.SendCommand(w);
            }

            return this.connection._ExpectToDouble();
        }
    }

}