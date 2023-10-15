using OpenTK;

namespace OpenTucan.Physics
{
    public readonly struct Normal
    {
        public readonly Vector3 Direction;
        public readonly float Distance;

        public Normal(Vector3 direction, float distance)
        {
            Direction = direction;
            Distance = distance;
        }
    }
}