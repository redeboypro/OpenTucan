using OpenTK.Graphics;
using OpenTucan;
using OpenTucan.Graphics;

namespace TucanFPS
{
    public class FPSApplication : TucanApplication
    {
        private Mesh _landscapeMesh;
        private Texture _landscapeTexture;
        
        public FPSApplication(string title, int windowWidth, int windowHeight, Color4 backgroundColor) : base(title, windowWidth, windowHeight, backgroundColor)
        {
            
        }

        protected override void PrepareStart()
        {
            _landscapeMesh = Mesh.FromFile("landscape.obj");
            //_landscapeTexture = new Texture("landscape.png");
        }
    }
}