using System;
using OpenTucan.Network;

namespace TucanFPS
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var client = new UDPClient(Console.ReadLine(), 7777);
            client.RunInOtherThread();
            client.ReceiveAnotherPacket = (i, packet) =>
            {
                Console.WriteLine(packet.ReadString());
            };

            while (true)
            {
                var msg = new Packet();
                msg.WriteString(Console.ReadLine());
                client.Send(msg);
            }
        }
    }
}