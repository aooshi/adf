//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Collections.Specialized;
//using System.Threading;

//namespace Adf
//{
//    /// <summary>
//    /// Http Server WebSocketContext Action Handler
//    /// </summary>
//    public class HttpServerWebSocketContextAction : WebSocketAction, IDisposable
//    {
//        HttpServerWebSocketContext webSocketContext;
//        /// <summary>
//        /// get HttpServer WebSocketContext
//        /// </summary>
//        public HttpServerWebSocketContext WebSocketContext
//        {
//            get { return this.webSocketContext; }
//        }

//        /// <summary>
//        /// initialize new instance
//        /// </summary>
//        /// <param name="webSocketContext"></param>
//        public HttpServerWebSocketContextAction(HttpServerWebSocketContext webSocketContext)
//            : base()
//        {
//            this.webSocketContext = webSocketContext;
//            webSocketContext.DataReceived += this.WebSocketContextDataReceived;
//        }

//        private void WebSocketContextDataReceived(object sender, HttpServerWebSocketMessageEventArgs e)
//        {
//            if (e.Opcode == WebSocketOpcode.Binary)
//            {
//                base.Parser(e.Buffer);
//            }
//        }

//        /// <summary>
//        /// send data
//        /// </summary>
//        /// <param name="data"></param>
//        protected override void Send(byte[] data)
//        {
//            this.webSocketContext.Send(data);
//        }

//        /// <summary>
//        /// clean resources
//        /// </summary>
//        public override void Dispose()
//        {
//            this.webSocketContext.DataReceived -= this.WebSocketContextDataReceived;

//            base.Dispose();
//        }
//    }

//    /// <summary>
//    /// WebSocketClient Action Handler
//    /// </summary>
//    public class WebSocketClientAction : WebSocketAction, IDisposable
//    {
//        WebSocketClient webSocketClient;
//        /// <summary>
//        /// get webSocketClient
//        /// </summary>
//        public WebSocketClient WebSocketClient
//        {
//            get { return this.webSocketClient; }
//        }

//        /// <summary>
//        /// initialize new instance
//        /// </summary>
//        /// <param name="webSocketClient"></param>
//        public WebSocketClientAction(WebSocketClient webSocketClient)
//            : base()
//        {
//            this.webSocketClient = webSocketClient;
//            this.webSocketClient.Message += this.WebSocketClientMessage;
//        }

//        private void WebSocketClientMessage(object sender, WebSocketMessageEventArgs e)
//        {
//            if (e.Opcode == WebSocketOpcode.Binary)
//            {
//                base.Parser(e.Buffer);
//            }
//        }

//        /// <summary>
//        /// send data
//        /// </summary>
//        /// <param name="data"></param>
//        protected override void Send(byte[] data)
//        {
//            this.webSocketClient.Send(data);
//        }

//        /// <summary>
//        /// clean resources
//        /// </summary>
//        public override void Dispose()
//        {
//            this.webSocketClient.Message -= this.WebSocketClientMessage;

//            base.Dispose();
//        }
//    }

//    /// <summary>
//    /// WebSocket Action
//    /// </summary>
//    public abstract class WebSocketAction : IDisposable
//    {
//        /// <summary>
//        /// general request action msg type
//        /// </summary>
//        public static byte MSG_TYPE_REQ = 1;
//        /// <summary>
//        /// general ack requect action msg type
//        /// </summary>
//        public static byte MSG_TYPE_ACK = 2;

//        Dictionary<ushort, InflightState> infightStateDictionary = new Dictionary<ushort, InflightState>();
//        ushort actionIdStore = 0;

//        /// <summary>
//        /// new message event
//        /// </summary>
//        public event EventHandler<WebSocketActionMessageEventArgs> NewMessage;

//        /// <summary>
//        /// 初始化新实例
//        /// </summary>
//        public WebSocketAction()
//        {
//        }
        
//        /// <summary>
//        /// generate new action id
//        /// </summary>
//        /// <returns></returns>
//        private ushort NewActionId()
//        {
//            lock (this.infightStateDictionary)
//            {
//                ushort actionId = 0;
//                for (ushort i = 1; i < short.MaxValue; i++)
//                {
//                    actionId = this.actionIdStore++;
//                    if (this.infightStateDictionary.ContainsKey(actionId) == false)
//                    {
//                        this.infightStateDictionary.Add(actionId, null);
//                        return actionId;
//                    }
//                }
//            }

//            throw new System.InvalidOperationException("busy or in-flight pool full, max allow short.MaxValue action in a same connection session");
//        }

