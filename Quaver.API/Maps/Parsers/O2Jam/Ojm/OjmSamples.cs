using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public struct OjmSampleOgg
    {
        public string SampleName;
        public int SampleSize;
        public short SampleType; // Unused in OMC format
        public short UnkFixedData; // Unused in OMC format
        public int UnkMusicFlag; // Unused in OMC format
        public short SampleNoteIndex; // Unused in OMC format
        public short UnkZero; // Unused in OMC format
        public int PcmSamples; // Unused in OMC format
        public byte[] Data;
        public string FileName;
    }

    public struct OjmSampleWav
    {
        public string SampleName;
        public short AudioFormat;
        public short ChannelCount;
        public int SampleRate;
        public int BitRate;
        public short BlockAlign;
        public short BitPerSample;
        public int ChunkData;
        public int SampleSize;
        public byte[] Data;
        public string FileName;
    }
}
