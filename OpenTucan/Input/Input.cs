using System;
using System.Collections.Generic;
using OpenTK.Input;

namespace OpenTucan.Input
{
    public class Input
    {
        private KeyboardState _keyboardState;
        private MouseState _mouseState;

        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        private int _mouseDeltaX;
        private int _mouseDeltaY;

        public Input()
        {
            _keyboardState = Keyboard.GetState();
            _mouseState = Mouse.GetState();
            _previousKeyboardState = _keyboardState;
            _previousMouseState = _mouseState;
        }

        public bool IsKeyDown(Key key)
        {
            return _keyboardState.IsKeyDown(key);
        }

        public bool IsKeyUp(Key key)
        {
            return _keyboardState.IsKeyUp(key);
        }

        public bool IsKeyPressed(Key key)
        {
            return _keyboardState != _previousKeyboardState && _keyboardState[key];
        }

        public bool IsKeyReleased(Key key)
        {
            return _keyboardState != _previousKeyboardState && !_keyboardState[key];
        }

        public bool IsMouseButtonDown(MouseButton button)
        {
            return _mouseState.IsButtonDown(button);
        }

        public bool IsMouseButtonUp(MouseButton button)
        {
            return _mouseState.IsButtonUp(button);
        }

        public bool IsMouseButtonPressed(MouseButton button)
        {
            return _mouseState != _previousMouseState && _mouseState[button];
        }

        public bool IsMouseButtonReleased(MouseButton button)
        {
            return _mouseState != _previousMouseState && !_mouseState[button];
        }

        public bool IsAnyKeyDown()
        {
            return _keyboardState.IsAnyKeyDown;
        }

        public bool IsAnyMouseButtonDown()
        {
            return _mouseState.IsAnyButtonDown;
        }

        public int GetMouseDX()
        {
            return _mouseDeltaX;
        }

        public int GetMouseDY()
        {
            return _mouseDeltaY;
        }
        
        public int GetMouseX()
        {
            return _mouseState.X;
        }

        public int GetMouseY()
        {
            return _mouseState.Y;
        }

        public void OnUpdateFrame()
        {
            _keyboardState = Keyboard.GetState();
            _mouseState = Mouse.GetState();
            
            _mouseDeltaX = _mouseState.X - _previousMouseState.X;
            _mouseDeltaY = _mouseState.Y - _previousMouseState.Y;
            
            _previousKeyboardState = _keyboardState;
            _previousMouseState = _mouseState;
        }
    }
}