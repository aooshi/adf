using System;
using System.Collections.Generic;
using System.Text;
using Adf.Config;
using System.Net.Mail;
using System.Net;

namespace Adf
{
    /// <summary>
    /// Smtp Helper
    /// </summary>
    public interface ISmtp
    {
        /// <summary>
        /// host
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// port
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Sender
        /// </summary>
        string Sender { get; set; }

        /// <summary>
        /// Sender Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Account
        /// </summary>
        string Account { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        /// <returns></returns>
        bool Enabled { get; }

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        bool SSLEnabled { get; set; }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="message">消息体，若未填充From属性，则将以配置填充</param>
        /// <returns></returns>
        void Send(MailMessage message);
    }
}
