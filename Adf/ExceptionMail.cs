using System;
using System.Collections.Generic;
using System.Text;
using Adf.Config;
using System.Net;

namespace Adf
{
    /// <summary>
    /// 异常邮件发送器
    /// </summary>
    /// <remarks>
    /// 配置： ExceptionMailRecipients, 以分号分隔的邮件接收人
    /// 配置： ExceptionMailSmtp, 指定发送邮件实例，此实例实现, ISmtp接口
    /// </remarks>
    public class ExceptionMail
    {
        /// <summary>
        /// 默认实例
        /// </summary>
        public static readonly ExceptionMail Instance = new ExceptionMail();

        /// <summary>
        /// 异常邮件接收人,配置： ExceptionMailRecipients, 以分号分隔的邮件接收人
        /// </summary>
        public string[] MailRecipients
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Available
        {
            get
            {
                return Smtp.Enabled && MailRecipients.Length > 0;
            }
        }

        /// <summary>
        /// 获取当前发送SMTP实例, 通过ExceptionMailSmtp配置初始实例
        /// </summary>
        public ISmtp Smtp
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始新实例
        /// 配置：ExceptionMailRecipients, 邮件地址，以逗号分隔
        /// 配置：ExceptionMailSmtp, 实现 ISmtp接口类型
        /// </summary>
        public ExceptionMail()
        {
            var recipients = GlobalConfig.Instance["ExceptionMailRecipients"] ?? "";
            if (string.IsNullOrEmpty(recipients))
            {
                MailRecipients = new string[0];
            }
            else
            {
                var parts = recipients.Split(';');
                var list = new List<string>(parts.Length);
                foreach (var part in parts)
                {
                    if (string.IsNullOrEmpty(part) == false)
                    {
                        list.Add(part);
                    }
                }
                MailRecipients = list.ToArray();
            }
            //
            var exceptionMailSmtp = GlobalConfig.Instance["ExceptionMailSmtp"] ?? "";
            if (!string.IsNullOrEmpty(exceptionMailSmtp))
            {
                var type = Type.GetType(exceptionMailSmtp, false);
                if (type == null)
                {
                    throw new System.Configuration.ConfigurationErrorsException("AppSetting ExceptionMailSmtp Error");
                }
                Smtp = Activator.CreateInstance(type) as ISmtp;
            }
            else
            {
                Smtp = Adf.Smtp.Instance;
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>返回是否执行了邮件发送</returns>
        /// <exception cref="ArgumentNullException">exception</exception>
        public bool SendMail(Exception exception)
        {
            if (!this.Available)
                return false;

            if (exception == null)
                throw new ArgumentNullException("exception");

            var message = new System.Net.Mail.MailMessage();
            message.Subject = exception.ToString(); //string.Format("{0} Exception", ConfigHelper.AppName);
            message.Body = string.Concat("HOST: ", Dns.GetHostName(), StringHelper.CRLF, exception.ToString());
            message.IsBodyHtml = false;
            //
            for (int i = 0, l = this.MailRecipients.Length; i < l; i++)
            {
                message.To.Add(this.MailRecipients[i]);
            }
            //
            this.Smtp.Send(message);

            return true;
        }
    }
}