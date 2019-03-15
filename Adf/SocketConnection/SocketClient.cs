using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Adf.SocketConnection
{
    /// <summary>
    /// socket client
    /// </summary>
    public class SocketClient : SocketConnection
    {
        string host;
        /// <summary>
        /// get listen host
        /// </summary>
        public string Host
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
        /// initialize a new instance
        /// </summary>
        /// <param name="ep">list endpoint</param>
        public SocketClient(string ep)
        {
            if (Adf.IpHelper.ParseEndPoint(ep, ref this.host, ref this.port) == false)
                throw new ArgumentException("ep");
        }

        /// <summary>
        /// initialize a new instance
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public SocketClient(string host, int port)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            if (Adf.IpHelper.CheckPort(port) == false)
                throw new ArgumentException("port");

            this.host = host;
            this.port = port;
        }
                
        /// <summary>
        /// begin connect, timeout 15s
        /// </summary>
        /// <exception cref="InvalidOperationException">no set new connection action</exception>
        public void Connect()
        {
            this.Connect(15000);
        }

        /// <summary>
        /// begin connect
        /// </summary>
        /// <exception cref="InvalidOperationException">no set new connection action</exception>
        /// <exception cref="TimeoutException"></exception>
        /// <param name="timeout"></param>
        public void Connect(int timeout)
        {
            IPAddress addr = null;
            Socket socket = null;

            //parse dns
            var he = Dns.GetHostEntry(this.host);   
            for (int i = 0, l = he.AddressList.Length; i < l; i++)
            {
                addr = he.AddressList[i];
                break;
            }

            //
            if (addr.AddressFamily == AddressFamily.InterNetworkV6)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            Adf.SocketHelper.Connect(socket, addr, this.port, timeout);


            NetworkStream stream = new NetworkStream(socket, true);
            stream.ReadTimeout = 30000; //30 * 1000

            //
            base.Stream = stream;
            base.RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;

            //
            this.OnConnectioned();
        }
        
        /// <summary>
        /// on disconnectioned
        /// </summary>
        public event EventHandler<ConnectionEventArgs> Connectioned = null;

        /// <summary>
        /// on new disconnection
        /// </summary>
        protected virtual void OnConnectioned()
        {
            var fun = this.Connectioned;
            if (fun != null)
            {
                var args = new ConnectionEventArgs(this);
                fun(this, args);
            }
        }
    }
}
