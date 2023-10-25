using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTucan;
using OpenTucan.Entities;
using OpenTucan.Graphics;
using OpenTucan.Input;
using OpenTucan.Network;

namespace Editor
{
    public class EditorApplication : TucanApplication
    {
        private Camera _camera;
        private float _pitch = 0f;
        private Mesh _cubeMesh;
        private GameObject _playerObject;
        private GameObject _another;
        private Client _client;
        private bool _isInsideWindow;
        
        public EditorApplication(string title, int windowWidth, int windowHeight, Color4 backgroundColor) : base(title, windowWidth, windowHeight, backgroundColor) { }

        protected override void PrepareStart()
        {
            Location = Point.Empty;
            CursorGrabbed = true;
            _camera = new Camera(Width, Height);
            _cubeMesh = Mesh.FromFile("cube.obj");
            var solid = Texture.Solid(Color.White);
            var shader = new ExternalShader(@"
#version 150

in vec3 InVertex;
in vec2 InUV;
in vec3 InNormal;

out vec2 PassUV;

uniform mat4 ModelMatrix;
uniform mat4 ProjectionMatrix;
uniform mat4 ViewMatrix;

void main(void){
	gl_Position = ProjectionMatrix * ViewMatrix * ModelMatrix * vec4(InVertex, 1.0);
	PassUV = InUV;
}
", @"
#version 150

in vec2 PassUV;

out vec4 OutColor;

uniform sampler2D MainTexture;

void main(void){
	OutColor = texture(MainTexture, PassUV);
}
");
            _playerObject = World.Instantiate("player");
            _playerObject.SetKinematic(false);
            _playerObject.SetConvexShapes(_cubeMesh.ConvexCollisionShape);

            _another = World.Instantiate("another", _cubeMesh, solid, shader);
            _another.SetKinematic(true);
            _playerObject.FallingAcceleration = -50;

            _camera.SetParent(_playerObject);
            _camera.LocalSpaceLocation = Vector3.Zero;
        }

        protected override void PrepareUpdate(FrameEventArgs eventArgs)
        {
            
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isInsideWindow = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isInsideWindow = false;
        }

        protected override void PostRender(FrameEventArgs eventArgs)
        {
            
        }
    }
}