using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using System.Net;

namespace Adf.Mail
{
    /// <summary>
    /// 使用简单邮件传输协议 (SMTP) 发送电子邮件。
    /// </summary>
    public class SmtpClient : IDisposable
    {
        ///// <summary>
        ///// 指定应该使用哪些证书来建立安全套接字层 (SSL) 连接。
        ///// </summary>
        //public X509CertificateCollection Certificates { get; private set; }


        long maxSize = 0;
        /// <summary>
        /// 获取当前连接支持的最大尺寸，为0则表示不支持该属性对比
        /// </summary>
        public long MaxSize
        {
            get { return this.maxSize; }
        }

        bool enableSSL = false;
        /// <summary>
        /// 指定是否使用安全套接字层 (SSL) 加密连接。
        /// </summary>
        public bool EnableSsl
        {
            get { return this.enableSSL; }
            set { this.enableSSL = value; }
        }

        bool enableTLS = true;
        /// <summary>
        /// 指定允许使用安全套接字层 (STARTSSL) 加密连接。
        /// </summary>
        public bool EnableTls
        {
            get { return this.enableTLS; }
            set { this.enableTLS = value; }
        }

        EndPoint localEndPoint = null;
        /// <summary>
        /// 获取或设置应用连接的本地接口
        /// </summary>
        /// <exception cref="System.ArgumentNullException">value is null</exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public EndPoint LocalEndPoint
        {
            get
            {
                if (this.localEndPoint == null && this.socket != null)
                {
                    return this.socket.LocalEndPoint;
                }
                return this.localEndPoint;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.localEndPoint = value;
            }
        }

        string host = "127.0.0.1";
        /// <summary>
        /// 获取主机的名称或 IP 地址。
        /// </summary>
        public string Host
        {
            get { return this.host; }
        }

        int port = 25;
        /// <summary>
        /// 获取主机端口
        /// </summary>
        public int Port
        {
            get { return this.port; }
        }

        int timeout = 90000;
        /// <summary>
        /// 获取或设置一个值，该值指定同步发送超时时间（以毫秒为单位）
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">value is less than zero.</exception>
        public int Timeout
        {
            get { return this.timeout; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value is less than zero.");
                this.timeout = value;
            }
        }

        /// <summary>
        /// 获取或设置与凭据关联的用户名的密码。
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 获取或设置与凭据关联的用户名。
        /// </summary>
        public string UserName { get; set; }


        LogWriter logWriter = null;
        /// <summary>
        /// 获取或设置日志书写器,禁用则设置为null
        /// </summary>
        public LogWriter LogWriter
        {
            get { return this.logWriter; }
            set { this.logWriter = value; }
        }

        string selfHost;
        /// <summary>
        /// 获取或设置当前发送域主机名称，设置为你当前主机的公网域名，建议设置该值, 默认为当前主机名
        /// </summary>
        public string SelfHost
        {
            get { return this.selfHost; }
            set { this.selfHost = value; }
        }

        /// <summary>
        /// 获取是否已连接
        /// </summary>
        public bool Connected
        {
            get
            {
                if (this.socket == null)
                    return false;

                return this.socket.Connected;
            }
        }
        /// <summary>
        /// 使用配置文件设置初始化新实例,并指定主机
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">port 不能小于 0</exception>
        public SmtpClient()
        {
            this.InitializeConfiguration();
            //
            if (this.port < 0)
                throw new ConfigException("port is less than or zero.");

            if (host == null)
                throw new ConfigException("host is null");

            if (host == "")
                throw new ConfigException("host is empty");
        }

        /// <summary>
        /// 使用配置文件设置初始化新实例,并指定主机
        /// </summary>
        /// <param name="host">System.String，包含用于 SMTP 事务的主机的名称或 IP 地址。</param>
        /// <param name="port">大于 0 的 System.Int32，包含要在 host 上使用的端口。</param>
        /// <exception cref="System.ArgumentOutOfRangeException">port 不能小于 0</exception>
        public SmtpClient(string host, int port)
        {
            if (port < 0)
                throw new ArgumentOutOfRangeException("port", "port is less than or zero.");

            if (host == null)
                throw new ArgumentNullException("host is null");

            if (host == "")
                throw new ArgumentException("host is empty");
            //
            this.host = host;
            this.port = port;
        }

        private void InitializeConfiguration()
        {
            var enable = Adf.Config.SmtpConfig.Instance.Enabled;
            if (enable == true)
            {
                this.host = Adf.Config.SmtpConfig.Instance.Host;
                this.port = Adf.Config.SmtpConfig.Instance.Port;
                this.selfHost = Adf.Config.SmtpConfig.Instance["SelfHost"];
                this.enableSSL = Adf.Config.SmtpConfig.Instance.SSLEnabled;
                this.enableTLS = Adf.Config.SmtpConfig.Instance.TLSEnabled;
                //
                this.UserName = Adf.Config.SmtpConfig.Instance.Account;
                this.Password = Adf.Config.SmtpConfig.Instance.Password;
            }
        }

