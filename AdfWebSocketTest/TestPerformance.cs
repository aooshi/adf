using System;
using System.Text;
using Adf;
using System.Net;
using System.Threading;

namespace AdfWebSocketTest
{
    class TestPerformance
    {
        static int v = 0;
        static int di = 0;
        static byte[][] datas = new byte[4][];

        public static void Test()
        {
            datas[0] = new byte[511];
            datas[1] = new byte[1024];
            datas[2] = new byte[4097];
            datas[3] = new byte[9555];

            //
            var server = new Adf.HttpServer(WebCallback, 888);

            //设置新连接回调
            server.WebSocketConnectioned += Connection;
            server.WebSocketDisconnected += Disconnect;
            server.WebSocketNewMessage += Message;

            server.Start();

            Console.ReadLine();
            server.Stop();
        }

        static HttpStatusCode WebCallback(HttpServerContext context)
        {
            context.ContentBuffer = System.IO.File.ReadAllBytes(System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Demo.html"));

            return HttpStatusCode.OK;

        }

        static void Message(HttpServerWebSocketContext context, WebSocketMessageEventArgs e)
        {
            //Console.WriteLine("Receive {0} Opcode:{1}", context.UserState, e.Opcode);

            //if (e.Opcode == WebSocketOpcode.Text)
            //{
            //if (e.Message == "1")
            //{
            //    Console.WriteLine("New Received");

            //    System.Threading.ThreadPool.QueueUserWorkItem(a =>
            //    {
            //        while (true)
            //        {
            //            if (context.Socket.Connected == false)
            //            {
            //                break;
            //            }

            //            try
            //            {
            //                context.Send(new byte[1024]);
            //            }
            //            catch
            //            {
            //                Console.WriteLine("Response but closed");
            //            }
            //        }

            //        Console.WriteLine("Response but disconnection");
            //        Console.WriteLine();

            //    });
            //}
            //}

            //System.Threading.ThreadPool.QueueUserWorkItem(us =>
            //{

                var index = System.Threading.Interlocked.Increment(ref di);

                try
                {
                    //context.Send(datas[index % 4]);
                    //context.Send(new byte[1024]);

                    if (e.Opcode == WebSocketOpcode.Text)
                    {
                        Console.WriteLine("Recv: " + e.Message);

                        context.Send("Msg:" + index);
                    }
                    else if (e.Opcode == WebSocketOpcode.Binary)
                    {
                        Console.WriteLine("Recv a binary: " + e.Message);
                    }
                    else
                    {
                        Console.WriteLine("Recv: " + e.Opcode);
                    }

                }
                catch (Exception exception)
                {
                    Console.WriteLine("Response err: " + exception.Message);
                }

            //});

        }

        static void Disconnect(HttpServerWebSocketContext context)
        {
            Console.WriteLine("Disconnect:{0}", context.UserState);
        }

        static void Connection(HttpServerWebSocketContext context)
        {
            context.UserState = v++;

            Console.WriteLine("New Connect,User:{0},Path:{1}", context.UserState, context.Url);

            //允许连接
            //context.Allowed = false;
        }
    }
}