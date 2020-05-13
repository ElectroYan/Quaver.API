using System.Collections.Generic;

namespace Quaver.API.Maps.Parsers.O2Jam.EventPackages
{
    public struct O2JamMainPackage
    {
        public int Measure;
        public short Channel;
        public short EventCount;
        public List<O2JamEventPackage> EventPackages;
    }
}
