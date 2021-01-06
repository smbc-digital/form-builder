using form_builder.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class AddressExtensionsTests
    {        
        [Theory]
        [InlineData("1 London RD, MAnChester, M12 4dr", "1 London Rd, Manchester, M12 4DR")]
        [InlineData("1 LONDON RD, MANCHESTER, M12 4DR", "1 London Rd, Manchester, M12 4DR")]
        [InlineData("1 london rd, manchester, m12 4dr", "1 London Rd, Manchester, M12 4DR")]
        public void AppointmentLocation_ShouldReturnTitleCaseString_WhenLocationFound(string address, string expectedResult)
        {
            var result = address.ConvertAddressToTitleCase();

            Assert.Equal(expectedResult, result);
        }
    }
}
