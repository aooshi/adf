using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;

namespace Adf
{
    /// <summary>
    /// Supports setting, adding, replacing, deleting compressed/uncompressed and
    /// serialized (can be stored as string if object is native class) objects to memcached.
    /// </summary>
    /// <example>
    /// //***To create cache client object and set params:***
    /// Memcache mc = new Memcache();
    /// 
    /// // compression is enabled by default	
    /// mc.setCompressEnable(true);
    ///
    ///	// set compression threshhold to 4 KB (default: 15 KB)	
    ///	mc.setCompressThreshold(4096);
    /// 
    /// 
    /// //***To store an object:***
    /// Memcache mc = new Memcache();
    /// string key   = "cacheKey1";	
    /// object value = SomeClass.getObject();	
    /// mc.set(key, value);
    /// 
    /// 
    /// //***To store an object using a custom server hashCode:***
    /// //The set method shown here will always set the object in the cache.
    /// //The add and replace methods do the same, but with a slight difference.
    /// //  add -- will store the object only if the server does not have an entry for this key
    /// //  replace -- will store the object only if the server already has an entry for this key
    ///	Memcache mc = new Memcache();
    ///	string key   = "cacheKey1";	
    ///	object value = SomeClass.getObject();	
    ///	int hash = 45;
    ///	mc.set(key, value, hash);
    /// 
    /// 
    /// //***To delete a cache entry:***
    /// Memcache mc = new Memcache();
    /// string key   = "cacheKey1";	
    /// mc.delete(key);
    /// 
    /// 
    /// //***To delete a cache entry using a custom hash code:***
    /// Memcache mc = new Memcache();
    /// string key   = "cacheKey1";	
    /// int hash = 45;
    /// mc.delete(key, hashCode);
    /// 
    /// 
    /// //***To store a counter and then increment or decrement that counter:***
    /// Memcache mc = new Memcache();
    /// string key   = "counterKey";	
    /// mc.storeCounter(key, 100);
    /// Console.WriteLine("counter after adding      1: " mc.incr(key));	
    /// Console.WriteLine("counter after adding      5: " mc.incr(key, 5));	
    /// Console.WriteLine("counter after subtracting 4: " mc.decr(key, 4));	
    /// Console.WriteLine("counter after subtracting 1: " mc.decr(key));	
    /// 
    /// 
    /// //***To store a counter and then increment or decrement that counter with custom hash:***
    /// Memcache mc = new Memcache();
    /// string key   = "counterKey";	
    /// int hash = 45;	
    /// mc.storeCounter(key, 100, hash);
    /// Console.WriteLine("counter after adding      1: " mc.incr(key, 1, hash));	
    /// Console.WriteLine("counter after adding      5: " mc.incr(key, 5, hash));	
    /// Console.WriteLine("counter after subtracting 4: " mc.decr(key, 4, hash));	
    /// Console.WriteLine("counter after subtracting 1: " mc.decr(key, 1, hash));	
    /// 
    /// 
    /// //***To retrieve an object from the cache:***
    /// Memcache mc = new Memcache();
    /// string key   = "key";	
    /// object value = mc.get(key);	
    ///
    ///
    /// //***To retrieve an object from the cache with custom hash:***
    /// Memcache mc = new Memcache();
    /// string key   = "key";	
    /// int hash = 45;	
    /// object value = mc.get(key, hash);
    /// 
    /// 
    /// //***To retrieve an multiple objects from the cache***
    /// Memcache mc = new Memcache();
    /// string[] keys   = { "key", "key1", "key2" };
    /// object value = mc.getMulti(keys);
    /// 
    ///
    /// //***To retrieve an multiple objects from the cache with custom hashing***
    /// Memcache mc = new Memcache();
    /// string[] keys    = { "key", "key1", "key2" };
    /// int[] hashes = { 45, 32, 44 };
    /// object value = mc.getMulti(keys, hashes);
    /// 
    ///
    /// //***To flush all items in server(s)***
    /// Memcache mc = new Memcache();
    /// mc.FlushAll();
    /// 
    ///
    /// //***To get stats from server(s)***
    /// Memcache mc = new Memcache();
    /// Hashtable stats = mc.stats();
    /// </example>
    public class Memcache : ICache, IPoolInstance, IDisposable
    {
        // return codes
        private const string VALUE = "VALUE"; // start of value line from server
        private const string STATS = "STAT"; // start of stats line from server
        private const string DELETED = "DELETED"; // successful deletion
        private const string TOUCHED = "TOUCHED"; // successful TOUCHED
        private const string NOTFOUND = "NOT_FOUND"; // record not found for delete or incr/decr
        private const string STORED = "STORED"; // successful store of data
        private const string NOTSTORED = "NOT_STORED"; // data not stored
        private const string OK = "OK"; // success
        private const string END = "END"; // end of data from server
        private const string ERROR = "ERROR"; // invalid command name from client
        //private const string CLIENT_ERROR = "CLIENT_ERROR"; // client error in input line - invalid protocol
        // private const string SERVER_ERROR = "SERVER_ERROR";	// server error

