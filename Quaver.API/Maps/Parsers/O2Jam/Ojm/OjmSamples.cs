namespace Quaver.API.Maps.Parsers.O2Jam
{
    public struct OjmSampleOgg
    {
        public string SampleName;
        public int SampleSize;

        /// <summary>
        /// SampleType = 0:
        ///     Music file
        /// SampleType = 5:
        ///     Keysound
        /// </summary>
        public short SampleType; // Unused in OMC/OJM format
        public short UnkFixedData; // Unused in OMC/OJM format
        public int UnkMusicFlag; // Unused in OMC/OJM format

        /// <summary>
        /// Reference index found in the note file
        /// </summary>
        public short SampleNoteIndex;
        public short UnkZero; // Unused in OMC/OJM format
        public int PcmSamples; // Unused in OMC/OJM format
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

        /// <summary>
        /// Reference index found in the note file
        /// </summary>

        public short SampleNoteIndex;
    }
}
