using System;
using System.Net;
using System.Text;
using System.IO;

namespace Adf
{
    /// <summary>
    /// http client
    /// </summary>
    public class HttpClient
    {
        static string UA = null;

        /// <summary>
        /// default instance
        /// </summary>
        public readonly static HttpClient Instance = new HttpClient();

        Encoding encoding = null;
        /// <summary>
        /// get or set encoding
        /// </summary>
        public Encoding Encoding
        {
            get { return this.encoding; }
            set { this.encoding = value; }
        }

        string userAgent = null;
        /// <summary>
        /// get or set user-agent
        /// </summary>
        public string UserAgent
        {
            get { return this.userAgent; }
            set { this.userAgent = value; }
        }

        int timeout = 60 * 1000;
        /// <summary>
        /// get or set timeout , default 60 seconds
        /// </summary>
        public int Timeout
        {
            get { return this.timeout; }
            set { this.timeout = value; }
        }

        /// <summary>
        /// initiaize a new instance
        /// </summary>
        public HttpClient()
        {
            this.userAgent = this.GetUserAgent();
            this.encoding = System.Text.Encoding.UTF8;
        }

        private string GetUserAgent()
        {
            if (UA == null)
            {
                var platform = Environment.OSVersion.Platform.ToString();
                var osVersion = Environment.OSVersion.VersionString;
                var clr = Environment.Version.ToString();

                //"Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:0.9.4) Gecko/20011128 Netscape6/6.2.1";

                var adfVersion = typeof(HttpClient).Assembly.GetName().Version.ToString();

                UA = "Mozilla/5.0 (" + platform + "; " + osVersion + "; .NET " + clr + ") Adf.HttpClient/" + adfVersion;
            }

            return UA;
        }

