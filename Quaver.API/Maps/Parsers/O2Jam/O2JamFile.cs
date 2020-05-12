using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class O2JamFile
    {
        private OjnParser ojnParser;
        private OjmParser ojmParser;

        public static O2JamFile Parse(string ojnFilePath)
        {
            var o2jamFile = new O2JamFile
            {
                ojnParser = new OjnParser(ojnFilePath)
            };
            o2jamFile.ojmParser = new OjmParser(o2jamFile.ojnParser.OjmFile);


            return o2jamFile;
        }

    }
}
