using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using OpenTK;
using OpenTK.Graphics;
using OpenTucan.Graphics;

namespace OpenTucan.GUI.Advanced
{
    public class GUIXml
    {
        private const string GUIElementCornerRadius = "GUICornerRadius";
        
        private const string GUIElementPosition = "GUIPosition";
        private const string GUIElementRotation = "GUIRotation";
        private const string GUIElementScale = "GUIScale";
        private const string GUIElementColor = "GUIColor";
        
        private const string GUITextureId = "GUITextureId";
        private const string GUIAutoScale = "GUIAutoScale";
        private const string GUIIdentifier = "GUIIdentifier";
        private const string GUIDefaultIdentifier = "Element";
        
        private const string GUIImage = "GUIImage";
        private const string GUIText = "GUIText";
        private const string GUITextContent = "GUITextContent";
        private const string GUIList = "GUIList";
        private const string GUIInputField = "GUIInputField";
        private const string GUICheckbox = "GUICheckbox";
        
        private const string GUIIsMasked = "GUIIsMasked";

        private readonly GUIController _controller;
        private readonly Dictionary<string, GUIControl> _elements;
        private readonly Texture[] _textures;
        private readonly Font _font;
        private int _undefinedElementCount;

        public GUIXml(string fileName, Font font, IReadOnlyList<Texture> textures, GUIController controller)
        {
            _controller = controller;
            _elements = new Dictionary<string, GUIControl>();
            
            _textures = new Texture[textures.Count + 1];
            var defaultTexture = new Bitmap(1, 1);
            defaultTexture.SetPixel(0, 0, Color.White);
            _textures[0] = new Texture(defaultTexture);
            for (var i = 0; i < textures.Count; i++)
            {
                _textures[i + 1] = textures[i];
            }

            _font = font;
            _undefinedElementCount = 0;
            
            var document = new XmlDocument();
            document.Load(fileName);

            var root = document.DocumentElement;
            foreach (XmlNode node in root)
            {
                var deserializedChild = DeserializeElement(node);
                _controller.AddElement(deserializedChild);
            }
        }

        public GUIControl GetElementByIdentifier(string identifier)
        {
            return _elements[identifier];
        }

        public int GetUndefinedElementCount()
        {
            return _undefinedElementCount;
        }
        
        public void IncreaseUndefinedElementCount()
        {
            _undefinedElementCount++;
        }

        private GUIControl DeserializeElement(XmlNode node)
        {
            var elementData = new GUIElementData(this, node);
            var imageData = new GUIImageData(node);
            var textData = new GUITextData(node);
            var textureId = imageData.TextureId;
            var texture = _textures[textureId];
            
            var element = new GUIControl();

            var name = node.Name;
            switch (name)
            {
                case GUIImage:
                    element = new Image(texture, _controller);
                    ((Image) element).SetCornerRadius(imageData.CornerRadius);
                    break;
                case GUIText:
                    element = new Text(textData.Content, _font, _controller);
                    break;
                case GUIList:
                    element = new ListView(texture, _controller);
                    ((Image) element).SetCornerRadius(imageData.CornerRadius);
                    break;
                case GUIInputField:
                    element = new InputField(textData.Content, texture, _font, _controller);
                    var inputField = (InputField) element;
                    inputField.SetCornerRadius(imageData.CornerRadius);
                    inputField.InactiveColor = elementData.Colors[0];
                    inputField.ActiveColor = elementData.Colors[1];
                    break;
                case GUICheckbox:
                    element = new Checkbox(texture, texture, _controller);
                    var checkbox = (Checkbox) element;
                    checkbox.Check.Color = elementData.Colors[1];
                    checkbox.SetCornerRadius(imageData.CornerRadius);
                    break;
            }

            foreach (XmlNode item in node)
            {
                if (!IsGUIElement(item.Name))
                {
                    continue;
                }

                var deserializedChild = DeserializeElement(item);
                
                if (name is GUIList)
                {
                    ((ListView) element).AddItem(deserializedChild);
                    continue;
                }
                element.AddChild(deserializedChild);
            }
            
            element.Color = elementData.Colors[0];
            element.IsMasked = elementData.IsMasked;
            element.LocalSpaceLocation = elementData.Position;
            element.LocalSpaceRotation = elementData.Rotation;
            element.LocalSpaceScale = elementData.Scale;

            _elements.Add(elementData.Identifier, element);
            
            return element;
        }

