using System;
using OpenTK;
using OpenTK.Input;
using OpenTucan.Common;
using OpenTucan.Graphics;

namespace OpenTucan.GUI
{
    public class ListView : Image
    {
        private readonly GUIControl _holder;
        private float _lastMinBorder;
        private bool _isNotEmpty;
        private int _scrollDirection;

        public ListView(Texture texture, GUIController guiController) : base(texture, guiController)
        {
            IsMasked = true;
            _scrollDirection = 1;
            _holder = new GUIControl();
            _holder.SetParent(this);

            Drag += (mousePos, mouseDelta) =>
            {
                _holder.LocalSpaceLocation = new Vector3(0,
                    MathHelper.Clamp(_holder.LocalSpaceLocation.Y - _scrollDirection * mouseDelta.Y,
                    0, -_lastMinBorder - 1), 0);
            };
        }

        public void ReflectScrollDirection()
        {
            _scrollDirection *= -1;
        }

        public void AddItem(GUIControl item)
        {
            item.ChangeActiveState += state =>
            {
                Refresh();
            };
            
            var worldScale = item.WorldSpaceScale;
            _holder.AddChild(item);
            item.WorldSpaceScale = worldScale;
            
            if (!_isNotEmpty)
            {
                _lastMinBorder = 1.0f;
                _isNotEmpty = true;
            }

            var left = item.LocalSpaceScale.X - 1;
            var top =  _lastMinBorder - item.LocalSpaceScale.Y;
            
            item.LocalSpaceLocation = new Vector3(left, top, 0);
            _lastMinBorder = item.LocalSpaceLocation.Y - item.LocalSpaceScale.Y;
        }

        public void Refresh()
        {
            _lastMinBorder = 1.0f;
            for (var i = 0; i < _holder.GetChildrenAmount(); i++)
            {
                var item = (GUIControl) _holder.GetChild(i);

                if (!item.IsActive)
                {
                    continue;
                }
                
                var left = item.LocalSpaceScale.X - 1;
                var top =  _lastMinBorder - item.LocalSpaceScale.Y;
            
                item.LocalSpaceLocation = new Vector3(left, top, 0);
                _lastMinBorder = item.LocalSpaceLocation.Y - item.LocalSpaceScale.Y;
            }
        }
    }
}