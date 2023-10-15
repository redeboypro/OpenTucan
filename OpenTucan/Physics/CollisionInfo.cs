using OpenTK;

namespace OpenTucan.Physics
{
    public readonly struct CollisionInfo
    {
        public readonly Vector3 Normal;
        public readonly float PenetrationDepth ;

        public CollisionInfo(Vector3 normal, float penetrationDepth)
        {
            Normal = normal;
            PenetrationDepth = penetrationDepth; 
        }
    }
}