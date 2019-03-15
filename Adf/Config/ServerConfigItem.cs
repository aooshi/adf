using System;
using System.Collections.Generic;
using System.Text;

namespace Adf.Config
{
    /// <summary>
    /// Server Config Item
    /// </summary>
    public class ServerConfigItem
    {
        /// <summary>
        /// Ip Address
        /// </summary>
        public string Ip
        {
            get;
            set;
        }

        /// <summary>
        /// port
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// config level
        /// </summary>
        public int Level
        {
            get;
            set;
        }

        /// <summary>
        /// description
        /// </summary>
        public string Description
        {
            get;
            set;
        }
    }
}
