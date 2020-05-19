using Quaver.API.Maps.Parsers.O2Jam;
using Quaver.API.Maps.Parsers.O2Jam.Enums;
using Quaver.API.Maps.Parsers.O2Jam.Ojm.OjmFormats;
using System.Linq;
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

            var ojmFormat = converter.OjmParser.OjmFormat;
            Assert.Equal(O2JamOjmFileSignature.M30, ojmFormat.FileSignature);
            Assert.Single(ojmFormat.SampleOggs);
            Assert.Equal("o2ma2666", ojmFormat.SampleOggs.First().SampleName);
            Assert.Equal(2659994, ((OjmFormatM30)ojmFormat).SampleDataSize);
        }

        [Fact]
        public void SuccussfulParseOJMHeader()
        {
            var converter = new O2JamFile("./O2Jam/Resources/o2ma487.ojn");
            Assert.True(converter.IsValid);

            var ojmFormat = converter.OjmParser.OjmFormat;
            Assert.Equal(O2JamOjmFileSignature.OJM, ojmFormat.FileSignature);
            Assert.Single(ojmFormat.SampleOggs);
            Assert.Equal("1.ogg", ojmFormat.SampleOggs.First().SampleName);
            Assert.Equal(3317315, ((OjmFormatOmcOjm)ojmFormat).FileSize);
        }

        [Fact]
        public void SuccussfulParseOMCHeader()
        {
            var converter = new O2JamFile("./O2Jam/Resources/o2ma514.ojn");
            Assert.True(converter.IsValid);

            var ojmFormat = converter.OjmParser.OjmFormat;
            Assert.Equal(O2JamOjmFileSignature.OMC, ojmFormat.FileSignature);
            Assert.Single(ojmFormat.SampleOggs);
            Assert.Equal(3507123, ((OjmFormatOmcOjm)ojmFormat).FileSize);
        }

        [Fact]
        public void CheckOjnMetadata()
        {
            var converter = new O2JamFile("./O2Jam/Resources/o2ma500.ojn");
            Assert.Equal(500, converter.OjnParser.SongID);
            Assert.Equal("DM Ashura", converter.OjnParser.SongArtist);
            Assert.Equal("o2ma500.ojm", converter.OjnParser.OjmFilePath);
            Assert.Equal(27, converter.OjnParser.Difficulties[0].Level);

            converter = new O2JamFile("./O2Jam/Resources/testing.ojn");
            Assert.Equal(O2JamGenre.Others, converter.OjnParser.SongGenre);
        }

        [Fact]
        public void CheckOjnHitObjectsAndTimingPoints()
        {
            var converter = new O2JamFile("./O2Jam/Resources/o2ma500.ojn");
            var qua = converter.ToQua(O2JamDifficulty.Hard);
            Assert.Equal(150, qua.TimingPoints.First().Bpm);
            Assert.Equal(1, qua.HitObjects.First().Lane);
            Assert.Equal(6, qua.HitObjects.Last().Lane);

            converter = new O2JamFile("./O2Jam/Resources/testing.ojn");
            qua = converter.ToQua(O2JamDifficulty.Easy);
            Assert.Equal(200, qua.TimingPoints.First().Bpm);
            Assert.Equal(1, qua.HitObjects.First().Lane);
        }
    }
}
