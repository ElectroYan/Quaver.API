using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class OjmParser
    {
        // Bitmasks for M30 xor'ing
        private static readonly byte[] XOR_NAMI = new byte[] { 0x6E, 0x61, 0x6D, 0x69 }; // nami
        private static readonly byte[] XOR_0412 = new byte[] { 0x30, 0x34, 0x31, 0x32 }; // 0412
        private readonly FileStream stream;

        public OjmParser(string filePath) => stream = new FileStream(filePath, FileMode.Open);
        public OjmParser(OjnParser ojn) => stream = new FileStream(Path.Combine(ojn.OriginalDirectory, ojn.OjmFilePath), FileMode.Open);

        private ByteDecoder decoder;

        public string FileSignature { get; set; }
        public List<OjmSampleOgg> SampleOggs { get; set; }
        public List<OjmSampleWav> SampleWav { get; set; }

        /// <summary>
        /// Parses an ojm file
        /// </summary>
        public void Parse()
        {
            decoder = new ByteDecoder(stream);
            FileSignature = decoder.ReadString(4);
            switch (FileSignature)
            {
                case "M30":
                    ParseAsM30();
                    break;
                case "OMC":
                    ParseAsOMC(true);
                    break;
                case "OJM":
                    ParseAsOMC(false);
                    break;
                default:
                    throw new Exception("Invalid file signature");
            }

            stream.Close();
        }

        /// <summary>
        /// Saves the audio files to a given path
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveAudioTo(string filePath)
        {
            if (FileSignature == "OJM" || FileSignature == "OMC")
            {
                for (var i = 0; i < SampleWav.Count; i++)
                {
                    var item = SampleWav[i];

                    Write($"W{item.SampleNoteIndex}.wav", item.Data);
                    item.FileName = $"W{item.SampleNoteIndex}.wav";
                }

                for (var i = 0; i < SampleOggs.Count; i++)
                {
                    var item = SampleOggs[i];

                    Write($"M{item.SampleNoteIndex}.ogg", item.Data);
                    item.FileName = $"M{item.SampleNoteIndex}.ogg";
                }
            } else
            {
                for (var i = 0; i < SampleOggs.Count; i++)
                {
                    var item = SampleOggs[i];
                    if (item.SampleType == 0)
                    {
                        Write($"M{item.SampleNoteIndex}.ogg", item.Data);
                        item.FileName = $"M{item.SampleNoteIndex}.ogg";
                    } else
                    {
                        Write($"W{item.SampleNoteIndex}.ogg", item.Data);
                        item.FileName = $"W{item.SampleNoteIndex}.ogg";
                    }
                }
            }

            void Write(string fileName, byte[] data)
            {
                File.WriteAllBytes(Path.Combine(filePath, fileName), data);
            }
        }

        /// <summary>
        /// Parses the ojm if the file signature is M30
        /// </summary>
        private void ParseAsM30()
        {
            var version = decoder.ReadInt();
            var encryptionSign = decoder.ReadInt();
            var sampleCount = decoder.ReadInt();
            var sampleStartOffset = decoder.ReadInt();
            var sampleDataSize = decoder.ReadInt();
            var padding = decoder.ReadBytes(4);

            SampleOggs = new List<OjmSampleOgg>();

            for (var i = 0; i < sampleCount; i++)
            {
                var sample = new OjmSampleOgg
                {
                    SampleName = decoder.ReadString(32),
                    SampleSize = decoder.ReadInt(),
                    SampleType = decoder.ReadShort(),
                    UnkFixedData = decoder.ReadShort(),
                    UnkMusicFlag = decoder.ReadInt(),
                    UnkZero = decoder.ReadShort(),
                    PcmSamples = decoder.ReadInt(),
                };

                sample.SampleNoteIndex = (short)((sample.SampleType == 0 ? 1000 : 0) + decoder.ReadShort());

                sample.Data = decoder.ReadBytes(sample.SampleSize);

                switch (encryptionSign)
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

        /// <summary>
        /// Parses the ojm if the file signature is OMC or OJM
        /// </summary>
        /// <param name="decrypt">OMC files need to be decrypted, so if it's one, decrypt = true</param>
        private void ParseAsOMC(bool decrypt)
        {
            var wavSampleCount = decoder.ReadShort();
            var oggSampleCount = decoder.ReadShort();
            var wavStartOffset = decoder.ReadInt();
            var oggStartOffset = decoder.ReadInt();
            var fileSize = decoder.ReadInt();

            SampleWav = new List<OjmSampleWav>();
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
                    SampleNoteIndex = (short)i,
                };
                sample.Data = decoder.ReadBytes(sample.SampleSize);
                if (decrypt)
                    sample.Data = AccXor(Rearrange(sample.Data)); // Decrypt if OMC
                SampleWav.Add(sample);
            }

            SampleOggs = new List<OjmSampleOgg>();
            stream.Position = oggStartOffset;
            for (var i = 0; i < oggSampleCount; i++)
            {
                var sample = new OjmSampleOgg
                {
                    SampleName = decoder.ReadString(32),
                    SampleSize = decoder.ReadInt(),
                    SampleNoteIndex = (short)(1000 + i), // Music files have an index-offset of 1000
                };
                sample.Data = decoder.ReadBytes(sample.SampleSize);
                SampleOggs.Add(sample);
            }

            stream.Close();
        }

        // vvv Following code is copied from the O2Jam documentation vvv
        // I have no idea what it does or why it does it but it works
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
