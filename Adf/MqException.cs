using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Messaging;
using System.IO;
using System.Threading;

namespace Adf
{
    /// <summary>
    /// TaskQueue Exception
    /// </summary>
    public class MqException : Exception
    {
        /// <summary>
        /// 初始新实例
        /// </summary>
        /// <param name="message"></param>
        public MqException(string message)
            : base(message)
        {
        }
    }
}
