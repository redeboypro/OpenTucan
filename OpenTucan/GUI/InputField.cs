using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTucan.Common;
using OpenTucan.Graphics;

namespace OpenTucan.GUI
{
    public class InputField : Image
    {
        private readonly Text _content;
        private readonly char _cursorChar;
        private int _cursorPos;
        private string _text;
        private int _visibleLength;
        private string _message;
        private bool _isReadyForInput;

        public InputField(string message, Texture texture, Font font, GUIController controller) : base(texture, controller)
        {
            IsMasked = true;
            _cursorChar = '_';
            _message = message;
            _text = string.Empty;
            _visibleLength = _message.Length;
            _content = new Text(_message, font, controller)
            {
                Color = Color4.Gray
            };
            _content.SetParent(this, false);

            ChangeColor += color4 =>
            {
                _content.Color = MathTools.GetContrastColor(color4, Color4.White, Color4.Gray);
            };

            Press += mousePos =>
            {
                _isReadyForInput = !_isReadyForInput;
                FocusStateChanged?.Invoke(_isReadyForInput);
                Color = _isReadyForInput ? ActiveColor : InactiveColor;
            };

            KeyPress += args =>
            {
                if (!_isReadyForInput)
                {
                    return;
                }

                var keyChar = args.KeyChar;

                _text = _text.Insert(_cursorPos, keyChar.ToString());
                _cursorPos++;
                UpdateContent();
                ValueChanged?.Invoke(keyChar);
            };
            
            KeyDown += args =>
            {
                if (!_isReadyForInput)
                {
                    return;
                }
                
                if (args.Key == Key.Right)
                {
                    _cursorPos = MathHelper.Clamp(_cursorPos + 1, 0, _text.Length);
                    UpdateContent();
                    return;
                }

                if (args.Key == Key.Left)
                {
                    _cursorPos = MathHelper.Clamp(_cursorPos - 1, 0, _text.Length);
                    UpdateContent();
                    return;
                }
                
                if (args.Key != Key.BackSpace || _text.Length == 0 || _cursorPos == 0)
                {
                    return;
                }
                
                _text = _text.Remove(_cursorPos - 1, 1);
                _cursorPos--;
                UpdateContent();
                ValueChanged?.Invoke('\b');
            };
        }
        
        public Color4 ActiveColor { get; set; }
        
        public Color4 InactiveColor { get; set; }

        public Action<char> ValueChanged { get; set; }

        public Action<bool> FocusStateChanged { get; set; }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                _cursorPos = _text.Length;
                UpdateContent();
                ValueChanged?.Invoke(_text.Last());
            }
        }

        private void UpdateContent()
        {
            _content.Content = GetVisiblePart();
            var localScale = _content.LocalSpaceScale;
            _content.LocalSpaceScale = localScale;
        }

        private string GetVisiblePart()
        {
            if (_text.Length == 0)
            {
                return _message;
            }

            var visibleText = _text.Insert(_cursorPos, _cursorChar.ToString());
            
            visibleText = _cursorPos < _text.Length - _visibleLength + 1 ?
                visibleText.Substring(_cursorPos, _visibleLength - 1) : GetTail(visibleText);

            return visibleText;
        }

        private string GetTail(string source)
        {
            return _visibleLength >= source.Length ? source : source.Substring(source.Length - _visibleLength);
        }
        
        public void SetVisiblePartLength(int length)
        {
            _visibleLength = length;
            UpdateContent();
        }
    }
}