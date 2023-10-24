using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace OpenTucan.Network
{
    public class UDPClient
    {
        public readonly string Address;
        public readonly int Port;

        private readonly UdpClient _client;
        private IPEndPoint _serverEndPoint;
        private bool _isConnected;
        private bool _idIsAssigned;

        public UDPClient(string address, int port)
        {
            Address = address;
            Port = port;
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            _client = new UdpClient();
        }
        
        public Action ReceiveServerInfo { get; set; }
        public Action<int, Packet> ReceiveAnotherPacket { get; set; }
        public int ServerClientsLimit { get; private set; }
        public int Id { get; private set; }
        
        private async void RunInOtherThread()
        {
            await Task.Run(() =>
            {
                _isConnected = true;
                while (_isConnected)
                {
                    var data = _client.Receive(ref _serverEndPoint);
                    var packet = new Packet();
                    packet.WriteBytes(data);

                    if (!_idIsAssigned)
                    {
                        ServerClientsLimit = packet.ReadInt32();
                        Id = packet.ReadInt32();
                        ReceiveServerInfo?.Invoke();
                        _idIsAssigned = true;
                        continue;
                    }

                    for (var clientId = 0; clientId < ServerClientsLimit; clientId++)
                    {
                        var globalClientId = packet.ReadInt32();
                        var length = packet.ReadInt32();
                        using (var anotherPacket = new Packet())
                        {
                            anotherPacket.WriteBytes(packet.ReadBytes(length));
                            ReceiveAnotherPacket?.Invoke(globalClientId, anotherPacket);
                        }
                    }
                    
                    packet.Dispose();
                }
                _client.Close();
            });
        }

        public void Send(Packet packet)
        {
            var data = packet.ToArray();
            _client.Send(data, data.Length, _serverEndPoint);
        }

        public void Disconnect()
        {
            _isConnected = false;
        }
    }
}