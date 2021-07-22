using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Providers.Booking;
using form_builder.SubmissionActions;
using form_builder_tests.Builders;
using Moq;
using StockportGovUK.NetStandard.Models.Booking.Request;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.SubmissionActions
{
    public class PostSubmissionActionTests
    {
        private readonly Mock<IBookingProvider> _bookingProvider = new();
        private readonly PostSubmissionAction _postSubmissionAction;
        private readonly IEnumerable<IBookingProvider> _bookingProviders;

        const string bookingProvider = "testBookingProvider";       

        public PostSubmissionActionTests()
        {
            _bookingProvider.Setup(_ => _.ProviderName).Returns(bookingProvider);

            _bookingProviders = new List<IBookingProvider>
            {
                _bookingProvider.Object
            };

            _postSubmissionAction = new PostSubmissionAction(_bookingProviders);
        }

        [Fact]
        public async Task Confirm_Should_Call_BookingProvider_Confirm()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithAppointmentType(new AppointmentType { AppointmentId = Guid.Parse("37588e67-9852-4713-9df5-0eb94e320675"), Environment = "local" })
                .WithBookingProvider(bookingProvider)
                .Build();

            var formAnswers = new FormAnswers
            {
                Path = "booking-confirm",
                Pages = new List<PageAnswers>
                {                    
                    new PageAnswers
                    {
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = "booking-reserved-booking-id",
                                Response = "93dd24cd-cea5-40e7-b72a-a6b4757786ba"
                            }
                        }
                    }
                }
            };

            var page = new PageBuilder()
                .WithPageSlug("booking-confirm")
                .WithElement(element)
                .Build();

            var formSchema = new FormSchemaBuilder()
               .WithPage(page)
               .Build();

            var _mappingEntity =
            new MappingEntityBuilder()
                .WithBaseForm(formSchema)
                .WithFormAnswers(formAnswers)
                .WithData(new ExpandoObject())
                .Build();

            await _postSubmissionAction.ConfirmBooking(_mappingEntity, "local");

            _bookingProvider.Verify(_ => _.Confirm(It.IsAny<ConfirmationRequest>()), Times.Once);
        }
    }
}