        // default compression threshold
        private const int COMPRESS_THRESH = 30720;

        // values for cache flags 
        //
        // using 8 (1 << 3) so other clients don't try to unpickle/unstore/whatever
        // things that are serialized... I don't think they'd like it. :)
        private const int F_COMPRESSED = 2;
        private const int F_SERIALIZED = 1;
        private const int F_BINARY = 4;


        //MemcacheNativeHandler nativeHandler;

        /// <summary>
        /// NUM
        /// </summary>
        static readonly Regex NUMREGEX = new Regex("\\d+", RegexOptions.Compiled);

        /// <summary>
        /// 主机
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// 发送超时
        /// </summary>
        public int SendTimeout { get; set; }

        /// <summary>
        /// 获取或设置读取超时
        /// </summary>
        public int ReadTimeout
        {
            get { return this.stream.ReadTimeout; }
            set { this.stream.ReadTimeout = value; }
        }

        /// <summary>
        /// 获取或设置二进制序列化器,默认 JsonBinarySerializable, 可通过在appsetting中配置 MemcacheBinarySerializable 来指定其它实例
        /// </summary>
        public IBinarySerializable BinarySerializable { get; set; }

        Socket socket;
        NetworkStream stream;


        /// <summary>
        /// 初始化新实例
        /// </summary>
        public Memcache(string host, int port)
            : this(host, port, Encoding.UTF8)
        {
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        public Memcache(string host, int port, Encoding encoding)
        {
            this.Host = host;
            this.Port = port;
            this.SendTimeout = -1;

            this.enableCompression = true;
            this.compressionThreshold = COMPRESS_THRESH;
            this.encoding = encoding;

            //this.nativeHandler = new MemcacheNativeHandler(encoding);

            var memcacheBinarySerializable = Adf.ConfigHelper.GetSetting("MemcacheBinarySerializable");
            if (string.IsNullOrEmpty(memcacheBinarySerializable))
                //this.BinarySerializable = new JsonBinarySerializable(encoding);
                this.BinarySerializable = new BinarySerializable();
            else
                this.BinarySerializable = (IBinarySerializable)Activator.CreateInstance(Type.GetType(memcacheBinarySerializable));

            this.PoolAbandon = false;

            this.Connect();
        }

        void Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.SendTimeout = SendTimeout;
            socket.Connect(Host, Port);
            if (!socket.Connected)
            {
                socket.Close();
                socket = null;
            }
            else
            {
                stream = new NetworkStream(socket, false);
            }
        }

        Encoding encoding;
        /// <summary>
        /// Sets default string encoding when storing primitives as strings. 
        /// Default is UTF-8.
        /// </summary>
        public Encoding Encoding
        {
            get { return this.encoding; }
        }

        bool enableCompression;
        /// <summary>
        /// Enable storing compressed data, provided it meets the threshold requirements.
        /// 
        /// If enabled, data will be stored in compressed form if it is
        /// longer than the threshold length set with setCompressThreshold(int)
        /// 
        /// The default is that compression is enabled.
        /// 
        /// Even if compression is disabled, compressed data will be automatically
        /// decompressed.
        /// </summary>
        /// <value><c>true</c> to enable compuression, <c>false</c> to disable compression</value>
        public bool EnableCompression
        {
            get { return this.enableCompression; }
            set { this.enableCompression = value; }
        }

