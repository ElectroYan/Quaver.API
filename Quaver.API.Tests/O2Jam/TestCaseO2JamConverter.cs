using Quaver.API.Maps.Parsers.O2Jam;
using Quaver.API.Maps.Parsers.O2Jam.EventPackages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;

namespace Quaver.API.Tests.O2Jam
{
    public class TestCaseO2JamConverter
    {
        [Fact]
        public void SuccussfulParseM30Header()
        {
            var converter = new O2JamFile("./O2Jam/Resources/o2ma500.ojn");
            Assert.True(converter.IsValid);
        }
        [Fact]
        public void SuccussfulParseOJMHeader()
        {
            var converter = new O2JamFile("./O2Jam/Resources/o2ma487.ojn");
            Assert.True(converter.IsValid);
        }

        [Fact]
        public void SuccussfulParseOMCHeader()
        {
            var converter = new O2JamFile("./O2Jam/Resources/o2ma514.ojn");
            Assert.True(converter.IsValid);
        }

        [Fact]
        public void CheckOjnMetadata()
        {
            var converter = new O2JamFile("./O2Jam/Resources/o2ma500.ojn");
            Assert.Equal(500, converter.OjnParser.IDSong);
            Assert.Equal("DM Ashura", converter.OjnParser.Artist);
            Assert.Equal("o2ma500.ojm", converter.OjnParser.OjmFile);
        }
    }
}
