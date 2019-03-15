using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Adf.Mail;

namespace AdfConsoleTest
{
    public class SmtpSelfTest
    {
        public void Test()
        {
            var to = "ins-u6xh0nlj@isnotspam.com";
            to = "xb.li@example.com";


            var build = new StringBuilder();
            for (int i = 0; build.Length < 512; i++)
            {
                build.AppendLine(Guid.NewGuid().ToString("N").PadRight(64, '0'));
                build.AppendLine(DateTime.UtcNow.Ticks.ToString("x"));
            }

            var message = new MailMessage();
            message.AddTo(to);
            message.Subject = Guid.NewGuid().ToString("N");
            message.Body = build.ToString();

            
            //message.IsBodyHtml = true;
            message.From = new MailAddress("fdsfasffds@beta.englory.net");

            //message.Save("c:\\1.log");

            var smtpClient = new SmtpClient("mx1.qq.com", 25);
            smtpClient = new SmtpClient("isnotspam.com", 25);
            smtpClient.SelfHost = "beta.englory.net";

            smtpClient.Send(message);
            
            Console.WriteLine("completed");
        }
    }
}
