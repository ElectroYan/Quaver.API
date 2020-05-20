using Quaver.API.Maps.Parsers.O2Jam.Enums;
using Quaver.API.Maps.Parsers.O2Jam.Ojm.OjmFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    /// <summary>
    ///     Music file, contains sound samples used in a specific chart
    ///     Represents a universal parser, which can read all possible .ojm formats
    /// </summary>
    /// <remarks>OJM -> O2Jam Music</remarks>
    public class OjmParser
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="filePath"></param>
        public OjmParser(string filePath) => Stream = new FileStream(filePath, FileMode.Open);
        public OjmParser(OjnParser ojn) => Stream = new FileStream(Path.Combine(ojn.OriginalDirectory, ojn.OjmFilePath), FileMode.Open);

        /// <summary>
        ///     Stream used to read from the file
        /// </summary>
        protected readonly FileStream Stream;

        /// <summary>
        ///     Helper, which makes reading values from a binary file easier
        /// </summary>
        protected ByteDecoder decoder;

        /// <summary>
        ///     General class for the provided format, which contains all relevant sound data
        /// </summary>
        public OjmFormat OjmFormat;

        /// <summary>
        ///     Parses an .ojm file
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
