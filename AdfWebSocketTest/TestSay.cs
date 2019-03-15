using System;
using System.Text;
using System.Net;
using Adf;
using System.Threading;

namespace AdfWebSocketTest
{
    class TestSay
    {
        static int v = 0;

        public static void Test()
        {

            var server = new Adf.HttpServer(WebCallback, 888);

            //设置新连接回调
            server.WebSocketConnectioned += Connection;
            server.WebSocketDisconnected += Disconnect;
            server.WebSocketNewMessage += Message;

            server.Start();

            using (var client = new Adf.WebSocketClient("127.0.0.1", 888))
            {
                client.Message += (object sender, WebSocketMessageEventArgs e) =>
                {
                    if (e.Opcode == WebSocketOpcode.Text)
                    {
                        Console.WriteLine("C Receive: {0}", e.Message);
                    }
                    else
                    {
                        Console.WriteLine("C Receive Opcode:{0}", e.Opcode);
                    }
                };

                client.Connection();

                while (client.IsConnectioned)
                {
                    var msg = "m" + DateTime.Now.Ticks.ToString();

                    Console.WriteLine();
                    Console.WriteLine("C Say " + msg);

                    client.Send(msg);
                    Thread.Sleep(1000);
                }
                Console.WriteLine("Close");
            }


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
            if (e.Opcode == WebSocketOpcode.Text)
            {
                Console.WriteLine("S Receive: {0} Say {1}", context.UserState, e.Message);

                if (e.Message == "说:16")
                {
                    context.Send(string.Format("Receive:{0}, Closed", e.Message));
                    //停止
                    context.Close();
                }
                else
                {
                    context.Send(string.Format(context.UserState + " Say {0}", e.Message));
                }
            }
            else
            {
                Console.WriteLine("{0} Opcode:{1}", context.UserState, e.Opcode);
            }
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
