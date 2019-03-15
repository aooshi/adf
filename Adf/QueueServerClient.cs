using System;
using System.Collections.Generic;
using System.Threading;
using Adf.Config;
using System.IO;
using System.Text;

namespace Adf
{
    /// <summary>
    /// 客户端事件委托
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="client"></param>
    /// <param name="args"></param>
    [Obsolete("class obsolete, please QueueServerBase")]
    public delegate void QueueServerClientEvent<T>(QueueServerClient client, T args);
    /// <summary>
    /// 客户端读取事件委托
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    [Obsolete("class obsolete, please QueueServerBase")]
    public delegate QueueServerReceiveOption QueueServerReceiveEvent(QueueServerClient client, QueueServerMessageAckArgs args);

    /// <summary>
    /// 队列服务客户端
    /// </summary>
    [Obsolete("class obsolete, please QueueServerBase")]
    public class QueueServerClient : IPoolInstance, IDisposable
    {
        //global flag
        const byte FLAG_USER_TRANSFER_ID = 1;
        //const byte FLAG_NO_ACK = 2;

        Dictionary<UInt64, AckWaitHandle> ackDictionary = null;

        AckWaitHandle connectionWaitHandle = null;
        WebSocketClient client = null;
        bool isDisposed = false;
        ushort commitTimeout = 0;
        ushort timeout = 5;
        int millisecondsTimeout = 5000;
        long transferId = 0;
        string topic = "";

        /// <summary>
        /// LPush ack event
        /// </summary>
        public event QueueServerClientEvent<QueueServerAckArgs> LPushAck = null;
        /// <summary>
        /// RPush ack event
        /// </summary>
        public event QueueServerClientEvent<QueueServerAckArgs> RPushAck = null;
        /// <summary>
        /// pull ack event
        /// </summary>
        public event QueueServerClientEvent<QueueServerMessageAckArgs> PullAck = null;
        /// <summary>
        /// commit ack event
        /// </summary>
        public event QueueServerClientEvent<QueueServerAckArgs> CommitAck = null;
        /// <summary>
        /// rollback ack event
        /// </summary>
        public event QueueServerClientEvent<QueueServerAckArgs> RollbackAck = null;
        /// <summary>
        /// receive error event
        /// </summary>
        public event QueueServerClientEvent<QueueServerErrorEventArgs> ReceiveError = null;
        /// <summary>
        /// Network error
        /// </summary>
        public event QueueServerClientEvent<QueueServerErrorEventArgs> NetworkError = null;

        /// <summary>
        /// 获取一个值表示是否已连接
        /// </summary>
        public bool IsConnectioned
        {
            get { return this.client.IsConnectioned; }
        }

        /// <summary>
        /// <para>获取消息处理超时时间,单位：秒, 默认： 30s</para>
        /// <para>get commit timeout,unit: seconds, default 30s</para>
        /// </summary>
        public ushort CommitTimeout
        {
            get { return this.commitTimeout; }
        }

        /// <summary>
        /// <para>获取或设置服务器响应超时时间,单位：秒, 默认: 10秒</para>
        /// <para>get or set server response timeout,unit: seconds, default: 10s</para>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">value allow 1-65535</exception>
        public ushort Timeout
        {
            get { return this.timeout; }
            set
            {
                if (timeout < 1)
                    throw new ArgumentOutOfRangeException("value", "value allow 1-65535");
                this.timeout = value;
                this.millisecondsTimeout = value * 1000;
            }
        }

        /// <summary>
        /// get topic
        /// </summary>
        public string Topic
        {
            get { return this.topic; }
        }

        /// <summary>
        /// get or set user state
        /// </summary>
        public object UserState
        {
            get;
            set;
        }

        /// <summary>
        /// 获取当前实例所使用的配置，通过 configFileName 初始化实例时本属性有效， 其它实例化方式下均为NULL
        /// </summary>
        public Adf.Config.NameValue Configuration
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化新实例,应用指定的配置文件名称
        /// </summary>
        /// <param name="configFileName">Config 下配置文件名称</param>
        public QueueServerClient(string configFileName)
        {
            var configuration = Adf.Config.NameValue.GetConfiguration(configFileName);
            string host = configuration.GetString("host");
            int port = configuration.GetInt32("port");
            string topic = configuration.GetString("topic");
            ushort commitTimeout = configuration.GetUInt16("commitTimeout", 30);
            //
            this.Configuration = configuration;
            //
            this.Initialize(host, port, topic, commitTimeout);
        }

        /// <summary>
        /// 初始化新实例,应用指定的配置文件名称
        /// </summary>
        /// <param name="configFileName">Config 下配置文件名称</param>
        /// <param name="prefix">当前配置前缀</param>
        public QueueServerClient(string configFileName, string prefix)
        {
            var configuration = Adf.Config.NameValue.GetConfiguration(configFileName);
            string host = configuration.GetString(prefix + "Host");
            int port = configuration.GetInt32(prefix + "Port");
            string topic = configuration.GetString(prefix + "Topic");
            ushort commitTimeout = configuration.GetUInt16(prefix + "CommitTimeout", 30);
            //
            this.Configuration = configuration;
            //
            this.Initialize(host, port, topic, commitTimeout);
        }

