using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Net;

namespace Adf
{
    /// <summary>
    /// Http Server WebSocket Context
    /// </summary>
    public class HttpServerWebSocketContext : HttpServerContextBase
    {
        static Int64 identity = 0;

        byte[] headerBuffer;
        readonly int headerLength = 2;
        int headerRead = 0;
        object sendLockObject = new object();

        bool isClosed = false;

        Socket socket;
        /// <summary>
        /// get Socket
        /// </summary>
        public Socket Socket
        {
            get { return this.socket; }
        }

        /// <summary>
        /// 获取连接创建时间
        /// </summary>
        public DateTime Time
        {
            get;
            private set;
        }

        ///// <summary>
        ///// 获取连接ID
        ///// </summary>
        //public string Id
        //{
        //    get;
        //    private set;
        //}

        Int64 id = 0;
        /// <summary>
        /// 获取连接ID
        /// </summary>
        public Int64 Id
        {
            get { return this.id; }
        }
        
        bool allowed = true;
        /// <summary>
        /// 在连接开始事件中标志是否通过所有事件的检测，当你不允许这个连接继续时，设置为false， 系统将禁止此一连接请求
        /// </summary>
        public bool Allowed
        {
            get { return this.allowed; }
            set { this.allowed = value; }
        }

        string closeMessage = "";
        /// <summary>
        /// get close message
        /// </summary>
        public string CloseMessage
        {
            get { return this.closeMessage; }
        }

        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="header"></param>
        /// <param name="server"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="protocol"></param>
        public HttpServerWebSocketContext(Socket socket, NameValueCollection header, HttpServer server, string url, string method, string protocol)
            : base(header, server, url, method, protocol)
        {
            this.socket = socket;
            base.RequestType = HttpServerRequestType.WebSocket;
            this.headerBuffer = new byte[2];
            //
            this.Time = DateTime.Now;
            this.id = System.Threading.Interlocked.Increment(ref identity);
            //
            this.ResponseHeader.Add("Upgrade", "websocket");
            this.ResponseHeader.Add("Connection", "Upgrade");
        }

        /// <summary>
        /// 获取远程节点标志, IP:PORT
        /// </summary>
        /// <returns></returns>
        public string GetRemoteNode()
        {
            var rep = (IPEndPoint)this.socket.RemoteEndPoint;
            return string.Concat(rep.Address, ":", rep.Port);
        }

        /// <summary>
        /// 获取远程节点
        /// </summary>
        /// <returns></returns>
        public IPEndPoint GetRemotePoint()
        {
            var rep = (IPEndPoint)this.socket.RemoteEndPoint;
            return rep;
        }
                
        /// <summary>
        /// 发送一个PONG帧
        /// </summary>
        /// <exception cref="System.Net.Sockets.SocketException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public void Pong()
        {
            this.Send(new byte[0], WebSocketOpcode.Pong);
        }

        /// <summary>
        /// 发送字节数组一个消息
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="System.Net.Sockets.SocketException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public void Send(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            this.Send(data, WebSocketOpcode.Binary);
        }

