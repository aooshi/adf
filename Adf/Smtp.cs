using System;
using System.Collections.Generic;
using System.Text;
using Adf.Config;
using System.Net.Mail;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

namespace Adf
{
    /// <summary>
    /// Smtp Helper
    /// </summary>
    /// <remarks>
    /// 默认实例可通过Smtp.Config,  Global.Config, AppSetting 配置,优先级以 AppSetting->Global.Config->Smtp.Config
    /// 参考： http://www.xiaobo.li/adf/527.html
    /// </remarks>
    public class Smtp : ISmtp
    {
        /// <summary>
        /// 默认实例
        /// </summary>
        public static readonly Smtp Instance = new Smtp();
        /// <summary>
        /// 无代理发送器
        /// </summary>
        private static NoProxyStmp noProxyStmp = new NoProxyStmp();
        /// <summary>
        /// 发送者地址是否随机
        /// </summary>
        public bool IsSenderRandom { get; private set; }
        string senderRandomDomain;
        /// <summary>
        /// 发送者地址随机域名,config:SmtpSenderRandomDomain
        /// </summary>
        public string SenderRandomDomain
        {
            get { return this.senderRandomDomain; }
            set
            {
                this.senderRandomDomain = value;
                this.IsSenderRandom = !string.IsNullOrEmpty(value);
            }
        }
        /// <summary>
        /// 发送者地址随机发生器
        /// </summary>
        Random senderRandom = new Random();
        /// <summary>
        /// host, config:SmtpHost
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// port, config:SmtpPort
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Sender, config:SmtpSender
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Sender Name ,config: SmtpName
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Account ,config:SmtpAccount
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Password, config:SmtpPassword
        /// </summary>
        public string Password { get; set; }
                
        /// <summary>
        /// 是否可用,config:SmtpEnabled
        /// </summary>
        /// <returns></returns>
        public bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// 是否启用SSL,SSL Enabled ,config: SmtpSSLEnabled
        /// </summary>
        public bool SSLEnabled
        {
            get;
            set;
        }
                
