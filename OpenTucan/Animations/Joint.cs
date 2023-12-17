using System.Collections.Generic;
using OpenTK;

namespace OpenTucan.Animations
{
    public class Joint
    {
        public const int NoneParentIndex = -1;
        
        public readonly Vector3 Translation;
        public readonly Quaternion Orientation;
        public readonly Vector3 Scale;

        public Joint(Vector3 translation, Quaternion orientation, Vector3 scale)
        {
            Translation = translation;
            Orientation = orientation;
            Scale = scale;
        }
    }
}