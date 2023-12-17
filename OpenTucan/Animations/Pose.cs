using System;
using System.Collections.Generic;
using OpenTK;

namespace OpenTucan.Animations
{
    public class Pose
    {
        public readonly AnimationRoot AnimationRoot;
        private readonly Joint[] _joints;

        public Pose(Joint[] joints, AnimationRoot animationRoot)
        {
            _joints = joints;
            AnimationRoot = animationRoot;
        }

        public Joint this[int index]
        {
            get
            {
                return _joints[index];
            }
        }

        public IReadOnlyList<(Matrix4 localTransform, int parentIndex)> Interpolate(Pose other, float factor)
        {
            var interpolatedMatrices = new (Matrix4, int)[_joints.Length];

            for (var i = 0; i < other._joints.Length; i++)
            {
                var joint1 = _joints[i];
                var joint2 = other._joints[i];
                var interpolatedPosition = Vector3.Lerp(joint1.Translation, joint2.Translation, factor);
                var interpolatedRotation = Quaternion.Slerp(joint1.Orientation, joint2.Orientation, factor);
                var interpolatedScale = Vector3.Lerp(joint1.Scale, joint2.Scale, factor);
                interpolatedMatrices[i] = (Matrix4.CreateScale(interpolatedScale) *
                                           Matrix4.CreateFromQuaternion(interpolatedRotation) *
                                           Matrix4.CreateTranslation(interpolatedPosition), AnimationRoot.GetWightPoint(i).ParentIndex);
            }

            return interpolatedMatrices;
        }
    }
}