using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    /// <summary>
    ///     Helper class, used to read bytes from a binary file and convert into various basic data types.
    /// </summary>
    /// <remarks>
    ///     The position advances by n bytes with each function call, so they can be called in
    ///     succession without worrying about the position.
    /// </remarks>
    public class ByteDecoder
    {
        public static UTF8Encoding Encoder = new UTF8Encoding(true);

        /// <summary>
        ///     File stream to read from.
        /// </summary>
        public FileStream Stream;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="stream">File stream to read from</param>
        public ByteDecoder(FileStream stream) => Stream = stream;

        /// <summary>
        ///     Reads n bytes from the given file stream.
        /// </summary>
        /// <param name="n"></param>
        /// <returns>Byte array with n elements.</returns>
        public byte[] ReadBytes(int n)
        {
            var bytes = new byte[n];
            Stream.Read(bytes, 0, n);
            return bytes;
        }

        /// <summary>
        ///     Reads a string with the given length and strips everything after a
        ///     (and including the) '\0' char.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string ReadString(int bytes)
        {
            var readString = Encoder.GetString(ReadBytes(bytes));
            return readString.Substring(0, readString.IndexOf('\0'));
        }

        /// <summary>
        ///     Reads two bytes and converts it into a short
        /// </summary>
        /// <returns>Value of the two bytes as a short</returns>
        public short ReadShort() => BitConverter.ToInt16(ReadBytes(2), 0);

        /// <summary>
        ///     Reads four bytes and converts it into an int
        /// </summary>
        /// <returns>Value of the four bytes as an int</returns>
        public int ReadInt() => BitConverter.ToInt32(ReadBytes(4), 0);

        /// <summary>
        ///     Reads four bytes and converts it into a float
        /// </summary>
        /// <returns>Value of the four bytes as a float</returns>
        public float ReadSingle() => BitConverter.ToSingle(ReadBytes(4), 0);

        /// <summary>
        ///     Generic function, which takes one if the ByteDecoder read functions to read n times
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">Should be one of the ByteDecoder read functions</param>
        /// <param name="n">Number of values to read</param>
        /// <returns>An array of the functions return type with length n</returns>
        public T[] ReadArray<T>(Func<T> func, int n)
        {
            var array = new List<T>();
            for (var i = 0; i < n; i++)
                array.Add(func());
            return array.ToArray();
        }
    }
}
