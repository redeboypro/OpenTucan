using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTucan.Entities;

namespace OpenTucan.Common
{
    public static class MathTools
    {
        /// <summary>
        /// Converts quaternion to euler angles
        /// </summary>
        public static Vector3 ToEulerAngles(this Quaternion quaternion)
        {
            const float edge = 0.4995f;
            
            var sqrtW = quaternion.W * quaternion.W;
            var sqrtX = quaternion.X * quaternion.X;
            var sqrtY = quaternion.Y * quaternion.Y;
            var sqrtZ = quaternion.Z * quaternion.Z;
            
            var unit = sqrtX + sqrtY + sqrtZ + sqrtW;
            var test = quaternion.X * quaternion.W - quaternion.Y * quaternion.Z;
            
            Vector3 resultAngles;

            if (test > edge * unit)
            {
                resultAngles.Y = 2.0f * (float) System.Math.Atan2(quaternion.Y, quaternion.X);
                resultAngles.X = (float) System.Math.PI / 2.0f;
                resultAngles.Z = 0.0f;
                return resultAngles;
            }

            if (test < -edge * unit)
            {
                resultAngles.Y = -2.0f * (float) System.Math.Atan2(quaternion.Y, quaternion.X);
                resultAngles.X = (float) -System.Math.PI / 2.0f;
                resultAngles.Z = 0.0f;
                return resultAngles;
            }

            resultAngles.Y = (float) System.Math.Atan2(
                2.0f * quaternion.X * quaternion.W + 2.0f * quaternion.Y * quaternion.Z,
                1.0f - 2.0f * (quaternion.Z * quaternion.Z + quaternion.W * quaternion.W));

            resultAngles.X =
                (float) System.Math.Asin(2.0f * (quaternion.X * quaternion.Z - quaternion.W * quaternion.Y));

            resultAngles.Z = (float) System.Math.Atan2(
                2.0f * quaternion.X * quaternion.Y + 2.0f * quaternion.Z * quaternion.W,
                1.0f - 2.0f * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z));

            return resultAngles;
        }
        
        private static float NormalizeAngle(float angle)
        {
            while (angle > System.Math.PI * 2.0f)
            {
                angle -= (float) System.Math.PI * 2.0f;
            }

            while (angle < 0.0f)
            {
                angle += (float) System.Math.PI * 2.0f;
            }

            return angle;
        }
        
        /// <summary>
        /// Gives forward direction of orientation
        /// </summary>
        public static Vector3 Front(this Quaternion quaternion)
        {
            return quaternion * Vector3.UnitZ;
        }
        
        /// <summary>
        /// Gives up direction of orientation
        /// </summary>
        public static Vector3 Up(this Quaternion quaternion)
        {
            return quaternion * Vector3.UnitY;
        }

        /// <summary>
        /// Gives right direction of orientation
        /// </summary>
        public static Vector3 Right(this Quaternion quaternion)
        {
            return quaternion * Vector3.UnitX;
        }

        /// <summary>
        /// Get forward direction of orientation
        /// </summary>
        public static Quaternion GetLookRotation(Vector3 forward, Vector3 up)
        {
            forward.Normalize();
            
            var a = Vector3.Normalize(forward);
            var b = Vector3.Normalize(Vector3.Cross(up, a));
            var c = Vector3.Cross(a, b);

            var m00 = b.X;
            var m01 = b.Y;
            var m02 = b.Z;

            var m10 = c.X;
            var m11 = c.Y;
            var m12 = c.Z;

            var m20 = a.X;
            var m21 = a.Y;
            var m22 = a.Z;

            var num8 = (m00 + m11) + m22;
            
            var quaternion = new Quaternion();
            if (num8 > 0.0f)
            {
                var num = (float) System.Math.Sqrt(num8 + 1.0f);
                quaternion.W = num * 0.5f;
                num = 0.5f / num;
                quaternion.X = (m12 - m21) * num;
                quaternion.Y = (m20 - m02) * num;
                quaternion.Z = (m01 - m10) * num;
                return quaternion;
            }

            if (m00 >= m11 && m00 >= m22)
            {
                var num7 = (float) System.Math.Sqrt(1.0f + m00 - m11 - m22);
                var num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (m01 + m10) * num4;
                quaternion.Z = (m02 + m20) * num4;
                quaternion.W = (m12 - m21) * num4;
                return quaternion;
            }

            if (m11 > m22)
            {
                var num6 = (float) System.Math.Sqrt(1.0f + m11 - m00 - m22);
                var num3 = 0.5f / num6;
                quaternion.X = (m10 + m01) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (m21 + m12) * num3;
                quaternion.W = (m20 - m02) * num3;
                return quaternion;
            }

            var num5 = (float) System.Math.Sqrt(1.0f + m22 - m00 - m11);
            var num2 = 0.5f / num5;
            
            quaternion.X = (m20 + m02) * num2;
            quaternion.Y = (m21 + m12) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (m01 - m10) * num2;
            
            return quaternion;
        }

        /// <summary>
        /// Transforms vector by matrix
        /// </summary>
        public static Vector3 Transform(this Vector3 vector, Matrix4 matrix)
        {
            var toVector4 = new Vector4(vector.X, vector.Y, vector.Z, 1.0f);
            Vector4.Transform(ref toVector4, ref matrix, out toVector4);
            return toVector4.Xyz;
        }

        /// <summary>
        /// Gives maximum of three values
        /// </summary>
        public static float Max(float a, float b, float c)
        {
            return System.Math.Max(System.Math.Max(a, b), c);
        }

        /// <summary>
        /// Gives minimum of three values
        /// </summary>
        public static float Min(float a, float b, float c)
        {
            return Math.Min(Math.Min(a, b), c);
        }

        /// <summary>
        /// Transforms vector relative transformation parameters
        /// </summary>
        public static Vector3 Transform(this Vector3 vec, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return rotation * (vec * scale) + position;
        }
        
        /// <summary>
        /// Transforms vector relative entity transformation
        /// </summary>
        public static Vector3 Transform(this Vector3 vec, Entity entity)
        {
            return vec.Transform(entity.WorldSpaceLocation, entity.WorldSpaceRotation, entity.WorldSpaceScale);
        }
        
        /// <summary>
        /// Gives color contrast on other
        /// </summary>
        public static Color4 GetContrastColor(Color4 background, Color4 black, Color4 white)
        {
            return GetContrastRatio(background, black) > GetContrastRatio(background, white) ? black : white;
        }

        /// <summary>
        /// Gives contrast ratio between two colors
        /// </summary>
        private static float GetContrastRatio(Color4 background, Color4 color)
        {
            var luminance1 = GetLuminance(background);
            var luminance2 = GetLuminance(color);
            
            return (Math.Max(luminance1, luminance2) + 0.05f) / (Math.Min(luminance1, luminance2) + 0.05f);
        }

        /// <summary>
        /// Gives RGB luminance
        /// </summary>
        private static float GetLuminance(Color4 color)
        {
            var r = GetLocalLuminance(color.R);
            var g = GetLocalLuminance(color.G);
            var b = GetLocalLuminance(color.B);
            return 0.2126f * r + 0.7152f * g + 0.0722f * b;
        }

        /// <summary>
        /// Gives channel luminance
        /// </summary>
        private static float GetLocalLuminance(float channel)
        {
            return (float) (channel > 0.03928f ? Math.Pow((channel + 0.055f) / 1.055f, 2.4f) : channel / 12.92f);
        }
    }
}