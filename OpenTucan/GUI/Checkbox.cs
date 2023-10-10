using System;
using OpenTK;
using OpenTucan.Graphics;

namespace OpenTucan.GUI
{
    public class Checkbox : Image
    {
        private bool _checkState;
        
        public Checkbox(Texture checkTexture, Texture backgroundTexture, GUIController controller) : base(backgroundTexture, controller)
        {
            Check = new Image(checkTexture, controller)
            {
                LocalSpaceScale = new Vector3(0.5f)
            };
            Check.SetParent(this);
            Check.SetActive(_checkState);

            Press += args =>
            {
                _checkState = !_checkState;
                CheckStateChanged?.Invoke(_checkState);
                Check.SetActive(_checkState);
            };
        }
        
        public Image Check { get; }
        
        public Action<bool> CheckStateChanged { get; set; }
    }
}