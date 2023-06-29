using Assimp;
using OpenTK;

namespace OpenTucan.Common
{
    public static class ConversionHelper
    {
        public static Vector3 ToOpenTK(this Vector3D source)
        {
            Vector3 res;
            res.X = source.X;
            res.Y = source.Y;
            res.Z = source.Z;
            return res;
        }
    }
}