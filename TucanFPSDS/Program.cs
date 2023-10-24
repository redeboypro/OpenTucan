using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using OpenTucan.Network;

namespace TucanFPSDS
{
    internal class Program
    {
        private static UdpClient client;
        private static bool isRunning;

        public static void Main()
        {
            client = new UdpClient();
            isRunning = true;

            // Allow user to enter their name
            Console.WriteLine("Enter your name:");
            string name = Console.ReadLine();

            // Set server IP address and port
            IPAddress serverIP = IPAddress.Parse("26.110.205.171");
            int serverPort = 7777;
            var serverEndPoint = new IPEndPoint(serverIP, serverPort);
            
            IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, 0);
            client.Client.Bind(localEndpoint);

            // Start a new thread to receive messages from the server
            Console.WriteLine("UDP client is started. Enter messages:");

            // Start a new thread to receive messages from the server
            StartReceiveThread();

            while (isRunning)
            {
                string message = Console.ReadLine();

                if (message.ToLower() == "quit")
                {
                    StopClient();
                    break;
                }

                // Create a formatted message with the sender's name
                string formattedMessage = $"{name}: {message}";
                byte[] sendBytes = Encoding.ASCII.GetBytes(formattedMessage);
                client.Send(sendBytes, sendBytes.Length, serverEndPoint);
            }
        }

        private static void StartReceiveThread()
        {
            // Start a new thread to continuously receive messages from the server
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }

        private static void ReceiveMessages()
        {
            while (isRunning)
            {
                IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] receiveBytes = client.Receive(ref serverEndpoint);
                string message = Encoding.ASCII.GetString(receiveBytes);

                Console.WriteLine("Received message: " + message);
            }
        }

        private static void StopClient()
        {
            isRunning = false;
            client.Close();
            Console.WriteLine("Client stopped.");
        }
    }
}