using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.IO;

namespace AdfConsoleTest
{
    public class SmtpTest
    {
        public void Test()
        {
            var to = "xb.li@example.com";
            
            var mailMessage  = new MailMessage();
            mailMessage.To.Add(to);
            mailMessage.Subject = Guid.NewGuid().ToString("N");
            mailMessage.Body = File.ReadAllText("c:\\windows\\DirectX.log") + DateTime.Now.ToString();

            Adf.Smtp.Instance.Enabled = true;
            Adf.Smtp.Instance.Send(mailMessage);

            Console.WriteLine("ok");
        }
    }
}
