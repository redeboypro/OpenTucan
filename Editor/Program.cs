using System;
using OpenTucan.Entities;

namespace Editor
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var display = new EditorWindow(800, 800);
            display.Run();
        }
    }
}