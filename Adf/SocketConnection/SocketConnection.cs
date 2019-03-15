using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Adf.SocketConnection
{
    /// <summary>
    /// socket connection
    /// </summary>
    public class SocketConnection : IDisposable
    {
        byte[] read_buffer = new byte[1];

        bool disposed = false;
        
        /// <summary>
        /// get or set user state
        /// </summary>
        public object UserState
        {
            get;
            set;
        }

        Stream stream = null;
        /// <summary>
        /// get or set stream
        /// </summary>
        public Stream Stream
        {
            get { return this.stream; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                this.stream = value;
            }
        }

        IPEndPoint remoteEndPoint = null;
        /// <summary>
        /// get or set remote end point
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return this.remoteEndPoint; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                this.remoteEndPoint = value;
            }
        }

        SocketListener listener = null;
        /// <summary>
        /// get or set listener
        /// </summary>
        public SocketListener Listener
        {
            get { return this.listener; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                this.listener = value;
            }
        }

        IConnectionHandler handler = null;
        /// <summary>
        /// get connection handler
        /// </summary>
        public IConnectionHandler Handler
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
        public SocketConnection()
        {
            this.handler = ConnectionHandler.Default;
        }

        /// <summary>
        /// output connection id
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat("Connection: " + this.Id);
        }

        /// <summary>
        /// close connection
        /// </summary>
        public virtual void Close()
        {
            this.disposed = true;

            try
            {
                this.stream.Close();
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// dispose server
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Close();
        }

        /// <summary>
        /// get connection id
        /// </summary>
        public long Id
        {
            get;
            set;
        }

        /// <summary>
        /// call read
        /// </summary>
        public virtual void Read()
        {
            this.stream.BeginRead(this.read_buffer, 0, 1, this.ReadCallback, null);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            if (this.disposed == true)
            {
                return;
            }

            int count = 0;
            try
            {
                count = this.stream.EndRead(ar);
            }
            catch
            {
            }

            if (this.disposed == true)
            {
                return;
            }

            if (count > 0)
            {
                object entity = null;
                bool isParser = true;
                bool isContinue = true;

                try
                {
                    entity = this.handler.Parse(this, this.read_buffer[0]);
                }
                catch (Exception exception)
                {
                    isParser = false;
                    //中断，禁止下一次进行错误的解析
                    isContinue = false;
                    //
                    this.OnError(new ParserException("parser entity failure, " + exception.Message, exception));
                }

                if (isParser == true)
                {
                    try
                    {
                        isContinue = this.OnMessage(entity);
                    }
                    catch (Exception exception)
                    {
                        isContinue = false;

                        this.OnError(new ParserException("process entity failure, " + exception.Message, exception));
                    }
                }

                if (this.disposed == true)
                {
                    //end
                }
                else if (isContinue == true)
                {
                    try
                    {
                        this.Read();
                    }
                    catch (IOException)
                    {
                        this.OnDisconnectioned();
                    }
                    catch (Exception exception)
                    {
                        this.OnError(exception);
                    }
                }
            }
            else
            {
                this.OnDisconnectioned();
            }
        }
        
        /// <summary>
        /// on error
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error = null;

        /// <summary>
        /// on disconnectioned
        /// </summary>
        public event EventHandler Disconnectioned = null;

        /// <summary>
        /// on new log
        /// </summary>
        public event EventHandler<LogEventArgs> Log = null;

        /// <summary>
        /// on new message
        /// </summary>
        public event EventHandler<MessageEventArgs> Message = null;

        /// <summary>
        /// on new disconnection
        /// </summary>
        protected virtual void OnDisconnectioned()
        {
            var fun = this.Disconnectioned;
            if (fun != null)
            {
                fun(this, EventArgs.Empty);
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
        /// on new log
        /// </summary>
        /// <param name="content"></param>
        protected virtual void OnLog(string content)
        {
            var fun = this.Log;
            if (fun != null)
            {
                var args = new LogEventArgs(content);
                fun(this, args);
            }
        }

        /// <summary>
        /// on new message
        /// </summary>
        /// <param name="message"></param>
        protected virtual bool OnMessage(object message)
        {
            var args = new MessageEventArgs(message);
            //
            var fun = this.Message;
            if (fun != null)
            {
                fun(this, args);
            }
            //
            return args.IsContinue;
        }

        /// <summary>
        /// write byte
        /// </summary>
        /// <param name="value"></param>
        public void WriteByte(byte value)
        {
            this.stream.WriteByte(value);
        }

        /// <summary>
        /// write content
        /// </summary>
        /// <param name="buffer"></param>
        public void Write(byte[] buffer)
        {
            this.stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// write content
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        public void Write(byte[] buffer, int offset, int length)
        {
            this.stream.Write(buffer, 0, length);
        }

        /// <summary>
        /// write utf8 content
        /// </summary>
        /// <param name="input"></param>
        public void WriteUTF8String(string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(input);

            this.Write(buffer);
        }

    }
}
