using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class OjmParser
    {
        private readonly FileStream stream;

        public OjmParser(string filePath) => stream = new FileStream(filePath, FileMode.Open);
        public OjmParser(OjnParser ojn) => stream = new FileStream(Path.Combine(ojn.OriginalDirectory, ojn.OjmFile), FileMode.Open);

        private ByteDecoder decoder;

        public void Parse()
        {

            decoder = new ByteDecoder(stream);
            var fileSignature = decoder.ReadString(4);
            switch (fileSignature)
            {
                case "M30\0":
                    ParseAsM30();
                    break;
                case "OMC\0":
                    ParseAsOMC();
                    break;
                default:
                    throw new Exception("Invalid file");
            }

            stream.Close();
        }

        private void ParseAsM30()
        {
            var enc = new UTF8Encoding(true);
            var version = decoder.ReadInt();
            var encryptionSign = decoder.ReadInt();
            var sampleCount = decoder.ReadInt();
            var sampleStartOffset = decoder.ReadInt();
            var sampleDataSize = decoder.ReadInt();
            var padding = decoder.ReadBytes(4);

            var samples = new List<OjmSampleOgg>();

            for (var i = 0; i < sampleCount; i++)
            {
                var sample = new OjmSampleOgg
                {
                    SampleName = decoder.ReadString(32),
                    SampleSize = decoder.ReadInt(),
                    SampleType = decoder.ReadShort(),
                    UnkFixedData = decoder.ReadShort(),
                    UnkMusicFlag = decoder.ReadInt(),
                    SampleNoteIndex = decoder.ReadShort(),
                    UnkZero = decoder.ReadShort(),
                    PcmSamples = decoder.ReadInt(),
                };
                sample.Data = decoder.ReadBytes(sample.SampleSize);
                for (var j = 0; j < sample.SampleSize - 4; j += 4)
                {
                    sample.Data[j + 0] = (byte)('n' ^ sample.Data[j + 0]);
                    sample.Data[j + 1] = (byte)('a' ^ sample.Data[j + 1]);
                    sample.Data[j + 2] = (byte)('m' ^ sample.Data[j + 2]);
                    sample.Data[j + 3] = (byte)('i' ^ sample.Data[j + 3]);
                }
                samples.Add(sample);
            }
        }

        private void ParseAsOMC()
        {
            var wavSampleCount = decoder.ReadSingle();
            var oggSampleCount = decoder.ReadSingle();
            var wavStartOffset = decoder.ReadInt();
            var oggStartOffset = decoder.ReadInt();
            var fileSize = decoder.ReadInt();

            var wavSamples = new List<OjmSampleWav>();
            stream.Position = wavStartOffset;
            for (var i = 0; i < wavSampleCount; i++)
            {
                var sample = new OjmSampleWav
                {
                    SampleName = decoder.ReadString(32),
                    AudioFormat = decoder.ReadShort(),
                    ChannelCount = decoder.ReadShort(),
                    BitRate = decoder.ReadInt(),
                    BlockAlign = decoder.ReadShort(),
                    BitPerSample = decoder.ReadShort(),
                    ChunkData = decoder.ReadInt(),
                    SampleSize = decoder.ReadInt(),
                };
                sample.Data = AccXor(Rearrange(decoder.ReadBytes(sample.SampleSize)));
                wavSamples.Add(sample);
            }

            var oggSamples = new List<OjmSampleOgg>();
            stream.Position = oggStartOffset;
            for (var i = 0; i < oggSampleCount; i++)
            {
                var sample = new OjmSampleOgg
                {
                    SampleName = decoder.ReadString(32),
                    SampleSize = decoder.ReadInt()
                };
                sample.Data = decoder.ReadBytes(sample.SampleSize);
                oggSamples.Add(sample);
            }

            stream.Close();
        }

        // vvv Following code is copied from the O2Jam is documentation vvv
        private static readonly byte[] REARRANGE_TABLE = new byte[]{
            0x10, 0x0E, 0x02, 0x09, 0x04, 0x00, 0x07, 0x01,
            0x06, 0x08, 0x0F, 0x0A, 0x05, 0x0C, 0x03, 0x0D,
            0x0B, 0x07, 0x02, 0x0A, 0x0B, 0x03, 0x05, 0x0D,
            0x08, 0x04, 0x00, 0x0C, 0x06, 0x0F, 0x0E, 0x10,
            0x01, 0x09, 0x0C, 0x0D, 0x03, 0x00, 0x06, 0x09,
            0x0A, 0x01, 0x07, 0x08, 0x10, 0x02, 0x0B, 0x0E,
            0x04, 0x0F, 0x05, 0x08, 0x03, 0x04, 0x0D, 0x06,
            0x05, 0x0B, 0x10, 0x02, 0x0C, 0x07, 0x09, 0x0A,
            0x0F, 0x0E, 0x00, 0x01, 0x0F, 0x02, 0x0C, 0x0D,
            0x00, 0x04, 0x01, 0x05, 0x07, 0x03, 0x09, 0x10,
            0x06, 0x0B, 0x0A, 0x08, 0x0E, 0x00, 0x04, 0x0B,
            0x10, 0x0F, 0x0D, 0x0C, 0x06, 0x05, 0x07, 0x01,
            0x02, 0x03, 0x08, 0x09, 0x0A, 0x0E, 0x03, 0x10,
            0x08, 0x07, 0x06, 0x09, 0x0E, 0x0D, 0x00, 0x0A,
            0x0B, 0x04, 0x05, 0x0C, 0x02, 0x01, 0x0F, 0x04,
            0x0E, 0x10, 0x0F, 0x05, 0x08, 0x07, 0x0B, 0x00,
            0x01, 0x06, 0x02, 0x0C, 0x09, 0x03, 0x0A, 0x0D,
            0x06, 0x0D, 0x0E, 0x07, 0x10, 0x0A, 0x0B, 0x00,
            0x01, 0x0C, 0x0F, 0x02, 0x03, 0x08, 0x09, 0x04,
            0x05, 0x0A, 0x0C, 0x00, 0x08, 0x09, 0x0D, 0x03,
            0x04, 0x05, 0x10, 0x0E, 0x0F, 0x01, 0x02, 0x0B,
            0x06, 0x07, 0x05, 0x06, 0x0C, 0x04, 0x0D, 0x0F,
            0x07, 0x0E, 0x08, 0x01, 0x09, 0x02, 0x10, 0x0A,
            0x0B, 0x00, 0x03, 0x0B, 0x0F, 0x04, 0x0E, 0x03,
            0x01, 0x00, 0x02, 0x0D, 0x0C, 0x06, 0x07, 0x05,
            0x10, 0x09, 0x08, 0x0A, 0x03, 0x02, 0x01, 0x00,
            0x04, 0x0C, 0x0D, 0x0B, 0x10, 0x05, 0x06, 0x0F,
            0x0E, 0x07, 0x09, 0x0A, 0x08, 0x09, 0x0A, 0x00,
            0x07, 0x08, 0x06, 0x10, 0x03, 0x04, 0x01, 0x02,
            0x05, 0x0B, 0x0E, 0x0F, 0x0D, 0x0C, 0x0A, 0x06,
            0x09, 0x0C, 0x0B, 0x10, 0x07, 0x08, 0x00, 0x0F,
            0x03, 0x01, 0x02, 0x05, 0x0D, 0x0E, 0x04, 0x0D,
            0x00, 0x01, 0x0E, 0x02, 0x03, 0x08, 0x0B, 0x07,
            0x0C, 0x09, 0x05, 0x0A, 0x0F, 0x04, 0x06, 0x10,
            0x01, 0x0E, 0x02, 0x03, 0x0D, 0x0B, 0x07, 0x00,
            0x08, 0x0C, 0x09, 0x06, 0x0F, 0x10, 0x05, 0x0A,
            0x04, 0x00
        };

        private static byte[] Rearrange(byte[] buf_encoded)
        {
            var length = buf_encoded.Length;
            var key = ((length % 17) << 4) + (length % 17);

            var block_size = length / 17;

            // Let's fill the buffer
            var buf_plain = new byte[length];
            Array.Copy(buf_encoded, 0, buf_plain, 0, length);

            for (var block = 0; block < 17; block++) // loopy loop
            {
                var block_start_encoded = block_size * block;   // Where is the start of the enconded block
                var block_start_plain = block_size * REARRANGE_TABLE[key];  // Where the final plain block will be
                Array.Copy(buf_encoded, block_start_encoded, buf_plain, block_start_plain, block_size);

                key++;
            }
            return buf_plain;
        }

        private static int acc_keybyte = 0xFF;
        private static int acc_counter = 0;
        private static byte[] AccXor(byte[] buf)
        {
            for (var i = 0; i < buf.Length; i++)
            {
                byte this_byte;
                int temp = this_byte = buf[i];

                if (((acc_keybyte << acc_counter) & 0x80) != 0)
                {
                    this_byte = (byte)~this_byte;
                }

                buf[i] = this_byte;
                acc_counter++;
                if (acc_counter > 7)
                {
                    acc_counter = 0;
                    acc_keybyte = temp;
                }
            }
            return buf;
        }
    }
}
