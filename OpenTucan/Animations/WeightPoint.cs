using OpenTK;

namespace OpenTucan.Animations
{
    public readonly struct WeightPoint
    {
        public readonly Vector3 Translation;
        public readonly float Weight;
        public readonly int ParentIndex;

        public WeightPoint(Vector3 translation, float weight, int parentIndex)
        {
            Translation = translation;
            Weight = weight;
            ParentIndex = parentIndex;
        }
        
        public Vector3 FindClosestPoint(Vector3 vertex)
        {
            var direction = (vertex - Translation).Normalized();
            return Translation + Weight * direction;
        }
    }
}