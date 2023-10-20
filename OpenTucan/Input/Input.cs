using System;
using System.Collections.Generic;
using OpenTK.Input;

namespace OpenTucan.Input
{
    public static class InputManager
    {
        private static KeyboardState _keyboardState;
        private static MouseState _mouseState;

        private static KeyboardState _previousKeyboardState;
        private static MouseState _previousMouseState;

        private static int _mouseDeltaX;
        private static int _mouseDeltaY;

        public static bool IsKeyDown(Key key)
        {
            return _keyboardState.IsKeyDown(key);
        }

        public static bool IsKeyUp(Key key)
        {
            return _keyboardState.IsKeyUp(key);
        }

        public static bool IsKeyPressed(Key key)
        {
            return _keyboardState != _previousKeyboardState && _keyboardState[key];
        }

        public static bool IsKeyReleased(Key key)
        {
            return _keyboardState != _previousKeyboardState && !_keyboardState[key];
        }

        public static bool IsMouseButtonDown(MouseButton button)
        {
            return _mouseState.IsButtonDown(button);
        }

        public static bool IsMouseButtonUp(MouseButton button)
        {
            return _mouseState.IsButtonUp(button);
        }

        public static bool IsMouseButtonPressed(MouseButton button)
        {
            return _mouseState != _previousMouseState && _mouseState[button];
        }

        public static bool IsMouseButtonReleased(MouseButton button)
        {
            return _mouseState != _previousMouseState && !_mouseState[button];
        }

        public static bool IsAnyKeyDown()
        {
            return _keyboardState.IsAnyKeyDown;
        }

        public static bool IsAnyMouseButtonDown()
        {
            return _mouseState.IsAnyButtonDown;
        }

        public static int GetMouseDX()
        {
            return _mouseDeltaX;
        }

        public static int GetMouseDY()
        {
            return _mouseDeltaY;
        }
        
        public static int GetMouseX()
        {
            return _mouseState.X;
        }

        public static int GetMouseY()
        {
            return _mouseState.Y;
        }
        
        public static void OnLoad()
        {
            _keyboardState = Keyboard.GetState();
            _mouseState = Mouse.GetState();
            _previousKeyboardState = _keyboardState;
            _previousMouseState = _mouseState;
        }

        public static void OnUpdateFrame()
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