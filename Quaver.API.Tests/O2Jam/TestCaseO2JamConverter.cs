using Quaver.API.Maps.Parsers.O2Jam;
using System.Linq;
using Xunit;

namespace Quaver.API.Tests.O2Jam
{
    public class TestCaseO2JamConverter
    {
        [Fact]
        public void SuccussfulParse()
        {
            var converter = new O2JamFile("./O2Jam/Resources/o2ma500.ojn");
            Assert.True(converter.IsValid);
        }

        [Fact]
        public void CheckOjnMetadata()
        {
            var converter = new O2JamFile("./O2Jam/Resources/o2ma500.ojn");
            Assert.Equal(500, converter.OjnParser.IDSong);
            Assert.Equal("DM Ashura", converter.OjnParser.Artist);
            Assert.Equal("o2ma500.ojm", converter.OjnParser.OjmFile);
            Assert.Equal(27, converter.OjnParser.Difficulties[0].Level);
        }

        [Fact]
        public void CheckOjnHitObjectsAndTimingPoints()
        {
            var converter = new O2JamFile("./O2Jam/Resources/o2ma500.ojn");
            var qua = converter.ToQua(O2JamDifficulty.Easy);
            //Assert.Equal(150, qua.TimingPoints.First().Bpm);
        }
    }
}