//        /// <summary>
//        /// 同步执行一个Send与Ack响应
//        /// </summary>
//        /// <param name="topic"></param>
//        /// <param name="data"></param>
//        /// <param name="millisecondsTimeout"></param>
//        /// <exception cref="InvalidOperationException">busy or in-flight pool full, only allow 65536 action in a same connection session</exception>
//        /// <exception cref="ArgumentNullException">data</exception>
//        /// <exception cref="ArgumentNullException">topic</exception>
//        /// <exception cref="ArgumentOutOfRangeException">millisecondsTimeout not lass than one</exception>
//        /// <exception cref="TimeoutException"></exception>
//        /// <returns></returns>
//        public byte[] SendReceive(string topic, byte[] data, int millisecondsTimeout)
//        {
//            return this.SendReceive(MSG_TYPE_REQ, topic, data, millisecondsTimeout);
//        }

//        /// <summary>
//        /// 同步执行一个Send与Ack响应
//        /// </summary>
//        /// <param name="msgType"></param>
//        /// <param name="topic"></param>
//        /// <param name="data"></param>
//        /// <param name="millisecondsTimeout"></param>
//        /// <exception cref="InvalidOperationException">busy or in-flight pool full, only allow 65536 action in a same connection session</exception>
//        /// <exception cref="ArgumentNullException">data</exception>
//        /// <exception cref="ArgumentNullException">topic</exception>
//        /// <exception cref="ArgumentOutOfRangeException">millisecondsTimeout not lass than one</exception>
//        /// <exception cref="TimeoutException"></exception>
//        /// <returns></returns>
//        public byte[] SendReceive(byte msgType, string topic, byte[] data, int millisecondsTimeout)
//        {
//            if (data == null)
//                throw new ArgumentNullException("data");

//            if (topic == null)
//                throw new ArgumentNullException("topic");

//            if (millisecondsTimeout < 1)
//                throw new ArgumentOutOfRangeException("millisecondsTimeout not lass than one");

//            ushort actionId = this.NewActionId();
//            byte[] buffer = this.DataPack(actionId, msgType, topic, data);
//            var state = new InflightState();
//            var stateDictionary = this.infightStateDictionary;
//            //
//            stateDictionary[actionId] = state;
//            //
//            bool isAck = false;
//            try
//            {
//                this.Send(buffer);
//                isAck = state.WaitHandle.WaitOne(millisecondsTimeout);
//            }
//            finally
//            {
//                lock (stateDictionary)
//                {
//                    stateDictionary.Remove(actionId);
//                }

//                state.WaitHandle.Close();
//            }

//            if (isAck == false)
//            {
//                throw new TimeoutException();
//            }

//            return state.AckData;
//        }

//        /// <summary>
//        /// 发送一个<see cref="MSG_TYPE_REQ"/>消息
//        /// </summary>
//        /// <param name="topic"></param>
//        /// <param name="data"></param>
//        /// <returns></returns>
//        public void Send(string topic, byte[] data)
//        {
//            this.Send(MSG_TYPE_REQ, topic, data);
//        }

//        /// <summary>
//        /// 开始异步发送一个消息
//        /// </summary>
//        /// <param name="topic"></param>
//        /// <param name="data"></param>
//        /// <param name="msgType"></param>
//        /// <returns></returns>
//        public void Send(byte msgType, string topic, byte[] data)
//        {
//            if (data == null)
//                throw new ArgumentNullException("data");

//            if (topic == null)
//                throw new ArgumentNullException("topic");

//            ushort actionId = this.actionIdStore++;
//            byte[] buffer = this.DataPack(actionId, msgType, topic, data);
//            this.Send(buffer);
//        }

//        /// <summary>
//        /// callback async send
//        /// </summary>
//        /// <param name="data"></param>
//        protected void Parser(byte[] data)
//        {
//            //unpack
//            ushort actionId;
//            string topic;
//            byte[] origin;
//            byte msgType;

//            if (this.DataUnpack(data, out msgType, out actionId, out topic, out origin))
//            {
//                if (msgType == MSG_TYPE_ACK)
//                {
//                    InflightState state = null;
//                    if (this.infightStateDictionary.TryGetValue(actionId, out state))
//                    {
//                        state.AckData = origin;
//                        state.IsCompleted = true;
//                        try
//                        {
//                            state.WaitHandle.Set();
//                        }
//                        catch { }
//                    }
//                    else
//                    {
//                        this.OnMessage(actionId, topic, msgType, origin);
//                    }
//                }
//                else
//                {
//                    this.OnMessage(actionId, topic, msgType, origin);
//                }
//            }
//        }

//        private void OnMessage(ushort actionId, string topic, byte msgType, byte[] data)
//        {
//            if (this.NewMessage != null)
//            {
//                this.NewMessage(this, new WebSocketActionMessageEventArgs(msgType, topic, actionId, data));
//            }
//        }

//        /// <summary>
//        /// send data
//        /// </summary>
//        /// <param name="data"></param>
//        protected abstract void Send(byte[] data);

//        /// <summary>
//        /// send ack
//        /// </summary>
//        /// <param name="actionId"></param>
//        /// <param name="topic"></param>
//        /// <param name="data"></param>
//        public void SendAck(ushort actionId, string topic, byte[] data)
//        {
//            var buffer = this.DataPack(actionId, MSG_TYPE_ACK, topic, data);
//            this.Send(buffer);
//        }

