using System;
using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Validators.IntegrityChecks.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using StockportGovUK.NetStandard.Models.Booking.Request;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class BookingElementCheckTests
    {
        private readonly BookingElementCheck _bookingElementCheck;
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

        public BookingElementCheckTests()
        {
            _mockHostingEnv.Setup(environment => environment.EnvironmentName).Returns("local");

            _bookingElementCheck = new BookingElementCheck(_mockHostingEnv.Object);
        }

        private AppointmentType BuildAppointmentType(string env, Guid appointmentId, string appointmentIdKey)
            => new AppointmentTypeBuilder()
                .WithEnvironment(env)
                .WithAppointmentId(appointmentId)
                .WithAppointmentIdKey(appointmentIdKey)
                .Build();

        private Element BuildBookingElement(string provider, AppointmentType appointmentType)
            => new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithBookingProvider(provider)
                .WithAppointmentType(appointmentType)
                .Build();

        [Theory]
        [InlineData(false, 0, "00000000-0000-0000-0000-000000000000")]
        [InlineData(true, 1, "69339C97-3924-477B-8D90-0986596072CE")]
        public void BookingElementCheck_ReturnValid_OptionalResourcesChecks(bool optionalResource, int optionalResourceQuantity, Guid optionalResourceId)
        {
            // Arrange
            var appointmentType = BuildAppointmentType("local", Guid.NewGuid(), string.Empty);

            if (optionalResource)
            {
                appointmentType.OptionalResources = new List<BookingResource>
                {
                    new ()
                    { 
                        Quantity = optionalResourceQuantity,
                        ResourceId = optionalResourceId
                    }
                };
            }

            var element = BuildBookingElement("provider", appointmentType);

            // Act
            var result = _bookingElementCheck.Validate(element);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", "69339C97-3924-477B-8D90-0986596072CE")]
        [InlineData("69339C97-3924-477B-8D90-0986596072CE", "")]
        public void BookingElementCheck_ReturnValid_AppointmentId_Or_AppointmentIdKeySet(string appointmentId, string appointmentIdKey)
        {
            // Arrange
            var appointmentType = BuildAppointmentType("local", new Guid(appointmentId), appointmentIdKey);

            var element = BuildBookingElement("provider", appointmentType);

            // Act
            var result = _bookingElementCheck.Validate(element);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }

        [Fact]
        public void BookingElementCheck_ShouldReturnInvalid_When_OptionalResourceQuantity_IsZeroOrLess()
        {
            // Arrange
            var appointmentType = BuildAppointmentType("local", Guid.NewGuid(), string.Empty);

            appointmentType.OptionalResources = new List<BookingResource>
            {
                new ()
                {
                    Quantity = 0,
                    ResourceId = Guid.NewGuid()
                }
            };

            var element = BuildBookingElement("provider", appointmentType);

            // Act
            var result = _bookingElementCheck.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void BookingElementCheck_ShouldReturnInvalid_When_OptionalResourceId_IsEmptyGuid()
        {
            // Arrange
            var appointmentType = BuildAppointmentType("local", Guid.NewGuid(), string.Empty);

            appointmentType.OptionalResources = new List<BookingResource>
            {
                new ()
                {
                    Quantity = 1,
                    ResourceId = new Guid()
                }
            };

            var element = BuildBookingElement("provider", appointmentType);

            // Act
            var result = _bookingElementCheck.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void BookingElementCheck_ShouldReturnInvalid_When_NoAppointmentType_ForEnvironment()
        {
            // Arrange
            var appointmentType = BuildAppointmentType("int", Guid.NewGuid(), string.Empty);
            var element = BuildBookingElement("provider", appointmentType);

            // Act
            var result = _bookingElementCheck.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void BookingElementCheck_ShouldReturnInvalid_When_AppointmentId_And_AppointmentIdKey_AreNotSet()
        {
            // Arrange
            var appointmentType = BuildAppointmentType("local", new Guid(), string.Empty);
            var element = BuildBookingElement("provider", appointmentType);

            // Act
            var result = _bookingElementCheck.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void BookingElementCheck_ShouldReturnInvalid_When_AppointmentId_And_AppointmentIdKey_AreBothSet()
        {
            // Arrange
            var appointmentType = BuildAppointmentType("local", Guid.NewGuid(), "69339C97-3924-477B-8D90-0986596072CE");
            var element = BuildBookingElement("provider", appointmentType);

            // Act
            var result = _bookingElementCheck.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void BookingElementCheck_ShouldReturnInvalid_When_ProviderNotSet()
        {
            // Arrange
            var appointmentType = BuildAppointmentType("local", Guid.NewGuid(), string.Empty);
            var element = BuildBookingElement(string.Empty, appointmentType);

            // Act
            var result = _bookingElementCheck.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}