        /// <summary>
        /// 初始化新实例,设置拉超时时间30s
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="topic"></param>
        public QueueServerClient(string host, int port, string topic)
        {
            ushort commitTimeout = 30;
            this.Initialize(host, port, topic, commitTimeout);
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="topic"></param>
        /// <param name="commitTimeout">
        /// 
        /// <para>commit timeout, unit: seconds;</para>
        /// <para>消息处理确认超时间,单位：秒;</para>
        /// <para>此时间用于服务器对消息判断超时恢复</para>
        /// <para>消息pull到后,超过此时间还未进行commit/rollback则会重新进入发送队列，并对 duplications + 1</para>
        /// <para>应用可通过判断 duplications 是否为零以确认该消息是第几次的复制品，第一次获取到的消息此值为零</para>
        /// <para>一般来说你需要对复制品进行业务上的二次调用检查</para>
        /// 
        /// </param>
        public QueueServerClient(string host, int port, string topic, ushort commitTimeout)
        {
            this.Initialize(host, port, topic, commitTimeout);
        }

        private void Initialize(string host, int port, string topic, ushort commitTimeout)
        {
            if (commitTimeout < 2)
                throw new ArgumentOutOfRangeException("commitTimeout", "commitTimeout allow 2 - 65535");
            //
            this.commitTimeout = commitTimeout;
            this.topic = topic;
            this.connectionWaitHandle = new AckWaitHandle();
            this.ackDictionary = new Dictionary<UInt64, AckWaitHandle>(32);
            //
            this.client = new WebSocketClient(host, port, "/queue" + topic);
            this.client.RequestHeader.Add("X-CommitTimeout", commitTimeout.ToString());
            this.client.Error += new EventHandler<WebSocketErrorEventArgs>(ClientOnError);
            this.client.Closed += new EventHandler<WebSocketCloseEventArgs>(ClientOnClosed);
            this.client.Message += new EventHandler<WebSocketMessageEventArgs>(ClientOnMessage);
        }

        private void ClientOnClosed(object sender, WebSocketCloseEventArgs e)
        {
            this.PoolAbandon = false;

            lock (this.ackDictionary)
            {
                foreach (var ack in this.ackDictionary)
                {
                    ack.Value.isClosed = true;
                    ack.Value.Set();
                }
            }
        }

        private void ClientOnError(object sender, WebSocketErrorEventArgs e)
        {
            if (this.NetworkError != null)
            {
                this.NetworkError(this, new QueueServerErrorEventArgs(e.Exception));
            }
        }

        private void ClientOnMessage(object sender, WebSocketMessageEventArgs e)
        {
            if (e.Opcode == WebSocketOpcode.Binary)
            {
                var buffer = e.Buffer;
                var bufferLength = buffer.Length;
                if (bufferLength < 12)
                {
                    return;
                }

                //1	            8	        1	    2	        2	    N
                //Transfer Type	Transfer ID	Flags	Error Code	Length	Error Message
                //传输类型	传输标识	标志	错误编码	块长度	消息内容

                var transferId = Adf.BaseDataConverter.ToUInt64(buffer, 1);
                var errorCode = (QueueServerErrorCode)Adf.BaseDataConverter.ToUInt16(buffer, 10);
                var errorMessage = "";
                if (errorCode == QueueServerErrorCode.None)
                { }
                else if (errorCode == QueueServerErrorCode.Normal)
                { }
                else
                {
                    if (bufferLength > 14)
                    {
                        var errorLength = Adf.BaseDataConverter.ToUInt16(buffer, 12);
                        if (errorLength > 0 && errorLength + 14 == buffer.Length)
                        {
                            errorMessage = Encoding.UTF8.GetString(buffer, 14, errorLength);
                        }
                    }
                    else
                    {
                        errorMessage = errorCode.ToString();
                    }
                }

                //
                var transferType = (TransferType)buffer[0];
                if (transferType == TransferType.Connect)
                {
                    this.connectionWaitHandle.errorCode = errorCode;
                    this.connectionWaitHandle.errorMessage = errorMessage;
                    this.connectionWaitHandle.Set();
                }
                else if ((buffer[9] & FLAG_USER_TRANSFER_ID) == FLAG_USER_TRANSFER_ID)
                {
                    var messageId = 0UL;

                    if (bufferLength > 23)
                    {
                        //PULL ACK:
                        //1	            8	        1	    2	        8
                        //Transfer Type	Transfer ID	Flags	Error Code	Id
                        //传输标识	    传输标识	标志	命令标识	标识
                        //2     	    2	    1-4096
                        //Duplications	Length	Message
                        //复制	        消息长度	消息
                        if (transferType == TransferType.Pull)
                        {
                            messageId = Adf.BaseDataConverter.ToUInt64(buffer, 12);
                            var duplications = Adf.BaseDataConverter.ToUInt16(buffer, 20);
                            var length = Adf.BaseDataConverter.ToUInt16(buffer, 22);
                            var data = new byte[length];
                            if (length > 0)
                            {
                                Array.Copy(buffer, 24, data, 0, length);
                            }
                            this.OnPullAck(transferId, messageId, errorCode, errorMessage, duplications, length, data);
                        }
                    }
                    else if (bufferLength == 20)
                    {
                        //PUSH ACK && COMMIT ACK &&ROLLBACK ACK
                        //1	            8	        1	    2		    8
                        //Transfer Type	Transfer ID	Flags	Error Code	Message Id
                        //传输标识	    传输标识	标志	命令标识	消息标识

                        messageId = Adf.BaseDataConverter.ToUInt64(buffer, 12);

                        if (transferType == TransferType.RPush)
                        {
                            this.OnRPushAck(transferId, messageId, errorCode, errorMessage);
                        }
                        else if (transferType == TransferType.Commit)
                        {
                            this.OnCommitAck(transferId, messageId, errorCode, errorMessage);
                        }
                        else if (transferType == TransferType.Rollback)
                        {
                            this.OnRollbackAck(transferId, messageId, errorCode, errorMessage);
                        }
                        else if (transferType == TransferType.LPush)
                        {
                            this.OnLPushAck(transferId, messageId, errorCode, errorMessage);
                        }
                    }
                    else
                    {
                        errorCode = QueueServerErrorCode.PacketInvalid;
                        errorMessage = "Packet Invalid";
                        //
                        if (transferType == TransferType.RPush)
                        {
                            this.OnRPushAck(transferId, messageId, errorCode, errorMessage);
                        }
                        else if (transferType == TransferType.LPush)
                        {
                            this.OnLPushAck(transferId, messageId, errorCode, errorMessage);
                        }
                        else if (transferType == TransferType.Pull)
                        {
                            this.OnPullAck(transferId, messageId, errorCode, errorMessage, 0, 0, null);
                        }
                        else if (transferType == TransferType.Commit)
                        {
                            this.OnCommitAck(transferId, messageId, errorCode, errorMessage);
                        }
                        else if (transferType == TransferType.Rollback)
                        {
                            this.OnRollbackAck(transferId, messageId, errorCode, errorMessage);
                        }
                    }
                }
                else
                {
                    AckWaitHandle ackWaitHandle = null;
                    lock (this.ackDictionary)
                    {
                        this.ackDictionary.TryGetValue(transferId, out ackWaitHandle);
                    }
                    if (ackWaitHandle != null)
                    {
                        ackWaitHandle.data = buffer;
                        ackWaitHandle.errorMessage = errorMessage;
                        ackWaitHandle.errorCode = errorCode;
                        ackWaitHandle.Set();
                    }
                }
            }
        }

        /// <summary>
        /// 连接到主机
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="QueueServerException">connect failure</exception>
        public void Connect()
        {
            this.Connect(this.timeout);
        }

        /// <summary>
        /// 连接到主机
        /// </summary>
        /// <param name="timeout">连接超时时间，单位: 秒,  connect timeout , unit: seconds</param>
        /// <exception cref="IOException">connection exception</exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="QueueServerException">connect failure</exception>
        public void Connect(ushort timeout)
        {
            if (this.client.IsConnectioned)
                throw new InvalidOperationException("already connected");

            this.connectionWaitHandle.errorCode = QueueServerErrorCode.None;
            this.connectionWaitHandle.errorMessage = "";
            this.connectionWaitHandle.Reset();

            try
            {
                //if (this.client.IsConnectioned)
                //    this.client.Close();

                this.client.Connection();
            }
            catch (IOException) { }
            catch (Exception exception)
            {
                throw new IOException(exception.Message, exception);
            }

            if (this.connectionWaitHandle.WaitOne(timeout * 1000) == false)
            {
                throw new TimeoutException();
            }
            if (this.connectionWaitHandle.errorCode != QueueServerErrorCode.Normal)
            {
                this.client.Close();
                throw new QueueServerException(this.connectionWaitHandle.errorCode, this.connectionWaitHandle.errorMessage);
            }
        }


        /// <summary>
        /// 使用当前提交看超时时间选择队列
        /// </summary>
        /// <param name="topic"></param>
        /// <exception cref="ArgumentNullException">topic is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        public void Select(string topic)
        {
            this.Select(topic, this.commitTimeout);
        }

        /// <summary>
        /// 选择队列
        /// </summary>
        /// <param name="commitTimeout"></param>
        /// <param name="topic"></param>
        /// <exception cref="ArgumentNullException">topic is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        public void Select(string topic, ushort commitTimeout)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            if (commitTimeout < 2)
                throw new ArgumentOutOfRangeException("commitTimeout", "commitTimeout allow 2 - 65535");

            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;

            //1	            8	        1	    2	            1	    1-255
            //Transfer Type	Transfer ID	Flags	Commit Timeout	Length	Topic
            //传输标识	    传输标识	标志	拉取超时时间	长度	队列主题

            var topicBuffer = Encoding.UTF8.GetBytes(topic);
            var topicLength = topicBuffer.Length;

            var buffer = new byte[13 + topicLength];
            buffer[0] = (byte)TransferType.Select;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;
            Adf.BaseDataConverter.ToBytes(commitTimeout, buffer, 10);
            buffer[12] = (byte)topicLength;
            Array.Copy(topicBuffer, 0, buffer, 13, topicLength);

            var success = this.SendAck(this.millisecondsTimeout, transferId, buffer, ack =>
            {
                this.topic = topic;
                this.commitTimeout = commitTimeout;
            });

        }

