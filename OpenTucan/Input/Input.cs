using System;
using System.Collections.Generic;
using OpenTK.Input;

namespace OpenTucan.Input
{
    public class Input
    {
        private KeyboardState keyboardState;
        private MouseState mouseState;

        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;

        private float mouseDeltaX;
        private float mouseDeltaY;

        public Input()
        {
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            previousKeyboardState = keyboardState;
            previousMouseState = mouseState;
        }

        public bool IsKeyDown(Key key)
        {
            return keyboardState.IsKeyDown(key);
        }

        public bool IsKeyUp(Key key)
        {
            return keyboardState.IsKeyUp(key);
        }

        public bool IsKeyPressed(Key key)
        {
            return keyboardState != previousKeyboardState && keyboardState[key];
        }

        public bool IsKeyReleased(Key key)
        {
            return keyboardState != previousKeyboardState && !keyboardState[key];
        }

        public bool IsMouseButtonDown(MouseButton button)
        {
            return mouseState.IsButtonDown(button);
        }

        public bool IsMouseButtonUp(MouseButton button)
        {
            return mouseState.IsButtonUp(button);
        }

        public bool IsMouseButtonPressed(MouseButton button)
        {
            return mouseState != previousMouseState && mouseState[button];
        }

        public bool IsMouseButtonReleased(MouseButton button)
        {
            return mouseState != previousMouseState && !mouseState[button];
        }

        public bool IsAnyKeyDown()
        {
            return keyboardState.IsAnyKeyDown;
        }

        public bool IsAnyMouseButtonDown()
        {
            return mouseState.IsAnyButtonDown;
        }

        public float GetMouseDX()
        {
            return mouseDeltaX;
        }

        public float GetMouseDY()
        {
            return mouseDeltaY;
        }

        public void OnUpdateFrame()
        {
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            
            mouseDeltaX = mouseState.X - previousMouseState.X;
            mouseDeltaY = mouseState.Y - previousMouseState.Y;
            
            previousKeyboardState = keyboardState;
            previousMouseState = mouseState;
        }
    }
}