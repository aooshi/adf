using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AdfConsoleTest
{
    class SocketConnectionTest
    {
        public void Test()
        {
            //v 
            var r = new Random();

            //server
            var listener = new Adf.SocketConnection.SocketListener("0.0.0.0:5624");
            //print log
            listener.LogAgent.Writing += (object sender, Adf.LogEventArgs e) =>
            {
                Console.WriteLine(e.Content);
            };
            //print error
            listener.Error += (object sender, Adf.SocketConnection.ErrorEventArgs e) =>
            {
                Console.WriteLine(e.Exception.ToString());
            };
            //new connection
            listener.NewConnection += new EventHandler<Adf.SocketConnection.ConnectionEventArgs>(listener_Connectioned);
            //start listen
            listener.Listen();


            //
            var client = new Adf.SocketConnection.SocketClient("127.0.0.1:5624");
            this.InitClient(client);
            //client connection to server
            client.Connect();
            //client read start
            client.Read();


            for(int i=0;i<int.MaxValue;i++)
            {
                System.Threading.Thread.Sleep(1000);

                var msg = "rand:num:" + r.NextDouble().ToString() + "\0";

                client.WriteUTF8String(msg);
                
                Console.WriteLine("Client Send: 0x{0:X4}, {1}", client.Id, msg);

                //client.WriteUTF8String("rand:num:" + r.NextDouble().ToString());
                //Console.Read();


                if (((int)(r.NextDouble() * 100)) % 10 == 0)
                {
                    Console.WriteLine("Client Close");
                    client.Close();

                    System.Threading.Thread.Sleep(1000);

                    client = new Adf.SocketConnection.SocketClient("127.0.0.1:5624");
                    this.InitClient(client);
                    client.Connect();
                    client.Read();
                }

            }



            Console.Read();
        }

        private void InitClient(Adf.SocketConnection.SocketClient client)
        {
            client.Id = Adf.SocketConnection.SocketIdentifier.Generator.NewSessionId();

            //client disconnectioned
            client.Disconnectioned += (object sender, EventArgs e) =>
            {
                var connection = (Adf.SocketConnection.SocketConnection)sender;
                Console.WriteLine("Client Connection: 0x{0:X4} Disconnection", connection.Id);
            };
            //client error
            client.Error += (object sender, Adf.SocketConnection.ErrorEventArgs e) =>
            {
                var connection = (Adf.SocketConnection.SocketConnection)sender;
                Console.WriteLine("Client Connection: 0x{0:X4}, {1}", connection.Id, e.Exception.ToString());
            };
            //client new log
            client.Log += (object sender, Adf.LogEventArgs e) =>
            {
                var connection = (Adf.SocketConnection.SocketConnection)sender;
                Console.WriteLine("Client Connection: 0x{0:X4}, {1}", connection.Id, e.Content);
            };
            //client new message
            client.Message += (object sender, Adf.SocketConnection.MessageEventArgs e) =>
            {
                var connection = (Adf.SocketConnection.SocketConnection)sender;
                Console.WriteLine("Client Connection: 0x{0:X4}, {1}\r\n\r\n", connection.Id, e.Message);
                //set is continue
                //e.IsContinue = true;
            };
        }

        void listener_Connectioned(object sender2, Adf.SocketConnection.ConnectionEventArgs e2)
        {
            var conn = e2.Connection;
            //conn disconnectioned
            conn.Disconnectioned += (object sender, EventArgs e) =>
            {
                var connection = (Adf.SocketConnection.SocketConnection)sender;
                Console.WriteLine("Server Connection: 0x{0:X4} Disconnection", connection.Id);
            };
            //conn error
            conn.Error += (object sender, Adf.SocketConnection.ErrorEventArgs e) =>
            {
                var connection = (Adf.SocketConnection.SocketConnection)sender;
                Console.WriteLine("Server Connection: 0x{0:X4}, {1}", connection.Id, e.Exception.ToString());
                
                //必需的

                if (e.Exception is Adf.SocketConnection.ParserException)
                {
                    //终止
                    ((Adf.SocketConnection.SocketConnection)sender).Close();
                }
            };
            //conn new log
            conn.Log += (object sender, Adf.LogEventArgs e) =>
            {
                var connection = (Adf.SocketConnection.SocketConnection)sender;
                Console.WriteLine("Server Connection: 0x{0:X4}, {1}", connection.Id, e.Content);
            };
            //conn new message
            conn.Message += (object sender, Adf.SocketConnection.MessageEventArgs e) =>
            {
                var connection = (Adf.SocketConnection.SocketConnection)sender;
                Console.WriteLine("Server Connection: 0x{0:X4}, {1}", connection.Id, e.Message);
                //set is continue
                //e.IsContinue = true;

                try
                {
                    connection.WriteUTF8String("OK:" + e.Message + "\0");
                }
                catch (IOException)
                {
                    //is disconnection
                }
            };
        }


    }
}
