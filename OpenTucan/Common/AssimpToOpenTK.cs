using Assimp;
using OpenTK;
using Quaternion = OpenTK.Quaternion;

namespace OpenTucan.Common
{
    public static class AssimpToOpenTK
    {
        public static Vector3 ToOpenTK(this Vector3D source)
        {
            Vector3 res;
            res.X = source.X;
            res.Y = source.Y;
            res.Z = source.Z;
            return res;
        }
        
        public static Matrix4 ToOpenTK(this Matrix4x4 source)
        {
            return new Matrix4(
                source.A1, source.B1, source.C1, source.D1,
                source.A2, source.B2, source.C2, source.D2,
                source.A3, source.B3, source.C3, source.D3,
                source.A4, source.B4, source.C4, source.D4);
        }
        
        public static Quaternion ToOpenTK(this Assimp.Quaternion source)
        {
            return new Quaternion(source.X, source.Y, source.Z, source.W);
        }
    }
}