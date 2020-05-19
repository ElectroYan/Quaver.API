using Quaver.API.Maps.Parsers.O2Jam.Enums;
using Quaver.API.Maps.Parsers.O2Jam.Ojm.OjmFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class OjmParser
    {
        public OjmParser(string filePath) => Stream = new FileStream(filePath, FileMode.Open);
        public OjmParser(OjnParser ojn) => Stream = new FileStream(Path.Combine(ojn.OriginalDirectory, ojn.OjmFilePath), FileMode.Open);

        protected readonly FileStream Stream;
        protected ByteDecoder decoder;

        public OjmFormat OjmFormat;
        public List<OjmSampleOgg> SampleOggs { get; set; }
        public List<OjmSampleWav> SampleWav { get; set; }

        /// <summary>
        ///     Parses an ojm file
        /// </summary>
        public void Parse()
        {
            decoder = new ByteDecoder(Stream);
            var fileSignature = (O2JamOjmFileSignature)Enum.Parse(typeof(O2JamOjmFileSignature), decoder.ReadString(4));
            switch (fileSignature)
            {
                case O2JamOjmFileSignature.M30:
                    OjmFormat = new OjmFormatM30(decoder);
                    break;
                case O2JamOjmFileSignature.OJM:
                case O2JamOjmFileSignature.OMC:
                    OjmFormat = new OjmFormatOmcOjm(decoder, fileSignature);
                    break;
                default:
                    throw new Exception("Invalid file signature");
            }

            OjmFormat.Parse();
            Stream.Close();
        }
    }
}
