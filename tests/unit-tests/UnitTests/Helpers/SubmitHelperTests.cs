using form_builder.Builders;
using form_builder.Enum;
using System.Dynamic;
using form_builder.Helpers.Submit;
using form_builder.Models;
using form_builder.Providers.Booking;
using form_builder_tests.Builders;
using Moq;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Request;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers;

public class SubmitHelperTests
{
    private readonly Mock<IBookingProvider> _bookingProvider = new();
    private readonly SubmitHelper _submitHelper;

    const string BookingProvider = "testBookingProvider";

    public SubmitHelperTests()
    {
        _bookingProvider
            .Setup(_ => _.ProviderName)
            .Returns(BookingProvider);

        var bookingProviders = new List<IBookingProvider> { _bookingProvider.Object };

        _submitHelper = new SubmitHelper(bookingProviders);
    }

    [Fact]
    public async Task ConfirmBookings_Should_Call_BookingProvider_Confirm()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.Booking)
            .WithQuestionId("booking")
            .WithBookingProvider(BookingProvider)
            .WithAutoConfirm(true)
            .Build();

        var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

        var behaviour = new BehaviourBuilder()
            .WithBehaviourType(EBehaviourType.SubmitForm)
            .WithSubmitSlug(submitSlug)
            .Build();

        var formAnswers = new FormAnswers
        {
            Path = "booking-confirm",
            Pages = new List<PageAnswers>
            {
                new ()
                {
                    Answers = new List<Answers>
                    {
                        new ()
                        {
                            QuestionId = "booking-reserved-booking-id",
                            Response = "93dd24cd-cea5-40e7-b72a-a6b4757786ba"
                        }
                    }
                }
            }
        };

        var page = new PageBuilder()
            .WithPageSlug("page-one")
            .WithBehaviour(behaviour)
            .WithElement(element)
            .Build();

        var schema = new FormSchemaBuilder()
           .WithPage(page)
           .Build();

        var mappingEntity = new MappingEntityBuilder()
            .WithBaseForm(schema)
            .WithFormAnswers(formAnswers)
            .WithData(new ExpandoObject())
            .Build();

        await _submitHelper.ConfirmBookings(mappingEntity, "local", "caseReference");

        _bookingProvider.Verify(_ => _.Confirm(It.IsAny<ConfirmationRequest>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmBookings_Should_Call_BookingProvider_Confirm_WithForeignReference()
    {
        // Arrange
        ConfirmationRequest confirmationRequestCallbackValue = new ConfirmationRequest();
        _bookingProvider
            .Setup(_ => _.Confirm(It.IsAny<ConfirmationRequest>()))
            .Callback<ConfirmationRequest>(request => confirmationRequestCallbackValue = request);

        var element = new ElementBuilder()
            .WithType(EElementType.Booking)
            .WithQuestionId("booking")
            .WithBookingProvider(BookingProvider)
            .WithAutoConfirm(true)
            .Build();

        var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

        var behaviour = new BehaviourBuilder()
            .WithBehaviourType(EBehaviourType.SubmitForm)
            .WithSubmitSlug(submitSlug)
            .Build();

        var formAnswers = new FormAnswers
        {
            Path = "booking-confirm",
            Pages = new List<PageAnswers>
            {
                new ()
                {
                    Answers = new List<Answers>
                    {
                        new ()
                        {
                            QuestionId = "booking-reserved-booking-id",
                            Response = "93dd24cd-cea5-40e7-b72a-a6b4757786ba"
                        }
                    }
                }
            }
        };

        var page = new PageBuilder()
            .WithPageSlug("page-one")
            .WithBehaviour(behaviour)
            .WithElement(element)
            .Build();

        var schema = new FormSchemaBuilder()
            .WithPage(page)
            .Build();

        var mappingEntity = new MappingEntityBuilder()
            .WithBaseForm(schema)
            .WithFormAnswers(formAnswers)
            .WithData(new ExpandoObject())
            .Build();

        // Act
        await _submitHelper.ConfirmBookings(mappingEntity, "local", "caseReference");

        // Assert
        Assert.NotNull(confirmationRequestCallbackValue.ForeignReferences);
        Assert.NotEmpty(confirmationRequestCallbackValue.ForeignReferences);
        Assert.Equal("caseReference", confirmationRequestCallbackValue.ForeignReferences.First().Reference);
    }

    [Fact]
    public async Task ConfirmBookings_ShouldNot_Call_BookingProvider_Confirm()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.Booking)
            .WithQuestionId("booking")
            .WithBookingProvider("testBookingProvider")
            .WithAutoConfirm(false)
            .Build();

        var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

        var behaviour = new BehaviourBuilder()
            .WithBehaviourType(EBehaviourType.SubmitForm)
            .WithSubmitSlug(submitSlug)
            .Build();

        var formAnswers = new FormAnswers
        {
            Path = "booking-confirm",
            Pages = new List<PageAnswers>
            {
                new ()
                {
                    Answers = new List<Answers>
                    {
                        new ()
                        {
                            QuestionId = "booking-reserved-booking-id",
                            Response = "93dd24cd-cea5-40e7-b72a-a6b4757786ba"
                        }
                    }
                }
            }
        };

        var page = new PageBuilder()
            .WithPageSlug("page-one")
            .WithBehaviour(behaviour)
            .WithElement(element)
            .Build();

        var schema = new FormSchemaBuilder()
           .WithPage(page)
           .Build();

        var mappingEntity = new MappingEntityBuilder()
            .WithBaseForm(schema)
            .WithFormAnswers(formAnswers)
            .WithData(new ExpandoObject())
            .Build();

        await _submitHelper.ConfirmBookings(mappingEntity, "local", "caseReference");

        _bookingProvider.Verify(_ => _.Confirm(It.IsAny<ConfirmationRequest>()), Times.Never);
    }
}
