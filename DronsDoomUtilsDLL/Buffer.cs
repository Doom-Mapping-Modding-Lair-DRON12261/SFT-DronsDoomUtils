using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DronDoomTexUtilsDLL
{
    public class Buffer
    {
        // Variables
        private bool _isBigEndian = true;
        private int _position = 0;
        private byte[] _buffer;



        // Constructor
        public Buffer(byte[] bytes, bool isBigEndian = false)
        {
            _isBigEndian = isBigEndian;
            _buffer = bytes;
        }



        // Properties
        public byte this[int index] => _buffer[index];
        public bool isBigEndian => _isBigEndian;
        public int Length => _buffer.Length;
        public int Position
        {
            get => _position;
            set
            {
                if (value < 0)
                    _position = 0;
                else if (value >= Length)
                    _position = Length - 1;
                else 
                    _position = value;
            }
        }



        // General methods
        private bool CheckValidPosition(int position) 
        {
            if (position >= Length || position < 0) return false;
            return true;
        }



        // Read methods
        public bool ReadBoolean() => ReadBytes(1)[0] == 1;
        public byte ReadByte() => ReadBytes(1)[0];
        public byte[] ReadBytes(int count)
        {
            byte[] resultBytes = new byte[count];

            for (int i = 0, currentPosition = _position; currentPosition < _position + count; i++, currentPosition++)
                resultBytes[i] = _buffer[currentPosition];

            if (isBigEndian && BitConverter.IsLittleEndian)
                Array.Reverse(resultBytes);

            _position += count;

            return resultBytes;
        }
        public short ReadInt16() => BitConverter.ToInt16(ReadBytes(2), 0);
        public int ReadInt32() => BitConverter.ToInt32(ReadBytes(4), 0);
        public long ReadInt64() => BitConverter.ToInt64(ReadBytes(8), 0);
        public float ReadFloat() => BitConverter.ToSingle(ReadBytes(4), 0);
        public double ReadDouble() => BitConverter.ToDouble(ReadBytes(8), 0);
        public char ReadChar() => (char)ReadBytes(1)[0];
        public string ReadString(int lenght)
        {
            string resultString = "";

            for (int i = 0, currentPosition = _position; currentPosition < _position + lenght; i++, currentPosition++)
                resultString += (char)_buffer[currentPosition];

            return resultString;
        }



        // Write methods
        public bool WriteBoolean(bool value)
        {
            if (!CheckValidPosition(_position)) return false;

            _buffer[_position++] = value == true ? (byte)1 : (byte)0;

            return true;
        }
        public bool WriteByte(byte value)
        {
            if (!CheckValidPosition(_position)) return false;

            _buffer[_position++] = value;

            return true;
        }
        public bool WriteBytes(byte[] value)
        {
            if (!CheckValidPosition(_position)) return false;
            if (!CheckValidPosition(_position + value.Length)) return false;

            foreach (byte i in value)
                _buffer[_position++] = i;

            return true;
        }
        public bool WriteInt16(short value)
        {
            byte[] shortBytes = BitConverter.GetBytes(value);

            if (!CheckValidPosition(_position)) return false;
            if (!CheckValidPosition(_position + shortBytes.Length)) return false;

            if (isBigEndian && BitConverter.IsLittleEndian)
                Array.Reverse(shortBytes);

            foreach (byte i in shortBytes)
                _buffer[_position++] = i;

            return true;
        }
        public bool WriteInt32(int value)
        {
            byte[] intBytes = BitConverter.GetBytes(value);

            if (!CheckValidPosition(_position)) return false;
            if (!CheckValidPosition(_position + intBytes.Length)) return false;

            if (isBigEndian && BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);

            foreach (byte i in intBytes)
                _buffer[_position++] = i;

            return true;
        }
        public bool WriteInt64(long value)
        {
            byte[] longBytes = BitConverter.GetBytes(value);

            if (!CheckValidPosition(_position)) return false;
            if (!CheckValidPosition(_position + longBytes.Length)) return false;

            if (isBigEndian && BitConverter.IsLittleEndian)
                Array.Reverse(longBytes);

            foreach (byte i in longBytes)
                _buffer[_position++] = i;

            return true;
        }
        public bool WriteFloat(float value)
        {
            byte[] floatBytes = BitConverter.GetBytes(value);

            if (!CheckValidPosition(_position)) return false;
            if (!CheckValidPosition(_position + floatBytes.Length)) return false;

            if (isBigEndian && BitConverter.IsLittleEndian)
                Array.Reverse(floatBytes);

            foreach (byte i in floatBytes)
                _buffer[_position++] = i;

            return true;
        }
        public bool WriteDouble(float value)
        {
            byte[] doubleBytes = BitConverter.GetBytes(value);

            if (!CheckValidPosition(_position)) return false;
            if (!CheckValidPosition(_position + doubleBytes.Length)) return false;

            if (isBigEndian && BitConverter.IsLittleEndian)
                Array.Reverse(doubleBytes);

            foreach (byte i in doubleBytes)
                _buffer[_position++] = i;

            return true;
        }
        public bool WriteChar(char value)
        {
            if (!CheckValidPosition(_position)) return false;

            _buffer[_position++] = (byte)value;

            return true;
        }
        public bool WriteString(string value)
        {
            byte[] stringBytes = new byte[value.Length];

            if (!CheckValidPosition(_position)) return false;
            if (!CheckValidPosition(_position + stringBytes.Length)) return false;

            for (int i = 0; i < value.Length; i++)
                stringBytes[i] = (byte)value[i];

            foreach (byte i in stringBytes)
                _buffer[_position++] = i;

            return true;
        }
    }
}