        ///// <summary>
        ///// 使用当前提交超时时间进行异步选择队列,通过SelectAck事件获取结果
        ///// </summary>
        ///// <param name="topic"></param>
        ///// <param name="transferId"></param>
        ///// <exception cref="ArgumentNullException">topic is null</exception>
        ///// <exception cref="ObjectDisposedException"></exception>
        ///// <exception cref="IOException">no connect on server or network disconnected</exception>
        //public void SelectAsync(string topic, UInt64 transferId)
        //{
        //    this.SelectAsync(topic, this.commitTimeout, transferId);
        //}

        ///// <summary>
        ///// 进行异步选择队列,通过SelectAck事件获取结果
        ///// </summary>
        ///// <param name="commitTimeout"></param>
        ///// <param name="topic"></param>
        ///// <param name="transferId"></param>
        ///// <exception cref="ArgumentNullException">topic is null</exception>
        ///// <exception cref="ObjectDisposedException"></exception>
        ///// <exception cref="IOException">no connect on server or network disconnected</exception>
        //public void SelectAsync(string topic, ushort commitTimeout, UInt64 transferId)
        //{
        //    if (topic == null)
        //        throw new ArgumentNullException("topic");

        //    if (commitTimeout < 2)
        //        throw new ArgumentOutOfRangeException("commitTimeout", "commitTimeout allow 2 - 65535");

        //    byte flag = 0;
        //    flag |= FLAG_USER_TRANSFER_ID;

        //    //1	            8	        1	    2	            1	    1-255
        //    //Transfer Type	Transfer ID	Flags	Commit Timeout	Length	Topic
        //    //传输标识	    传输标识	标志	拉取超时时间	长度	队列主题

        //    var topicBuffer = Encoding.UTF8.GetBytes(topic);
        //    var topicLength = topicBuffer.Length;

        //    var buffer = new byte[13 + topicLength];
        //    buffer[0] = (byte)TransferType.Select;
        //    Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
        //    buffer[9] = flag;
        //    Adf.BaseDataConverter.ToBytes(commitTimeout, buffer, 10);
        //    buffer[12] = (byte)topicLength;
        //    Array.Copy(topicBuffer, 0, buffer, 13, topicLength);

        //    if (this.isDisposed == true)
        //        throw new ObjectDisposedException(this.GetType().Name);

        //    if (this.isConnected == false)
        //        throw new IOException("no connect on server");

        //    this.client.Send(buffer);
        //}


        /// <summary>
        /// 删除队列
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        public void Delete()
        {
            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;

            //1	            8	        1
            //Transfer Type	Transfer ID	Flags
            //传输标识	    传输标识	标志

            var buffer = new byte[10];
            buffer[0] = (byte)TransferType.Delete;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;

            var success = this.SendAck(this.millisecondsTimeout, transferId, buffer, ack =>
            {

            });
        }

        private byte[] CreateRPushPacket(byte[] data, UInt64 transferId, byte flag)
        {
            //1	            8	        1	    2	    1-4096
            //Transfer Type	Transfer ID	Flags	Length	Message
            //传输标识	传输标识	标志	消息长度	消息体

            //if (data.Length > 4096)
            //    throw new ArgumentOutOfRangeException("data", "data length allow 1-4096");

            var buffer = new byte[12 + data.Length];
            buffer[0] = (byte)TransferType.RPush;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;
            Adf.BaseDataConverter.ToBytes((ushort)data.Length, buffer, 10);
            if (data.Length > 0)
            {
                Array.Copy(data, 0, buffer, 12, data.Length);
            }

            return buffer;
        }

        private byte[] CreateLPushPacket(byte[] data, UInt64 transferId, byte flag)
        {
            //1	            8	        1	    2	    1-4096
            //Transfer Type	Transfer ID	Flags	Length	Message
            //传输标识	传输标识	标志	消息长度	消息体

            //if (data.Length > 4096)
            //    throw new ArgumentOutOfRangeException("data", "data length allow 1-4096");

            var buffer = new byte[12 + data.Length];
            buffer[0] = (byte)TransferType.LPush;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;
            Adf.BaseDataConverter.ToBytes((ushort)data.Length, buffer, 10);
            if (data.Length > 0)
            {
                Array.Copy(data, 0, buffer, 12, data.Length);
            }

            return buffer;
        }

        /// <summary>
        /// 插入一个字符消息至服务器队列前端，确认服务器已收到
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <returns>message id</returns>
        public UInt64 LPush(string input)
        {
            if (input == null)
                throw new ArgumentNullException("data");

            var data = Encoding.UTF8.GetBytes(input);
            return this.LPush(data);
        }

        /// <summary>
        /// 插入一个消息至服务器队列前端，确认服务器已收到
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="ArgumentNullException">data is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <returns>message id</returns>
        public UInt64 LPush(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;

            var buffer = this.CreateLPushPacket(data, transferId, flag);

            var messageId = 0UL;
            var success = this.SendAck(this.millisecondsTimeout, transferId, buffer, ack =>
            {
                //PUSH ACK:
                //1	            8	        1	    2		    8
                //Transfer Type	Transfer ID	Flags	Error Code	Message Id
                //传输标识	    传输标识	标志	命令标识	消息标识
                if (ack.data.Length != 20)
                {
                    throw new QueueServerException(QueueServerErrorCode.PacketInvalid, "Packet Invalid");
                }
                messageId = Adf.BaseDataConverter.ToUInt64(ack.data, 12);
            });

            return messageId;
        }

        /// <summary>
        /// 推送一个字符消息至服务器队列末尾，确认服务器已收到
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <returns>message id</returns>
        public UInt64 RPush(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var data = Encoding.UTF8.GetBytes(input);
            return this.RPush(data);
        }

        /// <summary>
        /// 推送一个消息至服务器队列末尾，确认服务器已收到
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="ArgumentNullException">data is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <returns>message id</returns>
        public UInt64 RPush(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;
            var buffer = this.CreateRPushPacket(data, transferId, flag);

            var messageId = 0UL;
            var success = this.SendAck(this.millisecondsTimeout, transferId, buffer, ack =>
            {
                //PUSH ACK:
                //1	            8	        1	    2		    8
                //Transfer Type	Transfer ID	Flags	Error Code	Message Id
                //传输标识	    传输标识	标志	命令标识	消息标识
                if (ack.data.Length != 20)
                {
                    throw new QueueServerException(QueueServerErrorCode.PacketInvalid, "Packet Invalid");
                }
                messageId = Adf.BaseDataConverter.ToUInt64(ack.data, 12);
            });

            return messageId;
        }

        /// <summary>
        /// 推送一个消息至服务器队列末尾，不确认服务器已收到即返回，使用此方法可通过 Ack 事件获取 发送结果
        /// </summary>
        /// <param name="data"></param>
        /// <param name="transferId">传输标识，允许用户自定义，该值将会ACK时回传，你可使用此值跟踪消息</param>
        /// <exception cref="ArgumentNullException">data is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        public void RPushAsync(byte[] data, UInt64 transferId)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            byte flag = 0;
            flag |= FLAG_USER_TRANSFER_ID;
            var buffer = this.CreateRPushPacket(data, transferId, flag);

