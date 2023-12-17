using System;
using OpenTK.Graphics;

namespace TucanAnimator
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var application = new AnimatorApplication("Animator", 600, 600, Color4.RoyalBlue);
            application.Run();
        }
    }
}