using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.API.Maps.Parsers.O2Jam.EventPackages
{
    public class O2JamBpmEventPackage : O2JamEventPackage
    {
        public float BPM;
        public O2JamBpmEventPackage(float bpm) => BPM = bpm;
    }
}
