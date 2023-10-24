using System;
using OpenTK.Graphics;
using OpenTucan.Entities;

namespace Editor
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var display = new EditorApplication("Tucan Editor", 800, 800, Color4.Blue);
            display.Run();
        }
    }
}