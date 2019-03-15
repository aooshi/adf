using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using System.Text.RegularExpressions;

namespace Adf
{
    /// <summary>
    /// Http Server Authorization Callback
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public delegate bool HttpServerAuthorizationCallback(HttpServerContextBase context);
    /// <summary>
    /// WebSocket Disconnection Callback
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public delegate void HttpServerWebSocketCallback(HttpServerWebSocketContext context);
    /// <summary>
    /// WebSocket Message Action
    /// </summary>
    /// <param name="context"></param>
    /// <param name="args">args</param>
    /// <returns></returns>
    public delegate void HttpServerWebSocketMessage(HttpServerWebSocketContext context, WebSocketMessageEventArgs args);

    /// <summary>
    /// Simple Http Server
    /// </summary>
    public class HttpServer : IDisposable
    {
        Socket listenSocket;
        Dictionary<Int64, HttpServerWebSocketContext> websockets;
        IHttpServerFileHandler fileHandler;


        int maxRequestHeadLength = 1024 * 1024;// 1m

        /// <summary>
        /// 获取或设置最大允许请求头长度，单位：字节， 默认 1M
        /// </summary>
        public int MaxRequestHeadLength
        {
            get { return this.maxRequestHeadLength; }
            set { this.maxRequestHeadLength = value; }
        }

        int maxRequestContentLength = 10 * 1024 * 1024; //10m
        /// <summary>
        /// 获取或设置请求内容最大长度，单位: 字节
        /// 默认10M, 不建议直接修改此值，若有必要建议实现NewRequest事件，并在其中为指定的URL设置<see cref="HttpServerContext.MaxRequestContentLength"/>
        /// get or set request content entity max length,
        /// do not recommend setting this value directly, 
        /// recommend set <see cref="HttpServerContext.MaxRequestContentLength"/> from NewRequest event
        /// </summary>
        public int MaxRequestContentLength
        {
            get { return this.maxRequestContentLength; }
            set { this.maxRequestContentLength = value; }
        }

        int bufferSize = 4096;
        /// <summary>
        /// 获取或设置连接读取缓冲区大小，默认 4096
        /// </summary>
        public int BufferSize
        {
            get { return this.bufferSize; }
            set { this.bufferSize = value; }
        }

        /// <summary>
        /// Server Port
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// ip address
        /// </summary>
        public string Ip
        {
            get;
            private set;
        }

        /// <summary>
        /// is Runing
        /// </summary>
        public bool IsRuning
        {
            get;
            private set;
        }

        HttpServerCallback callback;
        /// <summary>
        /// get or set http callback, set null resume to default callback
        /// </summary>
        public HttpServerCallback Callback
        {
            get { return this.callback; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                else
                {
                    this.callback = value;
                }
            }
        }

        IHttpServerHandler handler;
        /// <summary>
        /// get or set http handler, this property override <see cref="Callback"/> method
        /// </summary>
        public IHttpServerHandler Handler
        {
            get { return this.handler; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.handler = value;
            }
        }

        /// <summary>
        /// 新连接事件
        /// </summary>
        public event HttpServerWebSocketCallback WebSocketConnectioned;

        /// <summary>
        /// 连接断开事件
        /// </summary>
        public event HttpServerWebSocketCallback WebSocketDisconnected;

        /// <summary>
        /// 新消息事件
        /// </summary>
        public event HttpServerWebSocketMessage WebSocketNewMessage;

        /// <summary>
        /// 消息发送成功事件
        /// </summary>
        public event EventHandler<WebSocketSendEventArgs> WebSocketSendCompleted;

        /// <summary>
        /// 新HTTP请求， 此事件中的任务异常均会中止请求，并返回403状态
        /// </summary>
        public event EventHandler<HttpServerEventArgs> NewRequest;

        /// <summary>
        /// 未处理的异常事件。注意：实现该事件时请确保能安全执行，若事件有异常发生有可能会引起应用崩溃
        /// </summary>
        public event EventHandler<HttpServerErrorEventArgs> Error;

        /// <summary>
        /// 获取或设置服务器名
        /// </summary>
        public string ServerName
        {
            get;
            set;
        }

        bool authorization = false;
        /// <summary>
        /// 获取一个值表示是否需要验证
        /// </summary>
        public bool Authorization
        {
            get { return this.authorization; }
        }

        HttpServerAuthorizationCallback authorizationCallback;

        /// <summary>
        /// Authorization callback
        /// </summary>
        public HttpServerAuthorizationCallback AuthorizationCallback
        {
            get { return this.authorizationCallback; }
            set { this.authorizationCallback = value; this.authorization = value != null; }
        }

