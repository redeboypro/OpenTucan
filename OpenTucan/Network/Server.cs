using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace OpenTucan.Network
{
    public class Server
    {
        public Server(int port, int clientCount)
        {
            var clients = new List<IPEndPoint>();
            var udpClient = new UdpClient(port);
            
            while (true)
            {
                var clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var receiveBytes = udpClient.Receive(ref clientEndPoint);

                if (!clients.Contains(clientEndPoint) && clients.Count < clientCount)
                {
                    clients.Add(clientEndPoint);
                    continue;
                }

                foreach (var client in clients)
                {
                    if (client.Equals(clientEndPoint))
                    {
                        continue;
                    }
                    
                    udpClient.Send(receiveBytes, receiveBytes.Length, client);
                }
            }
        }
    }
}