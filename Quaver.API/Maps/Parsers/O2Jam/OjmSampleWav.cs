using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class OjmSampleWav
    {
        public string SampleName { get; set; }
        public short AudioFormat { get; set; }
        public short ChannelCount { get; set; }
        public int SampleRate { get; set; }
        public int BitRate { get; set; }
        public short BlockAlign { get; set; }
        public short BitPerSample { get; set; }
        public int ChunkData { get; set; }
        public int SampleSize { get; set; }
        public byte[] Data { get; set; }
    }
}
