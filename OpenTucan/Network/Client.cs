using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OpenTucan.Network
{
    public class Client
    {
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _serverEndPoint;
        private bool _isConnected;

        public Client(string address, int port)
        {
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            _udpClient = new UdpClient();
            _isConnected = true;
            
            var localEndpoint = new IPEndPoint(IPAddress.Any, 0);
            _udpClient.Client.Bind(localEndpoint);
            
            var receiveThread = new Thread(ReceivePackets);
            receiveThread.Start();
        }
        
        public Action<Packet> ReceivePacket { get; set; }

        private void ReceivePackets()
        {
            while (_isConnected)
            {
                var serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
                var receiveBytes = _udpClient.Receive(ref serverEndpoint);
                var packet = new Packet();
                packet.WriteBytes(receiveBytes);
                ReceivePacket?.Invoke(packet);
            }
        }

        public void Send(Packet packet)
        {
            Send(packet.ToArray());
        }
        
        public void Send(byte[] data)
        {
            _udpClient.Send(data, data.Length, _serverEndPoint);
        }
        
        private void Disconnect()
        {
            _isConnected = false;
            _udpClient.Close();
        }
    }
}