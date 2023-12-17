using System.Collections.Generic;
using OpenTK;

namespace OpenTucan.Animations
{
    public class AnimationClip
    {
        private readonly List<Pose> _poses;

        public AnimationClip()
        {
            _poses = new List<Pose>();
        }

        public int Duration
        {
            get
            {
                return _poses.Count - 1;
            }
        }

        public void BindPose(Pose pose)
        {
            _poses.Add(pose);
        }

        public IReadOnlyList<(Matrix4 localTransform, int parentIndex)> Interpolate(float time)
        {
            time = MathHelper.Clamp(time, 0, Duration);

            var index = 0;
            while (index < time - 1)
            {
                index++;
            }

            var interpolationFactor = time - index;
            var currentPose = _poses[index];
            var nextPose = _poses[index + 1];

            return currentPose.Interpolate(nextPose, interpolationFactor);
        }
    }
}