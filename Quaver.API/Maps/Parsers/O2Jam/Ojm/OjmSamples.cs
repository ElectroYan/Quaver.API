namespace Quaver.API.Maps.Parsers.O2Jam
{
    public struct OjmSampleOgg
    {
        /// <summary>
        ///     The name the sample is currently saved as in the .ojm
        /// </summary>
        public string SampleName;

        /// <summary>
        ///     Data size of the sample
        /// </summary>
        public int SampleSize;

        /// <summary>
        ///     SampleType = 0: Music file
        ///     SampleType = 5: Keysound
        /// </summary>
        public short SampleType; // Unused in OMC/OJM format

        /// <summary>
        ///     Unknown data of fixed size
        /// </summary>
        public short UnkFixedData; // Unused in OMC/OJM format

        /// <summary>
        ///     Unknown data of fixed size
        /// </summary>
        public int UnkMusicFlag; // Unused in OMC/OJM format

        /// <summary>
        ///     Reference index found in the .ojn
        /// </summary>
        public short SampleNoteIndex;

        /// <summary>
        ///     Unknown data of fixed size with value 0
        /// </summary>
        public short UnkZero; // Unused in OMC/OJM format

        /// <summary>
        ///     Number of PCM (pulse-code modulation) samples
        /// </summary>
        public int PcmSamples; // Unused in OMC/OJM format

        /// <summary>
        ///     OGG file data
        /// </summary>
        public byte[] Data;

        /// <summary>
        ///     File name to save as
        /// </summary>
        public string FileName;
    }

    public struct OjmSampleWav
    {
        /// <summary>
        ///     The name the sample is currently saved as in the .ojm
        /// </summary>
        public string SampleName;

        /// <summary>
        ///     WAV relevant data
        /// </summary>
        public short AudioFormat;

        /// <summary>
        ///     WAV relevant data
        /// </summary>
        public short ChannelCount;

        /// <summary>
        ///     WAV relevant data
        /// </summary>
        public int SampleRate;

        /// <summary>
        ///     WAV relevant data
        /// </summary>
        public int BitRate;

        /// <summary>
        ///     WAV relevant data
        /// </summary>
        public short BlockAlign;

        /// <summary>
        ///     WAV relevant data
        /// </summary>
        public short BitPerSample;

        /// <summary>
        ///     WAV relevant data
        /// </summary>
        public int ChunkData;

        /// <summary>
        ///     WAV relevant data
        /// </summary>
        public int SampleSize;

        /// <summary>
        ///     WAV file data
        /// </summary>
        public byte[] Data;

        /// <summary>
        ///     File name to save as
        /// </summary>
        public string FileName;

        /// <summary>
        ///     Reference index found in the .ojn
        /// </summary>
        public short SampleNoteIndex;
    }
}
