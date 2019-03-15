using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Adf
{
    /// <summary>
    /// WebSocket 客户端
    /// </summary>
    public class WebSocketClient : IDisposable
    {
        bool disposed = false;
        Socket socket;
        NetworkStream stream;
        Timer timer;
        object sendLockObject = new object();
        const String MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        const int VERSION = 13;
        const int RECEIVE_SIZE = 512;
        const int PONG_TIMEOUT = 5; //second
        byte[] headBuffer = new byte[2];
        int headRead = 0;
        readonly int headLength = 2;

        string handshakeSecurityHash09;

        /// <summary>
        /// 连接成功事件
        /// </summary>
        public event EventHandler Connectioned;
        /// <summary>
        /// 连接断开/关闭事件
        /// </summary>
        public event EventHandler<WebSocketCloseEventArgs> Closed;
        /// <summary>
        /// 错误事件
        /// </summary>
        public event EventHandler<WebSocketErrorEventArgs> Error;
        /// <summary>
        /// 消息事件
        /// </summary>
        public event EventHandler<WebSocketMessageEventArgs> Message;
        /// <summary>
        /// 消息发送成功事件
        /// </summary>
        public event EventHandler<WebSocketSendEventArgs> SendCompleted;


        string host;
        /// <summary>
        /// 主机
        /// </summary>
        public string Host
        {
            get { return this.host; }
        }

        int port;
        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get { return this.port; }
        }

        int pingInterval = 0;
        /// <summary>
        /// 获取或设置PING时间间隔,单位：秒，默认60
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">value must than or equal zero.</exception>
        public int PingInterval
        {
            get { return this.pingInterval; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "value must than or equal zero.");

                if (this.pingInterval != value)
                {
                    if (value == 0)
                    {
                        if (this.timer != null)
                        {
                            this.timer.Dispose();
                            this.timer = null;
                        }
                    }
                    else
                    {
                        var millseconds = value * 1000;
                        if (this.timer == null)
                        {
                            this.timer = new System.Threading.Timer(this.TimerCallback, null, millseconds, millseconds);
                        }
                        else
                        {
                            this.timer.Change(millseconds, millseconds);
                        }
                    }
                    this.pingInterval = value;
                }
            }
        }

        Encoding encoding = null;
        /// <summary>
        /// 获取或设置字符解释编码,默认UTF8
        /// </summary>
        public Encoding Encoding
        {
            get { return this.encoding; }
            set { this.encoding = value; }
        }
        
        IPEndPoint localEndPoint = null;
        /// <summary>
        /// 获取或设置应用连接的本地接口
        /// </summary>
        /// <exception cref="System.ArgumentNullException">value is null</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public IPEndPoint LocalEndPoint
        {
            get
            {
                if (this.localEndPoint == null && this.socket != null)
                {
                    return (IPEndPoint)this.socket.LocalEndPoint;
                }
                return this.localEndPoint;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.localEndPoint = value;
            }
        }

        bool isConnectioned = false;
        /// <summary>
        /// 获取是否已连接至主机
        /// </summary>
        public bool IsConnectioned
        {
            get { return this.isConnectioned; }
        }

        string path;
        /// <summary>
        /// 获取当前连接请求路径
        /// </summary>
        public string Path
        {
            get { return this.path; }
        }

        Dictionary<string, string> requestHeader;
        /// <summary>
        /// 获取请求头
        /// </summary>
        public Dictionary<string, string> RequestHeader
        {
            get { return this.requestHeader; }
        }

        Dictionary<string, string> responseHeader;
        /// <summary>
        /// 获取响应头
        /// </summary>
        public Dictionary<string, string> ResponseHeader
        {
            get { return this.responseHeader; }
        }

        /// <summary>
        /// 用户自定义状态值
        /// </summary>
        public object UserState
        {
            get;
            set;
        }

        bool autoConnect = false;
        /// <summary>
        /// 获取或设置是否进行自动连接，默认false, 设置为true时随ping检查，若未连接则将自动连接，若禁用PingInterval则该设置无效。若调用close则该值将置为false
        /// </summary>
        public bool AutoConnect
        {
            get { return this.autoConnect; }
            set
            {
                this.autoConnect = value;

                if (value == true)
                {
                    var timer = this.timer;
                    var keeplive = this.pingInterval;
                    if (timer != null && keeplive > 0)
                    {
                        try
                        {
                            timer.Change(5000, keeplive * 1000);
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// 以指定主机初始化实例
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public WebSocketClient(string host, int port)
            : this(host, port, "/", 60)
        {
        }
        /// <summary>
        /// 以指定主机初始化实例
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="path"></param>
        public WebSocketClient(string host, int port, string path)
            : this(host, port, path, 60)
        {
        }
        /// <summary>
        /// 以指定主机初始化实例
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="path"></param>
        /// <param name="pingInterval">自动ping间隔，若设备为0则不进行ping检查</param>
        public WebSocketClient(string host, int port, string path, int pingInterval)
        {
            this.host = host;
            this.port = port;
            this.path = path;
            this.pingInterval = pingInterval;

            if (pingInterval < 0)
                throw new ArgumentOutOfRangeException("pingInterval", "pingInterval must than or equal zero.");

            //local
            this.encoding = Encoding.UTF8;

            //handshaking
            var secWebSocketKey = RandomHelper.LetterAndNumber(16);
            this.handshakeSecurityHash09 = WebSocketHandshake.HandshakeSecurityHash09(secWebSocketKey);

            //init request header
            this.requestHeader = new Dictionary<string, string>(5, StringComparer.OrdinalIgnoreCase);
            this.requestHeader.Add("Host", host);
            this.requestHeader.Add("Connection", "Upgrade");
            this.requestHeader.Add("Upgrade", "websocket");
            this.requestHeader.Add("Sec-WebSocket-Key", secWebSocketKey);
            this.requestHeader.Add("Sec-WebSocket-Version", "" + WebSocketClient.VERSION);
            //
            this.responseHeader = new Dictionary<string, string>(5);
            //
            if (pingInterval > 0)
            {
                var millseconds = pingInterval * 1000;
                this.timer = new System.Threading.Timer(this.TimerCallback, null, millseconds, millseconds);
            }
        }

        private void TimerCallback(object state)
        {
            if (this.disposed == true)
            {
            }
            else if (this.isConnectioned == true)
            {
                try
                {
                    this.Ping();
                }
                catch (ObjectDisposedException) { }
                catch (Exception exception)
                {
                    this.OnError(exception);
                    this.Close(WebSocketCloseReason.IOError);
                }
            }
            else if (this.autoConnect == true)
            {
                try
                {
                    this.Connection();
                }
                catch (Exception exception)
                {
                    this.OnError(exception);
                }
            }
        }

        /// <summary>
        /// 打开连接
        /// </summary>
        /// <exception cref="IOException">connection failure</exception>
        /// <exception cref="WebException">web handshake exception</exception>
        /// <returns></returns>
        public void Connection()
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                var localEP = this.localEndPoint;
                if (localEP != null)
                {
                    this.socket.Bind(localEP);
                }
                this.socket.Connect(this.host, this.port);
            }
            catch (Exception exception)
            {
                throw new IOException(exception.Message, exception);
            }
            this.stream = new NetworkStream(this.socket, false);

            //header
            var builder = new StringBuilder(128);
            builder.AppendFormat("GET {0} HTTP/1.1", this.Path);
            builder.AppendLine();
            foreach (var item in this.RequestHeader)
            {
                builder.AppendFormat("{0}: {1}", item.Key, item.Value);
                builder.AppendLine();
            }
            //end
            builder.AppendLine();

            //send
            var buffer = this.Encoding.GetBytes(builder.ToString());
            this.stream.Write(buffer, 0, buffer.Length);

            //receive header
            string line;
            string[] harr;
            //HTTP/1.1 101 Switching Protocols
            line = StreamHelper.ReadLine(this.stream, Encoding.ASCII);
            if (!line.StartsWith("HTTP/1.1 101"))
            {
                throw new WebException("Handshake failure, " + line);
            }
            while ((line = StreamHelper.ReadLine(this.stream, Encoding.ASCII)) != null && !line.Equals(string.Empty))
            {
                harr = line.Split(':');
                this.ResponseHeader[harr[0]] = harr[1].Trim();
            }


            //1、如果从服务器接收到的状态码不是101，按HTTP【RFC2616】程序处理响应。在特殊情况下，如果客户端接收到401状态码，可能执行认证；服务器可能用3xx状态码重定向客户端（但不要求客户端遵循他们），等等。否则按下面处理。

            //2、如果响应缺失Upgrade头域或Upgrade头域的值没有包含大小写不敏感的ASCII 值"websocket"，客户端必须使WebSocket连接失败。

            //3、如果响应缺失Connection头域或其值不包含大小写不敏感的ASCII值"Upgrade"，客户端必须使WebSocket连接失败。

            //4、如果响应缺失Sec-WebSocket-Accept头域或其值不包含 |Sec-WebSocket-Key |（作为字符串，非base64解码的）+ "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"的base64编码 SHA-1值，客户端必须使WebSocket连接失败。

            //5、如果响应包含Sec-WebSocket-Extensions头域，且其值指示使用的扩展不出现在客户端发送的握手（服务器指示的扩展不是客户端要求的），客户端必须使WebSocket连接失败。（解析此头域来决定哪个扩展是要求的在第9.1节描述。）

            //6、如果响应包含Sec-WebSocket-Protocol头域，且这个头域指示使用的子协议不包含在客户端的握手（服务器指示的子协议不是客户端要求的），客户端必须使WebSocket连接失败。


            //check， 此客户端仅验证 Sec-WebSocket-Accept,其它均不处理
            string sec_WebSocket_Accept;
            if (!this.ResponseHeader.TryGetValue("Sec-WebSocket-Accept", out sec_WebSocket_Accept))
            {
                throw new WebException("No Response Sec-WebSocket-Accept");
            }
            if (!this.handshakeSecurityHash09.Equals(sec_WebSocket_Accept))
            {
                throw new WebException("Handshake failure for Sec-WebSocket-Accept");
            }

            //receive
            this.Read();

            //ping start
            this.isConnectioned = true;

            //trigger event
            this.OnConnectioned();
        }

        /// <summary>
        /// 发送PING
        /// </summary>
        public void Ping()
        {
            this.Send(new byte[0], WebSocketOpcode.Ping);
        }

        /// <summary>
        /// 消息发送
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        /// <exception cref="IOException">network is closed</exception>
        public void Send(string data)
        {
            var buffer = this.encoding.GetBytes(data);
            this.Send(buffer, WebSocketOpcode.Text);
        }

        /// <summary>
        /// 异步发送一个消息
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        /// <exception cref="IOException">network is closed</exception>
        /// <param name="userState"></param>
        public void SendAsync(string data, object userState)
        {
            var buffer = this.encoding.GetBytes(data);
            this.SendAsync(buffer, WebSocketOpcode.Text, userState);
        }

        /// <summary>
        /// 发送字节数组
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        /// <exception cref="IOException">network is closed</exception>
        public void Send(byte[] data)
        {
            this.Send(data, WebSocketOpcode.Binary);
        }

        /// <summary>
        /// 异步发送字节数组
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        /// <exception cref="IOException">network is closed</exception>
        /// <param name="userState"></param>
        public void SendAsync(byte[] data, object userState)
        {
            this.SendAsync(data, WebSocketOpcode.Binary, userState);
        }

        /// <summary>
        /// 发送字节数组并指定内容类型
        /// </summary>
        /// <param name="data"></param>
        /// <param name="opcode"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        /// <exception cref="IOException">network is closed</exception>
        public void Send(byte[] data, WebSocketOpcode opcode)
        {
            //协议要求，客户端向服务端发送数据是mask必需为true

            if (!this.isConnectioned)
                throw new IOException("WebSocket Closed");

            if (data == null)
                throw new ArgumentNullException("data");

            var df = new WebSocketDataFrame(data, true, opcode);
            var buffer = df.GetFrameBytes();
            try
            {
                lock (this.sendLockObject)
                {
                    this.stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception exception)
            {
                this.OnError(exception);
                this.Close(WebSocketCloseReason.IOError);
                throw;
            }
        }

        /// <summary>
        /// 异步发送字节数组并指定内容类型
        /// </summary>
        /// <param name="data"></param>
        /// <param name="opcode"></param>
        /// <param name="userState"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        /// <exception cref="IOException">network is closed</exception>
        public void SendAsync(byte[] data, WebSocketOpcode opcode, object userState)
        {
            //协议要求，客户端向服务端发送数据是mask必需为true

            if (!this.isConnectioned)
                throw new IOException("WebSocket Closed");

            if (data == null)
                throw new ArgumentNullException("data");

            var df = new WebSocketDataFrame(data, true, opcode);
            var buffer = df.GetFrameBytes();
            try
            {
                lock (this.sendLockObject)
                {
                    this.stream.BeginWrite(buffer, 0, buffer.Length, this.SendCallback, userState);
                }
            }
            catch (Exception exception)
            {
                this.OnError(exception);
                this.Close(WebSocketCloseReason.IOError);
                throw;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            var action = this.SendCompleted;
            if (action != null)
            {
                WebSocketSendEventArgs args;
                try
                {
                    this.stream.EndWrite(ar);
                    args = new WebSocketSendEventArgs(ar.AsyncState);
                }
                catch (Exception exception)
                {
                    args = new WebSocketSendEventArgs(exception, ar.AsyncState);
                }
                action(this, args);
            }
            else
            {
                try
                {
                    this.stream.EndWrite(ar);
                }
                catch (Exception)
                {
                }
            }
        }

        private void Read()
        {
            this.headRead = 0;
            this.stream.BeginRead(this.headBuffer, 0, 2, this.ReadCallback, null);
        }

        /// <summary>
        /// 异步接收回调
        /// </summary>
        /// <param name="ar"></param>
        private void ReadCallback(IAsyncResult ar)
        {
            //read first byte
            var size = 0;
            try
            {
                size = this.stream.EndRead(ar);
                if (size == 0)
                {
                    //this.OnError(new IOException("stream is closed"));
                    this.Close(WebSocketCloseReason.Disconnected);
                    return;
                }

                this.headRead += size;
                if (this.headRead == this.headLength)
                {
                    this.ParseMessage();
                }
                else
                {
                    //read seconds byte
                    this.stream.BeginRead(this.headBuffer, this.headRead, this.headLength - this.headRead, this.ReadCallback, null);
                }
            }
            catch (ObjectDisposedException exception)
            {
                this.OnError(exception);
                this.Close(WebSocketCloseReason.Error);
            }
            catch (Exception exception)
            {
                this.OnError(exception);
                this.Close(WebSocketCloseReason.IOError);
            }
        }

        private void ParseMessage()
        {
            //receive data frame
            WebSocketDataFrame frame = new WebSocketDataFrame(this.stream, this.headBuffer);
            //trigger message event
            this.OnMessage(frame.Content, frame.Opcode);
            //new read
            this.Read();
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            this.disposed = true;

            this.Close(WebSocketCloseReason.Close);

            var timer = this.timer;
            if (timer != null)
            {
                timer.Dispose();
            }

            //local
            this.encoding = null;

            //handshaking
            this.handshakeSecurityHash09 = null;

            //init request header
            this.requestHeader = null;
            //
            this.responseHeader = null;
            //event
            this.Closed = null;
            this.Connectioned = null;
            this.Error = null;
            this.Message = null;
            this.SendCompleted = null;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            this.Close(WebSocketCloseReason.Close);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        protected void Close(WebSocketCloseReason reason)
        {
            lock (this.sendLockObject)
            {
                if (this.isConnectioned == true)
                {
                    this.isConnectioned = false;
                    if (reason == WebSocketCloseReason.Close)
                    {
                        this.autoConnect = false;
                    }
                    this.stream.Close();
                    this.socket.Close();
                    //
                    this.OnClosed(reason);
                }
            }
        }

        /// <summary>
        /// 引发异常事件
        /// </summary>
        /// <param name="exception"></param>
        protected void OnError(Exception exception)
        {
            if (this.Error != null)
            {
                this.Error(this, new WebSocketErrorEventArgs(exception));
            }
        }

        /// <summary>
        /// 引发消息事件
        /// </summary>
        /// <param name="data"></param>
        /// <param name="opcode"></param>
        protected void OnMessage(byte[] data, WebSocketOpcode opcode)
        {
            if (this.Message != null)
            {
                var args = new WebSocketMessageEventArgs(opcode);
                args.Buffer = data;
                if (opcode == WebSocketOpcode.Text)
                {
                    args.Message = this.encoding.GetString(data);
                }
                this.Message(this, args);
            }
        }

        /// <summary>
        /// 引发连接完成事件
        /// </summary>
        protected void OnConnectioned()
        {
            if (this.Connectioned != null)
            {
                this.Connectioned(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 引发关闭完成事件
        /// </summary>
        /// <param name="reason"></param>
        protected void OnClosed(WebSocketCloseReason reason)
        {
            if (this.Closed != null)
            {
                this.Closed(this, new WebSocketCloseEventArgs(reason));
            }
        }
    }
}