        /// <summary>
        /// WWW-Authenticate realm vaue
        /// </summary>
        public string AuthenticateRealm
        {
            get;
            set;
        }

        Encoding encoding;
        /// <summary>
        /// Encoding, default utf-8
        /// </summary>
        public Encoding Encoding
        {
            get { return this.encoding; }
            set { this.encoding = value; }
        }

        /// <summary>
        /// tcp AddressFamily
        /// </summary>
        public AddressFamily AddressFamily
        {
            get;
            private set;
        }


        /// <summary>
        /// 获取或设置监听队列长度
        /// </summary>
        public int Backlog
        {
            get;
            set;
        }

        int readHeadTimeout;
        /// <summary>
        /// 头读取超时时间 - KeepAlive 超时时间
        /// </summary>
        public int ReadHeadTimeout
        {
            get { return this.readHeadTimeout; }
            set { this.readHeadTimeout = value; }
        }

        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        /// <param name="callback"></param>
        public HttpServer(HttpServerCallback callback, int port, string ip = "*")
            : this(port, ip)
        {
            if (callback != null)
                this.callback = callback;
        }


        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        public HttpServer(int port, string ip = "*")
        {
            this.Port = port;
            this.Ip = ip;
            this.AddressFamily = ip != "*" && ValidateHelper.IsIPv6(ip, false) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
            this.encoding = Encoding.UTF8;
            this.AuthorizationCallback = null;
            this.Backlog = 256;
            this.ServerName = Dns.GetHostName();
            this.websockets = new Dictionary<Int64, HttpServerWebSocketContext>();
            this.readHeadTimeout = 10000;
            this.fileHandler = new HttpServerFileHandler();
        }

        /// <summary>
        /// start
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                if (this.IsRuning == false)
                {

                    //if (this.Callback == null)
                    //    throw new NotImplementedException("No Set Callback Property");

                    this.listenSocket = new Socket(this.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    //if (addressFamily == AddressFamily.InterNetworkV6)
                    //{  
                    //    // Set dual-mode (IPv4 & IPv6) for the socket listener.
                    //    // 27 is equivalent to IPV6_V6ONLY socket option in the winsock snippet below,
                    //    // based on http://blogs.msdn.com/wndp/archive/2006/10/24/creating-ip-agnostic-applications-part-2-dual-mode-sockets.aspx
                    //    this.listenSocket.SetSocketOption(SocketOptionLevel.IPv6,(SocketOptionName)27, false);
                    //}
                    //

                    IPEndPoint endPoint;
                    if (this.Ip == "*")
                        endPoint = new IPEndPoint(IPAddress.Any, this.Port);
                    else
                        endPoint = new IPEndPoint(IPAddress.Parse(this.Ip), this.Port);

                    this.listenSocket.Bind(endPoint);
                    this.listenSocket.Listen(this.Backlog);
                    this.listenSocket.ReceiveBufferSize = this.bufferSize;
                    this.listenSocket.BeginAccept(this.NewAccept, null);

                    this.IsRuning = true;
                }
            }
        }

        private void NewAccept(IAsyncResult ar)
        {
            Socket socket = null;
            try
            {
                socket = this.listenSocket.EndAccept(ar);
            }
            catch { }

            //new accept
            try
            {
                this.listenSocket.BeginAccept(this.NewAccept, null);
            }
            catch (ObjectDisposedException) { }

            //
            if (socket != null)
            {
                try
                {
                    ConnectionState connectionState;
                    //keep alive
                    while (true)
                    {
                        connectionState = this.NewConnection(socket);
                        if (connectionState == ConnectionState.KeepAlive)
                        {
                            //next request
                        }
                        else if (connectionState == ConnectionState.Closed)
                        {
                            //close and exit
                            SocketHelper.TryClose(socket);
                            break;
                        }
                        else if (connectionState == ConnectionState.KeepConnection)
                        {
                            //keep connection & exit
                            break;
                        }
                    }
                }
                catch (SocketException)
                {
                    SocketHelper.TryClose(socket);
                }
                catch (Exception exception)
                {
                    SocketHelper.TryClose(socket);
                    this.OnError(exception);
                }
            }
        }

        /// <summary>
        /// 发生未处理异常事件时
        /// </summary>
        /// <param name="exception"></param>
        protected virtual void OnError(Exception exception)
        {
            if (this.Error != null)
            {
                this.Error(this, new HttpServerErrorEventArgs(exception));
            }
        }

