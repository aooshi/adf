using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 一致性哈希节点接口
    /// </summary>
    public interface IConsistentHashingNode
    {
        /// <summary>
        /// 获取哈希标识字符串
        /// </summary>
        /// <returns></returns>
        string GetHashingIdentity();
    }

    /// <summary>
    /// Consistent Hashing
    /// 一致性哈希
    /// </summary>
    /// <typeparam name="T">实现 <see cref="IConsistentHashingNode"/> 接口节点</typeparam>
    public class ConsistentHashing<T> where T : IConsistentHashingNode
    {
        const int VNODE_SIZE = 128;

        private bool isHashing;
        private Encoding encoding;
        private int maxIndex = 0;

        int[] scores;
        int[] indexs;

        int nodeCount;
        /// <summary>
        /// 获取含虚拟节点的总节点数
        /// </summary>
        public int NodeCount { get { return this.nodeCount; } }

        T[] nodes;
        /// <summary>
        /// 获取实际节点
        /// </summary>
        public T[] Nodes { get { return this.nodes; } }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="nodes">节点数组</param>
        public ConsistentHashing(T[] nodes)
        {
            if (nodes == null || nodes.Length == 0)
                throw new ArgumentNullException("nodes");

            if (nodes.Length > short.MaxValue)
                throw new ArgumentOutOfRangeException("nodes", "nodes size limit " + short.MaxValue);

            this.nodes = nodes;
            this.encoding = Encoding.UTF8;

            if (nodes.Length == 1)
            {
                this.nodeCount = 1;
                this.isHashing = false;
            }
            else
            {
                this.nodeCount = nodes.Length * VNODE_SIZE;
                this.isHashing = true;

                this.scores = new int[this.nodeCount];
                this.indexs = new int[this.nodeCount];

                //对所有节点，生成虚拟结点
                string nodeIdentity = "";
                for (int i = 0, l = nodes.Length; i < l; i++)
                {
                    nodeIdentity = ((IConsistentHashingNode)nodes[i]).GetHashingIdentity();
                    for (int j = 0; j < VNODE_SIZE; j++)
                    {
                        this.scores[i * VNODE_SIZE + j] = this.Hash(nodeIdentity + "#" + j);
                        this.indexs[i * VNODE_SIZE + j] = i;
                    }
                }

                //sort
                this.Sort();
                this.maxIndex = this.nodeCount - 1;
            }
        }

        /// <summary>
        /// 获取一个节点
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetPrimary(string key)
        {
            if (this.isHashing)
            {
                return GetNodeForKey(this.Hash(key));
            }
            else
            {
                return this.nodes[0];
            }
        }

        T GetNodeForKey(int hash)
        {
            var nodeIndex = 0;
            //大于最大
            if (hash > this.scores[this.maxIndex])
            {
                nodeIndex = this.indexs[this.maxIndex];
                return this.nodes[nodeIndex];
            }
            //小于最小
            else if (hash < this.scores[0])
            {
                nodeIndex = this.indexs[0];
                return this.nodes[nodeIndex];
            }
            else
            {
                int min = 0;
                int max = this.maxIndex;
                int count = this.nodeCount;
                int index = 0;

                //match
                while (count > 10)
                {
                    index = min + count / 2;
                    if (this.scores[index] > hash)
                        max = index;
                    else
                        min = index;

                    count = (max - min) + 1;
                }

                //search
                for (; min <= max; min++)
                {
                    if (this.scores[min] > hash)
                    {
                        nodeIndex = this.indexs[min];
                        return this.nodes[nodeIndex];
                    }
                }

                nodeIndex = this.indexs[0];
                return this.nodes[nodeIndex];
            }
        }
        
        //T GetNodeForKey(int hash)
        //{
        //    if (hash > this.scores[this.nodeCount - 1])
        //    {
        //        return this.nodes[0];
        //    }
        //    else if (this.nodeCount > 10)
        //    {
        //        int min = 0, max = this.nodeCount - 1, count = max, index = 0;

        //        //match
        //        while (count > 5)
        //        {
        //            index = min + count / 2;
        //            if (this.scores[index] > hash)
        //                max = index;
        //            else
        //                min = index;

        //            count = max - min;
        //        }

        //        //search
        //        for (; min <= max; min++)
        //        {
        //            if (this.scores[min] > hash)
        //            {
        //                var index2 = this.indexs[min];
        //                return this.nodes[index2];

        //                //return this.values[min];
        //            }
        //        }

        //    }
        //    else
        //    {
        //        for (int i = 0; i < this.nodeCount; i++)
        //        {
        //            if (this.scores[i] > hash)
        //            {
        //                var index2 = this.indexs[i];
        //                return this.nodes[index2];

        //                //   return this.values[i];
        //            }
        //        }
        //    }

        //    return this.nodes[0];
        //}

        //T GetNodeForKey(uint hash)
        //{
        //    T n = default(T);

        //    //如果找到这个节点，直接取节点，返回   
        //    if (!this.nodelist.TryGetValue(hash, out n))
        //    {
        //        if (hash < this.max)
        //        {
        //            //for (var i = 0; i < this.NodeCountAll; i++)
        //            //{
        //            //    if (this.nodes.Keys[i] > hash)
        //            //    {
        //            //        n = this.nodes.Values[i];
        //            //        break;
        //            //    }
        //            //}

        //            int min, max, count, index;
        //            min = 0;
        //            max = this.NodeCount - 1;
        //            count = max;

        //            //match
        //            while (count > 5)
        //            {
        //                index = min + count / 2;
        //                if (this.nodelist.Keys[index] > hash)
        //                    max = index;
        //                else
        //                    min = index;

        //                count = max - min;
        //            }

        //            //search
        //            for (; min <= max; min++)
        //            {
        //                if (this.nodelist.Keys[min] > hash)
        //                {
        //                    n = this.nodelist.Values[min];
        //                    break;
        //                }
        //            }
        //        }

        //        if (n == null)
        //            n = this.nodelist.Values[0];
        //    }

        //    return n;
        //}

        /// <summary>
        /// Hash
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual int Hash(string key)
        {
            var bytes = this.encoding.GetBytes(key);
            return Adf.CRC32Helper.Encode(bytes);
        }

        private void Sort()
        {
            this.Sort(this.scores, this.indexs, 0, this.scores.Length - 1);
        }

        private void Sort(int[] numbers, int[] indexs, int left, int right)
        {
            if (left < right)
            {
                int middle = numbers[(left + right) / 2];
                int i = left - 1;
                int j = right + 1;
                int number;
                int index;
                while (true)
                {
                    while (numbers[++i] < middle) ;

                    while (numbers[--j] > middle) ;

                    if (i >= j)
                        break;

                    //Swap(numbers, i, j);
                    number = numbers[i];
                    numbers[i] = numbers[j];
                    numbers[j] = number;

                    index = indexs[i];
                    indexs[i] = indexs[j];
                    indexs[j] = index;
                }

                this.Sort(numbers, indexs, left, i - 1);
                this.Sort(numbers, indexs, j + 1, right);
            }
        }
    }
}