        /// <summary>
        /// 发送字节数组一个消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userState"></param>
        /// <exception cref="System.Net.Sockets.SocketException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public void Send(byte[] data, object userState)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            this.SendAsync(data, WebSocketOpcode.Binary, userState);
        }

        /// <summary>
        /// 发送一个文本消息
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="System.Net.Sockets.SocketException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public void Send(string message)
        {
            if (message == null)
                message = "";

            var data = this.Encoding.GetBytes(message);
            this.Send(data, WebSocketOpcode.Text);
        }

        /// <summary>
        /// 发送一个文本消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="userState"></param>
        /// <exception cref="System.Net.Sockets.SocketException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public void SendAsync(string message, object userState)
        {
            if (message == null)
                message = "";

            var data = this.Encoding.GetBytes(message);
            this.SendAsync(data, WebSocketOpcode.Text, userState);
        }

        /// <summary>
        /// 发送一组数据，并指定消息类型
        /// </summary>
        /// <param name="data"></param>
        /// <param name="opcode"></param>
        /// <exception cref="System.Net.Sockets.SocketException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public void Send(byte[] data, WebSocketOpcode opcode)
        {
            //协议原因，一些客户端不支持服务器端的 mask ,因此此值必需为  false

            var df = new WebSocketDataFrame(data, false, opcode);
            var buffer = df.GetFrameBytes();
            try
            {
                lock (this.sendLockObject)
                {
                    this.socket.Send(buffer);
                }
            }
            catch (Exception exception)
            {
                this.Close("IO Error on send," + exception.Message, WebSocketCloseReason.IOError);
                throw;
            }
        }

        /// <summary>
        /// 发送一组数据，并指定消息类型
        /// </summary>
        /// <param name="data"></param>
        /// <param name="opcode"></param>
        /// <param name="userSate"></param>
        /// <exception cref="System.Net.Sockets.SocketException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public void SendAsync(byte[] data, WebSocketOpcode opcode,object userSate)
        {
            //协议原因，一些客户端不支持服务器端的 mask ,因此此值必需为  false

            var df = new WebSocketDataFrame(data, false, opcode);
            var buffer = df.GetFrameBytes();
            try
            {
                lock (this.sendLockObject)
                {
                    this.socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, this.SendCallback, userSate);
                }
            }
            catch (Exception exception)
            {
                this.Close("IO Error on send," + exception.Message, WebSocketCloseReason.IOError);
                throw;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            WebSocketSendEventArgs args;
            try
            {
                this.socket.EndSend(ar);
                args = new WebSocketSendEventArgs(ar.AsyncState);
            }
            //catch (Ob
            catch (Exception exception)
            {
                args = new WebSocketSendEventArgs(exception, ar.AsyncState);
            }

            //
            this.Server.OnWebSocketSendCompleted(this, args);
        }

        /// <summary>
        /// 开始读取
        /// </summary>
        internal void Receive()
        {
            this.headerRead = 0;
            this.socket.BeginReceive(this.headerBuffer, 0, 2, SocketFlags.None, this.ReceiveMessageCallback, null);
        }

        private void ReceiveMessageCallback(IAsyncResult ar)
        {
            var receiveLength = 0;
            try
            {
                receiveLength = this.socket.EndReceive(ar);
                if (receiveLength == 0)
                {
                    this.Close("IO disconnected", WebSocketCloseReason.Disconnected);
                    return;
                }

                this.headerRead += receiveLength;

                if (this.headerRead == this.headerLength)
                {
                    this.ParseMessage();
                }
                else
                {
                    //receive seconds byte
                    this.socket.BeginReceive(this.headerBuffer, this.headerRead, this.headerLength - this.headerRead, SocketFlags.None, this.ReceiveMessageCallback, null);
                }
            }
            catch (SocketException exception)
            {
                this.Close("IOError," + exception.Message, WebSocketCloseReason.IOError);
            }
            catch (ObjectDisposedException exception)
            {
                this.Close("Error " + exception.Message, WebSocketCloseReason.Error);
            }
            catch (Exception exception)
            {
                this.Close("Error " + exception.Message, WebSocketCloseReason.Error);
            }
        }

        private void ParseMessage()
        {
            //receive Data Frame
            WebSocketDataFrame frame = new WebSocketDataFrame(this.socket, this.headerBuffer);

            //ack
            if (frame.Opcode == WebSocketOpcode.Ping)
            {
                this.Pong();
            }

            //call
            this.OnMessage(frame.Content, frame.Opcode);

            //new receive
            this.Receive();
        }

        /// <summary>
        /// create response header
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        protected override StringBuilder CreateResponseHeader(HttpStatusCode status)
        {
            var build = base.CreateResponseHeader(status);
            build.AppendLine();

            return build;
        }

        /// <summary>
        /// 响应客户端请求
        /// </summary>
        /// <param name="header"></param>
        /// <param name="status"></param>
        protected override void Response(StringBuilder header, HttpStatusCode status)
        {
            var headerBuffer = Encoding.ASCII.GetBytes(header.ToString());
            this.socket.Send(headerBuffer);
        }

        /// <summary>
        /// 引发消息事件
        /// </summary>
        /// <param name="data"></param>
        /// <param name="opcode"></param>
        protected virtual void OnMessage(byte[] data, WebSocketOpcode opcode)
        {
            this.Server.OnWebSocketNewMessage(this, data, opcode);
        }

        /// <summary>
        /// 关闭当前连接
        /// </summary>
        public void Close()
        {
            this.Close("User Close", WebSocketCloseReason.Close);
        }

        /// <summary>
        /// 关闭当前连接
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="message"></param>
        protected void Close(string message, WebSocketCloseReason reason)
        {
            var trigger = false;
            lock (this)
            {
                if (this.isClosed == false)
                {
                    this.socket.Close();

                    this.closeMessage = message;

                    trigger = true;

                    this.isClosed = true;
                }
            }
            if (trigger)
            {
                this.Server.OnWebSocketDisconnected(this);
            }
        }
    }
}


