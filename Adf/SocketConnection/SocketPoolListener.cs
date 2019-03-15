using System;
using System.Collections.Generic;

namespace Adf.SocketConnection
{
    /// <summary>
    /// socket listener with a connection pool
    /// </summary>
    public class SocketPoolListener : SocketListener
    {
        Dictionary<long, SocketConnection> connectionDict = new Dictionary<long, SocketConnection>(32);


        /// <summary>
        /// get connection count
        /// </summary>
        /// <returns></returns>
        public int ConnectionCount
        {
            get
            {
                return this.connectionDict.Count;
            }
        }


        /// <summary>
        /// initialize a new instance
        /// </summary>
        /// <param name="ep">list endpoint</param>
        public SocketPoolListener(string ep)
            : base(ep)
        {
        }

        /// <summary>
        /// initialize a new instance
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public SocketPoolListener(string host, int port)
            : base(host, port)
        {
        }

        /// <summary>
        /// close connection
        /// </summary>
        /// <param name="id"></param>
        /// <returns>not connection return false, success return true.</returns>
        public bool CloseConnection(long id)
        {
            SocketConnection connection = null;
            lock (this.connectionDict)
            {
                this.connectionDict.TryGetValue(id, out connection);
                this.connectionDict.Remove(id);
            }
            if (connection == null)
            {
                return false;
            }

            //
            connection.Close();

            return true;

            //try
            //{
            //    connection.Dispose();
            //}
            //catch (SocketException)
            //{
            //    //ignore
            //}
            ////catch (IOException)
            ////{
            ////    //ignore
            ////}
            //catch (Exception exception)
            //{
            //    //exception
            //    //Program.LogManager.Exception(exception);
            //    this.TriggerException(exception);
            //}
        }

        /// <summary>
        /// new connection
        /// </summary>
        /// <param name="id"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        protected override SocketConnection CreateConnection(long id, System.Net.Sockets.Socket socket)
        {
            SocketConnection connection = base.CreateConnection(id, socket);
            
            lock (this.connectionDict)
            {
                this.connectionDict.Add(id, connection);
            }

            return connection;
        }

        /// <summary>
        /// initialize connection
        /// </summary>
        /// <param name="connection"></param>
        protected override void ReadConnection(SocketConnection connection)
        {
            try
            {
                base.ReadConnection(connection);
            }
            catch
            {
                this.CloseConnection(connection.Id);
            }
        }

        /// <summary>
        /// get connection for enabled hold option.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SocketConnection GetConnection(long id)
        {
            SocketConnection connection = null;
            lock (this.connectionDict)
            {
                this.connectionDict.TryGetValue(id, out connection);
            }
            return connection;
        }

        /// <summary>
        /// for each all socket connection
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<SocketConnection> action)
        {
            Dictionary<long, SocketConnection> dict = this.connectionDict;
            SocketConnection[] connections = null;
            lock (dict)
            {
                connections = new SocketConnection[dict.Count];
                dict.Values.CopyTo(connections, 0);
            }

            for (int j = 0, l = connections.Length; j < l; j++)
            {
                action(connections[j]);
            }
        }

        /// <summary>
        /// dispose instance
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            
            lock (this.connectionDict)
            {
                foreach (var item in this.connectionDict)
                {
                    try
                    {
                        item.Value.Close();
                    }
                    catch
                    {
                        //ignore
                    }
                }

                this.connectionDict.Clear();
            }
        }
    }
}
