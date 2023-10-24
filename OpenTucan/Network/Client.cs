using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OpenTucan.Network
{
    public class Client
    {
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _serverEndPoint;
        private readonly Packet _packet;
        private bool _isConnected;
        private bool _infoIsAssigned;

        public Client(string address, int port)
        {
            _udpClient = new UdpClient();
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            _isConnected = true;

            var localEndpoint = new IPEndPoint(IPAddress.Any, 0);
            _udpClient.Client.Bind(localEndpoint);
            
            _packet = new Packet();
            Send();
            
            var receiveThread = new Thread(ReceivePackets);
            receiveThread.Start();
        }
        
        public int Id { get; private set; }
        public int ClientCount { get; private set; }
        
        public Action<Packet> ReceivePacket { get; set; }

        public int GetBufferSize()
        {
            return _packet.BufferSize;
        }
        
        public void WriteBytesToBuffer(IEnumerable<byte> data)
        {
            _packet.WriteBytes(data);
        }

        public void WriteInt16ToBuffer(short data)
        {
            _packet.WriteInt16(data);
        }
        
        public void WriteInt32ToBuffer(int data)
        {
            _packet.WriteInt32(data);
        }
        
        public void WriteInt64ToBuffer(long data)
        {
            _packet.WriteInt64(data);
        }
        
        public void WriteSingleToBuffer(float data)
        {
            _packet.WriteSingle(data);
        }
        
        public void WriteStringToBuffer(string data)
        {
            _packet.WriteString(data);
        }

        public void ClearBuffer()
        {
            _packet.Clear();
        }

        private void ReceivePackets()
        {
            while (_isConnected)
            {
                var serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
                var receiveBytes = _udpClient.Receive(ref serverEndpoint);
                var packet = new Packet();
                packet.WriteBytes(receiveBytes);

                if (!_infoIsAssigned)
                {
                    _infoIsAssigned = true;
                    ClientCount = packet.ReadInt32();
                    Id = packet.ReadInt32();
                    continue;
                }
                
                ReceivePacket?.Invoke(packet);
            }
        }

        public void Send()
        {
            var buffer = _packet.ToArray();
            _udpClient.Send(buffer, buffer.Length, _serverEndPoint);
        }
        
        public void Disconnect()
        {
            _isConnected = false;
            _udpClient.Close();
        }
    }
}