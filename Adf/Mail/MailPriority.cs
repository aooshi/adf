using System;

namespace Adf.Mail
{
    /// <summary>
    /// 指定邮件优先级。
    /// </summary>
    public enum MailPriority
    {
        /// <summary>
        /// 此电子邮件具有正常优先级。
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 此电子邮件具有低优先级。
        /// </summary>
        Low = 1,
        /// <summary>
        /// 此电子邮件具有高优先级。
        /// </summary>
        High = 2,
    }
}
