using Quaver.API.Maps.Parsers.O2Jam.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam.Ojm.OjmFormats
{
    /// <summary>
    ///     Abstract container for all existing O2Jam formats
    /// </summary>
    public abstract class OjmFormat
    {
        /// <summary>
        ///     Ojm format type
        /// </summary>
        public O2JamOjmFileSignature FileSignature;

        public int FileSize;

        /// <summary>
        ///     List of all OGG samples
        /// </summary>
        public List<OjmSampleOgg> SampleOggs { get; set; }

        /// <summary>
        ///     List of all OGG samples
        /// </summary>
        public List<OjmSampleWav> SampleWav { get; set; }

        protected ByteDecoder decoder;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="decoder"></param>
        /// <param name="fileSignature"></param>
        public OjmFormat(ByteDecoder decoder, O2JamOjmFileSignature fileSignature)
        {
            this.decoder = decoder;
            FileSignature = fileSignature;
        }

        /// <summary>
        ///     Parses a Ojm map, depending on the format
        /// </summary>
        public abstract void Parse();

        /// <summary>
        ///     Saves the audio to a specified directory
        /// </summary>
        public abstract void SaveAudioTo(string directoryPath);

        /// <summary>
        ///     Writes bytes to a file
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        public void Write(string directoryPath, string fileName, byte[] data) => File.WriteAllBytes(Path.Combine(directoryPath, fileName), data);
    }
}
