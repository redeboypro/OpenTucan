using System;
using System.Collections.Generic;
using OpenTK;

namespace OpenTucan.Physics
{
    public sealed class Simplex : IDisposable
    {
        private readonly Vector3[] _points;
        private int _size;
        private bool _isDisposed;

        public Simplex()
        {
            _points = new Vector3[4];
            _size = 0;
        }

        public void InitializeFromList(params Vector3[] points)
        {
            for (var i = 0; i < points.Length && i < 4; i++)
            {
                _points[i] = points[i];
                _size++;
            }
        }

        public void PushFront(Vector3 point)
        {
            Array.Copy(_points, 0, _points, 1, _size < 3 ? _size : 3);
            _points[0] = point;
            _size = Math.Min(_size + 1, 4);
        }

        public Vector3 this[int i]
        {
            get
            {
                return _points[i];
            }
        }

        public int Size
        {
            get
            {
                return _size;
            }
        }

        public IEnumerable<Vector3> Begin()
        {
            for (var i = 0; i < _size; i++)
            {
                yield return _points[i];
            }
        }

        public IEnumerable<Vector3> End()
        {
            var endIndex = 4 - _size;
            for (var i = _size; i < _size + endIndex; i++)
            {
                yield return _points[i];
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Array.Clear(_points, 0, 4);
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}