using System;
using System.Collections.Generic;
using System.Text;

namespace Adf.Config
{
    /// <summary>
    /// Smtp Config Handler
    /// </summary>
    public class SmtpConfig : ConfigValue
    {
        /// <summary>
        /// get default instance, for smtp.config
        /// </summary>
        public static readonly SmtpConfig Instance = new SmtpConfig();

        /// <summary>
        /// host, config:SmtpHost
        /// </summary>
        public string Host { get { return this["SmtpHost"]; } }

        int port = 25;
        /// <summary>
        /// port, config:SmtpPort
        /// </summary>
        public int Port
        {
            get { return this.port; }
        }

        /// <summary>
        /// Sender, config:SmtpSender
        /// </summary>
        public string Sender { get { return this["SmtpSender"]; } }

        /// <summary>
        /// Sender Name ,config: SmtpName
        /// </summary>
        public string Name { get { return this["SmtpName"]; } }

        /// <summary>
        /// Account ,config:SmtpAccount
        /// </summary>
        public string Account { get { return this["SmtpAccount"]; } }

        /// <summary>
        /// Password, config:SmtpPassword
        /// </summary>
        public string Password { get { return this["SmtpPassword"]; } }

        /// <summary>
        /// Enable,config:SmtpEnabled
        /// </summary>
        /// <returns></returns>
        public bool Enabled { get { return this["SmtpEnabled"] == "true"; } }

        /// <summary>
        /// SSL Enable,config:SmtpSSLEnabled
        /// </summary>
        /// <returns></returns>
        public bool SSLEnabled { get { return this["SmtpSSLEnabled"] == "true"; } }
        /// <summary>
        /// TLS Enable,config:SmtpTLSEnable, default true
        /// </summary>
        /// <returns></returns>
        public bool TLSEnabled { get { return this["SmtpTLSEnable"] != "false"; } }


        /// <summary>
        /// initialize new smtp.config instance
        /// </summary>
        private SmtpConfig()
            : base("Smtp.config")
        {
            base.AddWatcher();
            //
            int port = 0;
            if (int.TryParse(this["SmtpPort"], out port))
            {
                this.port = port;
            }
            base.Changed += new EventHandler(SmtpConfig_Changed);
        }

        private void SmtpConfig_Changed(object sender, EventArgs e)
        {
            var port = 0;
            if (int.TryParse(this["SmtpPort"], out port))
            {
                this.port = port;
            }
        }
    }
}