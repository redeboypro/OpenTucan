using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TucanFPS
{
    public class Client
    {
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private Thread receiveThread;

        public void Start(string serverAddress, int serverPort)
        {
            udpClient = new UdpClient();

            IPAddress ipAddress = IPAddress.Parse(serverAddress);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 0);
            udpClient.Client.Bind(localEndPoint);

            serverEndPoint = new IPEndPoint(ipAddress, serverPort);

            receiveThread = new Thread(new ThreadStart(ReceiveMessages));
            receiveThread.Start();
        }

        public void Stop()
        {
            udpClient.Close();
            receiveThread.Join();
        }

        public void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, serverEndPoint);
            Console.WriteLine("Sent: " + message);
        }

        private void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);;
                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    string message = Encoding.UTF8.GetString(data);
                    Console.WriteLine("Received: " + message);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException: " + ex.Message);
            }
        }
    }
}