        /// <summary>
        /// 初始一个新实例
        /// </summary>
        public Smtp()
        {
            this.InitConfig();
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        protected virtual void InitConfig()
        {
            if (SmtpConfig.Instance["SmtpEnabled"] != null)
            {
                this.Host = SmtpConfig.Instance["SmtpHost"];
                this.Port = int.Parse(SmtpConfig.Instance.GetItem("SmtpPort", "25"));
                this.Sender = SmtpConfig.Instance["SmtpSender"];
                this.Name = SmtpConfig.Instance["SmtpName"];
                this.Account = SmtpConfig.Instance["SmtpAccount"];
                this.Password = SmtpConfig.Instance["SmtpPassword"];
                this.Enabled = SmtpConfig.Instance["SmtpEnabled"] == "true";
                this.SSLEnabled = SmtpConfig.Instance["SmtpSSLEnabled"] == "true";

                this.SenderRandomDomain = SmtpConfig.Instance["SmtpSenderRandomDomain"];
            }
            else if (GlobalConfig.Instance["SmtpEnabled"] != null)
            {
                this.Host = GlobalConfig.Instance["SmtpHost"];
                this.Port = int.Parse(GlobalConfig.Instance.GetItem("SmtpPort", "25"));
                this.Sender = GlobalConfig.Instance["SmtpSender"];
                this.Name = GlobalConfig.Instance["SmtpName"];
                this.Account = GlobalConfig.Instance["SmtpAccount"];
                this.Password = GlobalConfig.Instance["SmtpPassword"];
                this.Enabled = GlobalConfig.Instance["SmtpEnabled"] == "true";
                this.SSLEnabled = GlobalConfig.Instance["SmtpSSLEnabled"] == "true";

                this.SenderRandomDomain = GlobalConfig.Instance["SmtpSenderRandomDomain"];
            }
            else
            {
                this.Host = Adf.ConfigHelper.GetSetting("SmtpHost");
                this.Port = int.Parse(Adf.ConfigHelper.GetSetting("SmtpPort","25"));
                this.Sender = Adf.ConfigHelper.GetSetting("SmtpSender");
                this.Name = Adf.ConfigHelper.GetSetting("SmtpName");
                this.Account = Adf.ConfigHelper.GetSetting("SmtpAccount");
                this.Password = Adf.ConfigHelper.GetSetting("SmtpPassword");
                this.Enabled = Adf.ConfigHelper.GetSetting("SmtpEnabled") == "true";
                this.SSLEnabled = Adf.ConfigHelper.GetSetting("SmtpSSLEnabled") == "true";

                this.SenderRandomDomain = Adf.ConfigHelper.GetSetting("SmtpSenderRandomDomain");
            }
        }
        
        /// <summary>
        /// 获取根据规则处理后的发送者地址
        /// </summary>
        /// <exception cref="Adf.ConfigException">No configuration or set smtp sender</exception>
        /// <returns></returns>
        public virtual string GetSenderAddress()
        {
            if (this.IsSenderRandom)
            {
                var name = DateTime.UtcNow.Ticks.ToString("x") + this.senderRandom.Next().ToString("x");
                return name + "@" + this.SenderRandomDomain;
            }

            if (string.IsNullOrEmpty(Sender))
                throw new ConfigException("No configuration or set smtp sender");

            return this.Sender;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="to"></param>
        /// <param name="content"></param>
        /// <param name="ishtml"></param>
        /// <exception cref="Adf.ConfigException">No configuration or set smtp sender</exception>
        /// <exception cref="Adf.SmtpException"></exception>
        public virtual void Send(string subject, string to, string content , bool ishtml = false)
        {
            var message = new MailMessage();
            message.Subject = subject;
            message.To.Add(to);
            message.Body = content;
            message.IsBodyHtml = ishtml;

            this.Send(message);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="tos"></param>
        /// <param name="content"></param>
        /// <param name="ishtml"></param>
        /// <exception cref="Adf.ConfigException">No configuration or set smtp sender</exception>
        /// <exception cref="Adf.SmtpException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">tos is empty</exception>
        public virtual void Send(string subject, string content, bool ishtml, params string[] tos)
        {
            if (tos.Length == 0)
                throw new ArgumentOutOfRangeException("tos is empty");

            var message = new MailMessage();
            message.Subject = subject;
            foreach (var to in tos)
            {
                message.To.Add(to);
            }
            message.Body = content;
            message.IsBodyHtml = ishtml;
            this.Send(message);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="message">消息体，若未填充From属性，则将以配置填充</param>
        /// <exception cref="Adf.SmtpException"></exception>
        /// <exception cref="Adf.ConfigException">No configuration or set smtp sender</exception>
        /// <exception cref="ArgumentOutOfRangeException">tos is empty</exception>
        /// <exception cref="ArgumentException">No set receiver</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public virtual void Send(MailMessage message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (this.Enabled == false)
                return;
            
            if (message.To.Count == 0)
                throw new ArgumentException("message", "No set receiver");

            if (message.From == null)
            {
                if (!string.IsNullOrEmpty(Name))
                    message.From = new MailAddress(this.GetSenderAddress(), Name);
                else
                    message.From = new MailAddress(this.GetSenderAddress());
            }

            if (string.IsNullOrEmpty(Host))
            {
                //无代理发送
                SendNoPorxy(message);
            }
            else
            {
                var smtpClient = new SmtpClient(Host, Port);
                smtpClient.EnableSsl = this.SSLEnabled;
                if (!string.IsNullOrEmpty(Account) && !string.IsNullOrEmpty(Password))
                {
                    smtpClient.Credentials = new NetworkCredential(Account, Password);
                }
                smtpClient.Send(message);
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="message">消息体，若未填充From属性，则将以配置填充</param>
        /// <exception cref="Adf.SmtpException"></exception>
        /// <exception cref="Adf.ConfigException">No configuration or set smtp sender</exception>
        /// <exception cref="ArgumentOutOfRangeException">tos is empty</exception>
        /// <exception cref="ArgumentException">No set receiver</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public virtual void Send(Adf.Mail.MailMessage message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (this.Enabled == false)
                return;

            if (message.To.Count == 0)
                throw new ArgumentException("message", "No set receiver");

            if (message.From == null)
            {
                if (!string.IsNullOrEmpty(Name))
                    message.From = new Adf.Mail.MailAddress(this.GetSenderAddress(), Name);
                else
                    message.From = new Adf.Mail.MailAddress(this.GetSenderAddress());
            }

            if (string.IsNullOrEmpty(Host))
            {
                //无代理发送
                SendNoPorxy(message);
            }
            else
            {
                using (var smtpClient = new Adf.Mail.SmtpClient(Host, Port))
                {
                    smtpClient.EnableSsl = this.SSLEnabled;
                    var account = this.Account;
                    var password = this.Password;
                    if (!string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(password))
                    {
                        smtpClient.UserName = account;
                        smtpClient.Password = password;
                    }
                    smtpClient.Send(message);
                }
            }
        }


        /// <summary>
        /// 验证授权是否通过
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckAuth()
        {
            var auth = new SmtpAuth(this.Host, this.Port, this.Account, this.Password,this.SSLEnabled);
            return auth.Success;
        }

        /// <summary>
        /// 验证授权是否通过
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckAuth(out string message)
        {
            var auth = new SmtpAuth(this.Host, this.Port, this.Account, this.Password,SSLEnabled);
            message = auth.Message;
            return auth.Success;
        }

        /// <summary>
        /// 无代理发送（暂不支持一个Message多域发送）
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="Adf.SmtpException"></exception>
        public static void SendNoPorxy(MailMessage message)
        {
            noProxyStmp.SendNoPorxy(message);
        }

        /// <summary>
        /// 无代理发送（暂不支持一个Message多域发送）
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="Adf.SmtpException"></exception>
        public static void SendNoPorxy(Adf.Mail.MailMessage message)
        {
            Adf.Mail.MailDeliver.Send(message);
        }

        class NoProxyStmp
        {
            /// <summary>
            /// mx record. domain,exchange
            /// </summary>
            Dictionary<string, MXRecordItem> mxrecords = new Dictionary<string, MXRecordItem>(5);

            /// <summary>
            /// 无代理发送
            /// </summary>
            /// <param name="message"></param>
            /// <exception cref="Adf.SmtpException"></exception>
            public void SendNoPorxy(MailMessage message)
            {
                if (message.To.Count == 1 && message.CC.Count == 0 && message.Bcc.Count == 0)
                {
                    this.SendNoPorxy(message.To[0].Host, message);
                    return;
                }

                //按域分组
                var domainGroup = new Dictionary<string, bool>();
                foreach (MailAddress address in message.To)
                {
                    if (!domainGroup.ContainsKey(address.Host))
                        domainGroup.Add(address.Host, false);
                }
                foreach (MailAddress address in message.CC)
                {
                    if (!domainGroup.ContainsKey(address.Host))
                        domainGroup.Add(address.Host, false);
                }
                foreach (MailAddress address in message.Bcc)
                {
                    if (!domainGroup.ContainsKey(address.Host))
                        domainGroup.Add(address.Host, false);
                }

                //不支持多域
                if (domainGroup.Count > 1)
                    throw new SmtpException("no proxy smtp not support multi domain");

                //按域发送
                foreach (KeyValuePair<string, bool> item in domainGroup)
                {
                    this.SendNoPorxy(item.Key, message);
                }
            }

            /// <summary>
            /// 按域进行无代理发送
            /// </summary>
            /// <param name="domain"></param>
            /// <param name="message"></param>
            /// <exception cref="Adf.SmtpException"></exception>
            public void SendNoPorxy(string domain, MailMessage message)
            {
                const int port = 25;

                //mx
                MXRecordItem mxe = null;
                DnsRecord record = DnsRecord.EMPTY;
                //
                lock (this.mxrecords)
                {
                    if (this.mxrecords.TryGetValue(domain, out mxe))
                    {
                        try
                        {
                            record = mxe.recordList[mxe.index];
                            if (Environment.TickCount - record.Expired > 0)
                            {
                                //set null, trigger dns query
                                mxe = null;
                            }
                        }
                        catch
                        {
                            mxe = null;
                        }
                    }
                }

                //query
                if (mxe == null)
                {
                    mxe = new MXRecordItem();
                    mxe.recordList = DnsHelper.GetMXRecordList(domain);
                    mxe.index = 0;
                    if (mxe.recordList == null || mxe.recordList.Count == 0)
                    {
                        throw new Adf.SmtpException("dns no found mx record " + domain);
                    }

                    //
                    lock (this.mxrecords)
                    {
                        this.mxrecords[domain] = mxe;
                    }
                }

                Exception firstException;
                try
                {
                    new SmtpClient(record.Value, port).Send(message);
                    //成功发送退出
                    return;
                }
                catch (Exception exception)
                {
                    //当为 socket 异常时，允许使用其它MX
                    if (exception.GetBaseException() is SocketException)
                    {
                        if (mxe.recordList.Count == 1)
                            throw;

                        firstException = exception;
                    }
                    else
                    {
                        throw;
                    }
                }

                //首选失败后重新选择mx服务器
                for (int i = 0, l = mxe.recordList.Count; i <= l; i++)
                {
                    try
                    {
                        new SmtpClient(mxe.recordList[i].Value, port).Send(message);
                        //发送成功，修改首选
                        mxe.index = i;
                        //发送成功退出
                        return;
                    }
                    catch (Exception exception)
                    {
                        if (exception.GetBaseException() is SocketException)
                        {
                            //当为 socket 异常时，允许使用其它MX
                            firstException = exception;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                //当首选服务器为列表中最后一个服务器且重试失败时将首选异常丢出.
                throw firstException;
            }
        }


        class MXRecordItem
        {
            /// <summary>
            /// 所有DNS列表
            /// </summary>
            public List<DnsRecord> recordList;
            /// <summary>
            /// 当前使用的索引序号
            /// </summary>
            public int index;
        }
    }

    /// <summary>
    /// Smtp Exception
    /// </summary>
    public class SmtpException : Exception
    {
        /// <summary>
        /// initialize
        /// </summary>
        /// <param name="message"></param>
        public SmtpException(string message)
            : base(message)
        {

        }
    }
}