//        /// <summary>
//        /// data pack
//        /// </summary>
//        /// <param name="actionId"></param>
//        /// <param name="msgType"></param>
//        /// <param name="topic"></param>
//        /// <param name="data"></param>
//        /// <returns></returns>
//        protected virtual byte[] DataPack(ushort actionId, byte msgType, string topic, byte[] data)
//        {
//            var topicBytes = Encoding.UTF8.GetBytes(topic);
//            if (topicBytes.Length > ushort.MaxValue)
//                throw new ArgumentOutOfRangeException("topic", "topic max allow length " + ushort.MaxValue + " byte");

//            //1 msg type + 4 Send ID + 2 Topic Length + 4 Data Length + Topic Data + Data
//            var buffer = new byte[11 + topicBytes.Length + data.Length];

//            buffer[0] = msgType;
//            var offset = 1;

//            Adf.BaseDataConverter.ToBytes(actionId, buffer, offset);
//            offset += 2;

//            Adf.BaseDataConverter.ToBytes((ushort)topicBytes.Length, buffer, offset);
//            offset += 2;

//            Adf.BaseDataConverter.ToBytes(data.Length, buffer, offset);
//            offset += 4;

//            Array.Copy(topicBytes, 0, buffer, offset, topicBytes.Length);
//            offset += topicBytes.Length;

//            Array.Copy(data, 0, buffer, offset, data.Length);
//            //offset += data.Length;

//            return buffer;
//        }

//        /// <summary>
//        /// data uppack
//        /// </summary>
//        /// <param name="data"></param>
//        /// <param name="msgType"></param>
//        /// <param name="actionId"></param>
//        /// <param name="topic"></param>
//        /// <param name="origin"></param>
//        /// <returns></returns>
//        protected virtual bool DataUnpack(byte[] data, out byte msgType, out ushort actionId, out string topic, out byte[] origin)
//        {
//            //1 msg type + 2 Send ID + 2 Topic Length + 4 Data Length + Topic Data + Data
//            if (data != null && data.Length > 8)
//            {
//                var offset = 1;
//                msgType = data[0];

//                actionId = Adf.BaseDataConverter.ToUInt16(data, offset);
//                offset += 2;

//                var topicLength = Adf.BaseDataConverter.ToUInt16(data, offset);
//                offset += 2;

//                var originLength = Adf.BaseDataConverter.ToInt32(data, offset);
//                offset += 4;

//                if (topicLength > 0)
//                    topic = Encoding.UTF8.GetString(data, offset, topicLength);
//                else
//                    topic = "";

//                origin = new byte[originLength];
//                if (originLength > 0)
//                {
//                    Array.Copy(data, offset, origin, 0, originLength);
//                }

//                return true;
//            }
//            else
//            {
//                msgType = 0;
//                actionId = 0;
//                topic = null;
//                origin = null;

//                return false;
//            }
//        }

//        /// <summary>
//        /// 资源释放
//        /// </summary>
//        public virtual void Dispose()
//        {
//            var dictionary = this.infightStateDictionary;
//            lock (dictionary)
//            {
//                foreach (var state in dictionary.Values)
//                {
//                    state.WaitHandle.Close();
//                }
//            }

//            this.NewMessage = null;
//        }
        
//        class InflightState
//        {
//            public ManualResetEvent WaitHandle = new ManualResetEvent(false);
//            public byte[] AckData = null;
//            public bool IsCompleted = false;
//        }
//    }
    
//    /// <summary>
//    /// WebSocketAction message event args
//    /// </summary>
//    public class WebSocketActionMessageEventArgs : EventArgs
//    {
//        private ushort actionId;

//        /// <summary>
//        /// get action id
//        /// </summary>
//        public ushort ActionId
//        {
//            get { return actionId; }
//        }


//        private byte msgType;

//        /// <summary>
//        /// get msg type
//        /// </summary>
//        public byte MsgType
//        {
//            get { return this.msgType; }
//        }

//        private byte[] data;
//        /// <summary>
//        /// get action data
//        /// </summary>
//        public byte[] Data
//        {
//            get { return this.data; }
//        }

//        string topic;
//        /// <summary>
//        /// get topic
//        /// </summary>
//        public string Topic
//        {
//            get { return this.topic; }
//        }

//        /// <summary>
//        /// initialize new instance
//        /// </summary>
//        /// <param name="msgType"></param>
//        /// <param name="actionId"></param>
//        /// <param name="data"></param>
//        /// <param name="topic"></param>
//        public WebSocketActionMessageEventArgs(byte msgType, string topic, ushort actionId, byte[] data)
//        {
//            this.actionId = actionId;
//            this.msgType = msgType;
//            this.data = data;
//            this.topic = topic;
//        }
//    }
//}