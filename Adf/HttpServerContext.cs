using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace Adf
{
    /// <summary>
    /// Http Server Callback
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public delegate HttpStatusCode HttpServerCallback(HttpServerContext context);

    /// <summary>
    /// Http Server Context
    /// </summary>
    public class HttpServerContext : HttpServerContextBase
    {
        internal ManualResetEvent chunkWriteEvent = null;
        GZipStream chunkGzipStream = null;
        MemoryStream chunkStream = null;
        bool isRequestHead = false;

        int maxRequestContentLength;
        /// <summary>
        /// 获取或设置当前请求允许的最大内容体长度，默认 <see cref="HttpServer.MaxRequestContentLength"/>.
        /// 实际判断时的误差值为<see cref="HttpServer.BufferSize"/>
        /// </summary>
        public int MaxRequestContentLength
        {
            get { return this.maxRequestContentLength; }
            set { this.maxRequestContentLength = value; }
        }

        /// <summary>
        /// 获取或设置当前连接的内容已上传的长度，此值不表示整个请求体，仅在请求为POST且模式为multipart/form-data时描述具体内容已上传的长度，此属性与Content-Length配合可用于上传进度的查询
        /// </summary>
        public int UploadedLength
        {
            get;
            set;
        }

        Socket socket;

        /// <summary>
        /// 当前连接对象
        /// </summary>
        public Socket Socket
        {
            get { return this.socket; }
        }

        bool keepAlive = false;
        /// <summary>
        /// 获取或设备是否保持连接
        /// </summary>
        /// <exception cref="NotSupportedException">post method not support Keep-Alive</exception>
        public bool KeepAlive
        {
            get { return this.keepAlive; }
            set
            {
                if ("POST".Equals(this.Method, StringComparison.OrdinalIgnoreCase) && value == true)
                {
                    throw new NotSupportedException("post method not support Keep-Alive");
                }
                this.keepAlive = value;
            }
        }

        NameValueCollection form;
        /// <summary>
        /// POST Parameters
        /// </summary>
        public NameValueCollection Form
        {
            get
            {
                if (this.form == null)
                {
                    this.form = new HttpQueryCollection(5);
                }
                return this.form;
            }
        }
        
        List<HttpServerFileParameter> files = null;
        /// <summary>
        /// get file list
        /// </summary>
        public List<HttpServerFileParameter> Files
        {
            get
            {
                if (this.files == null)
                {
                    this.files = new List<HttpServerFileParameter>();
                }
                return this.files;
            }
        }

        /// <summary>
        /// get or set file handler
        /// </summary>
        public IHttpServerFileHandler FileHandler
        {
            get;
            set;
        }

        HttpStatusCode status = HttpStatusCode.OK;
        /// <summary>
        /// get or set http status
        /// </summary>
        public HttpStatusCode Status
        {
            get { return this.status; }
            set { this.status = value; }
        }

        string content;
        /// <summary>
        /// Result
        /// </summary>
        public string Content
        {
            get { return this.content; }
            set { this.content = value; }
        }

        /// <summary>
        /// post data for request content-type no application/x-www-form-urlencoded and multipart/form-data
        /// </summary>
        public byte[] PostData
        {
            get;
            set;
        }

        byte[] contentBuffer;
        /// <summary>
        /// Result, override content
        /// </summary>
        public byte[] ContentBuffer
        {
            get { return this.contentBuffer; }
            set { this.contentBuffer = value; }
        }

        HttpServerChunkStatus chunkWriteStatus;
        /// <summary>
        /// 获取当前是否以区块输出
        /// </summary>
        public HttpServerChunkStatus ChunkWriteStatus
        {
            get { return this.chunkWriteStatus; }
        }

        bool acceptGzip = false;

        int gzipThreshold = 512;
        /// <summary>
        /// get or set gzip enable threshold, default 512, chunked mode no limit, zero to disabled
        /// </summary>
        public int GzipThreshold
        {
            get { return this.gzipThreshold; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "value not less than zero");
                } this.gzipThreshold = value;
            }
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
        /// <param name="maxRequestContentLength"></param>
        /// <param name="fileHandler"></param>
        internal HttpServerContext(Socket socket, NameValueCollection header, HttpServer server, string url, string method, string protocol, int maxRequestContentLength, IHttpServerFileHandler fileHandler)
            : base(header, server, url, method, protocol)
        {
            var version = 0d;
            try { double.TryParse(protocol.Split('/')[1], out version); }
            catch { }

            //Connection: keep-alive
            var keepAlive = header["Connection"] ?? string.Empty;
            if ("POST".Equals(method, StringComparison.OrdinalIgnoreCase))
            {
                this.keepAlive = false;
            }
            else if ("keep-alive".Equals(keepAlive, StringComparison.OrdinalIgnoreCase))
            {
                this.keepAlive = true;
            }
            else if ("close".Equals(keepAlive, StringComparison.OrdinalIgnoreCase))
            {
                this.keepAlive = false;
            }
            else if (version > 1.0)
            {
                this.keepAlive = true;
            }

            //this.ResponseHeader.Add("Date", DateTime.Now.ToString("r"));
            //this.ResponseHeader.Add("Connection", this.KeepAlive ? "keep-alive" : "Close");
            this.ResponseHeader.Add("Content-Type", "text/html");
            this.socket = socket;
            this.contentBuffer = null;
            this.chunkWriteStatus = HttpServerChunkStatus.NoBegin;
            base.RequestType = HttpServerRequestType.Http;
            this.maxRequestContentLength = maxRequestContentLength;
            this.FileHandler = fileHandler;
            this.isRequestHead = method.Equals("HEAD", StringComparison.OrdinalIgnoreCase);
            this.acceptGzip = this.isRequestHead == false && (header["Accept-Encoding"] ?? "").IndexOf("gzip", StringComparison.OrdinalIgnoreCase) != -1;

            //
            var authorization = header["Authorization"];
            if (string.IsNullOrEmpty(authorization) == false && authorization.StartsWith("Basic ") && authorization.Length > 6)
            {
                this.ParseAuthorize(authorization);
            }
        }

        private void ParseAuthorize(string authorization)
        {
            var auth = authorization.Substring(6);
            try
            {
                auth = this.Encoding.GetString(Convert.FromBase64String(auth));
            }
            catch (Exception)
            {
                return;
            }
            var auths = auth.Split(':');
            if (auths.Length == 2)
            {
                base.UserName = auths[0];
                base.Password = auths[1];
            }
        }

        /// <summary>
        /// Begin Async And Chunk Write, You Need Invoke EndWrite
        /// </summary>
        /// <exception cref="InvalidOperationException">Has been invoked</exception>
        public void BeginWrite()
        {
            if (this.chunkWriteEvent != null)
            {
                throw new InvalidOperationException("Has been invoked");
            }
            this.chunkWriteEvent = new ManualResetEvent(false);
            this.chunkWriteStatus = HttpServerChunkStatus.Writing;
            if (this.acceptGzip && this.gzipThreshold != 0)
            {
                this.chunkStream = new MemoryStream();
                this.chunkGzipStream = new GZipStream(this.chunkStream, CompressionMode.Compress, true);
            }
            this.Response(HttpStatusCode.OK);
        }

        /// <summary>
        /// Write Chunk Content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args"></param>
        /// <exception cref="InvalidOperationException">No Invoke BeginWrite / Has Been Invoked EndWrite / Write Chunk failure</exception>
        /// <exception cref="ArgumentNullException">contentBuffer</exception>
        public void Write(string content, params object[] args)
        {
            if (this.chunkWriteStatus == HttpServerChunkStatus.NoBegin)
                throw new InvalidOperationException("No Invoke BeginWrite");

            if (this.chunkWriteStatus == HttpServerChunkStatus.End)
                throw new InvalidOperationException("Has Been Invoked EndWrite");

            if (this.isRequestHead == false)
            {
                if (args.Length > 0)
                    content = string.Format(content, args);

                if (!string.IsNullOrEmpty(content))
                {
                    var contentBuffer = this.Encoding.GetBytes(content);
                    try
                    {
                        this.Chunk(contentBuffer);
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException("Write chunk failure", exception);
                    }
                }
            }
        }

        /// <summary>
        /// Write Chunk Content
        /// </summary>
        /// <param name="contentBuffer"></param>
        /// <exception cref="InvalidOperationException">No Invoke BeginWrite / Has Been Invoked EndWrite / Write Chunk failure</exception>
        /// <exception cref="ArgumentNullException">contentBuffer</exception>
        public void Write(byte[] contentBuffer)
        {
            if (this.chunkWriteStatus == HttpServerChunkStatus.NoBegin)
                throw new InvalidOperationException("No Invoke BeginWrite");

            if (this.chunkWriteStatus == HttpServerChunkStatus.End)
                throw new InvalidOperationException("Has Been Invoked EndWrite");

            if (contentBuffer == null)
                throw new ArgumentNullException("contentBuffer");

            if (this.isRequestHead == false && contentBuffer.Length > 0)
            {
                try
                {
                    this.Chunk(contentBuffer);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException("Write chunk failure", exception);
                }
            }
        }

        /// <summary>
        /// Write Chunk Content
        /// </summary>
        /// <param name="contentBuffer"></param>
        private void Chunk(byte[] contentBuffer)
        {
            lock (this.chunkWriteEvent)
            {
                var crlfBuffer = this.Encoding.GetBytes("\r\n");
                var buffers = new ArraySegment<byte>[4];
                //
                int contentLength = contentBuffer.Length;
                //gzip
                if (this.acceptGzip && this.gzipThreshold != 0)
                {
                    this.chunkGzipStream.Write(contentBuffer, 0, contentBuffer.Length);
                    contentLength = (int)this.chunkStream.Length;
                    if (contentLength < 256)
                    {
                        //小字节退出，待下次压缩或关闭压缩
                        return;
                    }
                    this.chunkStream.Position = 0;
                    contentBuffer = StreamHelper.Receive(this.chunkStream, contentLength);
                    this.chunkStream.SetLength(0);
                }
                //len
                var lenBuffer = this.Encoding.GetBytes(contentLength.ToString("x"));
                buffers[0] = new ArraySegment<byte>(lenBuffer);
                buffers[1] = new ArraySegment<byte>(crlfBuffer);
                //content
                buffers[2] = new ArraySegment<byte>(contentBuffer);
                buffers[3] = new ArraySegment<byte>(crlfBuffer);
                //
                try
                {
                    this.socket.Send(buffers);
                }
                catch
                {
                    SocketHelper.TryClose(this.socket);
                    StreamHelper.TryClose(this.chunkGzipStream);
                    StreamHelper.TryClose(this.chunkStream);
                    this.chunkWriteEvent.Set();
                    throw;
                }
            }
        }

        private void CloseChunkGzip()
        {
            var crlfBuffer = this.Encoding.GetBytes("\r\n");
            var buffers = new ArraySegment<byte>[4];
            //
            //gzip
            this.chunkGzipStream.Close();
            var contentLength = (int)this.chunkStream.Length;
            this.chunkStream.Position = 0;
            contentBuffer = StreamHelper.Receive(this.chunkStream, contentLength);
            this.chunkStream.SetLength(0);
            //len
            var lenBuffer = this.Encoding.GetBytes(contentLength.ToString("x"));
            buffers[0] = new ArraySegment<byte>(lenBuffer);
            buffers[1] = new ArraySegment<byte>(crlfBuffer);
            //content
            buffers[2] = new ArraySegment<byte>(contentBuffer);
            buffers[3] = new ArraySegment<byte>(crlfBuffer);
            //
            try
            {
                this.socket.Send(buffers);
            }
            catch
            {
                SocketHelper.TryClose(this.socket);
                StreamHelper.TryClose(this.chunkGzipStream);
                StreamHelper.TryClose(this.chunkStream);
                this.chunkWriteEvent.Set();
                throw;
            }
        }

        /// <summary>
        /// End Write
        /// </summary>
        /// <exception cref="InvalidOperationException">No Invoke BeginWrite / Has Been Invoked EndWrite</exception>
        public void EndWrite()
        {
            lock (this.chunkWriteEvent)
            {
                if (this.chunkWriteStatus == HttpServerChunkStatus.NoBegin)
                    throw new InvalidOperationException("No Invoke BeginWrite");

                if (this.chunkWriteStatus == HttpServerChunkStatus.End)
                    throw new InvalidOperationException("Has Been Invoked EndWrite");

                if (this.chunkWriteStatus == HttpServerChunkStatus.Writing)
                {
                    this.chunkWriteStatus = HttpServerChunkStatus.End;

                    try
                    {
                        //gzip
                        if (this.acceptGzip && this.gzipThreshold != 0)
                        {
                            this.CloseChunkGzip();
                        }

                        if (this.isRequestHead == false)
                        {
                            var crlfBuffer = this.Encoding.GetBytes("\r\n");

                            var buffers = new ArraySegment<byte>[3];
                            using (var m = new MemoryStream(512))
                            {
                                //len
                                var lenBuffer = this.Encoding.GetBytes("0");
                                buffers[0] = new ArraySegment<byte>(lenBuffer);
                                buffers[1] = new ArraySegment<byte>(crlfBuffer);
                                //content
                                buffers[2] = new ArraySegment<byte>(crlfBuffer);
                            }
                            //
                            try
                            {
                                this.socket.Send(buffers);
                            }
                            catch
                            {
                                SocketHelper.TryClose(this.socket);
                                throw;
                            }
                            finally
                            {
                                StreamHelper.TryClose(this.chunkGzipStream);
                                StreamHelper.TryClose(this.chunkStream);
                                this.chunkWriteEvent.Set();
                            }
                        }
                        else
                        {
                            this.chunkWriteEvent.Set();
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException("End check write failure", exception);
                    }
                }
            }
        }

        /// <summary>
        /// 创建响应头
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        protected override StringBuilder CreateResponseHeader(HttpStatusCode status)
        {
            var build = base.CreateResponseHeader(status);

            //connection & this.Encoding.BodyName
            if (this.ResponseHeader["Connection"] == null)
            {
                build.AppendLine(string.Concat("Connection: ", this.keepAlive ? "keep-alive" : "close"));
            }

            return build;
        }

        /// <summary>
        /// 响应客户端
        /// </summary>
        /// <param name="build"></param>
        /// <param name="status"></param>
        protected override void Response(StringBuilder build, HttpStatusCode status)
        {
            byte[] contentBuffer = null;
            byte[] headerBuffer = null;

            //header
            if (this.chunkWriteStatus == HttpServerChunkStatus.NoBegin)
            {
                if (this.contentBuffer != null)
                {
                    contentBuffer = this.contentBuffer;
                }
                else if (!string.IsNullOrEmpty(this.content))
                {
                    contentBuffer = this.Encoding.GetBytes(this.content);
                }
                else
                {
                    contentBuffer = new byte[0];
                }

                //gzip
                if (this.acceptGzip && this.gzipThreshold != 0 && contentBuffer.Length > this.gzipThreshold)
                {
                    build.AppendLine("Content-Encoding: gzip");

                    using (var m = new MemoryStream())
                    using (var stream = new GZipStream(m, CompressionMode.Compress, true))
                    {
                        stream.Write(contentBuffer, 0, contentBuffer.Length);
                        stream.Close();
                        //content
                        contentBuffer = m.ToArray();
                    }
                }

                build.AppendLine(string.Concat("Content-Length: ", contentBuffer.Length));
                build.AppendLine();
            }
            else
            {
                if (this.acceptGzip && this.gzipThreshold != 0)
                {
                    build.AppendLine("Content-Encoding: gzip");
                }
                build.AppendLine("Transfer-Encoding: chunked");
                build.AppendLine();
            }
            headerBuffer = Encoding.ASCII.GetBytes(build.ToString());

            //
            if (this.chunkWriteStatus == HttpServerChunkStatus.NoBegin)
            {
                if (this.isRequestHead == false && contentBuffer != null && contentBuffer.Length > 0)
                {
                    var buffers = new ArraySegment<byte>[2];
                    //header
                    buffers[0] = new ArraySegment<byte>(headerBuffer);
                    //content
                    buffers[1] = new ArraySegment<byte>(contentBuffer);
                    //send
                    this.socket.Send(buffers);
                }
                else
                {
                    this.socket.Send(headerBuffer);
                }
            }
            else
            {
                try
                {
                    this.socket.Send(headerBuffer);
                }
                catch
                {
                    SocketHelper.TryClose(this.socket);
                    StreamHelper.TryClose(this.chunkGzipStream);
                    StreamHelper.TryClose(this.chunkStream);
                    throw;
                }
            }
        }
              
        /// <summary>
        /// 进行必要的资源清理，此方法仅供系统调用， 不允许使用者直接调用
        /// </summary>
        internal void Clear()
        {
            if (this.files != null)
            {
                foreach (var file in this.files)
                {
                    StreamHelper.TryClose(file.Stream);
                }
            }
        }
    }
}