//客户端向服务器发送请求

//https://tools.ietf.org/html/draft-hixie-thewebsocketprotocol-75
//https://tools.ietf.org/html/draft-hixie-thewebsocketprotocol-76
//https://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-00

//7:    https://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-07
//8:    https://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-10
//13:   https://tools.ietf.org/html/rfc6455

//GET / HTTP/1.1
//Upgrade: websocket
//Connection: Upgrade
//Host: 127.0.0.1:8080
//Origin: http://test.com
//Pragma: no-cache
//Cache-Control: no-cache
//Sec-WebSocket-Key: OtZtd55qBhJF2XLNDRgUMg==
//Sec-WebSocket-Version: 13
//Sec-WebSocket-Extensions: x-webkit-deflate-frame
//User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36

//服务器给出响应

//HTTP/1.1 101 Switching Protocols
//Upgrade: websocket
//Connection: Upgrade
//Sec-WebSocket-Accept: xsOSgr30aKL2GNZKNHKmeT1qYjA=


// 在请求中的“Sec-WebSocket-Key”是随机的，服务器端会用这些数据来构造出一个SHA-1的信息摘要。
//把“Sec-WebSocket-Key”加上一个魔幻字符串“258EAFA5-E914-47DA-95CA-C5AB0DC85B11”。
//使用 SHA-1 加密，之后进行 BASE-64编码，将结果做为 “Sec-WebSocket-Accept” 头的值，返回给客户端（来自维基百科）。






//handshake request:
//GET /test HTTP/1.1
//Upgrade: websocket
//Connection: Upgrade
//Host: 192.168.123.102:8585
//Sec-WebSocket-Origin: http://192.168.123.5
//Sec-WebSocket-Key: YB0mPvJ5t8ggCeGUWY39uQ==
//Sec-WebSocket-Version: 8

//handshake response header :
//HTTP/1.1 101 Switching Protocols
//Upgrade: websocket
//Connection: Upgrade
//Sec-WebSocket-Accept: xt9iyCNryQTseELUkHPWjzxA2ts=

 
//4.4.       支持多版本的WebSocket协议
//This sectionprovides some guidance on supporting multiple versions
//of the WebSocket Protocol in clients and servers.
 
//Using the WebSocketversion advertisement capability (the
//|Sec-WebSocket-Version|header field), a client can initially request
//the version of theWebSocket Protocol that it prefers (which doesn’t
//necessarily have tobe the latest supported by the client). If the
//server supports therequested version and the handshake message is
//otherwise valid,the server will accept that version. If the server
//doesn’t support therequested version, it MUST respond with a
//|Sec-WebSocket-Version|header field (or multiple
//|Sec-WebSocket-Version|header fields) containing all versions it is
//willing to use. Atthis point, if the client supports one of the
//advertisedversions, it can repeat the WebSocket handshake using a
//new version value.
 
//The followingexample demonstrates version negotiation described
//above:
//GET/chat HTTP/1.1
//Host:server.example.com
//Upgrade:websocket
//Connection:Upgrade
//...
//Sec-WebSocket-Version:25
 
//The response from the server might look as follows:
//HTTP/1.1400 Bad Request
//...
//Sec-WebSocket-Version:13, 8, 7
 
//Note that the last response from the server might also look like:
//HTTP/1.1400 Bad Request
//...
//Sec-WebSocket-Version:13
//Sec-WebSocket-Version:8, 7
 
//The client now repeats the handshake that conforms to version 13:
//GET/chat HTTP/1.1
//Host:server.example.com
//Upgrade:websocket
//Connection:Upgrade
//...
//Sec-WebSocket-Version:13