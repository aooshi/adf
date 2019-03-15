using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Adf;

namespace AdfConsoleTest
{
    public class SocketTest
    {

        public static void Init()
        {

            System.Threading.ThreadPool.QueueUserWorkItem(state =>
            {
                var listener = new TcpListener(100);
                listener.Start();
                var socket = listener.AcceptSocket();
                //System.Threading.Thread.Sleep(10000);
                socket.Send(new byte[] { 2, 3 });
                //socket.Close();
            });


            var client = new TcpClient();
            client.Connect("127.0.0.1", 100);
            var stream = client.GetStream();
            var buffer = new byte[1];
            stream.BeginRead(buffer, 0, 1, ar =>
            {

                Console.WriteLine( stream.DataAvailable );
                buffer = StreamHelper.Receive(stream, 2);
                Console.WriteLine(  buffer.Length );

                ar.AsyncWaitHandle.Close();

            }, null);

        }
    }
}