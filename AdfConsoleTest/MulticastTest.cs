using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace AdfConsoleTest
{
    public class MulticastTest
    {
        public void Test()
        {
            //Multicast

            var senderThread = new System.Threading.Thread(this.SenderThreadCallback);
            var receiverThread = new System.Threading.Thread(this.ReceiveThreadCallback);
            //
            senderThread.IsBackground = true;
            receiverThread.IsBackground = true;


            Int32 port = 239;
            EndPoint bindPoint = new IPEndPoint(IPAddress.Any, port);

            String multicastIpString = "239.238.237.236";
            IPAddress multicastIpAddress = IPAddress.Parse(multicastIpString);
            EndPoint multicastPoint = new IPEndPoint(multicastIpAddress, port);

            //239.0.0.0～239.255.255.255
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(bindPoint);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastIpAddress));

            var state = new Dictionary<string, object>();
            state.Add("port", port);
            state.Add("bindPoint", bindPoint);
            state.Add("multicastIpString", multicastIpString);
            state.Add("multicastIpAddress", multicastIpAddress);
            state.Add("multicastPoint", multicastPoint);
            state.Add("socket", socket);

            //
            senderThread.Start(state);
            receiverThread.Start(state);
        }

        private void SenderThreadCallback(object stateObject)
        {
            var state = (Dictionary<string, object>)stateObject;

            Int32 port = Convert.ToInt32(state["port"]);
            EndPoint bindPoint = (EndPoint)state["bindPoint"];

            String multicastIpString = (String)state["multicastIpString"];
            IPAddress multicastIpAddress = (IPAddress)state["multicastIpAddress"];
            EndPoint multicastPoint = (EndPoint)state["multicastPoint"];
            Socket socket = (Socket)state["socket"];

            for (long i = 0; i < long.MaxValue; i++)
            {
                System.Threading.Thread.Sleep(1000);
                byte[] buffer = Encoding.ASCII.GetBytes("multicast test " + i);
                //
                socket.SendTo(buffer, multicastPoint);
            }
        }

        private void ReceiveThreadCallback(object stateObject)
        {
            var state = (Dictionary<string, object>)stateObject;

            Int32 port = Convert.ToInt32(state["port"]);
            EndPoint bindPoint = (EndPoint)state["bindPoint"];

            String multicastIpString = (String)state["multicastIpString"];
            IPAddress multicastIpAddress = (IPAddress)state["multicastIpAddress"];
            EndPoint multicastPoint = (EndPoint)state["multicastPoint"];
            Socket socket = (Socket)state["socket"];

            while (true)
            {
                byte[] buffer = new byte[1024];
                int size = socket.ReceiveFrom(buffer, ref multicastPoint);
                if (size == 0)
                {
                    break;
                }
                string msg = Encoding.Default.GetString(buffer, 0, size);
                Console.WriteLine(msg);
            }
        }
    }
}