using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTucan;
using OpenTucan.Entities;
using OpenTucan.Graphics;
using OpenTucan.Input;

namespace Editor
{
    public class EditorApplication : TucanApplication
    {
        private Camera _camera;
        private float _pitch = 0f;
        private Mesh _cubeMesh;
        private GameObject _playerObject;
        
        public EditorApplication(string title, int windowWidth, int windowHeight, Color4 backgroundColor) : base(title, windowWidth, windowHeight, backgroundColor) { }

        protected override void PrepareStart()
        {
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
            
            _camera.SetParent(_playerObject);
            _camera.LocalSpaceLocation = Vector3.Zero;

            var coneMesh = Mesh.FromFile("a.obj");
            var plane = World.Instantiate("plane", coneMesh, new Texture(new Bitmap("landscape.png")), shader);
            plane.WorldSpaceLocation = new Vector3(0, -10, 0);
            plane.SetConvexShapes(coneMesh.ConcaveCollisionCollection);
            plane.SetKinematic(false);
            plane.SetStatic(true);
        }

        protected override void PrepareUpdate(FrameEventArgs eventArgs)
        {
            var elapsedTime = (float) eventArgs.Time * 100;
            _pitch += elapsedTime * InputManager.GetMouseDY();
            _pitch = MathHelper.Clamp(_pitch, -90, 90);
            _playerObject.Rotate(MathHelper.DegreesToRadians(elapsedTime * -InputManager.GetMouseDX()), Vector3.UnitY);
            _camera.LocalSpaceEulerAngles = new Vector3(MathHelper.DegreesToRadians(_pitch), 0, 0);
            if (InputManager.IsKeyDown(Key.W))
            {
                _playerObject.WorldSpaceLocation += _playerObject.Front(Space.Global) * elapsedTime * 0.1f;
            }
            if (InputManager.IsKeyDown(Key.A))
            {
                _playerObject.WorldSpaceLocation -= _playerObject.Right(Space.Global) * elapsedTime * 0.1f;
            }
            if (InputManager.IsKeyDown(Key.S))
            {
                _playerObject.WorldSpaceLocation -= _playerObject.Front(Space.Global) * elapsedTime * 0.1f;
            }
            if (InputManager.IsKeyDown(Key.D))
            {
                _playerObject.WorldSpaceLocation += _playerObject.Right(Space.Global) * elapsedTime * 0.1f;
            }

            if (InputManager.IsKeyDown(Key.Space) && _playerObject.IsGrounded)
            {
                _playerObject.TossUp(20);
            }
        }

        protected override void PostRender(FrameEventArgs eventArgs)
        {
            
        }
    }
}