using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using OpenTK;

namespace OpenTucan.Animations
{
    public class AnimationRoot
    {
        private readonly WeightPoint[] _weightPoints;
        private readonly IDictionary<string, AnimationClip> _animationClips;

        public AnimationRoot(WeightPoint[] weightPoints)
        {
            _weightPoints = weightPoints;
            _animationClips = new Dictionary<string, AnimationClip>();
        }
        
        public int WeightPointCount
        {
            get
            {
                return _weightPoints.Length;
            }
        }

        public WeightPoint GetWightPoint(int index)
        {
            return _weightPoints[index];
        }

        public IReadOnlyList<Matrix4> Interpolate(string clipName, float time)
        {
            var interpolatedMatrices = _animationClips[clipName].Interpolate(time);
            var globalMatricesTransformStates = new bool[WeightPointCount];
            var globalMatrices = new Matrix4[WeightPointCount];
            
            while (!globalMatricesTransformStates.All(isTransformed => isTransformed))
            {
                for (var i = 0; i < WeightPointCount; i++)
                {
                    var (localTransform, parentIndex) = interpolatedMatrices[i];
                    
                    if (parentIndex is Joint.NoneParentIndex)
                    {
                        globalMatricesTransformStates[i] = true;
                        globalMatrices[i] = localTransform;
                        continue;
                    }

                    if (!globalMatricesTransformStates[parentIndex] || globalMatricesTransformStates[i])
                    {
                        continue;
                    }
                    
                    globalMatricesTransformStates[i] = true;
                    globalMatrices[i] = localTransform * globalMatrices[parentIndex];
                }
            }

            return globalMatrices;
        }

        public int[] GetJointsIds(Vector3[] vertices)
        {
            var bonesIds = new int[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                var closestDistance = float.PositiveInfinity;
                var closestId = -1;
                for (var j = 0; j < _weightPoints.Length; j++)
                {
                    var weightPoint = _weightPoints[j].FindClosestPoint(vertex);
                    var distance = Vector3.Distance(weightPoint, vertex);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestId = j;
                    }
                }

                bonesIds[i] = closestId;
            }

            return bonesIds;
        }

        public void AddClip(string name, AnimationClip clip)
        {
            _animationClips.Add(name, clip);
        }
    }
}