using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Booking.Request;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Elements;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class BookingElementCheckTests
    {
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();
        
        [Theory]
        [InlineData(EElementType.Booking, "local", "local", "provider", false, 0, "00000000-0000-0000-0000-000000000000")]
        [InlineData(EElementType.Booking, "local", "local", "provider", true, 1, "69339C97-3924-477B-8D90-0986596072CE")]
        public void BookingElementCheck_ReturnValid(
            EElementType elementType, 
            string actualEnv, 
            string appointmentTypeEnv, 
            string provider, 
            bool optionalResource,
            int optionalResourceCount,
            Guid optionalResourceId)
        {
            // Arrange
            _mockHostingEnv.Setup(environment => environment.EnvironmentName).Returns(actualEnv);

            var appointmentType = new AppointmentTypeBuilder()
                .WithEnvironment(appointmentTypeEnv)
                .Build();

            if (optionalResource)
            {
                appointmentType.OptionalResources = new List<BookingResource>
                {
                    new BookingResource
                    { 
                        Quantity = optionalResourceCount,
                        ResourceId = optionalResourceId
                    }
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
        [InlineData(EElementType.Booking, "local", "local", "provider", true, 0, "69339C97-3924-477B-8D90-0986596072CE")]
        [InlineData(EElementType.Booking, "local", "local", "provider", true, 1, "00000000-0000-0000-0000-000000000000")]
        [InlineData(EElementType.Booking, "local", "int", "provider", false, 0, "00000000-0000-0000-0000-000000000000")]
        [InlineData(EElementType.Booking, "local", "local", "", false, 0, "00000000-0000-0000-0000-000000000000")]
        public void BookingElementCheck_ReturnInValid(
            EElementType elementType,
            string actualEnv,
            string appointmentTypeEnv,
            string provider,
            bool optionalResource,
            int optionalResourceCount,
            Guid optionalResourceId)
        {
            // Arrange
            _mockHostingEnv.Setup(environment => environment.EnvironmentName).Returns(actualEnv);

            var appointmentType = new AppointmentTypeBuilder()
                .WithEnvironment(appointmentTypeEnv)
                .Build();

            if (optionalResource)
            {
                appointmentType.OptionalResources = new List<BookingResource>
                {
                    new BookingResource
                    {
                        Quantity = optionalResourceCount,
                        ResourceId = optionalResourceId
                    }
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
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}
