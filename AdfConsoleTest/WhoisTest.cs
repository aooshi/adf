using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AdfConsoleTest
{
    public class WhoisTest
    {
        public static void Test()
        {
            var tcpClient = new System.Net.Sockets.TcpClient();
            var bytes = new byte[0];


            //tcpClient.Connect("whois.cnnic.net.cn", 43);
            //bytes = Encoding.ASCII.GetBytes("cnnic.cn.\r\n");

            //tcpClient.Connect("whois.pandi.or.id", 43);
            //bytes = Encoding.ASCII.GetBytes("pandi.or.id\r\n");

            tcpClient.Connect("whois.crsnic.net", 43);
            bytes = Encoding.ASCII.GetBytes("internic.net\r\n");
            //bytes = Encoding.ASCII.GetBytes("icann.org\r\n");


            using (var stream = tcpClient.GetStream())
            {
                stream.Write(bytes, 0, bytes.Length);

                using (var m = new System.IO.MemoryStream())
                {
                    var buffer = new byte[2048];
                    var length = 0;
                    do
                    {
                        length = stream.Read(buffer, 0, buffer.Length);

                        if (length == 0)
                            break;

                        m.Write(buffer, 0, length);
                    }
                    while (stream.DataAvailable);

                    Console.WriteLine("RESULT:");

                    //
                    var body = Encoding.UTF8.GetString(m.ToArray());
                    Console.WriteLine(body);


                    //
                    string line = null;
                    using (var r = new StringReader(body))
                    {
                        while(true)
                        {
                            line = r.ReadLine();
                            if (line == null)
                                break;

                            line = line.Trim();

                            if (line.ToUpper().StartsWith("NAME SERVER:"))
                            {
                                var arr = line.Split(':');
                                if (arr.Length == 2)
                                {
                                    Console.WriteLine("NAME SERVER:" + (arr[1] + "").Trim());
                                }
                            }
                        }
                    }
                }
            }

        }
    }
}