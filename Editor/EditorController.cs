using OpenTK;
using OpenTucan.Entities;
using OpenTucan.Graphics;

namespace Editor
{
    public class EditorController
    {
        public EditorController(INativeWindow window, string projectPath)
        {
            Directory = projectPath;
            Display = window;
            Resources = new AssetManager(projectPath);
            GUI = new EditorGUI(this);
        }
        
        public Entity Entity { get; private set; }

        public Texture TextureAsset { get; private set; }
        
        public Mesh MeshAsset { get; private set; }

        public string Directory { get; }

        public INativeWindow Display { get; }
        
        public EditorGUI GUI { get; }

        public AssetManager Resources { get; }

        public void AssignEntity(Entity inEntity)
        {
            Entity = inEntity;
        }
        
        public void AssignTextureAsset(Texture inTexture)
        {
            TextureAsset = inTexture;
        }
        
        public void AssignMeshAsset(Mesh inMesh)
        {
            MeshAsset = inMesh;
        }
    }
}