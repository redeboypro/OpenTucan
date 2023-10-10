using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTucan.Graphics;
using OpenTucan.GUI;
using OpenTucan.GUI.Advanced;
using Font = OpenTucan.GUI.Font;
using Image = OpenTucan.GUI.Image;

namespace Editor
{
    public class EditorGUI
    {
        private const string FontFilePath = "res\\font.png";
        
        private GUIController _guiController;
        private Font _font;
        
        private ListView _objectListView;
        private ListView _resourceListView;
        private ListView _inspectorView;
        
        private Texture _panelTexture;
        private Texture _buttonTexture;
        private Texture _checkTexture;

        private Checkbox _gravityCheckbox;
        
        public EditorGUI(EditorController host)
        {
            Host = host;
            _guiController = new GUIController(host.Display);
            
            var panelSkin = new Bitmap(1, 1);
            panelSkin.SetPixel(0, 0, Color.MediumSpringGreen);
            
            var buttonSkin = new Bitmap(1, 1);
            buttonSkin.SetPixel(0, 0, Color.MediumSeaGreen);
            
            var checkboxSkin = new Bitmap(1, 1);
            checkboxSkin.SetPixel(0, 0, Color.White);

            _panelTexture = new Texture(panelSkin);
            _buttonTexture = new Texture(buttonSkin);
            _checkTexture = new Texture(checkboxSkin);
            
            _font = new Font(new Texture(new Bitmap(FontFilePath)));
            
            var xmlGUI = new GUIXml("editor.tui", _font, new [] {_panelTexture, _buttonTexture, _checkTexture}, _guiController);
        }
        
        public EditorController Host { get; }

        public void OnRenderFrame(FrameEventArgs eventArgs)
        {
            _guiController.OnRenderFrame(eventArgs, Host.Display);
        }

        public void RefreshResources()
        {
            var assetManager = Host.Resources;
            var imageAssets = assetManager.CollectImageAssets();
            var meshAssets = assetManager.CollectMeshAssets();

            foreach (var imgFile in imageAssets)
            {
                var asset = assetManager.ImageAssets[imgFile];
                
                var imgPicker = new Image(asset, _guiController)
                {
                    WorldSpaceScale = new Vector3(.1f, .1f, 1f)
                };
                
                imgPicker.SetCornerRadius(0.03f);
                
                imgPicker.Press += args =>
                {
                    Host.AssignTextureAsset(asset);
                    imgPicker.Color = Color4.Gray;
                };

                imgPicker.Release += () =>
                {
                    imgPicker.Color = Color4.White;
                };
                
                _resourceListView.AddItem(imgPicker);
            }
            
            foreach (var meshFile in meshAssets)
            {
                var asset = assetManager.MeshAssets[meshFile];
                var name = Path.GetFileName(meshFile);
                
                var meshPicker = new Text(name, _font, _guiController);

                meshPicker.Press += args =>
                {
                    Host.AssignMeshAsset(asset);
                    meshPicker.Color = Color4.Gray;
                };

                meshPicker.Release += () =>
                {
                    meshPicker.Color = Color4.White;
                };

                _resourceListView.AddItem(meshPicker);
            }
        }
    }
}