        /// <summary>
        /// new socket
        /// </summary>
        /// <param name="socket"></param>
        private ConnectionState NewConnection(Socket socket)
        {
            socket.ReceiveTimeout = this.readHeadTimeout;
            var context = this.ParseRequest(socket);
            if (context == null)
            {
                return ConnectionState.Closed;
            }

            //Authorization
            if (this.authorization)
            {
                var status = this.ParseAuthorization(context);
                if (status != HttpStatusCode.OK)
                {
                    context.Response(status);
                    return ConnectionState.Closed;
                }
            }

            //
            if (context.RequestType == HttpServerRequestType.Http)
            {
                var httpContext = (HttpServerContext)context;
                try
                {
                    //new request
                    this.OnNewRequest(httpContext);
                }
                catch (Exception)
                {
                    //服务器拒绝执行请求
                    //if (string.IsNullOrEmpty(httpContext.Content) && httpContext.ContentBuffer == null)
                    //{
                    //    httpContext.Content = "Request Abort " + exception.Message;
                    //}
                    httpContext.Response(HttpStatusCode.Forbidden);
                    return ConnectionState.Closed;
                }

                //post
                if (httpContext.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.ParsePostData(socket, httpContext) == false)
                    {
                        //解析失败
                        return ConnectionState.Closed;
                    }
                }

                //process
                if (this.HttpProcess(httpContext) == true)
                {
                    return ConnectionState.KeepAlive;
                }
                else
                {
                    return ConnectionState.Closed;
                }
            }
            else if (context.RequestType == HttpServerRequestType.WebSocket)
            {
                socket.ReceiveTimeout = System.Threading.Timeout.Infinite;
                if (this.WebSocketProcess((HttpServerWebSocketContext)context) == true)
                {
                    return ConnectionState.KeepConnection;
                }
                else
                {
                    return ConnectionState.Closed;
                }
            }
            else
            {
                throw new NotSupportedException("not support request type " + context.RequestType);
            }
        }

        private void OnNewRequest(HttpServerContext httpServerContext)
        {
            if (this.NewRequest != null)
            {
                this.NewRequest(this, new HttpServerEventArgs(httpServerContext));
            }
        }

        private bool WebSocketProcess(HttpServerWebSocketContext context)
        {
            var secKey = context.RequestHeader["Sec-WebSocket-Key"];
            if (string.IsNullOrEmpty(secKey))
            {
                //请求的版本不接受支持，最低版本：  7
                context.Response(HttpStatusCode.HttpVersionNotSupported);
                return false;
            }

            //握手信息
            var acceptKey = WebSocketHandshake.HandshakeSecurityHash09(secKey);
            context.ResponseHeader.Add("Sec-WebSocket-Accept", acceptKey);

            //响应客户端连接
            context.Response(HttpStatusCode.SwitchingProtocols);

            //call event
            if (this.WebSocketConnectioned == null)
            {
                return false;
            }

            this.WebSocketConnectioned(context);

            if (context.Allowed == false)
            {
                return false;
            }

            //receive
            context.Receive();

            //add session
            lock (this.websockets)
            {
                this.websockets.Add(context.Id, context);
            }

            return true;
        }

        /// <summary>
        /// Http 请求处理
        /// </summary>
        /// <param name="context"></param>
        private bool HttpProcess(HttpServerContext context)
        {
            try
            {
                var status = HttpStatusCode.OK;
                if (this.handler != null)
                {
                    try
                    {
                        this.handler.Process(context);
                        status = context.Status;
                    }
                    catch (Exception e)
                    {
                        status = HttpStatusCode.InternalServerError;
                        context.Content = e.Message;
                        this.OnError(e);
                    }
                }
                else if (this.callback != null)
                {
                    try
                    {
                        status = this.callback(context);
                    }
                    catch (Exception e)
                    {
                        status = HttpStatusCode.InternalServerError;
                        context.Content = e.Message;
                        this.OnError(e);
                    }
                }
                else
                {
                    context.Content = "no set callback or handler";
                    status = HttpStatusCode.ServiceUnavailable;
                }
                //非异步响应
                if (context.ChunkWriteStatus == HttpServerChunkStatus.NoBegin)
                {
                    if (status != HttpStatusCode.OK && string.IsNullOrEmpty(context.Content) && context.ContentBuffer == null)
                    {
                        context.Content = status.ToString();
                    }
                    context.Response(status);
                }
                else
                {
                    //等待结束写入
                    context.chunkWriteEvent.WaitOne();
                    context.chunkWriteEvent.Close();
                }

            }
            finally
            {
                //进行必要的资源清理
                context.Clear();
            }
            return context.KeepAlive && context.Socket.Connected;
        }

        private HttpStatusCode ParseAuthorization(HttpServerContextBase context)
        {
            if (string.IsNullOrEmpty(context.UserName))
            {
                context.ResponseHeader.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", this.AuthenticateRealm));
                return HttpStatusCode.Unauthorized;
            }

            try
            {
                //call
                if (this.authorizationCallback(context))
                {
                    return HttpStatusCode.OK;
                }
                else
                {
                    context.ResponseHeader.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", this.AuthenticateRealm));
                    return HttpStatusCode.Unauthorized;
                }
            }
            catch (SocketException exception)
            {
                throw new MemberAccessException("authorization callback failure", exception);
            }


            //var authorization = context.RequestHeader["Authorization"];
            //if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Basic ") && authorization.Length > 6)
            //{
            //    context.ResponseHeader.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", this.AuthenticateRealm));
            //    return HttpStatusCode.Unauthorized;
            //}
            //else
            //{
            //    var auth = authorization.Substring(6);
            //    try
            //    {
            //        auth = this.encoding.GetString(Convert.FromBase64String(auth));
            //    }
            //    catch (Exception e)
            //    {
            //        if (context.RequestType == HttpServerRequestType.Http)
            //            ((HttpServerContext)context).Content = e.Message;
            //        return HttpStatusCode.Unauthorized;
            //    }
            //    var auths = auth.Split(':');
            //    if (auths.Length == 2)
            //    {
            //        context.UserName = auths[0];
            //        context.Password = auths[1];
            //        try
            //        {
            //            //call
            //            if (this.authorizationCallback(context))
            //            {
            //                return HttpStatusCode.OK;
            //            }
            //            else
            //            {
            //                context.ResponseHeader.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", this.AuthenticateRealm));
            //                return HttpStatusCode.Unauthorized;
            //            }
            //        }
            //        catch (SocketException exception)
            //        {
            //            throw new MemberAccessException("authorization callback failure", exception);
            //        }
            //    }
            //    else
            //    {
            //        return HttpStatusCode.BadRequest;
            //    }
            //}
        }

        private HttpServerContextBase ParseRequest(Socket socket)
        {
            SocketReader headReader = new SocketReader(socket, this.encoding, this.maxRequestHeadLength);
            var line = headReader.ReadStringLine();
            var arr = line.Split(' ');
            if (arr.Length != 3)
            {
                return null;
            }
            var method = arr[0];
            var url = arr[1];
            var protocol = arr[2];

            var header = new NameValueCollection(10, StringComparer.OrdinalIgnoreCase);
            while ((line = headReader.ReadStringLine()) != null && !line.Equals(string.Empty))
            {
                var index = line.IndexOf(':');
                header[line.Substring(0, index)] = line.Substring(index + 1).Trim();
            }

            HttpServerContextBase context;
            if ("WebSocket".Equals(header["Upgrade"], StringComparison.OrdinalIgnoreCase))
            {
                context = new HttpServerWebSocketContext(socket, header, this, url, method, protocol);
            }
            else
            {
                context = new HttpServerContext(socket, header, this, url, method, protocol, this.maxRequestContentLength, this.fileHandler);
            }

            //querystring
            string path;
            UriHelper.ParseQueryString(context.Url, this.encoding, context.QueryString, out path);
            //
            return context;
        }

        /// <summary>
        /// 解析POST数扰
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="context"></param>
        /// <returns>是否成功</returns>
        protected virtual bool ParsePostData(Socket socket, HttpServerContext context)
        {
            var success = false;
            var contentLength = 0;
            int.TryParse(context.RequestHeader["Content-Length"], out contentLength);
            if (contentLength > context.MaxRequestContentLength)
            {
                //请求主体超过限制
                if (string.IsNullOrEmpty(context.Content) && context.ContentBuffer == null)
                {
                    context.Content = "Request Entity Too Large";
                }
                context.Response(HttpStatusCode.RequestEntityTooLarge);
            }
            else if (contentLength > 0)
            {
                string contentType = context.RequestHeader["Content-Type"];
                if (contentType == null)
                {
                    context.PostData = SocketHelper.Receive(socket, contentLength);
                    success = true;
                }
                else if ("application/x-www-form-urlencoded".Equals(contentType, StringComparison.OrdinalIgnoreCase))
                {
                    var data = SocketHelper.Receive(socket, contentLength);
                    var postdata = this.encoding.GetString(data);
                    UriHelper.ParseQueryString(postdata, this.encoding, context.Form);
                    success = true;
                }
                else if (contentType.IndexOf("multipart/form-data", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    //"multipart/form-data; boundary=---------------------boundary123data"
                    var boundary = contentType.Substring(contentType.IndexOf("boundary=") + 9);
                    var receiver = new HttpServerMultipartReceiver(socket, boundary, context, contentLength);
                    try
                    {
                        return receiver.Receive();
                    }
                    catch (Exception exception)
                    {
                        //请求主体超过限制
                        if (string.IsNullOrEmpty(context.Content) && context.ContentBuffer == null)
                        {
                            context.Content = "Paser Post Data Failure " + exception.Message;
                        }
                        context.Response(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    context.PostData = SocketHelper.Receive(socket, contentLength);
                    success = true;
                }
            }
            else
            {
                //要求必要的 Content-Length
                if (string.IsNullOrEmpty(context.Content) && context.ContentBuffer == null)
                {
                    context.Content = "Require Content-Length";
                }
                context.Response(HttpStatusCode.LengthRequired);
            }

            return success;
        }

        /// <summary>
        /// 获取指定id websocket,未找到则返回null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpServerWebSocketContext GetWebSocket(Int64 id)
        {
            HttpServerWebSocketContext context;
            return this.websockets.TryGetValue(id, out context) ? context : null;
        }

        /// <summary>
        /// 遍历当前具有的WebSocket连接
        /// </summary>
        /// <param name="action"></param>
        public void WebSocketForEach(Action<HttpServerWebSocketContext> action)
        {
            HttpServerWebSocketContext[] contexts;
            lock (this.websockets)
            {
                contexts = new HttpServerWebSocketContext[this.websockets.Count];
                this.websockets.Values.CopyTo(contexts, 0);
            }

            //执行
            foreach (var context in contexts)
            {
                action(context);
            }
        }
        
        /// <summary>
        /// 触发一个消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="opcode"></param>
        /// <param name="context"></param>
        internal protected void OnWebSocketNewMessage(HttpServerWebSocketContext context, byte[] data, WebSocketOpcode opcode)
        {
            var action = this.WebSocketNewMessage;
            if (action != null)
            {
                var e = new WebSocketMessageEventArgs(opcode);
                e.Buffer = data;
                if (opcode == WebSocketOpcode.Text)
                {
                    e.Message = this.Encoding.GetString(data);
                }
                action(context, e);
            }
        }

        /// <summary>
        /// 引发连接断开事件
        /// </summary>
        /// <param name="context"></param>
        internal protected void OnWebSocketDisconnected(HttpServerWebSocketContext context)
        {
            lock (this.websockets)
                this.websockets.Remove(context.Id);

            if (this.WebSocketDisconnected != null)
            {
                this.WebSocketDisconnected(context);
            }
        }

        /// <summary>
        /// 触发一个消息发送完成事件，置状态失败
        /// </summary>
        /// <param name="args"></param>
        /// <param name="context"></param>
        internal protected void OnWebSocketSendCompleted(HttpServerWebSocketContext context, WebSocketSendEventArgs args)
        {
            var action = this.WebSocketSendCompleted;
            if (action != null)
            {
                action(context, args);
            }
        }
        
        /// <summary>
        /// 断开所有连接
        /// </summary>
        public void WebSocketDisconnectAll()
        {
            this.WebSocketForEach(context => { context.Close(); });
        }

        /// <summary>
        /// Stop
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (this.IsRuning == true)
                {
                    this.listenSocket.Close();

                    this.WebSocketDisconnectAll();

                    //reset ,clear content size
                    this.websockets = new Dictionary<Int64, HttpServerWebSocketContext>();

                    this.IsRuning = false;
                }
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.Stop();
            //
            this.NewRequest = null;
            this.WebSocketConnectioned = null;
            this.WebSocketDisconnected = null;
            this.WebSocketNewMessage = null;
            this.WebSocketSendCompleted = null;
        }

        enum ConnectionState
        {
            KeepAlive,
            KeepConnection,
            Closed
        }

        //文件处理器
        class HttpServerFileHandler : IHttpServerFileHandler
        {
            public HttpServerFileParameter Create(string name, string fileName, string contentType, HttpServerContext context)
            {
                return new HttpServerFileParameter(name, fileName, contentType);
            }
        }
    }
}


//POST /User/Login.api HTTP/1.1
//Referer: http://www.aooshi.org
//Content-Type: application/x-www-form-urlencoded
//X-Requested-With: XMLHttpRequest
//Accept: application/json, text/javascript, */*; q=0.01
//Accept-Language: zh-cn
//Accept-Encoding: gzip, deflate
//User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)
//Host: www.aooshi.org
//Content-Length: 56
//Connection: Keep-Alive
//Cache-Control: no-cache

//userMailorPhone=fafasf&userPwd=asfsafdsafd&remeber=false