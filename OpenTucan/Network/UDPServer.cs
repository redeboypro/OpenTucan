using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace OpenTucan.Network
{
    public class UDPServer
    {
        public readonly int Port;
        public readonly int MaxClients;
        
        private readonly UdpClient _server;
        private readonly Dictionary<IPEndPoint, Packet> _clientsData;
        private bool _isStarted;

        public UDPServer(int port, int maxClients)
        {
            Port = port;
            MaxClients = maxClients;
            _server = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            _clientsData = new Dictionary<IPEndPoint, Packet>();
        }
        
        public async void RunInOtherThread()
        {
            await Task.Run(() =>
            {
                _isStarted = true;
                while (_isStarted)
                {
                    var clientEndPoint = new IPEndPoint(IPAddress.Any, Port);
                    
                    var data = _server.Receive(ref clientEndPoint);
                    var packet = new Packet();
                    packet.WriteBytes(data);
                    
                    if (_clientsData.ContainsKey(clientEndPoint))
                    {
                        _clientsData[clientEndPoint] = packet;
                    }
                    else
                    {
                        if (MaxClients >= _clientsData.Count)
                        {
                            continue;
                        }

                        using (var initInfoPacket = new Packet())
                        {
                            initInfoPacket.WriteInt32(MaxClients);
                            initInfoPacket.WriteInt32(_clientsData.Count);
                            SendTo(clientEndPoint, initInfoPacket);
                        }
                        _clientsData.Add(clientEndPoint, packet);
                    }

                    if (_clientsData.Count < MaxClients)
                    {
                        continue;
                    }

                    var otherDataPacket = new Packet();
                    var anotherId = 0;
                    foreach (var anotherClient in _clientsData)
                    {
                        if (Equals(anotherClient.Key, clientEndPoint))
                        {
                            anotherId++;
                            continue;
                        }

                        var anotherClientData = anotherClient.Value.ToArray();
                        otherDataPacket.WriteInt32(anotherId);
                        otherDataPacket.WriteInt32(anotherClientData.Length);
                        otherDataPacket.WriteBytes(anotherClientData);
                        anotherId++;
                    }
                    
                    SendTo(clientEndPoint, otherDataPacket);
                    otherDataPacket.Dispose();
                }
                _server.Close();
            });
        }

        private void SendTo(IPEndPoint clientEndPoint, Packet packet)
        {
            var data = packet.ToArray();
            _server.Send(data, data.Length, clientEndPoint);
        }

        public void Disconnect()
        {
            _isStarted = false;
        }
    }
}