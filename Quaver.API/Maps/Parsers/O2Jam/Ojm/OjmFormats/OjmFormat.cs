using Quaver.API.Maps.Parsers.O2Jam.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam.Ojm.OjmFormats
{
    public abstract class OjmFormat
    {
        public O2JamOjmFileSignature FileSignature;
        public List<OjmSampleOgg> SampleOggs { get; set; }
        public List<OjmSampleWav> SampleWav { get; set; }

        protected ByteDecoder decoder;

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