        ///// <summary>
        ///// 异常发送完成通知事件
        ///// </summary>
        //public event EventHandler<SendCompletedEventArgs> SendCompleted;


        ///// <summary>
        ///// 引发 SendCompleted 事件
        ///// </summary>
        ///// <param name="exception"></param>
        ///// <param name="userToken"></param>
        //private void OnSendCompleted(Exception exception, object userToken)
        //{
        //    var action = this.SendCompleted;
        //    if (action != null)
        //    {
        //        var e = new SendCompletedEventArgs(exception, userToken);
        //        action(this, e);
        //    }
        //}

        ///// <summary>
        ///// 引发 SendCompleted 事件
        ///// </summary>
        ///// <param name="userToken"></param>
        //private void OnSendCompleted(object userToken)
        //{
        //    var action = this.SendCompleted;
        //    if (action != null)
        //    {
        //        Exception exception = null;
        //        var e = new SendCompletedEventArgs(exception, userToken);
        //        action(this, e);
        //    }
        //}        

        /// <summary>
        /// send a message
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="Adf.SmtpException">send failure</exception>
        /// <exception cref="System.ArgumentNullException">message is null</exception>
        /// <exception cref="System.ArgumentException">no recipients or no from in message</exception>
        /// <exception cref="System.ObjectDisposedException">instance is disposed</exception>
        /// <exception cref="System.IO.IOException">network error</exception>
        public void Send(MailMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message is null");
            }

            if (message.To.Count == 0)
            {
                throw new ArgumentException("no recipients");
            }

            if (message.From == null || message.From.Address == null || message.From.Address == "")
            {
                throw new ArgumentException("message.From invalid");
            }

            bool connected = true;

            if (this.socket == null)
            {
                connected = false;
            }
            else if (this.socket.Connected == false)
            {
                connected = false;
                this.CleanNetwork();
            }

            //
            if (connected == false)
            {
                this.Connect();
                this.Helo();
            }

            //进行邮件写入
            this.Dialog("MAIL FROM: <" + message.From.Address + ">\r\n", "250", "MAIL FROM"); //操作完成   //写入邮件发送人

            //输出 收件人
            foreach (var mail in message.To)
            {
                this.Dialog("RCPT TO: <" + mail.Address + ">\r\n", "250", "RCPT TO");
            }
            foreach (var mail in message.CC)
            {
                this.Dialog("RCPT TO: <" + mail.Address + ">\r\n", "250", "RCPT TO");
            }
            foreach (var mail in message.Bcc)
            {
                this.Dialog("RCPT TO: <" + mail.Address + ">\r\n", "250", "RCPT TO");
            }

            //完成头输入，进行邮件输入
            this.Dialog("DATA\r\n", "354", "DATA");
            
            //输入邮件
            this.Write(message.GetHead());
            this.Write(message.GetBody());
            //结束一个邮件输入
            this.Write(".\r\n");

            //结束
            this.CheckError("250", "DATA END");
        }

        private void Helo()
        {
            this.CheckError("220", "Connect");  //与服务器连接成功

            //Dialog("EHLO " + Dns.GetHostName() + MailCommon.NewLine, "250"); //250 : 操作完成  //尝试登录服务器成功
            //220 newmx16.qq.com MX QQ Mail Server
            //ehlo aa
            //250-newmx16.qq.com
            //250-SIZE 73400320
            //250-STARTTLS
            //250 OK

            var esmtpHelo = string.IsNullOrEmpty(this.UserName) ? "HELO " : "EHLO ";

            //var input = "EHLO " + Dns.GetHostName() + MailCommon.NewLine;
            var input = "";
            if (string.IsNullOrEmpty(this.selfHost))
            {
                input = esmtpHelo + Dns.GetHostName() + MailCommon.NewLine;
            }
            else
            {
                input = esmtpHelo + this.selfHost + MailCommon.NewLine;
            }
            this.Write(input);

            //reset max size
            this.maxSize = 0;

            //read response
            bool tls = false;
            long size = 0;
            while (true)
            {
                var position = 0;
                StreamHelper.ReadLine(this.stream, this.receiveBuffer, ref position);
                if (position == 0)
                {
                    throw new IOException("connection is closed from HELO/EHLO");
                }
                //
                var line = Encoding.ASCII.GetString(this.receiveBuffer, 0, position);
                //
                if (this.logWriter != null && this.logWriter.Enable)
                {
                    this.logWriter.WriteTimeLine(line);
                }
                //
                var head = line.Substring(0, 4);
                if (head == "250 ")
                {
                    break;
                }
                else if (head != "250-")
                {
                    throw new SmtpException(line);
                }
                else if (line == "250-STARTTLS")
                {
                    tls = this.enableSSL == false && this.enableTLS == true;
                }
                else if (line.StartsWith("250-SIZE "))
                {
                    var items = line.Split(' ');
                    if (items.Length == 2)
                    {
                        if (long.TryParse(items[1], out size))
                        {
                            this.maxSize = size;
                        }
                    }
                }
            }

            if (tls == true && this.stream is SslStream == false)
            {
                this.Dialog("STARTTLS\r\n", "220", "STARTTLS");

                // create ssl stream
                this.stream = new SslStream(this.stream, false);

                // authenticate ssl stream
                ((SslStream)this.stream).AuthenticateAsClient(this.host, new X509CertificateCollection(), System.Security.Authentication.SslProtocols.Tls, false);
            }

            //判断是否为登录
            if (!string.IsNullOrEmpty(this.UserName) && !string.IsNullOrEmpty(this.Password))
            {
                this.Dialog("AUTH LOGIN\r\n", "334", "AUTH LOGIN"); //334响应验证  //响应服务器的帐户认证

                this.Dialog(Convert.ToBase64String(Encoding.ASCII.GetBytes(this.UserName)) + MailCommon.NewLine, "334", "INPUT USERNAME");

                this.Dialog(Convert.ToBase64String(Encoding.ASCII.GetBytes(this.Password)) + MailCommon.NewLine, "235", "INPUT PASSWORD");
            }
        }