            if (this.isDisposed == true)
                throw new ObjectDisposedException(this.GetType().Name);

            if (this.client.IsConnectioned == false)
                throw new IOException("no connect on server");

            this.client.SendAsync(buffer,null);
        }


        /// <summary>
        /// 插入一个消息至服务器队列前端，不确认服务器已收到即返回，使用此方法可通过 Ack 事件获取 发送结果
        /// </summary>
        /// <param name="data"></param>
        /// <param name="transferId">传输标识，允许用户自定义，该值将会ACK时回传，你可使用此值跟踪消息</param>
        /// <exception cref="ArgumentNullException">data is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        public void LPushAsync(byte[] data, UInt64 transferId)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            byte flag = 0;
            flag |= FLAG_USER_TRANSFER_ID;
            var buffer = this.CreateLPushPacket(data, transferId, flag);

            if (this.isDisposed == true)
                throw new ObjectDisposedException(this.GetType().Name);

            if (this.client.IsConnectioned == false)
                throw new IOException("no connect on server");

            this.client.SendAsync(buffer, null);
        }

        /// <summary>
        /// 从服务器拉取一个消息
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <exception cref="QueueServerException"></exception>
        public QueueServerMessage Pull()
        {
            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;

            //1	            8	        1
            //Transfer Type	Transfer ID	Flags
            //传输标识	    传输标识	标志
            var buffer = new byte[10];
            buffer[0] = (byte)TransferType.Pull;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;

            //
            QueueServerMessage qsm = null;
            this.SendAck(System.Threading.Timeout.Infinite, transferId, buffer, ack =>
            {
                qsm = this.ParseAckMessage(ack.data);
            });

            return qsm;
        }

        /// <summary>
        /// 异步拉取一个消息，通过 PullAck 事件获取 结果
        /// </summary>
        /// <param name="transferId">传输标识，允许用户自定义，该值将会ACK时回传，你可使用此值跟踪消息</param>
        /// <exception cref="ArgumentNullException">data is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        public void PullAsync(UInt64 transferId)
        {
            if (this.isDisposed == true)
                throw new ObjectDisposedException(this.GetType().Name);

            if (this.client.IsConnectioned == false)
                throw new IOException("no connect on server");

            byte flag = 0;
            flag |= FLAG_USER_TRANSFER_ID;

            //1	            8	        1
            //Transfer Type	Transfer ID	Flags
            //传输标识	    传输标识	标志
            var buffer = new byte[10];
            buffer[0] = (byte)TransferType.Pull;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;

            this.client.SendAsync(buffer, null);
        }

        ///// <summary>
        ///// 从服务器拉取一个已知消息
        ///// </summary>
        ///// <exception cref="ObjectDisposedException"></exception>
        ///// <exception cref="IOException">no connect on server or network disconnected</exception>
        ///// <exception cref="QueueServerException"></exception>
        ///// <returns>find return message, not find return null</returns>
        //public QueueServerMessage Peek(UInt64 messageId)
        //{
        //    ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
        //    byte flag = 0;

        //    //PEEK:
        //    //1	            8	        1	    8
        //    //Transfer Type	Transfer ID	Flags	Message Id
        //    //传输标识	    传输标识	标志	消息标识

        //    var buffer = new byte[18];
        //    buffer[0] = (byte)TransferType.Peek;
        //    Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
        //    buffer[9] = flag;
        //    Adf.BaseDataConverter.ToBytes(messageId, buffer, 10);

        //    //
        //    QueueServerMessage qsm = null;
        //    try
        //    {
        //        this.SendAck(this.millisecondsTimeout, transferId, buffer, ack =>
        //        {
        //            qsm = this.ParseAckMessage(ack.data);

        //            if (qsm.MessageId != messageId)
        //            {
        //                throw new QueueServerException(QueueServerErrorCode.PacketInvalid, "received error peek packet");
        //            }
        //        });
        //    }
        //    catch (QueueServerException exception)
        //    {
        //        if (exception.ErrorCode == QueueServerErrorCode.MessageNotExists)
        //        {
        //            qsm = null;
        //        }
        //        else
        //        {
        //            throw exception;
        //        }
        //    }

        //    return qsm;
        //}

        /// <summary>
        /// 提交一个消息处理完毕通知
        /// </summary>
        /// <param name="messageId"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <exception cref="QueueServerException"></exception>
        public void Commit(UInt64 messageId)
        {
            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;

            //COMMIT:
            //1	            8	        1	    8
            //Transfer Type	Transfer ID	Flags	Message Id
            //传输标识	    传输标识	标志	消息标识

            var buffer = new byte[18];
            buffer[0] = (byte)TransferType.Commit;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;
            Adf.BaseDataConverter.ToBytes(messageId, buffer, 10);

            //
            this.SendAck(this.millisecondsTimeout, transferId, buffer, ack =>
            {
                //COMMIT ACK:
                //1	            8	        1	    2		    8
                //Transfer Type	Transfer ID	Flags	Error Code	Message Id
                //传输标识	    传输标识	标志	命令标识	消息标识

                if (ack.data.Length != 20)
                {
                    throw new QueueServerException(QueueServerErrorCode.PacketInvalid, "received error commit packet");
                }
            });
        }

        /// <summary>
        /// 提交一个消息处理完毕通知
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="transferId">传输标识，允许用户自定义，该值将会ACK时回传，你可使用此值跟踪消息</param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        public void CommitAsync(UInt64 messageId, UInt64 transferId)
        {
            byte flag = 0;
            flag |= FLAG_USER_TRANSFER_ID;

            //COMMIT:
            //1	            8	        1	    8
            //Transfer Type	Transfer ID	Flags	Message Id
            //传输标识	    传输标识	标志	消息标识

            var buffer = new byte[18];
            buffer[0] = (byte)TransferType.Commit;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;
            Adf.BaseDataConverter.ToBytes(messageId, buffer, 10);

            if (this.isDisposed == true)
                throw new ObjectDisposedException(this.GetType().Name);

            if (this.client.IsConnectioned == false)
                throw new IOException("no connect on server");

            this.client.SendAsync(buffer, null);
        }

        /// <summary>
        /// 回滚一个未提交的消息至队列，以便重新接收
        /// </summary>
        /// <param name="messageId"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <exception cref="QueueServerException"></exception>
        public void Rollback(UInt64 messageId)
        {
            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;

            //ROLLBACK:
            //1	            8	        1	    8
            //Transfer Type	Transfer ID	Flags	Message Id
            //传输标识	    传输标识	标志	消息标识

            var buffer = new byte[18];
            buffer[0] = (byte)TransferType.Rollback;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;
            Adf.BaseDataConverter.ToBytes(messageId, buffer, 10);

            //
            this.SendAck(this.millisecondsTimeout, transferId, buffer, ack =>
            {
                //ROLLBACK ACK:
                //1	            8	        1	    2		    8
                //Transfer Type	Transfer ID	Flags	Error Code	Message Id
                //传输标识	    传输标识	标志	命令标识	消息标识

                if (ack.data.Length != 20)
                {
                    throw new QueueServerException(QueueServerErrorCode.PacketInvalid, "received error rollback packet");
                }
            });
        }

        /// <summary>
        /// 回滚一个未提交的消息至队列，以便重新接收
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="transferId">传输标识，允许用户自定义，该值将会ACK时回传，你可使用此值跟踪消息</param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <exception cref="QueueServerException"></exception>
        public void RollbackAsync(UInt64 messageId, UInt64 transferId)
        {
            byte flag = 0;
            flag |= FLAG_USER_TRANSFER_ID;

            //ROLLBACK:
            //1	            8	        1	    8
            //Transfer Type	Transfer ID	Flags	Message Id
            //传输标识	    传输标识	标志	消息标识

            var buffer = new byte[18];
            buffer[0] = (byte)TransferType.Rollback;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;
            Adf.BaseDataConverter.ToBytes(messageId, buffer, 10);

            if (this.isDisposed == true)
                throw new ObjectDisposedException(this.GetType().Name);

            if (this.client.IsConnectioned == false)
                throw new IOException("no connect on server");

            this.client.SendAsync(buffer, null);
        }

        /// <summary>
        /// 获取当前队列消息数量
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <exception cref="QueueServerException"></exception>
        /// <returns>return message count in queue</returns>
        public uint Count()
        {
            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;

            //1	            8	        1
            //Transfer Type	Transfer ID	Flags
            //传输标识	    传输标识	标志
            var buffer = new byte[10];
            buffer[0] = (byte)TransferType.Count;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;

            //
            uint count = 0;
            this.SendAck(System.Threading.Timeout.Infinite, transferId, buffer, ack =>
            {
                //COUNT ACK:
                //1	            8	        1	    2		    4
                //Transfer Type	Transfer ID	Flags	Error Code	Count
                //传输标识	    传输标识	标志	命令标识	数量

                if (ack.data.Length != 16)
                {
                    throw new QueueServerException(QueueServerErrorCode.PacketInvalid, "Packet Invalid");
                }
                count = Adf.BaseDataConverter.ToUInt32(ack.data, 12);
            });

            return count;
        }

        /// <summary>
        /// 清空队列，已被pull但未被commit/rollback的现有消息同时清空，当客户端进行commit/rollback时会返回 队列不存在的 错误提示
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <exception cref="QueueServerException"></exception>
        /// <returns>return message count in queue</returns>
        public uint Clear()
        {
            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;

            //1	            8	        1
            //Transfer Type	Transfer ID	Flags
            //传输标识	    传输标识	标志
            var buffer = new byte[10];
            buffer[0] = (byte)TransferType.Clear;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;

            //
            uint count = 0;
            this.SendAck(System.Threading.Timeout.Infinite, transferId, buffer, ack =>
            {
                //CLEAR ACK:
                //1	            8	        1	    2		    4
                //Transfer Type	Transfer ID	Flags	Error Code	Count
                //传输标识	    传输标识	标志	命令标识	数量

                if (ack.data.Length != 16)
                {
                    throw new QueueServerException(QueueServerErrorCode.PacketInvalid, "Packet Invalid");
                }
                count = Adf.BaseDataConverter.ToUInt32(ack.data, 12);
            });

            return count;
        }

        private bool SendAck(int timeout, UInt64 transferId, byte[] buffer, Action<AckWaitHandle> action)
        {
            using (var ack = new AckWaitHandle())
            {
                if (this.isDisposed == true)
                    throw new ObjectDisposedException(typeof(QueueServerClient).Name);

                if (this.client.IsConnectioned == false)
                    throw new IOException("no connect on server");

                try
                {
                    lock (this.ackDictionary)
                    {
                        try
                        {
                            this.ackDictionary.Add(transferId, ack);
                        }
                        catch (ArgumentException)
                        {
                            throw new QueueServerException(QueueServerErrorCode.TransferIdConflict, "transfer id conflict, this situation is not normal, if often appear to destroy this instance and initial new client.");
                        }
                    }

                    this.client.Send(buffer);

                    if (ack.WaitOne(timeout) == false)
                    {
                        throw new TimeoutException();
                    }
                    if (ack.errorCode != QueueServerErrorCode.Normal)
                    {
                        if (ack.isClosed == true)
                        {
                            throw new IOException("connection is closed");
                        }

                        throw new QueueServerException(ack.errorCode, ack.errorMessage);
                    }
                    action(ack);
                }
                finally
                {
                    lock (this.ackDictionary)
                    {
                        this.ackDictionary.Remove(transferId);
                    }
                }

                return ack.errorCode == QueueServerErrorCode.Normal;
            }
        }

        private QueueServerMessage ParseAckMessage(byte[] ackdata)
        {
            //PULL ACK:
            //1	            8	        1	    2	        8
            //Transfer Type	Transfer ID	Flags	Error Code	Id
            //传输标识	    传输标识	标志	命令标识	队列
            //2	                2	    1-4096
            //Duplications  	Length	Message
            //复本	            消息长度	消息

            //ack.data;
            if (ackdata.Length < 24)
            {
                throw new QueueServerException(QueueServerErrorCode.PacketInvalid, "Packet Invalid");
            }

            var messageId = Adf.BaseDataConverter.ToUInt64(ackdata, 12);
            var duplications = Adf.BaseDataConverter.ToUInt16(ackdata, 20);
            var length = Adf.BaseDataConverter.ToUInt16(ackdata, 22);
            var data = new byte[length];
            if (length > 0)
            {
                Array.Copy(ackdata, 24, data, 0, length);
            }
            return new QueueServerMessage(messageId, duplications, data);
        }

        private void OnPullAck(ulong transferId, ulong messageId, QueueServerErrorCode errorCode, string errorMessage, ushort duplications, ushort length, byte[] data)
        {
            var action = this.PullAck;
            if (action != null)
            {
                action(this, new QueueServerMessageAckArgs(messageId, errorCode, errorMessage, transferId, duplications, data));
            }
        }

        //private void OnSelectAck(UInt64 transferId, UInt64 messageId, QueueServerErrorCode errorCode, string errorMessage)
        //{
        //    var action = this.SelectAck;
        //    if (action != null)
        //    {
        //        action(this, new QueueServerAckArgs(messageId, errorCode, errorMessage, transferId));
        //    }
        //}

        private void OnRPushAck(UInt64 transferId, UInt64 messageId, QueueServerErrorCode errorCode, string errorMessage)
        {
            var action = this.RPushAck;
            if (action != null)
            {
                action(this, new QueueServerAckArgs(messageId, errorCode, errorMessage, transferId));
            }
        }

        private void OnLPushAck(UInt64 transferId, UInt64 messageId, QueueServerErrorCode errorCode, string errorMessage)
        {
            var action = this.LPushAck;
            if (action != null)
            {
                action(this, new QueueServerAckArgs(messageId, errorCode, errorMessage, transferId));
            }
        }

        private void OnCommitAck(UInt64 transferId, UInt64 messageId, QueueServerErrorCode errorCode, string errorMessage)
        {
            var action = this.CommitAck;
            if (action != null)
            {
                action(this, new QueueServerAckArgs(messageId, errorCode, errorMessage, transferId));
            }
        }

        private void OnRollbackAck(UInt64 transferId, UInt64 messageId, QueueServerErrorCode errorCode, string errorMessage)
        {
            var action = this.RollbackAck;
            if (action != null)
            {
                action(this, new QueueServerAckArgs(messageId, errorCode, errorMessage, transferId));
            }
        }

        private void OnReceiveError(Exception exception)
        {
            var action = this.ReceiveError;
            if (action != null)
            {
                action(this, new QueueServerErrorEventArgs(exception));
            }
        }


        //int receiveErrorWait = 100;
        ///// <summary>
        ///// <para>获取或设置错误发生后进行下一次尝试的等待时间，单位：毫秒， 默认: 100</para>
        ///// <para>error wait X milliseconds to continue next</para>
        ///// </summary>
        //public int ReceiveErrorWait
        //{
        //    get { return this.receiveErrorWait; }
        //    set
        //    {
        //        if (value < 1)
        //            throw new ArgumentOutOfRangeException("value must than 0");
        //        this.receiveErrorWait = value;
        //    }
        //}

        QueueServerReceiveEvent receivaCallback = null;
        int receiveMaxThreadSize = 0;

        /// <summary>
        /// <para>连续接收一系列消息,完成则commit,失败则rollback，通过返回的client.disposed停止接收。 </para>
        /// <para>该方法会忽略所有异常，通过ReceiveError事件捕获内部发生的异常，IOException系统自行恢复，对于非IOException异常会导致接收中断，你应对其进行修复或记录处理。</para>
        /// <para>注意： 此client应保持独占性，除非说明均不应再应用于其它业务</para>
        /// </summary>
        /// <param name="callback">receive callback</param>
        /// <exception cref="System.ArgumentNullException">callback is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        /// <exception cref="System.InvalidOperationException">client Has Been Received. a client only allow start one receive</exception>
        public void Receive(QueueServerReceiveEvent callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if (this.receivaCallback != null)
            {
                throw new InvalidOperationException("client Has Been Received. a client only allow start one receive.");
            }

            this.PullAck += ReceivePullAck;
            this.NetworkError += ReceiveNetworkError;
            this.receivaCallback = callback;
            this.receiveMaxThreadSize = 0;

            try
            {
                if (this.client.IsConnectioned == false)
                {
                    this.Connect();
                }
                //
                this.PullAsync(0);
            }
            catch
            {
                this.PullAck -= ReceivePullAck;
                this.NetworkError -= ReceiveNetworkError;
                this.receivaCallback = null;
                throw;
            }
        }

        /// <summary>
        /// <para>使用多个线程连续接收一系列消息,完成则commit,失败则rollback，通过返回的client.disposed停止接收。 </para>
        /// <para>该方法会忽略所有异常，通过ReceiveError事件捕获内部发生的异常，IOException系统自行恢复，对于非IOException异常会导致接收中断，你应对其进行修复或记录处理。</para>
        /// <para>注意： 此client应保持独占性，除非说明均不应再应用于其它业务</para>
        /// </summary>
        /// <param name="callback">receive callback</param>
        /// <param name="maxThreadSize">max thread size,value must than 0</param>
        /// <exception cref="System.ArgumentNullException">callback is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        /// <exception cref="System.InvalidOperationException">client Has Been Received. a client only allow start one receive</exception>
        public void Receive(QueueServerReceiveEvent callback, int maxThreadSize)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if (this.receivaCallback != null)
            {
                throw new InvalidOperationException("client Has Been Received. a client only allow start one receive.");
            }

            if (maxThreadSize < 1)
            {
                throw new ArgumentOutOfRangeException("maxThreadSize", "max thread size,value must than 0");
            }

            this.PullAck += ReceivePullAck;
            this.NetworkError += ReceiveNetworkError;
            this.receivaCallback = callback;
            this.receiveMaxThreadSize = maxThreadSize;

            try
            {
                if (this.client.IsConnectioned == false)
                {
                    this.Connect();
                }
                //
                for (int i = 0; i < maxThreadSize; i++)
                {
                    this.PullAsync((UInt64)i);
                }
            }
            catch
            {
                this.PullAck -= ReceivePullAck;
                this.NetworkError -= ReceiveNetworkError;
                this.receivaCallback = null;
                throw;
            }
        }

        /// <summary>
        /// 使用配置初始化连续
        /// <para>使用配置的连接数连续接收一系列消息,完成则commit,失败则rollback，通过返回的client.disposed停止接收。 </para>
        /// <para>该方法会忽略所有异常，通过ReceiveError事件捕获内部发生的异常，IOException系统自行恢复，对于非IOException异常会导致接收中断，你应对其进行修复或记录处理。</para>
        /// <para>注意： 此返回的client应保持Pull独占性，除非说明均不应再应用于其它业务</para>
        /// </summary>
        /// <param name="configFile">配置文件名，需提供相对应的属性或配置项  commitTimeout, maxThreadSize, topic</param>
        /// <param name="callback">receive callback</param>
        /// <exception cref="System.ArgumentNullException">callback is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        /// <exception cref="System.InvalidOperationException">client Has Been Received. a client only allow start one receive</exception>
        public static QueueServerClient[] ReceiveFromConfiguration(string configFile, QueueServerReceiveEvent callback)
        {
            var config = Adf.Config.ServerConfig.GetConfiguration(configFile);
            var serverCount = config.Count;
            var servers = config.GetItems();
            var topic = config.GetAttr("topic");
            var commitTimeout = config.GetAttrAsUInt16("commitTimeout");
            int maxThreadSize = config.GetAttrAsInt32("maxThreadSize");
            var clients = new Adf.QueueServerClient[serverCount];
            for (int i = 0; i < serverCount; i++)
            {
                QueueServerClient client = null;
                try
                {
                    client = new QueueServerClient(servers[i].Ip, servers[i].Port, topic, commitTimeout);
                    if (maxThreadSize > 1)
                    {
                        client.Receive(callback, maxThreadSize);
                    }
                    else
                    {
                        client.Receive(callback);
                    }
                    //
                    clients[i] = client;
                }
                catch
                {
                    DisposeClients(clients);
                    throw;
                }
            }
            return clients;
        }

        /// <summary>
        /// 使用配置初始化连续
        /// <para>使用配置的连接数连续接收一系列消息,完成则commit,失败则rollback，通过返回的client.disposed停止接收。 </para>
        /// <para>该方法会忽略所有异常，通过ReceiveError事件捕获内部发生的异常，IOException系统自行恢复，对于非IOException异常会导致接收中断，你应对其进行修复或记录处理。</para>
        /// <para>注意： 此返回的client应保持Pull独占性，除非说明均不应再应用于其它业务</para>
        /// </summary>
        /// <param name="configFile">配置文件名，需提供相对应的属性或配置项  commitTimeout, maxThreadSize, topic</param>
        /// <param name="callback">receive callback</param>
        /// <param name="creatorCallback">create callback for client initalize</param>
        /// <exception cref="System.ArgumentNullException">callback is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        /// <exception cref="System.InvalidOperationException">client Has Been Received. a client only allow start one receive</exception>
        public static QueueServerClient[] ReceiveFromConfiguration(string configFile, QueueServerReceiveEvent callback, Action<QueueServerClient> creatorCallback)
        {
            var config = Adf.Config.ServerConfig.GetConfiguration(configFile);
            var serverCount = config.Count;
            var servers = config.GetItems();
            var topic = config.GetAttr("topic");
            var commitTimeout = config.GetAttrAsUInt16("commitTimeout");
            int maxThreadSize = config.GetAttrAsInt32("maxThreadSize");
            var clients = new Adf.QueueServerClient[serverCount];
            for (int i = 0; i < serverCount; i++)
            {
                QueueServerClient client = null;
                try
                {
                    client = new QueueServerClient(servers[i].Ip, servers[i].Port, topic, commitTimeout);
                    creatorCallback(client);
                    if (maxThreadSize > 1)
                    {
                        client.Receive(callback, maxThreadSize);
                    }
                    else
                    {
                        client.Receive(callback);
                    }
                    //
                    clients[i] = client;
                }
                catch
                {
                    DisposeClients(clients);
                    throw;
                }
            }
            return clients;
        }


        private static void DisposeClients(QueueServerClient[] clients)
        {
            for (int i = 0; i < clients.Length; i++)
            {
                if (clients[i] != null)
                {
                    clients[i].Dispose();
                }
            }
        }

        private static void ReceiveNetworkError(QueueServerClient client, QueueServerErrorEventArgs args)
        {
            client.OnReceiveError(args.Exception);
            //
            var thread = new Thread(us =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                    //
                    if (client.isDisposed == true)
                        break;

                    try
                    {
                        client.Connect();
                        //re-pull
                        if (client.receiveMaxThreadSize > 1)
                        {
                            for (int i = 0; i < client.receiveMaxThreadSize; i++)
                            {
                                client.PullAsync((UInt64)i);
                            }
                        }
                        else
                        {
                            client.PullAsync(0);
                        }
                        //
                        break;
                    }
                    catch (IOException)
                    {
                        //ignore
                    }
                    catch (TimeoutException)
                    {
                        //ignore
                    }
                    catch (Exception exception)
                    {
                        client.OnReceiveError(exception);
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private static void ReceivePullAck(QueueServerClient client, QueueServerMessageAckArgs args)
        {
            if (args.ErrorCode == QueueServerErrorCode.Normal)
            {
                var option = client.receivaCallback(client, args);
                try
                {
                    if (option == QueueServerReceiveOption.Commit)
                    {
                        client.CommitAsync(args.MessageId, args.TransferId);
                    }
                    else if (option == QueueServerReceiveOption.Rollback)
                    {
                        client.RollbackAsync(args.MessageId, args.TransferId);
                    }
                    else
                    {

                    }
                    client.PullAsync(args.TransferId);
                }
                catch (IOException)
                {
                    //ignore error
                }
                catch (Exception exception)
                {
                    client.OnReceiveError(exception);
                }
            }
            else
            {
                client.OnReceiveError(new QueueServerException(args.ErrorCode, args.ErrorMessage));
            }
        }

        /// <summary>
        /// 将当前主题订阅到另一个主题
        /// </summary>
        /// <param name="subscribeToTopic"></param>
        /// <exception cref="ArgumentNullException">topic is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <exception cref="QueueServerException"></exception>
        public void Subscribe(string subscribeToTopic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;

            //1	            8	        1	    1	    1-255
            //Transfer Type	Transfer ID	Flags	Length	Topic
            //传输标识	    传输标识	标志	长度	队列主题

            var topicBuffer = Encoding.UTF8.GetBytes(subscribeToTopic);
            var topicLength = topicBuffer.Length;

            var buffer = new byte[11 + topicLength];
            buffer[0] = (byte)TransferType.Subscribe;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;
            buffer[10] = (byte)topicLength;
            Array.Copy(topicBuffer, 0, buffer, 11, topicLength);

            var success = this.SendAck(this.millisecondsTimeout, transferId, buffer, ack =>
            {
            });

        }

        /// <summary>
        /// 取消一个主题对当前主题的订阅
        /// </summary>
        /// <param name="subscribeToTopic"></param>
        /// <exception cref="ArgumentNullException">topic is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <exception cref="QueueServerException"></exception>
        public void Unsubscribe(string subscribeToTopic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;

            //1	            8	        1	    1	    1-255
            //Transfer Type	Transfer ID	Flags	Length	Topic
            //传输标识	    传输标识	标志	长度	队列主题

            var topicBuffer = Encoding.UTF8.GetBytes(subscribeToTopic);
            var topicLength = topicBuffer.Length;

            var buffer = new byte[11 + topicLength];
            buffer[0] = (byte)TransferType.Unsubscribe;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;
            buffer[10] = (byte)topicLength;
            Array.Copy(topicBuffer, 0, buffer, 11, topicLength);

            var success = this.SendAck(this.millisecondsTimeout, transferId, buffer, ack =>
            {
            });
        }

        /// <summary>
        /// 获取当前主题订阅列表
        /// </summary>
        /// <exception cref="ArgumentNullException">topic is null</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">no connect on server or network disconnected</exception>
        /// <returns>topic list</returns>
        public string[] SubscribeList()
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            ulong transferId = (ulong)System.Threading.Interlocked.Increment(ref this.transferId);
            byte flag = 0;

            //1	            8	        1
            //Transfer Type	Transfer ID	Flags
            //传输标识	    传输标识	标志

            var buffer = new byte[10];
            buffer[0] = (byte)TransferType.SubscribeList;
            Adf.BaseDataConverter.ToBytes(transferId, buffer, 1);
            buffer[9] = flag;

            string[] topics = new string[0];
            var success = this.SendAck(this.millisecondsTimeout, transferId, buffer, ack =>
            {
                //SUBSCRIBE LIST ACK：
                //1	            8	        1	    2	        4
                //Transfer Type	Transfer ID	Flags	Error Code	Count
                //传输标识	传输标识	标志	错误编码	订阅数
                //1	        1-255		    1	    1-255
                //Length	To Topic	…	Length	To Topic
                //长度	    目标主题		长度	目标主题

                var ackLength = ack.data.Length;
                if (ackLength == 16)
                {
                    topics = new string[0];
                }
                else if (ackLength < 16)
                {
                    throw new QueueServerException(QueueServerErrorCode.PacketInvalid, "Packet Invalid");
                }
                else
                {
                    var position = 16;
                    var count = Adf.BaseDataConverter.ToInt32(ack.data, 12);
                    topics = new string[count];
                    for (int i = 0; i < count; i++)
                    {
                        byte length = ack.data[position];
                        position += 1;

                        if (length == 0)
                        {
                            throw new QueueServerException(QueueServerErrorCode.PacketInvalid, "Packet Invalid");
                        }
                        else if (ackLength < position + length)
                        {
                            throw new QueueServerException(QueueServerErrorCode.PacketInvalid, "Packet Invalid");
                        }
                        else
                        {
                            topics[i] = Encoding.UTF8.GetString(ack.data, position, length);
                            position += length;
                        }
                    }
                }

            });

            return topics;
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            if (this.isDisposed == true)
                throw new ObjectDisposedException(this.GetType().Name);

            this.isDisposed = true;

            if (this.client != null)
            {
                try { client.Close(); }
                catch { }
                try { client.Dispose(); }
                catch { }
            }

            if (this.ackDictionary != null)
            {
                lock (this.ackDictionary)
                {
                    foreach (var item in this.ackDictionary)
                    {
                        try
                        {
                            item.Value.Close();
                        }
                        catch { }
                    }
                }
            }

            this.RPushAck = null;
            this.PullAck = null;
            this.CommitAck = null;
            this.RollbackAck = null;
            this.NetworkError = null;
            this.ReceiveError = null;
            this.receivaCallback = null;
        }

        enum TransferType : byte
        {
            None = 0,
            Connect = 1,
            LPush = 2,
            RPush = 3,
            Pull = 4,
            Commit = 5,
            Rollback = 6,
            //Peek = 7,
            Count = 8,
            Clear = 9,
            Delete = 10,
            Subscribe = 11,
            Unsubscribe = 12,
            SubscribeList = 13,
            Select = 14
        }
        
        class AckWaitHandle : EventWaitHandle
        {
            public readonly object userState = null;
            public bool isClosed = false;
            public QueueServerErrorCode errorCode = QueueServerErrorCode.None;
            public string errorMessage = "";
            public byte[] data = null;

            public AckWaitHandle()
                : base(false, EventResetMode.ManualReset)
            {
            }

            public AckWaitHandle(object userState)
                : base(false, EventResetMode.ManualReset)
            {
                this.userState = userState;
            }

            public override string ToString()
            {
                if (this.errorCode == QueueServerErrorCode.Normal)
                { }
                else if (this.errorCode == QueueServerErrorCode.None)
                { }
                else if (this.errorMessage == "")
                {
                    return this.errorCode.ToString();
                }
                return this.errorMessage;
            }
        }

        /// <summary>
        /// for pool use
        /// </summary>
        public bool PoolAbandon
        {
            get;
            set;
        }
    }

    /// <summary>
    /// receive option
    /// </summary>
    public enum QueueServerReceiveOption
    {
        /// <summary>
        /// no nothing
        /// </summary>
        Nothing = 0,
        /// <summary>
        /// commit
        /// </summary>
        Commit = 1,
        /// <summary>
        /// rollback
        /// </summary>
        Rollback = 2,
    }

    /// <summary>
    /// queue server error code
    /// </summary>
    public enum QueueServerErrorCode : ushort
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// normal
        /// </summary>
        Normal = 1,
        /// <summary>
        /// topic exists
        /// </summary>
        TopicExists = 2,
        /// <summary>
        /// topic not exists
        /// </summary>
        TopicNotExists = 3,
        /// <summary>
        /// message exists
        /// </summary>
        MessageExists = 4,
        /// <summary>
        /// message not exits
        /// </summary>
        MessageNotExists = 5,
        /// <summary>
        /// transfer type invalid
        /// </summary>
        TransferTypeInvalid = 6,
        /// <summary>
        /// packet invalid
        /// </summary>
        PacketInvalid = 7,
        /// <summary>
        /// server error
        /// </summary>
        ServerError = 8,
        /// <summary>
        /// commit timeout invalid
        /// </summary>
        CommitTimeoutInvalid = 9,
        /// <summary>
        /// transfer id conflict
        /// </summary>
        TransferIdConflict = 10,
        ///// <summary>
        ///// message no allow commit
        ///// </summary>
        //NotAllowCommit = 11,
        /// <summary>
        /// client error
        /// </summary>
        ClientError = 12,
        /// <summary>
        /// no access permission
        /// </summary>
        NoPermission = 13,
        /// <summary>
        /// queue is expired or rebuiled
        /// </summary>
        QueueExpired = 14,
        /// <summary>
        /// connection limit exceeded
        /// </summary>
        ConnectionLimit = 15,
        /// <summary>
        /// Server unavailable
        /// </summary>
        ServerUnavailable = 16,
        /// <summary>
        /// command or function no support
        /// </summary>
        NotSupport = 17,
        /// <summary>
        /// function prohibit
        /// </summary>
        Prohibit = 18
    }

    /// <summary>
    /// queue server exception
    /// </summary>
    public class QueueServerException : Exception
    {
        /// <summary>
        /// get error code
        /// </summary>
        public QueueServerErrorCode ErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        public QueueServerException(QueueServerErrorCode errorCode, string errorMessage)
            : base(errorMessage + "(" + errorCode.ToString() + ")")
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="message"></param>
        public QueueServerException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// 队列服务响应结果
    /// </summary>
    public struct QueueServerAckArgs
    {
        QueueServerErrorCode errorCode;

        /// <summary>
        /// <para>normal is success, other is failure, failure reasons in error message </para>
        /// <para>normal 表示成功，忽略errorMessage, 其它均表示失败, 可通过errorMessage获取原因</para>
        /// </summary>
        public QueueServerErrorCode ErrorCode
        {
            get { return errorCode; }
        }

        string errorMessage;
        /// <summary>
        /// <para>获取成功时的消息标识</para>
        /// <para>get message id after success</para>
        /// </summary>
        public string ErrorMessage
        {
            get { return errorMessage; }
        }

        UInt64 messageId;
        /// <summary>
        /// <para>获取成功时的消息标识</para>
        /// <para>get message id after success</para>
        /// </summary>
        public UInt64 MessageId
        {
            get { return messageId; }
        }

        UInt64 transferId;
        /// <summary>
        /// <para>invoke method paramter transferId</para>
        /// <para>使用者调用请求方法时传入的值</para>
        /// </summary>
        public UInt64 TransferId
        {
            get { return transferId; }
        }

        internal QueueServerAckArgs(UInt64 messageId
            , QueueServerErrorCode errorCode
            , string errorMessage
            , UInt64 transferId)
        {
            this.messageId = messageId;
            this.errorCode = errorCode;
            this.errorMessage = errorMessage;
            this.transferId = transferId;
        }
    }

    /// <summary>
    /// 队列服务消息响应结果
    /// </summary>
    public struct QueueServerMessageAckArgs
    {
        QueueServerErrorCode errorCode;

        /// <summary>
        /// <para>normal is success, other is failure, failure reasons in error message </para>
        /// <para>normal 表示成功，忽略errorMessage, 其它均表示失败, 可通过errorMessage获取原因</para>
        /// </summary>
        public QueueServerErrorCode ErrorCode
        {
            get { return errorCode; }
        }

        string errorMessage;
        /// <summary>
        /// <para>获取成功时的消息标识</para>
        /// <para>get message id after success</para>
        /// </summary>
        public string ErrorMessage
        {
            get { return errorMessage; }
        }

        UInt64 messageId;
        /// <summary>
        /// <para>获取成功时的消息标识</para>
        /// <para>get message id after success</para>
        /// </summary>
        public UInt64 MessageId
        {
            get { return messageId; }
        }

        UInt64 transferId;
        /// <summary>
        /// <para>invoke method paramter transferId</para>
        /// <para>使用者调用请求方法时传入的值</para>
        /// </summary>
        public UInt64 TransferId
        {
            get { return transferId; }
        }

        UInt16 duplications;
        /// <summary>
        /// 获取该消息被获取的次数
        /// </summary>
        public UInt16 Duplications
        {
            get { return duplications; }
        }

        byte[] data;
        /// <summary>
        /// 获取消息数据
        /// </summary>
        public byte[] Data
        {
            get { return data; }
        }

        internal QueueServerMessageAckArgs(UInt64 messageId
            , QueueServerErrorCode errorCode
            , string errorMessage
            , UInt64 transferId
            , UInt16 duplications
            , byte[] data)
        {
            this.messageId = messageId;
            this.errorCode = errorCode;
            this.errorMessage = errorMessage;
            this.transferId = transferId;
            this.duplications = duplications;
            this.data = data;
        }

        /// <summary>
        /// 将数据转换成字符形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.data == null)
                return null;

            return Encoding.UTF8.GetString(this.data);
        }
    }

    /// <summary>
    /// 队列服务消息体
    /// </summary>
    public class QueueServerMessage
    {
        UInt64 messageId = 0;
        /// <summary>
        /// 获取消息标识
        /// </summary>
        public UInt64 MessageId
        {
            get { return messageId; }
        }

        UInt16 duplications = 0;
        /// <summary>
        /// 获取该消息被获取的次数
        /// </summary>
        public UInt16 Duplications
        {
            get { return duplications; }
        }

        byte[] data = null;
        /// <summary>
        /// 获取消息数据
        /// </summary>
        public byte[] Data
        {
            get { return data; }
        }

        internal QueueServerMessage(UInt64 messageId
            , UInt16 duplications
            , byte[] data)
        {
            this.messageId = messageId;
            this.duplications = duplications;
            this.data = data;
        }

        /// <summary>
        /// 将数据转换成字符形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.data == null)
                return null;

            return Encoding.UTF8.GetString(this.data);
        }
    }


    /// <summary>
    /// 错误事件参数
    /// </summary>
    public class QueueServerErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 获取引发事件的异常
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }

        ///// <summary>
        ///// 获取或设置对该错误的描述（如果有）
        ///// </summary>
        //public string Message
        //{
        //    get;
        //    set;
        //}

        ///// <summary>
        ///// 获取或设置该错误的错误码（如果有）
        ///// </summary>
        //public int ErrorCode
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="exception"></param>
        public QueueServerErrorEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
    }
}