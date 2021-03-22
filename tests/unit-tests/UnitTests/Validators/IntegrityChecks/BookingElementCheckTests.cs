using System;
using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using StockportGovUK.NetStandard.Models.Booking.Request;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class BookingCheckTests
    {
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();
        
        [Theory]
        [InlineData(EElementType.Booking, "local", "local", "provider", true)]
        public void BookingElementCheck_ReturnValid(EElementType elementType, string actualEnv, string appointmentTypeEnv, string provider, bool optionalResource)
        {
            // Arrange
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns(actualEnv);

            var appointmentType = new AppointmentTypeBuilder()
                .WithEnvironment(appointmentTypeEnv)
                .WithOptionalResource(new BookingResource { ResourceId = Guid.Empty, Quantity = 1 })
                .Build();

            if (optionalResource)
            {
                appointmentType.OptionalResources = new List<BookingResource>
                {
                    new BookingResource { }
                };
            }

            var element = new ElementBuilder()
               .WithType(elementType)
               .WithQuestionId("booking")
               .WithBookingProvider(provider)
               .WithAppointmentType(appointmentType)
               .Build();

            // Act
            var check = new BookingElementCheck(_mockHostingEnv.Object);
            var result = check.Validate(element);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }

        [Theory]
        [InlineData(EElementType.Booking, "local", "int", "provider")]
        [InlineData(EElementType.Booking, "local", "local", "")]
        [InlineData(EElementType.Address, "local", "local", "provider")]
        public void BookingElementCheck_ReturnInValid(EElementType elementType, string actualEnv, string appointmentTypeEnv, string provider)
        {
            // Arrange
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns(actualEnv);

            var element = new ElementBuilder()
               .WithType(elementType)
               .WithQuestionId("booking")
               .WithBookingProvider(provider)
               .WithAppointmentType(new AppointmentType { AppointmentId = Guid.NewGuid(), Environment = appointmentTypeEnv })
               .Build();

            // Act
            var check = new BookingElementCheck(_mockHostingEnv.Object);
            var result = check.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        //[Fact]
        //public void BookingElementCheck_IsNotValid_WhenBookingElement_Contains_EmptyGuid_ForOptionalResources()
        //{
        //    // Arrange
        //    var pages = new List<Page>();

        //    var appointmentType = new AppointmentTypeBuilder()
        //        .WithEnvironment("local")
        //        .WithOptionalResource(new BookingResource { ResourceId = Guid.Empty, Quantity = 1 })
        //        .Build();

        //    var element = new ElementBuilder()
        //        .WithType(EElementType.Booking)
        //        .WithQuestionId("booking")
        //        .WithBookingProvider("TestProvider")
        //        .WithAppointmentType(appointmentType)
        //        .Build();

        //    var page = new PageBuilder()
        //        .WithElement(element)
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithName("test-name")
        //        .WithPage(page)
        //        .Build();

        //    var check = new BookingElementCheck(_mockHostingEnv.Object);

        //    // Act
        //    var result = check.Validate(schema);
        //    Assert.False(result.IsValid);
        //    Assert.Contains("FAILURE - Booking Element Check, Booking element 'booking', optional resources are invalid, ResourceId cannot be an empty Guid on form test-name.", result.Messages);
        //}
    }
}
