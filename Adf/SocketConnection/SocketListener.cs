using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Adf.SocketConnection
{
    /// <summary>
    /// socket listener
    /// </summary>
    public class SocketListener : IDisposable
    {
        bool disposed = false;
        Socket listenSocket = null;

        long identity = 0;
        IPAddress host;
        /// <summary>
        /// get listen host
        /// </summary>
        public IPAddress Host
        {
            get { return this.host; }
        }

        int port;
        /// <summary>
        /// get listen port
        /// </summary>
        public int Port
        {
            get { return this.port; }
        }

        /// <summary>
        /// is listen start
        /// </summary>
        public bool IsListened
        {
            get { return this.listenSocket != null; }
        }
        
        LogAgent logAgent = null;
        /// <summary>
        /// log agent
        /// </summary>
        public LogAgent LogAgent
        {
            get { return this.logAgent; }
        }

        /// <summary>
        /// on error
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error = null;
        
        /// <summary>
        /// on new connection
        /// </summary>
        public event EventHandler<ConnectionEventArgs> NewConnection = null;

        IListenerHandler handler = null;
        /// <summary>
        /// get listener handler
        /// </summary>
        public IListenerHandler Handler
        {
            get { return this.handler; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.handler = value;
            }
        }

        /// <summary>
        /// initialize a new instance
        /// </summary>
        /// <param name="ep">list endpoint</param>
        public SocketListener(string ep)
        {
            var point = Adf.IpHelper.ParseEndPoint(ep);
            if (point == null)
            {
                throw new ArgumentOutOfRangeException("ep");
            }
            //
            this.Initialize(point.Address, point.Port);
        }

        /// <summary>
        /// initialize a new instance
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public SocketListener(string host, int port)
        {
            IPAddress addr;
            if (IPAddress.TryParse(host, out addr) == false)
            {
                throw new ArgumentOutOfRangeException("host");
            }
            //
            this.Initialize(addr, port);
        }

        /// <summary>
        /// initialize listen
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        private void Initialize(IPAddress host, int port)
        {
            if (port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException("port", "listen port allow 1-65535.");

            this.host = host;
            this.port = port;
            //
            this.logAgent = new LogAgent();
            this.handler = ListenerHandler.Default;
        }

        /// <summary>
        /// output host and port
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat("Tcp Listen: ", this.host.ToString(), ":", this.port);
        }

        /// <summary>
        /// listen start
        /// </summary>
        public void Listen()
        {
            this.Listen(4096);
        }

        /// <summary>
        /// listen start
        /// </summary>
        /// <param name="backlog"></param>
        /// <exception cref="InvalidOperationException">no set new connection action</exception>
        public void Listen(int backlog)
        {
            if (this.host.AddressFamily == AddressFamily.InterNetworkV6)
            {
                this.listenSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                this.listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            //
            this.logAgent.Message.WriteTimeLine("Listener: listen " + this.host + ":" + this.port);
            this.logAgent.Message.WriteTimeLine("Listener: listen start");
            //
            this.listenSocket.Bind(new IPEndPoint(this.host, this.port));
            this.listenSocket.Listen(backlog);
            //
            this.listenSocket.BeginAccept(this.AcceptCallback, null);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            if (this.disposed == true)
                return;

            Socket listenSocket = this.listenSocket;
            Socket acceptSocket = null;

            if (listenSocket == null)
                return;

            try
            {
                acceptSocket = listenSocket.EndAccept(ar);
                listenSocket.BeginAccept(this.AcceptCallback, null);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (this.disposed == true)
                return;

            if (acceptSocket != null)
            {
                //call
                //System.Threading.ThreadPool.QueueUserWorkItem(this.AcceptSocket, acceptSocket);
                this.AcceptSocket(acceptSocket);
            }
        }

        private void AcceptSocket(Socket acceptSocket)
        {
            //Socket acceptSocket = (Socket)state;

            var id = System.Threading.Interlocked.Increment(ref this.identity);

            string rep = "";
            try
            {
                rep = acceptSocket.RemoteEndPoint.ToString();
            }
            catch { }

            this.logAgent.Message.WriteTimeLine("Listener: accept new connection: " + rep + " assign id: " + id);

            SocketConnection connection = this.CreateConnection(id, acceptSocket);

            if (connection == null)
            {
                this.logAgent.Message.WriteTimeLine("Listener: connection " + id + " create failure, skip.");
            }
            else
            {
                this.logAgent.Message.WriteTimeLine("Listener: connection " + id + ": " + rep + " created, read start.");
                //trigger event
                this.OnConnectioned(connection);
                //begin read
                this.ReadConnection(connection);
            }
        }

        /// <summary>
        /// connection call read
        /// </summary>
        /// <param name="connection"></param>
        protected virtual void ReadConnection(SocketConnection connection)
        {
            connection.Read();
        }

        /// <summary>
        /// new connection
        /// </summary>
        /// <param name="id"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        protected virtual SocketConnection CreateConnection(long id, Socket socket)
        {
            SocketConnection connection = null;
            try
            {
                NetworkStream stream = new NetworkStream(socket, true);
                //stream.ReadTimeout = 30000; //30 * 1000
                //
                connection = this.handler.CreateConnection(this);
                connection.Stream = stream;
                connection.Listener = this;
                connection.Id = id;
                connection.RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            }
            catch (Exception exception)
            {
                this.OnError(exception);
            }
            return connection;
        }

        /// <summary>
        /// on new error
        /// </summary>
        /// <param name="connection"></param>
        protected virtual void OnConnectioned(SocketConnection connection)
        {
            var fun = this.NewConnection;
            if (fun != null)
            {
                var args = new ConnectionEventArgs(connection);
                fun(this, args);
            }
        }

        /// <summary>
        /// on new error
        /// </summary>
        /// <param name="exception"></param>
        protected virtual void OnError(Exception exception)
        {
            var fun = this.Error;
            if (fun != null)
            {
                var args = new ErrorEventArgs(exception);
                fun(this, args);
            }
        }

        /// <summary>
        /// dispose server
        /// </summary>
        public virtual void Dispose()
        {
            this.disposed = true;

            Socket listenSocket2 = this.listenSocket;
            this.listenSocket = null;

            if (listenSocket2 != null)
            {
                try
                {
                    listenSocket2.Close();
                }
                catch (Exception)
                { }
            }
        }
    }
}
