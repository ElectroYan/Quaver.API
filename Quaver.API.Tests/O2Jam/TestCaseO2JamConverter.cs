using Quaver.API.Maps.Parsers.O2Jam;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Quaver.API.Tests.O2Jam
{
    public class TestCaseO2JamConverter
    {
        [Fact]
        public void OjmParse()
        {
            var converter = new OjmParser("O2Jam/Resources/o2ma500.ojm");
            converter.Parse();
        }

        [Fact]
        public void OjnParse()
        {
            var converter = new OjnParser("O2Jam/Resources/o2ma500.ojn");
            converter.Parse();
        }
    }
}