        private static bool IsGUIElement(string serialized)
        {
            return serialized is GUIImage || serialized is GUIText ||
                   serialized is GUIList || serialized is GUIInputField ||
                   serialized is GUICheckbox;
        }

        private static IReadOnlyList<float> DeserializeVector(string serialized)
        {
            var tokens = Separate(serialized);
            var deserializedNumbers = tokens.Select(float.Parse).ToList();
            return deserializedNumbers;
        }
        
        private static IEnumerable<string> Separate(string source)
        {
            var tokenStringBuilder = new StringBuilder();
            var tokenList = new List<string>();

            foreach (var sym in source)
            {
                var token = tokenStringBuilder.ToString();

                if (sym is ' ')
                {
                    if (token.Length > 0)
                    {
                        tokenList.Add(token);
                    }

                    tokenStringBuilder.Clear();
                    continue;
                }
                
                tokenStringBuilder.Append(sym);
            }

            if (tokenStringBuilder.Length > 0)
            {
                tokenList.Add(tokenStringBuilder.ToString());
            }

            return tokenList.ToArray();
        }

        private struct GUIElementData
        {
            public readonly Vector3 Position;

            public readonly Quaternion Rotation;

            public readonly Vector3 Scale;

            public readonly List<Color4> Colors;

            public readonly bool IsMasked;

            public readonly string Identifier;

            public GUIElementData(GUIXml guiXml, IEnumerable xmlNode)
            {
                Position = new Vector3();
                Rotation = Quaternion.Identity;
                Scale = Vector3.One;
                IsMasked = false;
                Identifier = GUIDefaultIdentifier;
                Colors = new List<Color4>();

                var identifierIsDefault = true;
                foreach (XmlNode child in xmlNode)
                {
                    switch (child.Name)
                    {
                        case GUIElementPosition:
                            var deserializedPositionXYZ = DeserializeVector(child.InnerText);
                            Position.X = deserializedPositionXYZ[0];
                            Position.Y = deserializedPositionXYZ[1];
                            break;
                        case GUIElementRotation:
                            Rotation = Quaternion.FromEulerAngles(0, 0, float.Parse(child.InnerText));
                            break;
                        case GUIElementScale:
                            var deserializedSpaceXYZ = DeserializeVector(child.InnerText);
                            Scale.X = deserializedSpaceXYZ[0];
                            Scale.Y = deserializedSpaceXYZ[1];
                            break;
                        case GUIElementColor:
                            var deserializedRGBA = DeserializeVector(child.InnerText);
                            var color = new float[]
                            {
                                1, 1, 1, 1
                            };
                            for (var i = 0; i < deserializedRGBA.Count; i++)
                            {
                                color[i] = deserializedRGBA[i];
                            }
                            Colors.Add(new Color4(color[0], color[1], color[2], color[3]));
                            break;
                        case GUIIsMasked:
                            IsMasked = bool.Parse(child.InnerText);
                            break;
                        case GUIIdentifier:
                            Identifier = child.InnerText.Trim();
                            identifierIsDefault = false;
                            break;
                    }
                }

                switch (Colors.Count)
                {
                    case 0:
                        Colors.Add(Color4.White);
                        break;
                    case 1:
                        Colors.Add(Color4.Gray);
                        break;
                }

                guiXml.IncreaseUndefinedElementCount();
                
                if (!identifierIsDefault || guiXml.GetUndefinedElementCount() <= 0)
                {
                    return;
                }
                
                Identifier += guiXml.GetUndefinedElementCount();
            }
        }
        
        private struct GUIImageData
        {
            public GUIImageData(IEnumerable xmlNode)
            {
                TextureId = 0;
                CornerRadius = .0f;
                foreach (XmlNode child in xmlNode)
                {
                    switch (child.Name)
                    {
                        case GUITextureId:
                            TextureId = int.Parse(child.InnerText);
                            break;
                        case GUIElementCornerRadius:
                            CornerRadius = float.Parse(child.InnerText);
                            break;
                    }
                }
            }
            
            public int TextureId { get; }
            
            public float CornerRadius { get; }
        }
        
        private struct GUITextData
        {
            public GUITextData(IEnumerable xmlNode)
            {
                Content = "None";
                foreach (XmlNode child in xmlNode)
                {
                    switch (child.Name)
                    {
                        case GUITextContent:
                            Content = child.InnerText;
                            break;
                    }
                }
            }
            
            public string Content { get; }
        }
    }
}