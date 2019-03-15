using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// adf config exception
    /// </summary>
    public class ConfigException : Exception
    {
        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="message"></param>
        public ConfigException(string message)
            : base(message)
        {
        }
    }
}
