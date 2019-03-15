using System;

namespace Adf
{
    /// <summary>
    /// Queue Server Action Type
    /// </summary>
    public class QueueServerAction
    {
        /// <summary>
        /// LPUSH
        /// </summary>
        public const byte LPUSH = 1;
        /// <summary>
        /// RPUSH
        /// </summary>
        public const byte RPUSH = 2;
        /// <summary>
        /// DELETE
        /// </summary>
        public const byte DELETE = 3;
        /// <summary>
        /// PULL
        /// </summary>
        public const byte PULL = 4;
        /// <summary>
        /// CLEAR
        /// </summary>
        public const byte CLEAR = 5;
        /// <summary>
        /// COUNT
        /// </summary>
        public const byte COUNT = 6;
        /// <summary>
        /// LCANCEL
        /// </summary>
        public const byte LCANCEL = 7;
        /// <summary>
        /// RCANCEL
        /// </summary>
        public const byte RCANCEL = 8; 
        /// <summary>
        /// CREATE QUEUE
        /// </summary>
        public const byte CREATEQUEUE = 9;
        /// <summary>
        /// DELETE QUEUE
        /// </summary>
        public const byte DELETEQUEUE = 10;

        /// <summary>
        /// RESULT OK
        /// </summary>
        public const string OK = "ok";
    }
    
    /// <summary>
    /// Queue Server Encoder
    /// </summary>
    public static class QueueServerEncoder
    {
        /// <summary>
        /// generate a request id,  id is 32 byte uuid
        /// </summary>
        /// <returns></returns>
        public static string GenerateRequestID()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// encode lpush
        /// </summary>
        /// <param name="body"></param>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] LPush(string queue, string requestId, string body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            var body_data = System.Text.Encoding.UTF8.GetBytes(body);
            return LPush(queue, requestId, body_data);
        }

        /// <summary>
        /// encode lpush
        /// </summary>
        /// <param name="body"></param>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] LPush(string queue, string requestId, byte[] body)
        {
            if (queue == null || queue == "")
                throw new ArgumentNullException("queue");

            if (requestId == null || requestId == "")
                throw new ArgumentNullException("requestId");

            if (body == null)
                throw new ArgumentNullException("body");

            var queue_data = System.Text.Encoding.ASCII.GetBytes(queue);
            var id_data = System.Text.Encoding.ASCII.GetBytes(requestId);
            //
            var qdl = queue_data.Length;
            var idl = id_data.Length;
            var bdl = body.Length;
            //
            var position = 0;
            //
            var data2 = new byte[7 + qdl + idl + bdl];
            data2[position] = QueueServerAction.LPUSH;
            position += 1;
            //
            Adf.BaseDataConverter.ToBytes((ushort)idl, data2, position);
            position += 2;
            Array.Copy(id_data, 0, data2, position, idl);
            position += idl;
            //
            Adf.BaseDataConverter.ToBytes((ushort)qdl, data2, position);
            position += 2;
            Array.Copy(queue_data, 0, data2, position, qdl);
            position += qdl;
            //
            Adf.BaseDataConverter.ToBytes((ushort)bdl, data2, position);
            position += 2;
            if (bdl > 0)
            {
                Array.Copy(body, 0, data2, position, bdl);
                //position += ddl;
            }
            //
            return data2;
        }

