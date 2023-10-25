using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace OpenTucan.Network
{
    public class Packet : IDisposable
    {
        private readonly List<byte> _buffer;
        private int _readPosition;
        private bool _isDisposed;

        public Packet()
        {
            _buffer = new List<byte>();
        }

        public int BufferSize
        {
            get
            {
                return _buffer.Count;
            }
        }

        public int UnreadLength
        {
            get
            {
                return BufferSize - _readPosition;
            }
        }

        public byte[] ToArray()
        {
            return _buffer.ToArray();
        }

        public byte[] ReadBytes(int length)
        {
            var data = _buffer.GetRange(_readPosition, length).ToArray();
            _readPosition += length;
            return data;
        }
        
        public short ReadInt16()
        {
            return BitConverter.ToInt16(ReadBytes(2), 0);
        }
        
        public int ReadInt32()
        {
            return BitConverter.ToInt32(ReadBytes(4), 0);
        }
        
        public long ReadInt64()
        {
            return BitConverter.ToInt64(ReadBytes(8), 0);
        }
        
        public float ReadSingle()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }
        
        public string ReadString()
        {
            var length = ReadInt32();
            return Encoding.ASCII.GetString(ReadBytes(length), 0, length);
        }
        
        public Vector2 ReadVector2()
        {
            return new Vector2(ReadSingle(), ReadSingle());
        }
        
        public Vector3 ReadVector3()
        {
            return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }
        
        public Vector4 ReadVector4()
        {
            return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }
        
        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }
        
        public Matrix4 ReadMatrix()
        {
            var matrix = new Matrix4();
            for (var r = 0; r < 4; r++)
            {
                for (var c = 0; c < 4; c++)
                {
                    matrix[r, c] = ReadSingle();
                }
            }

            return matrix;
        }

        public bool TryReadBytes(int length, out byte[] data)
        {
            data = new byte[0];
            if (length > UnreadLength)
            {
                return false;
            }

            data = ReadBytes(length);
            return true;
        }
        
        public bool TryReadInt16(out short data)
        {
            data = 0;
            if (2 > UnreadLength)
            {
                return false;
            }

            data = ReadInt16();
            return true;
        }
        
        public bool TryReadInt32(out int data)
        {
            data = 0;
            if (4 > UnreadLength)
            {
                return false;
            }

            data = ReadInt32();
            return true;
        }
        
        public bool TryReadInt64(out long data)
        {
            data = 0;
            if (8 > UnreadLength)
            {
                return false;
            }

            data = ReadInt64();
            return true;
        }
        
        public bool TryReadSingle(out float data)
        {
            data = 0;
            if (4 > UnreadLength)
            {
                return false;
            }

            data = ReadSingle();
            return true;
        }
        
        public bool TryReadString(out string data)
        {
            data = string.Empty;
            if (5 > UnreadLength)
            {
                return false;
            }

            data = ReadString();
            return true;
        }
        
        public bool TryReadVector2(out Vector2 data)
        {
            data = Vector2.Zero;
            if (4 * 2 > UnreadLength)
            {
                return false;
            }

            data = ReadVector2();
            return true;
        }
        
        public bool TryReadVector3(out Vector3 data)
        {
            data = Vector3.Zero;
            if (4 * 3 > UnreadLength)
            {
                return false;
            }

            data = ReadVector3();
            return true;
        }
        
        public bool TryReadVector4(out Vector4 data)
        {
            data = Vector4.Zero;
            if (4 * 4 > UnreadLength)
            {
                return false;
            }

            data = ReadVector4();
            return true;
        }
        
        public bool TryReadQuaternion(out Quaternion data)
        {
            data = Quaternion.Identity;
            if (4 * 4 > UnreadLength)
            {
                return false;
            }

            data = ReadQuaternion();
            return true;
        }
        
        public bool TryReadMatrix(out Matrix4 data)
        {
            data = Matrix4.Identity;
            if (4 * 16 > UnreadLength)
            {
                return false;
            }

            data = ReadMatrix();
            return true;
        }

        public void WriteBytes(IEnumerable<byte> data)
        {
            _buffer.AddRange(data);
        }

        public void WriteInt16(short data)
        {
            var bytes = BitConverter.GetBytes(data);
            _buffer.AddRange(bytes);
        }
        
        public void WriteInt32(int data)
        {
            var bytes = BitConverter.GetBytes(data);
            _buffer.AddRange(bytes);
        }
        
        public void WriteInt64(long data)
        {
            var bytes = BitConverter.GetBytes(data);
            _buffer.AddRange(bytes);
        }
        
        public void WriteSingle(float data)
        {
            var bytes = BitConverter.GetBytes(data);
            _buffer.AddRange(bytes);
        }
        
        public void WriteString(string data)
        {
            _buffer.AddRange(BitConverter.GetBytes(data.Length));
            _buffer.AddRange(Encoding.ASCII.GetBytes(data));
        }
        
        public void WriteVector2ToBuffer(Vector2 data)
        {
            WriteSingle(data.X);
            WriteSingle(data.Y);
        }
        
        public void WriteVector3ToBuffer(Vector3 data)
        {
            WriteSingle(data.X);
            WriteSingle(data.Y);
            WriteSingle(data.Z);
        }
        
        public void WriteVector4ToBuffer(Vector4 data)
        {
            WriteSingle(data.X);
            WriteSingle(data.Y);
            WriteSingle(data.Z);
            WriteSingle(data.W);
        }
        
        public void WriteQuaternionToBuffer(Quaternion data)
        {
            WriteSingle(data.X);
            WriteSingle(data.Y);
            WriteSingle(data.Z);
            WriteSingle(data.W);
        }
        
        public void WriteMatrixToBuffer(Matrix4 data)
        {
            for (var r = 0; r < 4; r++)
            {
                for (var c = 0; c < 4; c++)
                {
                    WriteSingle(data[r, c]);
                }
            }
        }
        
        public void Clear()
        {
            _buffer.Clear();
            _readPosition = 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Clear();
                }
                _readPosition = 0;
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