        private void Connect()
        {
            var localEP = this.localEndPoint;

            if (this.logWriter != null && this.logWriter.Enable)
            {
                this.logWriter.WriteTimeLine("Connect {0}:{1}", this.host, this.port);
            }

            try
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (localEP != null)
                {
                    this.socket.Bind(localEP);
                    //
                    if (this.logWriter != null && this.logWriter.Enable)
                    {
                        this.logWriter.WriteTimeLine("Bind local interface " + localEP.ToString());
                    }
                }

                //this.socket.Connect(this.host, this.port);
                SocketHelper.Connect(this.socket,this.host,this.port, this.timeout);

                //this.socket.ReceiveTimeout = this.timeout;
                //this.socket.SendTimeout = this.timeout;
                //
                var networkStream = new NetworkStream(socket, false);
                //
                if (this.enableSSL == true)
                {
                    this.stream = new SslStream(networkStream);

                    if (this.logWriter != null && this.logWriter.Enable)
                    {
                        this.logWriter.WriteTimeLine("SSL handshake");
                    }
                }
                else
                {
                    this.stream = networkStream;
                }
                this.stream.ReadTimeout = this.timeout;
                this.stream.WriteTimeout = this.timeout;
            }
            catch (Exception exception)
            {
                throw new IOException(exception.Message, exception);
            }
        }

        private void CleanNetwork()
        {
            try { this.stream.Close(); }
            catch { }
            try { this.stream.Dispose(); }
            catch { }
            try { this.socket.Close(); }
            catch { }
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (this.socket.Connected == true)
                {
                    //请求退出服务器成功，完成邮件的所有操作
                    this.Dialog("QUIT\r\n", "221", "QUIT");
                }
            }
            catch { }
            //
            this.CleanNetwork();
        }

        Socket socket;
        Stream stream;
        byte[] receiveBuffer = new byte[4096];

        /// <summary>
        /// 接收SMTP服务器回应
        /// </summary>
        private string ReceiveResponse()
        {
            var position = 0;
            StreamHelper.ReadLine(this.stream, this.receiveBuffer, ref position);
            if (position == 0)
            {
                return "";
            }
            //
            var line = Encoding.ASCII.GetString(this.receiveBuffer, 0, position);
            if (this.logWriter != null && this.logWriter.Enable)
            {
                this.logWriter.WriteTimeLine(line);
            }
            return line;


            //var read = this.stream.Read(this.receiveBuffer, 0, this.receiveBuffer.Length);
            //if (read == 0)
            //    return "";

            //var receive = Encoding.ASCII.GetString(this.receiveBuffer, 0, read);

            //if (this.logWriter != null && this.logWriter.Enable)
            //{
            //    this.logWriter.WriteTimeLine(receive);
            //}

            //return receive;
        }

        /// <summary>
        /// 写入内容
        /// </summary>
        /// <param name="input">要写入的字符串</param>
        private void Write(string input)
        {
            if (input == null)
                return;
            if (input == "")
                return;

            var buffer = Encoding.ASCII.GetBytes(input);

            if (this.logWriter != null && this.logWriter.Enable)
            {
                this.logWriter.WriteTime(input);
            }

            this.stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 与服务器交互，发送一条命令并接收回应，并返回网络操作是否正确。
        /// </summary>
        /// <param name="request">一个要发送的命令</param>
        /// <param name="successCode">要与服务器端所返回的代码进行验证的代码值</param>
        /// <param name="description">请求描述</param>
        private void Dialog(string request, string successCode, string description)
        {
            this.Write(request);
            this.CheckError(successCode, description);
        }

        /// <summary>
        /// 验证正确性
        /// </summary>
        /// <param name="successCode"></param>
        /// <param name="description"></param>
        private void CheckError(string successCode, string description)
        {
            var response = this.ReceiveResponse();

            //验证正确性
            string code = response.Substring(0, 3);
            if (code != successCode)
            {
                throw new SmtpException(description + ": " + response);
            }
        }
    }
}