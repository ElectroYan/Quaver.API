using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class ByteDecoder
    {
        public static UTF8Encoding Encoder = new UTF8Encoding(true);
        public FileStream Stream;

        public ByteDecoder(FileStream stream) => Stream = stream;

        public byte[] ReadBytes(int count)
        {
            var bytes = new byte[count];
            Stream.Read(bytes, 0, count);
            return bytes;
        }

        public string ReadString(int bytes) => Encoder.GetString(ReadBytes(bytes)).Trim('\0');
        public short ReadShort() => BitConverter.ToInt16(ReadBytes(2), 0);
        public int ReadInt() => BitConverter.ToInt32(ReadBytes(4), 0);
        public float ReadSingle() => BitConverter.ToSingle(ReadBytes(4), 0);
    }
}
