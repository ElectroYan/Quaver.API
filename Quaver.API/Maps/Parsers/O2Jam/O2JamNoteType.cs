using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam
{
    public enum O2JamNoteType
    {
        NormalNote = 1, // WAV
        StartLongNote = 2,
        EndLongNote = 3,
        BgmNote = 4 // OGG
    }
}