        /// <summary>
        /// invoke a get request
        /// </summary>
        /// <param name="path">ex: /user/register</param>
        /// <param name="hosts">ex: https://example.com,http://192.168.1.100, http://example.com:8080, http://192.168.1.100:8080</param>
        /// <returns></returns>
        public string GetString(string[] hosts, string path)
        {
            var url = hosts[0] + path;
            try
            {
                return this.GetString(url);
            }
            catch (System.Net.WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ConnectFailure)
                {
                    var data = this.TryGet(hosts, path);
                    return this.encoding.GetString(data);
                }
                else
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// invoke a get request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetString(string url)
        {
            var data2 = this.Get(url);
            //
            return this.encoding.GetString(data2);
        }


        /// <summary>
        /// invoke a get request
        /// </summary>
        /// <param name="path">ex: /user/register</param>
        /// <param name="hosts">ex: https://example.com,http://192.168.1.100, http://example.com:8080, http://192.168.1.100:8080</param>
        /// <returns></returns>
        public byte[] Get(string[] hosts, string path)
        {
            var url = hosts[0] + path;
            try
            {
                return this.Get(url);
            }
            catch (System.Net.WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ConnectFailure)
                {
                    return this.TryGet(hosts, path);
                }
                else
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// invoke a get request
        /// </summary>
        /// <param name="path">ex: /user/register</param>
        /// <param name="hosts">ex: https://example.com,http://192.168.1.100, http://example.com:8080, http://192.168.1.100:8080</param>
        /// <returns></returns>
        private byte[] TryGet(string[] hosts, string path)
        {
            var i = 0;
            byte[] result = null;
            var url = "";
            var host = "";
            var success = false;
            //
            for (; i < hosts.Length; i++)
            {
                host = hosts[i];
                url = host + path;
                try
                {
                    result = this.Get(url);
                    success = true;
                    break;
                }
                catch (System.Net.WebException exception)
                {
                    if (exception.Status == WebExceptionStatus.ConnectFailure)
                    {
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            //
            if (success == false)
            {
                throw new WebException("no active host in hosts");
            }
            //
            lock (hosts)
            {
                var list = new System.Collections.Generic.List<string>(hosts);
                //
                list.Remove(host);
                list.Insert(0, host);
                //
                list.CopyTo(hosts);
            }
            return result;
        }

        /// <summary>
        /// invoke a get request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public byte[] Get(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.KeepAlive = false;
            //request.Connection = "Close";
            request.UserAgent = this.userAgent;
            request.Timeout = this.timeout;
            //
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var length = (int)response.ContentLength;
                if (length < 1)
                {
                    length = 4096;
                }

                using (var responseStream = response.GetResponseStream())
                {
                    responseStream.ReadTimeout = this.timeout;

                    using (MemoryStream memoryStream = new MemoryStream(length))
                    {
                        int count = 0;
                        byte[] buffer = new byte[4096];
                        while (true)
                        {
                            count = responseStream.Read(buffer, 0, buffer.Length);
                            if (count == 0)
                            {
                                break;
                            }
                            memoryStream.Write(buffer, 0, count);
                        }

                        //
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// invoke a post request
        /// </summary>
        /// <param name="path">ex: /user/register</param>
        /// <param name="hosts">ex: https://example.com,http://192.168.1.100, http://example.com:8080, http://192.168.1.100:8080</param>
        /// <param name="formdata"></param>
        /// <returns></returns>
        public string Post(string[] hosts, string path, string formdata)
        {
            var url = hosts[0] + path;
            try
            {
                return this.Post(url, formdata, "application/x-www-form-urlencoded");
            }
            catch (System.Net.WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ConnectFailure)
                {
                    var data = this.encoding.GetBytes(formdata);
                    var data1 = this.TryPost(hosts, path, data, "application/x-www-form-urlencoded");
                    return this.encoding.GetString(data1);
                }
                else
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// invoke a post request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="formdata"></param>
        /// <returns></returns>
        public string Post(string url, string formdata)
        {
            var data1 = this.encoding.GetBytes(formdata);
            var data2 = this.Post(url, data1, "application/x-www-form-urlencoded");
            //
            return this.encoding.GetString(data2);
        }

        /// <summary>
        /// invoke a post request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public string Post(string url, string data, string contentType)
        {
            var data1 = this.encoding.GetBytes(data);
            var data2 = this.Post(url, data1, contentType);
            //
            return this.encoding.GetString(data2);
        }

        /// <summary>
        /// invoke a post request
        /// </summary>
        /// <param name="path">ex: /user/register</param>
        /// <param name="hosts">ex: https://example.com,http://192.168.1.100, http://example.com:8080, http://192.168.1.100:8080</param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public string Post(string[] hosts, string path, string data, string contentType)
        {
            var url = hosts[0] + path;
            try
            {
                return this.Post(url, data, contentType);
            }
            catch (System.Net.WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ConnectFailure)
                {
                    var data2 = this.encoding.GetBytes(data);
                    var data3 = this.TryPost(hosts, path, data2, contentType);
                    //
                    return this.encoding.GetString(data3);
                }
                else
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// invoke a post request
        /// </summary>
        /// <param name="path">ex: /user/register</param>
        /// <param name="hosts">ex: https://example.com,http://192.168.1.100, http://example.com:8080, http://192.168.1.100:8080</param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public byte[] Post(string[] hosts, string path, byte[] data, string contentType)
        {
            var url = hosts[0] + path;
            try
            {
                return this.Post(url, data, contentType);
            }
            catch (System.Net.WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ConnectFailure)
                {
                    return this.TryPost(hosts, path, data, contentType);
                }
                else
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// invoke a post request
        /// </summary>
        /// <param name="path">ex: /user/register</param>
        /// <param name="hosts">ex: https://example.com,http://192.168.1.100, http://example.com:8080, http://192.168.1.100:8080</param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        private byte[] TryPost(string[] hosts, string path, byte[] data, string contentType)
        {
            var i = 0;
            byte[] result = null;
            var url = "";
            var host = "";
            var success = false;
            //
            for (; i < hosts.Length; i++)
            {
                host = hosts[i];
                url = host + path;
                try
                {
                    result = this.Post(url, data, contentType);
                    success = true;
                    break;
                }
                catch (System.Net.WebException exception)
                {
                    if (exception.Status == WebExceptionStatus.ConnectFailure)
                    {
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            //
            if (success == false)
            {
                throw new WebException("no active host in hosts");
            }
            //
            lock (hosts)
            {
                var list = new System.Collections.Generic.List<string>(hosts);
                //
                list.Remove(host);
                list.Insert(0, host);
                //
                list.CopyTo(hosts);
            }
            return result;
        }

        /// <summary>
        /// invoke a post request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public byte[] Post(string url, byte[] data, string contentType)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.KeepAlive = false;
            //request.Connection = "Close";
            request.ContentLength = data.Length;
            request.ContentType = contentType;
            request.UserAgent = this.userAgent;
            //
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);

                using (var response = request.GetResponse())
                {
                    var length = (int)response.ContentLength;
                    if (length < 1)
                    {
                        length = 4096;
                    }
                    using (var responseStream = response.GetResponseStream())
                    {
                        responseStream.ReadTimeout = this.timeout;

                        using (MemoryStream memoryStream = new MemoryStream(length))
                        {
                            int count = 0;
                            byte[] buffer = new byte[4096];
                            while (true)
                            {
                                count = responseStream.Read(buffer, 0, buffer.Length);
                                if (count == 0)
                                {
                                    break;
                                }
                                memoryStream.Write(buffer, 0, count);
                            }

                            //
                            return memoryStream.ToArray();
                        }
                    }
                }
            }
        }
    }
}