        long compressionThreshold = 0;
        /// <summary>
        /// Sets the required length for data to be considered for compression.
        /// 
        /// If the length of the data to be stored is not equal or larger than this value, it will
        /// not be compressed.
        /// 
        /// This defaults to 15 KB.
        /// </summary>
        /// <value>required length of data to consider compression</value>
        public long CompressionThreshold
        {
            get { return this.compressionThreshold; }
            set { this.compressionThreshold = value; }
        }

        /// <summary>
        /// Gets whether or not the socket is connected.  Returns <c>true</c> if it is.
        /// </summary>
        public bool IsConnected
        {
            get { return this.socket != null && this.socket.Connected; }
        }

        /// <summary>
        /// reads a line
        /// intentionally not using the deprecated readLine method from DataInputStream 
        /// </summary>
        /// <returns>String that was read in</returns>
        private string ReadLine()
        {
            //if (this.socket == null || this.socket.Connected == false)
            //{
            //    throw new IOException("connection closed");
            //}

            return Adf.StreamHelper.ReadLine(this.stream, this.encoding);

            //byte[] b = new byte[1];
            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //    bool eol = false;

            //    while (this.stream.Read(b, 0, 1) != -1)
            //    {

            //        if (b[0] == 13)
            //        {
            //            eol = true;

            //        }
            //        else
            //        {
            //            if (eol)
            //            {
            //                if (b[0] == 10)
            //                    break;

            //                eol = false;
            //            }
            //        }

            //        // cast byte into char array
            //        memoryStream.Write(b, 0, 1);
            //    }

            //    if (memoryStream == null || memoryStream.Length <= 0)
            //    {
            //        throw new IOException("closing dead stream");
            //    }

            //    // else return the string
            //    //string temp = this.Encoding.GetString(memoryStream.ToArray()).TrimEnd('\0', '\r', '\n');
            //    string temp = this.Encoding.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length).TrimEnd('\0', '\r', '\n');
            //    return temp;
            //}
        }

        ///// <summary>
        ///// reads up to end of line and returns nothing 
        ///// </summary>
        //private void ClearEndOfLine()
        //{
        //    if (this.socket == null || !this.socket.Connected)
        //    {
        //        throw new IOException("socket closed");
        //    }

        //    byte[] b = new byte[1];
        //    bool eol = false;
        //    while (this.stream.Read(b, 0, 1) != -1)
        //    {

        //        // only stop when we see
        //        // \r (13) followed by \n (10)
        //        if (b[0] == 13)
        //        {
        //            eol = true;
        //            continue;
        //        }

        //        if (eol)
        //        {
        //            if (b[0] == 10)
        //                break;

        //            eol = false;
        //        }
        //    }
        //}

        ///// <summary>
        ///// reads length bytes into the passed in byte array from stream
        ///// </summary>
        ///// <param name="bytes">byte array</param>
        //private void Read(byte[] bytes)
        //{
        //    if (this.socket == null || !this.socket.Connected)
        //    {
        //        throw new IOException("socket closed");
        //    }

        //    if (bytes == null)
        //        return;

        //    int count = 0;
        //    while (count < bytes.Length)
        //    {
        //        int cnt = this.stream.Read(bytes, count, (bytes.Length - count));
        //        count += cnt;
        //    }
        //}

        /// <summary>
        /// flushes output stream 
        /// </summary>
        private void Flush()
        {
            //if (this.socket == null || this.socket.Connected == false)
            //{
            //    throw new IOException("socket closed");
            //}
            this.stream.Flush();
        }

