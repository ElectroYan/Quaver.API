using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam.Ojm.OjmFormats
{
    public class OjmFormatM30 : OjmFormat
    {
        // Bitmasks for M30 xor'ing
        private static readonly byte[] XOR_NAMI = new byte[] { 0x6E, 0x61, 0x6D, 0x69 }; // nami
        private static readonly byte[] XOR_0412 = new byte[] { 0x30, 0x34, 0x31, 0x32 }; // 0412

        public int Version { get; private set; }
        public int EncryptionSign { get; private set; }
        public int SampleCount { get; private set; }
        public int SampleStartOffset { get; private set; }
        public int SampleDataSize { get; private set; }
        public byte[] Padding { get; private set; }

        public OjmFormatM30(ByteDecoder decoder) : base(decoder, Enums.O2JamOjmFileSignature.M30)
        {
        }

        /// <summary>
        /// Parses the ojm if the file signature is M30
        /// </summary>
        public override void Parse()
        {
            Version = decoder.ReadInt();
            EncryptionSign = decoder.ReadInt();
            SampleCount = decoder.ReadInt();
            SampleStartOffset = decoder.ReadInt();
            SampleDataSize = decoder.ReadInt();
            Padding = decoder.ReadBytes(4);

            SampleOggs = new List<OjmSampleOgg>();

            for (var i = 0; i < SampleCount; i++)
            {
                var sample = new OjmSampleOgg
                {
                    SampleName = decoder.ReadString(32),
                    SampleSize = decoder.ReadInt(),
                    SampleType = decoder.ReadShort(),
                    UnkFixedData = decoder.ReadShort(),
                    UnkMusicFlag = decoder.ReadInt(),
                    UnkZero = decoder.ReadShort(),
                    PcmSamples = decoder.ReadInt()
                };

                sample.SampleNoteIndex = (short)((sample.SampleType == 0 ? 1000 : 0) + decoder.ReadShort());
                sample.Data = decoder.ReadBytes(sample.SampleSize);

                switch (EncryptionSign)
                {
                    case 0: break;
                    case 16:
                        sample.Data = XorM30(sample.Data, XOR_NAMI);
                        break;
                    case 32:
                        sample.Data = XorM30(sample.Data, XOR_0412);
                        break;
                    default:
                        throw new Exception("Invalid M30 encryption");
                }

                SampleOggs.Add(sample);
            }
        }

        public override void SaveAudioTo(string directoryPath)
        {
            for (var i = 0; i < SampleOggs.Count; i++)
            {
                var item = SampleOggs[i];
                if (item.SampleType == 0)
                {
                    Write(directoryPath, $"M{item.SampleNoteIndex}.ogg", item.Data);
                    item.FileName = $"M{item.SampleNoteIndex}.ogg";
                } else
                {
                    Write(directoryPath, $"W{item.SampleNoteIndex}.ogg", item.Data);
                    item.FileName = $"W{item.SampleNoteIndex}.ogg";
                }
            }
        }

        /// <summary>
        /// Applies XOR to some data in blocks of 4. If data.Length % 4 != 0 the remaining bytes won't be xor'ed
        /// </summary>
        /// <param name="data">The data to be xor'ed</param>
        /// <param name="mask">The bitmask to xor the data with</param>
        /// <returns></returns>
        private byte[] XorM30(byte[] data, byte[] mask)
        {
            for (var j = 0; j < data.Length - 3; j += 4)
            {
                data[j + 0] = (byte)(mask[0] ^ data[j + 0]);
                data[j + 1] = (byte)(mask[1] ^ data[j + 1]);
                data[j + 2] = (byte)(mask[2] ^ data[j + 2]);
                data[j + 3] = (byte)(mask[3] ^ data[j + 3]);
            }
            return data;
        }
    }
}
