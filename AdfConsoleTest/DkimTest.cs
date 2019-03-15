using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
    class DkimTest
    {
        public void Test()
        {
            var host = "192.168.199.10";
            var port = 25;

            var from = "e4ed22af7@caping.co.id";
            var to = "4234523@qq.com";
            to = "test-763047a4@appmaildev.com";

            var domain = "caping.co.id";
            var selector = "newsletter17110";

            var dkim = new Adf.Mail.DKIM(domain, selector);
            dkim.LoadKey(@"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQCxcjbutjZfE1trW5oFt7t4AnjDRHeHwbi2AGE5n1M8YZSO2fGi
fBnsSy/qNoaKwoROhNl9S0mya7Q5odloyN3IEVoUCZjnd3onTsZ4vmXD/Ei4r0+S
/ZdUxVtMxQoqRd/4NgW0M+OyGez2rNPfvbF18aa0RdIZxaLRDcpp4RZ1rwIDAQAB
AoGAT4/fk479uAmM3wk2eUPVeczZ6uvjEGrK8EghT93hS1yRaK/OCUXNtcZMmJ6U
...
beMUEFuoc9NvfOLP7RkCQQCcow3n3YOX/yJB+2EN2272Uo07hbVxcA7sbUYLfYuh
ba3kqofe7BP7QpMwqZmLALDngIp4htRrTYFehzZ6zavB
-----END RSA PRIVATE KEY-----
");

            var message = new Adf.Mail.MailMessage();
            message.Subject = "You password expired";
            message.IsBodyHtml = false;
            message.Body = @"hi\r\nyou password expired, please check.\r\n\r\n services.";

            message.From = new Adf.Mail.MailAddress(from);
            message.To.Add(new Adf.Mail.MailAddress(to));

            message.Dkim = dkim;

            //message.Save("c:\\" + message.MessageId.Replace("@", "_") + ".eml");
            //message.Save(Console.OpenStandardOutput());

            using (var smtpClient = new Adf.Mail.SmtpClient(host, port))
            {
                smtpClient.Send(message);
            }

            Console.WriteLine(message.MessageId.Replace("@", "_"));
            Console.WriteLine("Send a mail, entry continue.");
            Console.ReadLine();
        }
    }
}