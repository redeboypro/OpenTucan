using OpenTucan.Network;

namespace TucanFPSDS
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var server = new UDPServer(7777, 2);
            server.RunInOtherThread();
            while (true);
        }
    }
}