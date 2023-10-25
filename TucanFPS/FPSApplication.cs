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

namespace TucanFPS
{
    public class FPSApplication : TucanApplication
    {
        private Camera _camera;
        private Mesh _cubeMesh;
        private GameObject _playerObject;
        private GameObject _another;
        private Client _client;

        public FPSApplication(string title, int windowWidth, int windowHeight, Color4 backgroundColor) : base(title, windowWidth, windowHeight, backgroundColor) { }

        protected override void PrepareStart()
        {
            _camera = new Camera(Width, Height);
            _cubeMesh = Mesh.Cube();
            
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
            _playerObject.AddBehaviour<PlayerController>();

            _another = World.Instantiate("another", _cubeMesh, solid, shader);
            _another.SetKinematic(true);
            _playerObject.FallingAcceleration = -50;

            _camera.SetParent(_playerObject);
            _camera.LocalSpaceLocation = Vector3.Zero;

            var coneMesh = Mesh.FromFile("a.obj");
            var plane = World.Instantiate("plane", coneMesh, new Texture(new Bitmap("landscape.png")), shader);
            plane.WorldSpaceLocation = new Vector3(0, -10, 0);
            plane.SetConvexShapes(coneMesh.ConcaveCollisionCollection);
            plane.SetKinematic(false);
            plane.SetStatic(true);
            
            //_client = new Client("26.110.205.171", 7777);
            //_client.ReceivePacket += packet =>
            {
                //_another.WorldSpaceLocation = packet.ReadVector3();
                //_another.WorldSpaceRotation = packet.ReadQuaternion();
            };
        }

        protected override void PrepareUpdate(FrameEventArgs eventArgs)
        {
            //_client.ClearBuffer();
            //_client.WriteVector3ToBuffer(_playerObject.WorldSpaceLocation);
            //_client.WriteQuaternionToBuffer(_playerObject.WorldSpaceRotation);
            //_client.Send();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            
        }

        protected override void PostRender(FrameEventArgs eventArgs)
        {
            
        }
    }
}