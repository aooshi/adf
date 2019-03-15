using System;
using System.Collections.Generic;
using System.Text;
using Adf;
using System.Net;
using System.Threading;

namespace AdfConsoleTest
{
    public class HttpServerTest
    {

        public HttpServerTest()
        {
            var server = new Adf.HttpServer(8080);
            server.Callback = this.HttpServerCallback;
            server.AuthorizationCallback = this.HttpServerAuthorizationCallback;
            server.Start();

            Console.WriteLine("Port:{0}", server.Port);
            Console.ReadLine();
        }

        HttpStatusCode HttpServerCallback(HttpServerContext context)
        {
            Console.WriteLine("Request Path:{0},{1}", context.Path, Guid.NewGuid());

            context.ResponseHeader["content-type"] = "text/html;charset=utf8";

            //异步块输出
            if (context.Path == "/chunk")
            {
                context.BeginWrite();
                try
                {
                    context.Write("同步块输出<br />");
                    var i = 0;
                    while (i++ < 6)
                    {
                        Thread.Sleep(1000);
                        context.Write(DateTime.Now.ToString() + "<br />");
                    }
                }
                finally
                {
                    //必需确保调用
                    context.EndWrite();
                }
                return HttpStatusCode.OK;
            }
            //模拟异步块输出
            else if (context.Path == "/async")
            {
                context.BeginWrite();
                ThreadPool.QueueUserWorkItem((object o) =>
                {
                    try
                    {
                        context.Write("异步块输出<br />");
                        var i = 0;
                        while (i++ < 6)
                        {
                            Thread.Sleep(1000);
                            context.Write(DateTime.Now.ToString() + "<br />");
                        }
                    }
                    finally
                    {
                        //必需确保调用
                        context.EndWrite();
                    }
                });
                return HttpStatusCode.OK;
            }
            //内容输出
            else
            {
                //Thread.Sleep(300);
                context.Content = string.Format("Request Path:{0}", context.Path);
                return HttpStatusCode.OK;
            }
        }

        bool HttpServerAuthorizationCallback(HttpServerContextBase context)
        {
            return true;
        }
    }
}