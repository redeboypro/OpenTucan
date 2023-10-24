using System;
using System.Collections.Generic;
using System.Text;

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