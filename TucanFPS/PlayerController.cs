using System;
using OpenTK;
using OpenTK.Input;
using OpenTucan.Components;
using OpenTucan.Entities;
using OpenTucan.Input;

namespace TucanFPS
{
    public class PlayerController : Behaviour
    {
        private Camera _camera;
        private float _pitch;
        
        public override void Start()
        {
           _camera = Camera.Main;
        }

        public override void Update(FrameEventArgs eventArgs)
        {
            var elapsedTime = (float) eventArgs.Time * 10;
            
            _pitch += elapsedTime * 10 * InputManager.GetMouseDY();
            _pitch = MathHelper.Clamp(_pitch, -90, 90);
            
            GameObject.Rotate(MathHelper.DegreesToRadians(elapsedTime * 10 * -InputManager.GetMouseDX()), Vector3.UnitY);
            _camera.LocalSpaceEulerAngles = new Vector3(MathHelper.DegreesToRadians(_pitch), 0, 0);

            if (InputManager.IsKeyDown(Key.W))
            {
                GameObject.WorldSpaceLocation += GameObject.Front() * elapsedTime;
            }
            if (InputManager.IsKeyDown(Key.A))
            {
                GameObject.WorldSpaceLocation -= GameObject.Right() * elapsedTime;
            }
            if (InputManager.IsKeyDown(Key.S))
            {
                GameObject.WorldSpaceLocation -= GameObject.Front() * elapsedTime;
            }
            if (InputManager.IsKeyDown(Key.D))
            {
                GameObject.WorldSpaceLocation += GameObject.Right() * elapsedTime;
            }
            if (InputManager.IsKeyDown(Key.Space) && GameObject.IsGrounded)
            {
                GameObject.TossUp(20);
            }
        }
    }
}