        /// <summary>
        /// writes a byte array to the output stream
        /// </summary>
        /// <param name="bytes">byte array to write</param>
        private void Write(byte[] bytes)
        {
            Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// writes a byte array to the output stream
        /// </summary>
        /// <param name="bytes">byte array to write</param>
        /// <param name="offset">offset to begin writing from</param>
        /// <param name="count">count of bytes to write</param>
        private void Write(byte[] bytes, int offset, int count)
        {
            //if (this.socket == null || !this.socket.Connected)
            //{
            //    throw new IOException("socket closed");
            //}

            if (this.stream == null)
            {
                throw new IOException("connection not open.");
            }

            if (bytes != null)
            {
                this.stream.Write(bytes, offset, count);
            }
        }

        /// <summary>
        /// Deletes an object from cache given cache key.
        /// </summary>
        /// <param name="key">the key to be removed</param>
        /// <returns><c>true</c>, if the data was deleted successfully</returns>
        public bool Delete(string key)
        {
            return Delete(key, 0);
        }

        /// <summary>
        /// Deletes an object from cache given cache key, a delete time, and an optional hashcode.
        /// 
        /// The item is immediately made non retrievable.<br/>
        /// Keep in mind: 
        /// <see cref="Add(string,object)">add(string, object)</see> and <see cref="Replace(string,object)">replace(string, object)</see>
        ///	will fail when used with the same key will fail, until the server reaches the
        ///	specified time. However, <see cref="Set(string,object)">set(string, object)</see> will succeed
        /// and the new value will not be deleted.
        /// </summary>
        /// <param name="key">the key to be removed</param>
        /// <param name="timeout">seconds or unix-stamp</param>
        /// <returns><c>true</c>, if the data was deleted successfully</returns>
        public bool Delete(string key, long timeout)
        {
            if (key == null)
            {
                return false;
            }

            // build command
            StringBuilder command = new StringBuilder("delete ").Append(key);
            if (timeout > 0)
                command.Append(" " + timeout);
            command.Append("\r\n");

            //try
            //{
            this.Write(this.encoding.GetBytes(command.ToString()));
            this.Flush();

            //read
            string line = this.ReadLine();
            if (DELETED == line)
            {
                return true;
            }
            else if (NOTFOUND == line)
            {
                //delete key not found
            }
            else
            {
                //delete key error
            }
            //}
            //catch (IOException)
            //{
            //}

            return false;
        }

        /// <summary>
        /// set expires for a key
        /// </summary>
        /// <param name="key">the key to be removed</param>
        /// <param name="expires">seconds or unix-stamp</param>
        /// <returns><c>true</c>, if the data was touch successfully</returns>
        public bool Touch(string key, long expires)
        {
            if (key == null)
            {
                return false;
            }

            //build command
            //touch <key> <exptime> [noreply]\r\n
            StringBuilder command = new StringBuilder("touch ").Append(key).Append(" " + expires).Append("\r\n");

            //try
            //{
            this.Write(this.encoding.GetBytes(command.ToString()));
            this.Flush();

            //read
            string line = this.ReadLine();
            if (TOUCHED == line)
            {
                return true;
            }
            else if (NOTFOUND == line)
            {
                //delete key not found
            }
            else
            {
                //delete key error
            }
            //}
            //catch (IOException)
            //{
            //}

            return false;
        }

        /// <summary>
        /// Stores data on the server; only the key and the value are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Set(string key, object value)
        {
            return Set("set", key, value, 0);
        }

        /// <summary>
        /// Stores data on the server; the key, value, and an expiration time are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <param name="expires">when to expire the record</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Set(string key, object value, long expires)
        {
            return Set("set", key, value, expires);
        }

        /// <summary>
        /// Adds data to the server; only the key and the value are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Add(string key, object value)
        {
            return Set("add", key, value, 0);
        }

        /// <summary>
        /// Adds data to the server; the key, value, and an expiration time are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <param name="expires">when to expire the record</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Add(string key, object value, long expires)
        {
            return Set("add", key, value, expires);
        }

        /// <summary>
        /// Updates data on the server; only the key and the value are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Replace(string key, object value)
        {
            return Set("replace", key, value, 0);
        }

        /// <summary>
        /// Updates data on the server; the key, value, and an expiration time are specified.
        /// </summary>
        /// <param name="key">key to store data under</param>
        /// <param name="value">value to store</param>
        /// <param name="expires">when to expire the record</param>
        /// <returns>true, if the data was successfully stored</returns>
        public bool Replace(string key, object value, long expires)
        {
            return Set("replace", key, value, expires);
        }

        /// <summary>
        /// Stores data to cache.
        /// 
        /// If data does not already exist for this key on the server, or if the key is being
        /// deleted, the specified value will not be stored.
        /// The server will automatically delete the value when the expiration time has been reached.
        /// 
        /// If compression is enabled, and the data is longer than the compression threshold
        /// the data will be stored in compressed form.
        /// 
        /// As of the current release, all objects stored will use .NET serialization.
        /// </summary>
        /// <param name="cmdname">action to take (set, add, replace)</param>
        /// <param name="key">key to store cache under</param>
        /// <param name="obj">object to cache</param>
        /// <param name="expires">expiration</param>
        /// <returns>true/false indicating success</returns>
        private bool Set(string cmdname, string key, object obj, long expires)
        {
            if (cmdname == null || cmdname.Trim().Length == 0 || key == null || key.Length == 0)
            {
                return false;
            }

            if (expires < 0)
                return false;

            // store flags
            int flags = 0;

            // byte array to hold data
            byte[] val;
            int length = 0;

            if (obj == null)
            {
                val = new byte[0];
                length = 0;
            }
            else if (obj is byte[])
            {
                val = (byte[])obj;
                length = val.Length;
                flags |= F_BINARY;
            }
            else if (obj is string
                || obj is UInt16
                || obj is UInt32
                || obj is UInt64
                || obj is Int16
                || obj is Int32
                || obj is Int64
                || obj is byte)
            {
                //set store data as string
                val = this.encoding.GetBytes(obj.ToString());
                length = val.Length;
            }
            else
            {
                //try
                //{
                val = this.BinarySerializable.Serialize(obj);
                length = val.Length;
                flags |= F_SERIALIZED;
                //}
                //catch (Exception)
                //{
                //    return false;
                //}
            }

            // now try to compress if we want to
            // and if the length is over the threshold 
            if (this.enableCompression && length > this.compressionThreshold)
            {
                //set trying to compress data
                //set size prior

                //try
                //{
                // store it and set compression flag
                val = Adf.CompressHelper.Compress(val);
                length = val.Length;
                flags |= F_COMPRESSED;
                //}
                //catch (Exception)
                //{
                //    //set compression failure
                //}
            }

            //if (this.socket == null || !this.socket.Connected)
            //{
            //    throw new IOException("socket closed");
            //    //return false;
            //}

            // now write the data to the cache server
            string cmd = new StringBuilder(cmdname).Append(" ").Append(key).Append(" ").Append(flags).Append(" ")
                .Append(expires).Append(" ").Append(length).Append("\r\n").ToString();
            //try
            //{
            this.Write(this.encoding.GetBytes(cmd));
            this.Write(val, 0, length);
            this.Write(this.encoding.GetBytes("\r\n"));
            this.Flush();

            // get result code
            string line = this.ReadLine();
            if (STORED == line)
            {
                //success
                return true;
            }
            else if (NOTSTORED == line)
            {
                //not stored
            }
            else
            {
                //error
            }
            //}
            //catch (IOException)
            //{
            //}
            return false;
        }

        /// <summary>
        /// Increment the value at the specified key by 1, and then return it.
        /// </summary>
        /// <param name="key">key where the data is stored</param>
        /// <returns>-1, if the key is not found, the value after incrementing otherwise</returns>
        public long Increment(string key)
        {
            return IncrementOrDecrement("incr", key, 1);
        }

        /// <summary>
        /// Increment the value at the specified key by passed in val. 
        /// </summary>
        /// <param name="key">key where the data is stored</param>
        /// <param name="inc">how much to increment by</param>
        /// <returns>-1, if the key is not found, the value after incrementing otherwise</returns>
        public long Increment(string key, long inc)
        {
            return IncrementOrDecrement("incr", key, inc);
        }

        /// <summary>
        /// Decrement the value at the specified key by 1, and then return it.
        /// </summary>
        /// <param name="key">key where the data is stored</param>
        /// <returns>-1, if the key is not found, the value after incrementing otherwise</returns>
        public long Decrement(string key)
        {
            return IncrementOrDecrement("decr", key, 1);
        }

        /// <summary>
        /// Decrement the value at the specified key by passed in value, and then return it.
        /// </summary>
        /// <param name="key">key where the data is stored</param>
        /// <param name="inc">how much to increment by</param>
        /// <returns>-1, if the key is not found, the value after incrementing otherwise</returns>
        public long Decrement(string key, long inc)
        {
            return IncrementOrDecrement("decr", key, inc);
        }

        /// <summary>
        /// Increments/decrements the value at the specified key by inc.
        /// 
        /// Note that the server uses a 32-bit unsigned integer, and checks for
        /// underflow. In the event of underflow, the result will be zero.  Because
        /// Java lacks unsigned types, the value is returned as a 64-bit integer.
        /// The server will only decrement a value if it already exists;
        /// if a value is not found, -1 will be returned.
        /// 
        /// TODO: C# has unsigned types.  We can fix this.
        /// </summary>
        /// <param name="cmdname">increment/decrement</param>
        /// <param name="key">cache key</param>
        /// <param name="inc">amount to incr or decr</param>
        /// <returns>new value or -1 if not exist</returns>
        private long IncrementOrDecrement(string cmdname, string key, long inc)
        {

            //if (this.socket == null || !this.socket.Connected)
            //{
            //    throw new IOException("socket closed");
            //    //return false;
            //}


            //try
            //{
            string cmd = new StringBuilder().Append(cmdname).Append(" ").Append(key).Append(" ").Append(inc).Append("\r\n").ToString();

            this.Write(this.encoding.GetBytes(cmd));
            this.Flush();

            // get result back
            string line = this.ReadLine();
            if (NUMREGEX.Match(line).Success)
            {
                return (long)ulong.Parse(line);
            }
            else if (NOTFOUND == line)
            {
                //key not found
            }
            else
            {
                //error
            }
            //}
            //catch (IOException)
            //{
            //}

            return -1;
        }

        /// <summary>
        /// Retrieve a key from the server, using a specific hash.
        /// 
        /// If the data was compressed or serialized when compressed, it will automatically
        /// be decompressed or serialized, as appropriate. (Inclusive or)
        /// 
        /// Non-serialized data will be returned as a string, so explicit conversion to
        /// numeric types will be necessary, if desired
        /// </summary>
        /// <param name="key">key where data is stored</param>
        /// <returns>the object that was previously stored, or null if it was not previously stored</returns>
        public string Get(string key)
        {
            return this.Get<string>(key);
        }


        /// <summary>
        /// Retrieve a key from the server, using a specific hash.
        /// 
        /// If the data was compressed or serialized when compressed, it will automatically
        /// be decompressed or serialized, as appropriate. (Inclusive or)
        /// 
        /// Non-serialized data will be returned as a string, so explicit conversion to
        /// numeric types will be necessary, if desired
        /// </summary>
        /// <param name="key">key where data is stored</param>
        /// <returns>the object that was previously stored, or null if it was not previously stored</returns>
        public T Get<T>(string key)
        {
            return (T)this.Get(key, typeof(T));
        }

        /// <summary>
        /// Retrieve a key from the server, using a specific hash.
        /// 
        /// If the data was compressed or serialized when compressed, it will automatically
        /// be decompressed or serialized, as appropriate. (Inclusive or)
        /// 
        /// Non-serialized data will be returned as a string, so explicit conversion to
        /// numeric types will be necessary, if desired
        /// </summary>
        /// <param name="key">key where data is stored</param>
        /// <param name="type"></param>
        /// <returns>the object that was previously stored, or null if it was not previously stored</returns>
        public object Get(string key, Type type)
        {
            //if (this.socket == null || !this.socket.Connected)
            //{
            //    throw new IOException("socket closed");
            //    //return false;
            //}

            //try
            //{
            string cmd = string.Concat("get ", key, "\r\n");

            this.Write(this.encoding.GetBytes(cmd));
            this.Flush();

            return this.LoadItem(key, type);
            //}
            //catch (IOException)
            //{
            //}

            //return null;
        }

        /// <summary>
        /// get item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        private object LoadItem(string key, Type type)
        {
            object value = null;
            //
            while (true)
            {
                string line = this.ReadLine();
                //
                if (line.StartsWith(VALUE))
                {
                    string key2;
                    object value2;
                    this.ParseValue(line, type, out key2, out value2);
                    if (key == key2)
                    {
                        value = value2;
                    }
                }
                else if (END == line)
                {
                    //finished
                    break;
                }
            }
            //
            return value;
        }

        ///// <summary>
        ///// This method loads the data from cache into a Hashtable.
        ///// </summary>
        ///// <param name="hm">hashmap to store data into</param>
        ///// <param name="type"></param>
        //private void LoadItems(Hashtable hm, Type type)
        //{
        //    while (true)
        //    {
        //        string line = this.ReadLine();

        //        if (line.StartsWith(VALUE))
        //        {
        //            String key = "";
        //            Object value = null;

        //            this.ParseValue(line, type, out key, out value);
        //            // store the object into the cache
        //            hm[key] = value;
        //        }
        //        else if (END == line)
        //        {
        //            //finished
        //            break;
        //        }
        //    }
        //}

        private void ParseValue(string line, Type type, out string key, out object value)
        {
            string[] info = line.Split(' ');
            key = info[1];
            int flag = int.Parse(info[2], NumberFormatInfo.InvariantInfo);
            int length = int.Parse(info[3], NumberFormatInfo.InvariantInfo);

            // read obj into buffer
            byte[] buf = StreamHelper.Receive(this.stream, length);
            //clear end \r\n
            var clearCRLFofEnd = new byte[128];
            var clearPosition = 0;
            Adf.StreamHelper.ReadLine(this.stream, clearCRLFofEnd, ref clearPosition);

            // check for compression
            if ((flag & F_COMPRESSED) == F_COMPRESSED)
            {
                try
                {
                    buf = Adf.CompressHelper.Decompress(buf);
                }
                catch (Exception e)
                {
                    throw new IOException("uncompression Exception " + e.Message);
                }
            }

            // we can only take out serialized objects
            if ((flag & F_SERIALIZED) == F_SERIALIZED)
            {
                try
                {
                    value = this.BinarySerializable.Deserialize(type, buf);
                }
                catch (Exception e)
                {
                    throw new IOException("SerializationException " + e.Message);
                }
            }
            else if ((flag & F_BINARY) == F_BINARY)
            {
                value = buf;
            }
            else
            {
                value = this.Encoding.GetString(buf);
            }
        }

        /// <summary>
        /// Invalidates the entire cache.
        /// 
        /// Will return true only if succeeds in clearing all servers.
        /// </summary>
        /// <returns>success true/false</returns>
        public bool FlushAll()
        {
            //if (this.socket == null || !this.socket.Connected)
            //{
            //    throw new IOException("socket closed");
            //    //return false;
            //}

            bool success = true;

            // build command
            string command = "flush_all\r\n";

            //try
            //{
            this.Write(this.encoding.GetBytes(command));
            this.Flush();

            // if we get appropriate response back, then we return true
            string line = this.ReadLine();
            success = (OK == line)
                ? success && true
                : false;
            //}
            //catch (IOException)
            //{
            //    success = false;
            //}

            return success;
        }

        /// <summary>
        /// Retrieves stats for passed in servers (or all servers).
        /// 
        /// Returns a map keyed on the servername.
        /// The value is another map which contains stats
        /// with stat name as key and value as value.
        /// </summary>
        /// <returns>Stats map</returns>
        public Hashtable Stats()
        {
            //if (this.socket == null || !this.socket.Connected)
            //{
            //    throw new IOException("socket closed");
            //    //return false;
            //}

            // build command
            string command = "stats\r\n";

            // map to hold key value pairs
            Hashtable stats = new Hashtable();

            //try
            //{
            this.Write(this.encoding.GetBytes(command));
            this.Flush();

            // loop over results
            while (true)
            {
                string line = this.ReadLine();
                if (line.StartsWith(STATS))
                {
                    string[] info = line.Split(' ');
                    string key = info[1];
                    string val = info[2];

                    stats[key] = val;
                }
                else if (END == line)
                {
                    //stats finished
                    break;
                }
            }
            //}
            //catch
            //{
            //}

            return stats;
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            if (this.stream != null)
            {
                this.stream.Close();
                this.stream = null;
            }

            if (this.socket != null)
            {
                this.socket.Close();
                this.socket = null;
            }
        }

        #region ICACHE

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expires"></param>
        public void Set(string key, object value, int expires)
        {
            this.Set(key, value, (long)expires);
        }

        void ICache.Delete(string key)
        {
            this.Delete(key);
        }

        #endregion

        #region IPoolInstance
        /// <summary>
        /// 获取或设置是否放弃此实例
        /// </summary>
        public bool PoolAbandon
        {
            get;
            set;
        }
        #endregion
    }
}