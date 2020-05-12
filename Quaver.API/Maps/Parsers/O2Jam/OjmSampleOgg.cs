using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class OjmSampleOgg
    {
        public string SampleName { get; set; }
        public int SampleSize { get; set; }
        public short SampleType { get; set; } // Unused in OMC format
        public short UnkFixedData { get; set; } // Unused in OMC format
        public int UnkMusicFlag { get; set; } // Unused in OMC format
        public short SampleNoteIndex { get; set; } // Unused in OMC format
        public short UnkZero { get; set; } // Unused in OMC format
        public int PCMSamples { get; set; } // Unused in OMC format
        public byte[] Data { get; set; }
    }
}
