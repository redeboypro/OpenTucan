using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace OpenTucan.Network
{
    public class Server
    {
        private readonly UdpClient _udpClient;
        private bool _isRunning;

        public Server(int port, int clientCount)
        {
            var clients = new List<IPEndPoint>();
            _udpClient = new UdpClient(port);

            _isRunning = true;
            while (_isRunning)
            {
                var clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var receiveBytes = _udpClient.Receive(ref clientEndPoint);

                if (!clients.Contains(clientEndPoint))
                {
                    if (clients.Count >= clientCount)
                    {
                        continue;
                    }
                    
                    using (var packet = new Packet())
                    {
                        packet.WriteInt32(clientCount);
                        packet.WriteInt32(clients.Count);
                        var buffer = packet.ToArray();
                        _udpClient.Send(buffer, buffer.Length, clientEndPoint);
                    }
                    clients.Add(clientEndPoint);
                    continue;
                }

                foreach (var client in clients)
                {
                    if (client.Equals(clientEndPoint))
                    {
                        continue;
                    }
                    
                    _udpClient.Send(receiveBytes, receiveBytes.Length, client);
                }
            }
        }
        
        public void Stop()
        {
            _isRunning = false;
            _udpClient.Close();
        }
    }
}