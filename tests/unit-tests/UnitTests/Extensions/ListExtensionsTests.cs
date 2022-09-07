using form_builder.Extensions;
using form_builder.Models;
using StockportGovUK.NetStandard.Models.Booking.Request;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class ListExtensionsTests
    {
        [Fact]
        public void GetAppointmentTypeForEnvironment_ShouldReturnAppointmenType_Object()
        {
            var id = Guid.NewGuid();
            var testList = new List<AppointmentType>
            {
                new AppointmentType
                {
                    Environment = "local",
                    AppointmentId = id,
                    OptionalResources = new List<BookingResource>{
                        new BookingResource()
                    }
                }
            };
            var result = testList.GetAppointmentTypeForEnvironment("uitest");


            Assert.NotNull(result);
            Assert.Equal(id, result.AppointmentId);
            Assert.Single(result.OptionalResources);
        }

        [Fact]
        public void GetAppointmentTypeForEnvironment_Should_Throw_Exception_When_ListDoesNotContain_Environemnt()
        {
            var testList = new List<AppointmentType>();

            Assert.Throws<InvalidOperationException>(() => testList.GetAppointmentTypeForEnvironment("unknown-env"));
        }
    }
}