using System;
using System.Collections.Generic;
using form_builder.Extensions;
using StockportGovUK.NetStandard.Models.Booking.Request;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class BookingResourceExtensionsTests
    {

        [Fact]
        public void BookingResources_ShouldReturnEmptyString_WhenBooking_HasNoElements()
        {
            var result = new List<BookingResource>().CreateKeyFromResources();

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void BookingResources_ShouldReturn_String_WhenQuanity_And_ResourceId()
        {
            var guid = Guid.NewGuid();
            var quantity = 2;
            var result = new List<BookingResource> { new BookingResource { Quantity = quantity, ResourceId = guid } }.CreateKeyFromResources();

            Assert.Equal($"-{quantity}{guid}", result);
        }
    }
}
