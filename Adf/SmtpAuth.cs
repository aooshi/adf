using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Adf
{
    internal class SmtpAuth
    {
        public SmtpAuth(string host, int port, string username, string password, bool ssl)
        {
            this.Success = false;
            
            using (var tcp = new TcpClient(host, port))
            using (var ns = tcp.GetStream())
            {
                if (ssl)
                {
                    var sslstream = new SslStream(ns);
                    sslstream.AuthenticateAsClient(host);
                    this.Login(sslstream, username, password);
                }
                else
                {
                    this.Login(ns, username, password);
                }

                this.Send(ns, "QUIT\r\n");
            }
        }

        private void Login(Stream ns,string username,string password)
        {
            var result = this.Read(ns);
            if (!result.StartsWith("220"))
            {
                this.Message = "Connect - " + result;
                return ;
            }
            //
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                this.Send(ns, string.Format("EHLO {0}\r\n", System.Net.Dns.GetHostName()));
            else
                this.Send(ns, string.Format("HELO {0}\r\n", System.Net.Dns.GetHostName()));
            //
            result = this.Read(ns);
            if (!result.StartsWith("250"))
            {
                this.Message = "EHLO/HELO - " + result;
                return ;
            }
            //
            result = this.EhloResult(ns);
            if (!result.StartsWith("250"))
            {
                this.Message = "EHLO/HELO List - " + result;
                return ;
            }

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                this.Send(ns, "AUTH LOGIN\r\n");
                result = this.Read(ns);
                if (!result.StartsWith("334"))
                {
                    this.Message = "AUTH LOGIN - " + result;
                    return ;
                }
                //
                this.Send(ns, Convert.ToBase64String(Encoding.ASCII.GetBytes(username)) + "\r\n");
                result = this.Read(ns);
                if (!result.StartsWith("334"))
                {
                    this.Message = "UserName - " + result;
                    return ;
                }
                //
                this.Send(ns, Convert.ToBase64String(Encoding.ASCII.GetBytes(password)) + "\r\n");
                result = this.Read(ns);
                if (!result.StartsWith("235"))
                {
                    this.Message = "Password - " + result;
                    return ;
                }
            }

            this.Success = true;
        }

        public string Message
        {
            get;
            private set;
        }

        public bool Success
        {
            get;
            private set;
        }

        void Send(Stream ns, string sendString)
        {
            var buffer = Encoding.ASCII.GetBytes(sendString);
            ns.Write(buffer, 0, buffer.Length);
        }
        string Read(Stream ns)
        {
            var buffer = StreamHelper.ReadLine(ns);
            var result = Encoding.ASCII.GetString(buffer.Array, 0, buffer.Count);
            System.Diagnostics.Debug.WriteLine(result);
            return result;
        }
        string EhloResult(Stream ns)
        {
            var str = this.Read(ns);
            while (!str.StartsWith("250 "))
            {
               //tls = tls || str == "250-STARTTLS";
                str = this.Read(ns);
            }
            return str;
        }
    }
}
