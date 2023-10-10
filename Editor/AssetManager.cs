using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTucan.Graphics;

namespace Editor
{
    public class AssetManager
    {
        private static readonly string[] MeshAssetExtensions =
        {
            ".obj", ".dae", ".fbx", ".gltf"
        };
        
        private static readonly string[] ImageAssetExtensions =
        {
            ".png", ".jpg", ".bmp"
        };
        public AssetManager(string projectPath)
        {
            Directory = projectPath;
            ImageAssets = new Dictionary<string, Texture>();
            MeshAssets = new Dictionary<string, Mesh>();
        }

        public string Directory { get; }
        
        public Dictionary<string, Texture> ImageAssets { get; }
        
        public Dictionary<string, Mesh> MeshAssets { get; }

        public IEnumerable<string> CollectImageAssets()
        {
            var inImageAssets = GetFilesWithExtensions(Directory, ImageAssetExtensions);
            var outImageAssets = new List<string>();

            foreach (var imageAsset in inImageAssets)
            {
                if (!ImageAssets.ContainsKey(imageAsset))
                {
                    var outAsset = new Texture(new Bitmap(imageAsset));
                    ImageAssets.Add(imageAsset, outAsset);
                    outImageAssets.Add(imageAsset);
                }
            }

            return outImageAssets;
        }
        
        public IEnumerable<string> CollectMeshAssets()
        {
            var inMeshAssets = GetFilesWithExtensions(Directory, MeshAssetExtensions);
            var outMeshAssets = new List<string>();

            foreach (var meshAsset in inMeshAssets)
            {
                if (!MeshAssets.ContainsKey(meshAsset))
                {
                    var outAsset = Mesh.FromFile(meshAsset);
                    Console.WriteLine(meshAsset);
                    MeshAssets.Add(meshAsset, outAsset);
                    outMeshAssets.Add(meshAsset);
                }
            }
            
            return outMeshAssets;
        }

        private IEnumerable<string> GetFilesWithExtensions(string path, string[] extensions)
        {
            return System.IO.Directory
                .GetFiles(path)
                .Where(file => extensions.Any(file.ToLower().EndsWith))
                .ToList();
        }
    }
}