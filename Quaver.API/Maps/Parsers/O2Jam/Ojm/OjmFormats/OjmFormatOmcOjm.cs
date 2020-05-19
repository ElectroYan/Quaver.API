using Quaver.API.Maps.Parsers.O2Jam.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam.Ojm.OjmFormats
{
    public class OjmFormatOmcOjm : OjmFormat
    {
        public short WavSampleCount { get; private set; }
        public short OggSampleCount { get; private set; }
        public int WavStartOffset { get; private set; }
        public int OggStartOffset { get; private set; }
        public int FileSize { get; private set; }

        public OjmFormatOmcOjm(ByteDecoder decoder, O2JamOjmFileSignature fileSignature) : base(decoder, fileSignature)
        {
        }

        /// <summary>
        /// Parses the ojm if the file signature is OMC or OJM
        /// </summary>
        /// <param name="decrypt">OMC files need to be decrypted, so if it's one, decrypt = true</param>
        public override void Parse()
        {
            WavSampleCount = decoder.ReadShort();
            OggSampleCount = decoder.ReadShort();
            WavStartOffset = decoder.ReadInt();
            OggStartOffset = decoder.ReadInt();
            FileSize = decoder.ReadInt();

            SampleWav = new List<OjmSampleWav>();
            decoder.Stream.Position = WavStartOffset;
            for (var i = 0; i < WavSampleCount; i++)
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
                if (FileSignature == O2JamOjmFileSignature.OMC)
                    sample.Data = AccXor(Rearrange(sample.Data)); // Decrypt if OMC
                SampleWav.Add(sample);
            }

            SampleOggs = new List<OjmSampleOgg>();
            decoder.Stream.Position = OggStartOffset;
            for (var i = 0; i < OggSampleCount; i++)
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

            decoder.Stream.Close();
        }

        public override void SaveAudioTo(string directoryPath)
        {
            for (var i = 0; i < SampleWav.Count; i++)
            {
                var item = SampleWav[i];

                Write(directoryPath, $"W{item.SampleNoteIndex}.wav", item.Data);
                item.FileName = $"W{item.SampleNoteIndex}.wav";
            }

            for (var i = 0; i < SampleOggs.Count; i++)
            {
                var item = SampleOggs[i];

                Write(directoryPath, $"M{item.SampleNoteIndex}.ogg", item.Data);
                item.FileName = $"M{item.SampleNoteIndex}.ogg";
            }
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
