using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTucan;
using OpenTucan.Animations;
using OpenTucan.Common;
using OpenTucan.Entities;
using OpenTucan.Graphics;
using OpenTucan.GUI;
using OpenTucan.GUI.Advanced;
using OpenTucan.Input;
using Font = OpenTucan.GUI.Font;
using Image = OpenTucan.GUI.Image;
using ListView = OpenTucan.GUI.ListView;

namespace TucanAnimator
{
    public class AnimatorApplication : TucanApplication
    {
        private readonly List<GameObject> _bones;
        
        private BasicShader _shader;
        private Camera _camera;
        private GameObject _modelObject;

        private AnimationRoot _animationRoot;
        
        private Mesh _modelGeometry;
        private Mesh _boneGeometry;

        private Texture _boneTexture;

        private GUIXml _guiXml;

        private GameObject _currentBone;

        public AnimatorApplication(string title, int windowWidth, int windowHeight, Color4 backgroundColor) : base(title, windowWidth, windowHeight, backgroundColor)
        {
            _bones = new List<GameObject>();
        }

        protected override void PrepareStart()
        {
            _shader = new BasicShader();
            _camera = new Camera(Width, Height)
            {
                WorldSpaceLocation = new Vector3(0, 0, -10)
            };

            var importModelDialog = new OpenFileDialog
            {
                Filter = "3D Model Files|*.obj;*.fbx;*.gltf;*.dae",
                CheckFileExists = true
            };
            importModelDialog.ShowDialog();
            
            _modelGeometry = Mesh.FromFile(importModelDialog.FileName);
            _boneGeometry = Mesh.Sphere(8, 8);

            var solid = Texture.Solid(Color.White);

            _modelObject = World.Instantiate("Model", _modelGeometry, solid, _shader);
            _boneTexture = Texture.Solid(Color.Red);
            
            var font = new Font(new Texture("font.png"));

            _guiXml = new GUIXml("animator.tui", font, new [] {solid}, GUIController);
            var boneList = (ListView) _guiXml.GetElementByIdentifier("BoneList");
            var createButton = (Image) _guiXml.GetElementByIdentifier("CreateButton");
            var parentNameInputField = (InputField) _guiXml.GetElementByIdentifier("ParentInputField");
            
            var boneNameText = (Text) _guiXml.GetElementByIdentifier("BoneName");
            boneNameText.Color = Color4.Gray;
            
            var bonePositionXInputField = (InputField) _guiXml.GetElementByIdentifier("BonePositionX");
            var bonePositionYInputField = (InputField) _guiXml.GetElementByIdentifier("BonePositionY");
            var bonePositionZInputField = (InputField) _guiXml.GetElementByIdentifier("BonePositionZ");
            var boneRadiusInputField = (InputField) _guiXml.GetElementByIdentifier("BoneRadius");

            bonePositionXInputField.ValueChanged += c =>
            {
                if (_currentBone is null)
                {
                    return;
                }
                
                if (float.TryParse(bonePositionXInputField.Text, out var x))
                {
                    _currentBone.WorldSpaceLocation = _currentBone.WorldSpaceLocation.SetX(x);
                }
            };
            
            bonePositionYInputField.ValueChanged += c =>
            {
                if (_currentBone is null)
                {
                    return;
                }

                if (float.TryParse(bonePositionYInputField.Text, out var y))
                {
                    _currentBone.WorldSpaceLocation = _currentBone.WorldSpaceLocation.SetY(y);
                }
            };
            
            bonePositionZInputField.ValueChanged += c =>
            {
                if (_currentBone is null)
                {
                    return;
                }

                if (float.TryParse(bonePositionZInputField.Text, out var z))
                {
                    _currentBone.WorldSpaceLocation = _currentBone.WorldSpaceLocation.SetZ(z);
                }
            };
            
            boneRadiusInputField.ValueChanged += c =>
            {
                if (_currentBone is null)
                {
                    return;
                }

                if (float.TryParse(boneRadiusInputField.Text, out var r))
                {
                    if (r == 0)
                    {
                        return;
                    }
                    _currentBone.WorldSpaceScale = new Vector3(r);
                }
            };
            
            createButton.Press = point =>
            {
                var index = -1;
                if (int.TryParse(parentNameInputField.Text, out var resIndex))
                {
                    index = resIndex;
                }
                
                var boneInstance = InstantiateBone(index);
                var boneButton = new Text(boneInstance.Name, font, GUIController)
                {
                    WorldSpaceScale = new Vector3(0.25f, 0.05f, 1),
                    Color = Color4.Gray
                };
                boneButton.Press += cursorPosition =>
                {
                    _currentBone = boneInstance;
                    var boneLocation = boneInstance.WorldSpaceLocation;
                    boneNameText.Content = boneInstance.Name;
                    bonePositionXInputField.Text = boneLocation.X.ToString(CultureInfo.CurrentCulture);
                    bonePositionYInputField.Text = boneLocation.Y.ToString(CultureInfo.CurrentCulture);
                    bonePositionZInputField.Text = boneLocation.Z.ToString(CultureInfo.CurrentCulture);
                    boneRadiusInputField.Text = boneInstance.WorldSpaceScale.X.ToString(CultureInfo.CurrentCulture);
                };
                boneList.AddItem(boneButton);
            };
        }

        protected override void PrepareUpdate(FrameEventArgs eventArgs)
        {
            if (!InputManager.IsMouseButtonDown(MouseButton.Right))
            {
                return;
            }
            
            var elapsedTime = (float) eventArgs.Time;
            var speed = 5 * elapsedTime;
            
            _camera.Rotate(MathHelper.DegreesToRadians(speed * 15) * -InputManager.GetMouseDY(), _camera.Right());
            _camera.Rotate(MathHelper.DegreesToRadians(speed * 15) * -InputManager.GetMouseDX(), Vector3.UnitY);

            if (InputManager.IsKeyDown(Key.W))
            {
                _camera.LocalSpaceLocation += _camera.Front() * speed;
            }
            if (InputManager.IsKeyDown(Key.S))
            {
                _camera.LocalSpaceLocation -= _camera.Front() * speed;
            }
            if (InputManager.IsKeyDown(Key.A))
            {
                _camera.LocalSpaceLocation -= _camera.Right() * speed;
            }
            if (InputManager.IsKeyDown(Key.D))
            {
                _camera.LocalSpaceLocation += _camera.Right() * speed;
            }
        }

        private GameObject InstantiateBone(int parentIndex)
        {
            var boneInstance = World.Instantiate("Bone " + _bones.Count, _boneGeometry, _boneTexture, _shader, "BonesLayer");

            if (parentIndex >= 0 && parentIndex < _bones.Count)
            {
                boneInstance.SetParent(_bones[parentIndex]);
            }
            
            _bones.Add(boneInstance);
            return boneInstance;
        }
    }
}