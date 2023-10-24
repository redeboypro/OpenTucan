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
        public static void Main()
        {
            var server = new Server(7777, 2);
        }
    }
}