        /// <summary>
        /// encode rpush
        /// </summary>
        /// <param name="body"></param>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] RPush(string queue, string requestId, string body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            var body_data = System.Text.Encoding.UTF8.GetBytes(body);
            return RPush(queue, requestId, body_data);
        }

        /// <summary>
        /// encode rpush
        /// </summary>
        /// <param name="body"></param>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] RPush(string queue, string requestId, byte[] body)
        {
            if (queue == null || queue == "")
                throw new ArgumentNullException("queue");

            if (requestId == null || requestId == "")
                throw new ArgumentNullException("requestId");

            if (body == null)
                throw new ArgumentNullException("body");

            var queue_data = System.Text.Encoding.ASCII.GetBytes(queue);
            var id_data = System.Text.Encoding.ASCII.GetBytes(requestId);
            //
            var qdl = queue_data.Length;
            var idl = id_data.Length;
            var bdl = body.Length;
            //
            var position = 0;
            //
            var data2 = new byte[7 + qdl + idl + bdl];
            data2[position] = QueueServerAction.RPUSH;
            position += 1;
            //
            Adf.BaseDataConverter.ToBytes((ushort)idl, data2, position);
            position += 2;
            Array.Copy(id_data, 0, data2, position, idl);
            position += idl;
            //
            Adf.BaseDataConverter.ToBytes((ushort)qdl, data2, position);
            position += 2;
            Array.Copy(queue_data, 0, data2, position, qdl);
            position += qdl;
            //
            Adf.BaseDataConverter.ToBytes((ushort)bdl, data2, position);
            position += 2;
            if (bdl > 0)
            {
                Array.Copy(body, 0, data2, position, bdl);
                //position += bdl;
            }
            //
            return data2;
        }

        /// <summary>
        /// encode clear, invoke QueueServerActionResult.GetBodyIntger() get clear count.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] Clear(string queue, string requestId)
        {
            return Action(QueueServerAction.CLEAR, queue, requestId);
        }

        /// <summary>
        /// encode count, invoke QueueServerActionResult.GetBodyIntger() get result.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] Count(string queue, string requestId)
        {
            return Action(QueueServerAction.COUNT, queue, requestId);
        }

        /// <summary>
        /// encode delete
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] Delete(string queue, string requestId)
        {
            return Action(QueueServerAction.DELETE, queue, requestId);
        }

        /// <summary>
        /// encode lcancel
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] LCancel(string queue, string requestId)
        {
            return Action(QueueServerAction.LCANCEL, queue, requestId);
        }

        /// <summary>
        /// encode rcancel
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] RCancel(string queue, string requestId)
        {
            return Action(QueueServerAction.RCANCEL, queue, requestId);
        }

        /// <summary>
        /// encode pull
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] Pull(string queue, string requestId)
        {
            return Action(QueueServerAction.PULL, queue, requestId);
        }

        /// <summary>
        /// encode create queue
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] CreateQueue(string queue, string requestId)
        {
            return Action(QueueServerAction.CREATEQUEUE, queue, requestId);
        }

        /// <summary>
        /// encode delete queue
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        public static byte[] DeleteQueue(string queue, string requestId)
        {
            return Action(QueueServerAction.DELETEQUEUE, queue, requestId);
        }

        /// <summary>
        /// encode clear
        /// </summary>
        /// <param name="action"></param>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">queue or requestId or data is null</exception>
        private static byte[] Action(byte action, string queue, string requestId)
        {
            if (queue == null || queue == "")
                throw new ArgumentNullException("queue");

            if (requestId == null || requestId == "")
                throw new ArgumentNullException("requestId");

            var queue_data = System.Text.Encoding.ASCII.GetBytes(queue);
            var id_data = System.Text.Encoding.ASCII.GetBytes(requestId);
            //
            var qdl = queue_data.Length;
            var idl = id_data.Length;
            //
            var position = 0;
            //
            var data2 = new byte[5 + qdl + idl];
            //action
            data2[position] = action;
            position += 1;
            //id
            Adf.BaseDataConverter.ToBytes((ushort)idl, data2, position);
            position += 2;
            Array.Copy(id_data, 0, data2, position, idl);
            position += idl;
            //queue
            Adf.BaseDataConverter.ToBytes((ushort)qdl, data2, position);
            position += 2;
            Array.Copy(queue_data, 0, data2, position, qdl);
            //position += qdl;
            //
            return data2;
        }

        /// <summary>
        /// 解释一个操作结果
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="Adf.QueueServerDecodeException"></exception>
        /// <returns></returns>
        public static QueueServerActionResult Decode(byte[] data)
        {
            byte action = 0;
            var id = "";
            var queue = "";
            var result = "";
            //
            QueueServerActionResult ar = null;
            //
            try
            {
                int position = 0;
                int length = 0;
                //action
                action = data[position];
                position = 1;
                //id
                length = Adf.BaseDataConverter.ToUInt16(data, position);
                position += 2;
                id = System.Text.Encoding.ASCII.GetString(data, position, length);
                position += length;
                //queue
                length = Adf.BaseDataConverter.ToUInt16(data, position);
                position += 2;
                queue = System.Text.Encoding.ASCII.GetString(data, position, length);
                position += length;
                //result
                length = Adf.BaseDataConverter.ToUInt16(data, position);
                position += 2;
                result = System.Text.Encoding.ASCII.GetString(data, position, length);
                position += length;
                //
                ar = new QueueServerActionResult(action
                , id
                , queue
                , result);
                //
                if (result == QueueServerAction.OK)
                {
                    if (action == QueueServerAction.PULL)
                    {
                        //body
                        length = Adf.BaseDataConverter.ToUInt16(data, position);
                        position += 2;
                        var body = new byte[length];
                        Array.Copy(data, position, body, 0, length);
                        position += length;
                        //duplications
                        var duplications = Adf.BaseDataConverter.ToUInt16(data, position);
                        position += 2;
                        //message id
                        var messageId = Adf.BaseDataConverter.ToUInt64(data, position);
                        //position += 8;
                        ar.SetBody(body, duplications, messageId);
                    }
                    else if (action == QueueServerAction.RPUSH)
                    {
                        //message id
                        var messageId = Adf.BaseDataConverter.ToUInt64(data, position);
                        //position += 8;
                        ar.SetMessageId(messageId);
                    }
                    else if (action == QueueServerAction.LPUSH)
                    {
                        //message id
                        var messageId = Adf.BaseDataConverter.ToUInt64(data, position);
                        //position += 8;
                        ar.SetMessageId(messageId);
                    }
                    else if (action == QueueServerAction.COUNT)
                    {
                        //count
                        var count = Adf.BaseDataConverter.ToInt32(data, position);
                        //position += 4;
                        ar.SetCount(count);
                    }
                    else if (action == QueueServerAction.CLEAR)
                    {
                        //count
                        var count = Adf.BaseDataConverter.ToInt32(data, position);
                        //position += 4;
                        ar.SetCount(count);
                    }
                }
                //
                return ar;
            }
            catch (Exception exception)
            {
                throw new QueueServerDecodeException("data length invalid.", exception);
            }
        }
    }

    /// <summary>
    /// 表示一个队列操作结果
    /// </summary>
    public class QueueServerActionResult
    {
        /// <summary>
        /// 获取当前操作类型，对应 <see cref="QueueServerActionResult"/>
        /// </summary>
        public byte Action { get; private set; }
        /// <summary>
        /// 获取请求标识
        /// </summary>
        public string RequestId { get; private set; }
        /// <summary>
        /// 获取请求队列
        /// </summary>
        public string Queue { get; private set; }
        /// <summary>
        /// 获取请求结果
        /// </summary>
        public string Result { get; private set; }
        /// <summary>
        /// 获取PULL时消息实体
        /// </summary>
        public byte[] Body { get; private set; }
        /// <summary>
        /// 获取PULL时获取当前消息已被获取的次数，首次被获取该值为1
        /// </summary>
        public ushort Duplications { get; private set; }
        /// <summary>
        /// 获取PULL/RPUSH/LPUSH 时描述消息ID
        /// </summary>
        public ulong MessageId { get; private set; }
        /// <summary>
        /// 获取COUNT/CLEAR 时数量描述
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="action"></param>
        /// <param name="requestId"></param>
        /// <param name="queue"></param>
        /// <param name="result"></param>
        public QueueServerActionResult(byte action, string requestId, string queue, string result)
        {
            this.Action = action;
            this.RequestId = requestId;
            this.Queue = queue;
            this.Result = result;
        }

        /// <summary>
        /// 获取消息实体UTF8解析的字符串表现形式
        /// </summary>
        /// <returns></returns>
        public string GetBodyString()
        {
            var body = this.Body;

            if (body == null)
                return null;

            if (body.Length == 0)
                return "";

            return System.Text.Encoding.UTF8.GetString(body);
        }
        
        /// <summary>
        /// 设置当前消息已被获取的次数
        /// </summary>
        /// <param name="duplications"></param>
        /// <param name="body"></param>
        /// <param name="messageId"></param>
        public void SetBody(byte[] body, ushort duplications, ulong messageId)
        {
            this.Duplications = duplications;
            this.Body = body;
            this.MessageId = messageId;
        }

        /// <summary>
        /// 设置当前消息对数量的描述
        /// </summary>
        /// <param name="count"></param>
        public void SetCount(int count)
        {
            this.Count = count;
        }

        /// <summary>
        /// 设置当前消息的消息ID
        /// </summary>
        /// <param name="messageId"></param>
        public void SetMessageId(ulong messageId)
        {
            this.MessageId = messageId;
        }
    }

    /// <summary>
    /// 解析失败异常
    /// </summary>
    public class QueueServerDecodeException : Exception
    {
        /// <summary>
        /// 初始化一个新异常
        /// </summary>
        /// <param name="message">消息</param>
        public QueueServerDecodeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 初始化一个新异常
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="innerException">内嵌异常</param>
        public QueueServerDecodeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}