//
// redis-sharp.cs: ECMA CLI Binding to the Redis key-value storage system
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2010 Novell, Inc.
//
// Licensed under the same terms of reddis: new BSD license.
//
//#define DEBUG

using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace Adf
{
    /// <summary>
    /// Redis 客户端
    /// </summary>
    public class RedisConnection : IDisposable
    {
        /// <summary>
        /// CRLF
        /// </summary>
        public static byte[] CRLF = new byte[] { (byte)'\r', (byte)'\n' };

        Socket socket;
        RedisClient client;
        bool disposed;

        /// <summary>
        /// client
        /// </summary>
        /// <param name="client"></param>
        internal RedisConnection(RedisClient client)
        {
            this.client = client;
            this.disposed = false;
            this.Connect();
        }

        string ReadLine()
        {
            var sb = new StringBuilder();
            byte[] buffer = new byte[1];
            while (this.socket.Receive(buffer) != 0)
            {
                if (buffer[0] == '\r')
                    continue;
                if (buffer[0] == '\n')
                    break;
                sb.Append((char)buffer[0]);
            }
            return sb.ToString();
        }

        void Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.SendTimeout = this.client.SendTimeout;
            socket.Connect(this.client.Host, this.client.Port);
            if (!socket.Connected)
            {
                socket.Close();
                socket = null;
                return;
            }

            if (this.client.Password != null)
            {
                //this.SendCommand("AUTH {0}\r\n", this.client.Password);
                using (var w = new RedisWriter(this.client, 2, "AUTH"))
                {
                    w.WriteArgument(this.client.Password);
                    this.SendCommand(w);
                }
                var r = this.ExpectString();
                if (!"OK".Equals(r))
                {
                    throw new RedisResponseException(r);
                }
            }
        }

        //public void SendCommand(string cmd, params object[] args)
        //{
        //    var s = args.Length > 0 ? String.Format(cmd, args) : cmd;
        //    byte[] r = this.client.Encoding.GetBytes(s);
        //    try
        //    {
        //        this.socket.Send(r);
        //    }
        //    catch (SocketException)
        //    {
        //        // timeout;
        //        socket.Close();
        //        socket = null;

        //        throw new IOException("Unable to connect");
        //    }
        //}

        public void SendCommand(RedisWriter redisWriter)
        {
            var data = redisWriter.GetBuffer();
            var count = (int)redisWriter.Length;
            try
            {
                this.socket.Send(data, 0, count, SocketFlags.None);
            }
            catch (SocketException)
            {
                // timeout;
                socket.Close();
                socket = null;

                throw new IOException("Unable to connect");
            }
        }
        
        int ReceiveByte()
        {
            if (this.socket == null)
                return -1;

            byte[] buffer = new byte[1];
            if (this.socket.Receive(buffer, 0, 1, SocketFlags.None) == 0)
            {
                return -1;
            }
            return buffer[0];
        }

        public bool ExpectSuccess()
        {
            return ExpectString() == "OK";
        }

        public int ExpectInt()
        {
            int c = this.ReceiveByte();
            if (c == -1)
                throw new RedisResponseException("No more data");

            var s = ReadLine();
            //Log("R: " + s);
            if (c == '-')
                throw new RedisResponseException(s.StartsWith("ERR") ? s.Substring(4) : s);
            if (c == ':')
            {
                int num;
                int.TryParse(s, out num);
                return num;
            }

            //http://redis.io/topics/protocol#simple-string-reply
            //For Simple Strings the first byte of the reply is "+"
            //For Errors the first byte of the reply is "-"
            //For Integers the first byte of the reply is ":"
            //For Bulk Strings the first byte of the reply is "$"
            //For Arrays the first byte of the reply is "*"

            throw new RedisResponseException("Unknown reply on integer request: " + c + s);
        }

        //public double _ExpectDouble()
        //{
        //    double num;
        //    double.TryParse(this.ExpectString(), out num);
        //    return num;
        //}

        //public bool _ExpectSuccess()
        //{
        //    int c = this.ReceiveByte();
        //    if (c == -1)
        //        throw new RedisResponseException("No more data");

        //    var s = ReadLine();
        //    //Log("R: " + s);
        //    if (c == '-')
        //        throw new RedisResponseException(s.StartsWith("ERR") ? s.Substring(4) : s);
        //    if (c == '+')
        //    {
        //        return s == "OK";
        //    }

        //    //http://redis.io/topics/protocol#simple-string-reply
        //    //For Simple Strings the first byte of the reply is "+"
        //    //For Errors the first byte of the reply is "-"
        //    //For Integers the first byte of the reply is ":"
        //    //For Bulk Strings the first byte of the reply is "$"
        //    //For Arrays the first byte of the reply is "*"

        //    throw new RedisResponseException("Unknown reply on simple string request: " + c + s);
        //}

        public string ExpectString()
        {
            int c = this.ReceiveByte();
            if (c == -1)
                throw new RedisResponseException("No more data");

            var s = ReadLine();
            //Log("R: " + s);
            if (c == '-')
                throw new RedisResponseException(s.StartsWith("ERR") ? s.Substring(4) : s);
            if (c == '+')
                return s;

            //http://redis.io/topics/protocol#simple-string-reply
            //For Simple Strings the first byte of the reply is "+"
            //For Errors the first byte of the reply is "-"
            //For Integers the first byte of the reply is ":"
            //For Bulk Strings the first byte of the reply is "$"
            //For Arrays the first byte of the reply is "*"

            throw new RedisResponseException("Unknown reply on simple string request: " + c + s);
        }

        public int? ExpectIntOrNil()
        {            
            string r = ReadLine();
            //    Log("R: {0}", r);
            if (r.Length == 0)
                throw new RedisResponseException("Zero length respose");

            if (r == "$-1")
                return null;

            char c = r[0];
            if (c == '-')
                throw new RedisResponseException(r.StartsWith("-ERR") ? r.Substring(5) : r.Substring(1));

            //int
            if (c == ':')
            {
                return int.Parse(r.Substring(1));
            }

            throw new RedisResponseException("Unexpected reply: " + r);
        }

        public byte[] ReadBulkReply(string len)
        {
            int n;
            if (Int32.TryParse(len, out n))
            {
                byte[] retbuf = new byte[n];
                if (n > 0)
                {
                    int bytesRead = 0;
                    do
                    {
                        //int read = bstream.Read(retbuf, bytesRead, n - bytesRead);
                        int read = this.socket.Receive(retbuf, bytesRead, n - bytesRead, SocketFlags.None);
                        if (read < 1)
                            throw new RedisResponseException("Invalid termination mid stream");
                        bytesRead += read;
                    }
                    while (bytesRead < n);

                }
                if (this.ReceiveByte() != '\r' || this.ReceiveByte() != '\n')
                    throw new RedisResponseException("Invalid termination");

                return retbuf;
            }

            throw new RedisResponseException("Invalid length");
        }

        public byte[] ExpectBulkReply()
        {
            string r = ReadLine();
            //    Log("R: {0}", r);
            if (r.Length == 0)
                throw new RedisResponseException("Zero length respose");
                 
            char c = r[0];
            if (c == '-')
                throw new RedisResponseException(r.StartsWith("-ERR") ? r.Substring(5) : r.Substring(1));

            if (c == '$')
            {
                if (r == "$-1")
                    return null;

                return this.ReadBulkReply(r.Substring(1));
            }

            //returns the number of matches
            if (c == '*')
            {
                int n;
                if (Int32.TryParse(r.Substring(1), out n))
                    return n <= 0 ? new byte[0] : this.ExpectBulkReply();

                throw new RedisResponseException("Unexpected length parameter" + r);
            }

            throw new RedisResponseException("Unexpected reply: " + r);
        }

        public byte[][] ExpectMultiBulkReply()
        {
            int c = this.ReceiveByte();
            if (c == -1)
                throw new RedisResponseException("No more data");

            var s = ReadLine();
            //Log("R: " + s);
            if (c == '-')
                throw new RedisResponseException(s.StartsWith("ERR") ? s.Substring(4) : s);
            if (c == '*')
            {
                int count;
                if (int.TryParse(s, out count))
                {
                    var result = new byte[count][];

                    for (int i = 0; i < count; i++)
                        result[i] = this.ExpectBulkReply();

                    return result;
                }
            }
            throw new RedisResponseException("Unknown reply on multi-request: " + c + s);
        }

        public RedisSubscribeResult ExpectSubscribeResult()
        {
            var s = ReadLine();
            if (string.IsNullOrEmpty(s))
                throw new RedisResponseException("No more data");

            var c = s[0];
            if (c == '-')
                throw new RedisResponseException(s.StartsWith("ERR") ? s.Substring(4) : s);

            if (c != '*')
                throw new RedisResponseException("Unknown reply on subscribe: " + c + s);

            int len;
            int.TryParse(s.Substring(1), out len);
            if (len != 3)
            {
                throw new RedisResponseException("Unknown reply on subscribe: " + c + s);
            }

            byte[] buffer;

            buffer = this.ExpectBulkReply();
            var type = this.client.Encoding.GetString( buffer );
            
            buffer = this.ExpectBulkReply();
            var channel = this.client.Encoding.GetString( buffer );

            var result = new RedisSubscribeResult(type, channel);
            if (type.Equals("message"))
            {
                buffer = this.ExpectBulkReply();
                result.Message = this.client.Encoding.GetString( buffer );
            }
            else
            {
                result.SubscribeCount = this.ExpectInt();
            }
            return result;
        }

        public string[] ExpectToStringArray(byte[][] data)
        {
            if (data == null)
                return null;

            var r = new string[data.Length];
            for (int i = 0, l = data.Length; i < l; i++)
            {
                r[i] = this.client.Encoding.GetString(data[i]);
            }
            return r;
        }

        public string[] ExpectToStringArray()
        {
            var data = this.ExpectMultiBulkReply();
            if (data == null)
                return null;
            return this.ExpectToStringArray(data);
        }

        public string ExpectToString()
        {
            var data = this.ExpectBulkReply();
            if (data == null)
                return null;
            return this.client.Encoding.GetString(data);
        }

        public double _ExpectToDouble()
        {
            double num;
            double.TryParse(this.ExpectToString(), out num);
            return num;
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                //
                //this.SendCommand(null, "QUIT\r\n");
                using (var w = new RedisWriter(this.client, 1, "QUIT"))
                {
                    this.SendCommand(w);
                }

                socket.Close();
                socket = null;
            }
        }
    }
}