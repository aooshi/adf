using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace Adf
{

    /// <summary>
    /// Http Server Context base
    /// </summary>
    public abstract class HttpServerContextBase
    {
        static readonly byte[] EMPTY_BUFFER = new byte[0];

        /// <summary>
        /// 存储用户自定义数据
        /// </summary>
        public object UserState
        {
            get;
            set;
        }

        /// <summary>
        /// RequestHeader
        /// </summary>
        public NameValueCollection RequestHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// ResponseHeader
        /// </summary>
        public NameValueCollection ResponseHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// QueryString
        /// </summary>
        public NameValueCollection QueryString
        {
            get;
            private set;
        }

        /// <summary>
        /// Request Type
        /// </summary>
        public HttpServerRequestType RequestType
        {
            get;
            protected set;
        }

        NameValueCollection requestCookie = null;
        /// <summary>
        /// get request cookie
        /// </summary>
        public NameValueCollection RequestCookie
        {
            get
            {
                if (this.requestCookie == null)
                {
                    this.requestCookie = new HttpQueryCollection(5);
                    var cookie = this.RequestHeader["Cookie"];
                    if (cookie != null && cookie != "")
                    {
                        string[] cookies = cookie.Split(';');
                        string[] segments;
                        foreach (string item in cookies)
                        {
                            segments = item.Split('=');
                            if (segments.Length == 2)
                            {
                                this.requestCookie.Add(segments[0].Trim(), UriHelper.UrlDecode(segments[1], this.encoding));
                            }
                        }
                    }
                }
                return this.requestCookie;
            }
        }

        CookieCollection responseCookie = null;
        /// <summary>
        /// get response cookie collection
        /// </summary>
        public CookieCollection ResponseCookie
        {
            get
            {
                if (this.responseCookie == null)
                {
                    this.responseCookie = new CookieCollection();
                }
                return this.responseCookie;
            }
        }

        /// <summary>
        /// Get Method
        /// </summary>
        public string Method
        {
            get;
            private set;
        }

        /// <summary>
        /// Url
        /// </summary>
        public string Url
        {
            get;
            private set;
        }

        /// <summary>
        /// Protocol
        /// </summary>
        public string Protocol
        {
            get;
            private set;
        }
        /// <summary>
        /// Path
        /// </summary>
        public string Path
        {
            get;
            private set;
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get;
            internal set;
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get;
            internal set;
        }
        Encoding encoding;
        /// <summary>
        /// get/or set encoding
        /// </summary>
        public Encoding Encoding
        {
            get { return this.encoding; }
            set { this.encoding = value; }
        }

        /// <summary>
        /// get server
        /// </summary>
        public HttpServer Server
        {
            get;
            private set;
        }

        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="header"></param>
        /// <param name="server"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="protocol"></param>
        internal HttpServerContextBase(NameValueCollection header, HttpServer server, string url, string method, string protocol)
        {
            this.RequestHeader = header;
            this.ResponseHeader = new NameValueCollection(10, StringComparer.OrdinalIgnoreCase);
            this.QueryString = new HttpQueryCollection(5);
            //
            this.Server = server;
            this.encoding = server.Encoding;
            //
            this.Url = url;
            this.Method = method;
            this.Protocol = protocol;
            //
            var index = url.IndexOf('?');
            this.Path = index == -1 ? url : url.Substring(0, index);
        }

        /// <summary>
        /// 创建响应头
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        protected virtual StringBuilder CreateResponseHeader(HttpStatusCode status)
        {
            var build = new StringBuilder(512);

            build.AppendFormat("{0} {1} {2}", this.Protocol, (int)status, HttpStatusHelper.GetStatusDescription(status));
            build.AppendLine();

            foreach (var key in this.ResponseHeader.AllKeys)
            {
                build.AppendLine(string.Concat(key, ": ", this.ResponseHeader[key]));
            }

            this.CreateCookieHeader(build);

            return build;
        }

        /// <summary>
        /// 创建Cookie响应头
        /// </summary>
        /// <param name="build"></param>
        protected virtual void CreateCookieHeader(StringBuilder build)
        {
            if (this.responseCookie != null)
            {
                foreach (Cookie cookie in this.responseCookie)
                {
                    build.Append("Set-Cookie: ");
                    if (cookie.Value == null)
                        build.Append(cookie.Name + "=");
                    else
                        build.Append(cookie.Name + "=" + UriHelper.UrlEncode(cookie.Value, this.encoding));

                    if (DateTime.MinValue.Equals(cookie.Expires) == false)
                    {
                        var e = cookie.Expires.ToUniversalTime();
                        build.Append(";expires=" + e.ToString("r"));

                        var s = e.Subtract(DateTime.UtcNow).TotalSeconds;
                        if (s > 0 && s <= UInt64.MaxValue)
                        {
                            build.Append(";max-age=" + ((UInt64)s).ToString());
                        }
                    }
                    
                    if (string.IsNullOrEmpty(cookie.Domain) == false)
                        build.Append(";domain=" + cookie.Domain);

                    if (string.IsNullOrEmpty(cookie.Path) == false)
                        build.Append(";path=" + cookie.Path);

                    if (cookie.Secure)
                        build.Append(";secure");

                    if (cookie.HttpOnly)
                        build.Append(";httponly");

                    build.AppendLine();
                }
            }
        }

        /// <summary>
        /// 应答客户端
        /// </summary>
        /// <param name="status"></param>
        internal void Response(HttpStatusCode status)
        {
            var header = this.CreateResponseHeader(status);
            this.Response(header, status);
        }

        /// <summary>
        /// 应答客户端
        /// </summary>
        /// <param name="status"></param>
        /// <param name="header"></param>
        protected abstract void Response(StringBuilder header, HttpStatusCode status);

        /// <summary>
        /// HTTP查询集合
        /// </summary>
        protected class HttpQueryCollection : NameValueCollection
        {
            /// <summary>
            /// 初始化新实例
            /// </summary>
            /// <param name="capacity"></param>
            public HttpQueryCollection(int capacity)
                : base(capacity, StringComparer.OrdinalIgnoreCase)
            {
            }

            /// <summary>
            /// 将查询集合转换为字符串形式
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return UriHelper.QueryStringToString(this);
            }
        }
    }
}