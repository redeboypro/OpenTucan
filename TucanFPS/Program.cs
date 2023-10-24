using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using OpenTK.Graphics;
using OpenTucan.Network;

namespace TucanFPS
{
    internal class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new FPSApplication("Multiplayer FPS", 600, 600, Color4.RoyalBlue);
            app.Run();
        }
    }
}