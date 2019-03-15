using System;
using System.Collections.Generic;
using System.Text;
using Adf;
using System.Net;
using System.Threading;

namespace AdfConsoleTest
{
    public class HttpServerTest2
    {

        public HttpServerTest2()
        {
            var server = new Adf.HttpServer(8080);
            server.Callback = this.HttpServerCallback;
            server.Start();

            Console.WriteLine("Port:{0}", server.Port);

            WebClient wc;
            string content;
            wc = new WebClient();
            content = wc.DownloadString("http://127.0.0.1:8080/");
            Console.WriteLine("Server Time:" + content);

            wc = new WebClient();
            content = wc.DownloadString("http://127.0.0.1:8080/");
            Console.WriteLine("Server Time:" + content);



            wc = new WebClient();
            content = wc.UploadString("http://127.0.0.1:8080/?a=b", "POST", "value=x");
            Console.WriteLine("Server Time:" + content);

            wc = new WebClient();
            content = wc.UploadString("http://127.0.0.1:8080/?a=b", "POST", "value=x");
            Console.WriteLine("Server Time:" + content);


            //wc = new WebClient();
            //content = server.Encoding.GetString(wc.UploadFile("http://127.0.0.1:8080/", "POST", "c:\\windows\\explorer.exe"));
            //Console.WriteLine("File:" + content);

            //wc = new WebClient();
            //content = server.Encoding.GetString(wc.UploadFile("http://127.0.0.1:8080/", "POST", "c:\\windows\\explorer.exe"));
            //Console.WriteLine("File:" + content);

            Console.ReadLine();
        }

        HttpStatusCode HttpServerCallback(HttpServerContext context)
        {
            context.ResponseHeader["Content-Type"] = "text/html; charset=utf-8";

            if (context.Files.Count == 0)
            {
                context.Content = Adf.UnixTimestampHelper.ToTimestamp().ToString();
                if (context.Method == "POST")
                {
                    Console.WriteLine("POST:" + context.Form["value"]);
                    context.Content = "";
                    foreach (var key in context.Form.AllKeys)
                    {
                        context.Content += key + "\t" + context.Form[key] + "<br />";
                    }
                }
            }
            else
            {
                //Console.WriteLine("FILE:" + context.Files[0].FileName);
                context.Content = "";
                foreach (var file in context.Files)
                {
                    context.Content += file.Name + "\t" + file.FileName + "\t" + file.ContentType + "\t" + file.Stream.Length + "<br />";
                    //file.Save("c:\\" + file.FileName);
                }
            }

            return HttpStatusCode.OK;
        }
    }
}