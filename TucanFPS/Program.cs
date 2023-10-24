using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using OpenTucan.Network;

namespace TucanFPS
{
    internal class Program
    {
        private static Dictionary<IPEndPoint, string> clientDictionary;

        public static void Main()
        {
            clientDictionary = new Dictionary<IPEndPoint, string>();

            // Create UDP server socket
            UdpClient server = new UdpClient(7777);

            Console.WriteLine("UDP server is started. Waiting for client messages...");

            while (true)
            {
                // Receive message from client
                IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] receiveBytes = server.Receive(ref clientEndpoint);
                string message = Encoding.ASCII.GetString(receiveBytes);

                Console.WriteLine("Received message from client: " + message);

                // Add or update client in the dictionary with their endpoint and message
                clientDictionary[clientEndpoint] = message;

                // Send messages to all clients, excluding the sender
                foreach (KeyValuePair<IPEndPoint, string> clientEntry in clientDictionary)
                {
                    if (clientEntry.Key.Equals(clientEndpoint))
                        continue;

                    byte[] responseBytes = Encoding.ASCII.GetBytes(message);
                    server.Send(responseBytes, responseBytes.Length, clientEntry.Key);
                    Console.WriteLine("Sent message to client: " + clientEntry.Key);
                }
            }
        }
    }
}