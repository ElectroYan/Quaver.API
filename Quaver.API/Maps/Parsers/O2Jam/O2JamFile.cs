using Quaver.API.Maps.Parsers.O2Jam.EventPackages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public class O2JamFile
    {
        public OjnParser OjnParser; // Note file, contains metadata, note data (difficulties) and background image
        public OjmParser OjmParser; // Music file, contains the samples of the map (BGM, keysounds)
        public bool IsValid { get; set; }

        public O2JamFile(string ojnFilePath)
        {
            OjnParser = new OjnParser(ojnFilePath);
            OjnParser.Parse();

            IsValid = true;

            foreach (O2JamDifficulty difficulty in Enum.GetValues(typeof(O2JamDifficulty)))
                IsValid &= OjnParser.GetDifficulty(difficulty).Validate();

            Console.WriteLine();

        }
    }
}
