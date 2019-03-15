using System;

namespace Adf.Mail
{
    /// <summary>
    /// 邮件用户
    /// </summary>
    public class MailAddress
    {
        /// <summary>
        /// 使用指定的地址初始化新实例。
        /// </summary>
        /// <param name="mailAddress">包含电子邮件地址。</param>
        /// <exception cref="ArgumentException">address invalid</exception>
        public MailAddress(string mailAddress)
        {
            this.Address = mailAddress;
            //
            var items = mailAddress.Split('@');
            if (items.Length != 2)
            {
                throw new ArgumentException("address invalid", "address");
            }
            this.User = items[0];
            this.Host = items[1];
            //
            if (string.IsNullOrEmpty(items[0]) || string.IsNullOrEmpty(items[1]))
            {
                throw new ArgumentException("address invalid", "address");
            }
        }

        /// <summary>
        /// 使用指定的地址初始化新实例。
        /// </summary>
        /// <param name="name">邮件显示名</param>
        /// <param name="mailAddress">包含电子邮件地址。</param>
        /// <exception cref="ArgumentException">address invalid</exception>
        public MailAddress(string mailAddress, string name)
            : this(mailAddress)
        {
            this.Name = name;
        }

        /// <summary>
        /// 获取创建此实例时指定的电子邮件地址。
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// 获取创建此实例时指定的地址的主机部分。
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// 获取创建此实例时指定的地址中的用户信息。
        /// </summary>
        public string User { get; private set; }

        /// <summary>
        /// 获取创建此实例时指定的地址中的用户显示名。
        /// </summary>
        public string Name { get; private set; }
    }
}
