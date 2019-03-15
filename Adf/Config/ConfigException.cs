using System;
using System.Collections.Generic;
using System.Text;

namespace Adf.Config
{
    /// <summary>
    /// 配置错误, configuraion error
    /// </summary>
    public class ConfigException : Exception
    {
        /// <summary>
        /// initialzie new instance
        /// </summary>
        /// <param name="message"></param>
        public ConfigException(string message)
            : base(message) { }
        /// <summary>
        /// initialzie new instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ConfigException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}