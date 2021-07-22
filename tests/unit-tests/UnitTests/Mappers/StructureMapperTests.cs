using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Mappers.Structure;
using form_builder.Models;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Mappers
{
    public class StructureMapperTests
    {
        private readonly StructureMapper _structureMapper;
        private readonly Mock<ISchemaFactory> _mockSchemaFactory = new();

        public StructureMapperTests()
        {
            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(new FormSchema { Pages = new List<Page>() });

            _structureMapper = new StructureMapper(_mockSchemaFactory.Object);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldCallSchemaFactory()
        {
            // Act
            await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            _mockSchemaFactory.Verify(_ => _.Build(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldIncludeCaseReference_And_PaymentAmount()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>) await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.NotNull(result["CaseReference"]);
            Assert.NotNull(result["PaymentAmount"]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldUseTargetMappingIfProvided()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("questionId")
                .WithTargetMapping("targetQuestionId")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.NotNull(result["TargetQuestionId"]);
            result.TryGetValue("QuestionId", out var questionIdKey);
            Assert.Null(questionIdKey);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldCreateObjectForTargetMapping()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("questionId")
                .WithTargetMapping("textbox.targetQuestionId")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.NotNull(result["Textbox"]);
            result.TryGetValue("Textbox", out var textboxObj);
            IDictionary<string, dynamic> textboxDict = textboxObj;
            Assert.NotNull(textboxDict["TargetQuestionId"]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldAddIncomingValues_IfPresent_WithTypeValueOfDynamic()
        {
            // Arrange
            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("incomingValue")
                .Build();

            var page = new PageBuilder()
                .WithIncomingValue(incomingValue)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.NotNull(result["IncomingValue"]);
            Assert.Equal("Dynamic", result["IncomingValue"]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldAddRetrieveExternalDataAction_IfPresent_WithTypeValueOfDynamic()
        {
            // Arrange
            var pageAction = new ActionBuilder()
                .WithActionType(EActionType.RetrieveExternalData)
                .WithTargetQuestionId("externalData")
                .Build();

            var page = new PageBuilder()
                .WithPageActions(pageAction)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.NotNull(result["ExternalData"]);
            Assert.Equal("Dynamic", result["ExternalData"]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldAddStructureForAddAnother()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("questionId")
                .Build();

            var addAnotherElement = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithNestedElement(element)
                .WithQuestionId("addAnother")
                .Build();

            var page = new PageBuilder()
                .WithElement(addAnotherElement)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.NotNull(result["AddAnother"]);
            Assert.Single(result["AddAnother"]);
            Assert.NotNull(result["AddAnother"][0]["QuestionId"]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldSetDataTypeToString_ForCheckbox()
        {
            // Arrange
            var checkbox = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId("checkbox")
                .Build();

            var page = new PageBuilder()
                .WithElement(checkbox)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.Equal("String", result["Checkbox"][0]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldSetDataTypeToDateTime_ForDateInput_And_DatePicker_AndTimeInput()
        {
            // Arrange
            var dateInput = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("dateInput")
                .Build();

            var datePicker = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("datePicker")
                .Build();

            var timeInput = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("timeInput")
                .Build();

            var page = new PageBuilder()
                .WithElement(dateInput)
                .WithElement(datePicker)
                .WithElement(timeInput)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.Equal("DateTime", result["DateInput"]);
            Assert.Equal("DateTime", result["DatePicker"]);
            Assert.Equal("DateTime", result["TimeInput"]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldSetDataTypes_ForAddress_And_Street()
        {
            // Arrange
            var address = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithQuestionId("address")
                .Build();

            var street = new ElementBuilder()
                .WithType(EElementType.Street)
                .WithQuestionId("street")
                .Build();

            var page = new PageBuilder()
                .WithElement(address)
                .WithElement(street)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.Equal("String", result["Address"]["SelectedAddress"]);
            Assert.Equal("String", result["Address"]["AddressLine1"]);
            Assert.Equal("String", result["Address"]["AddressLine2"]);
            Assert.Equal("String", result["Address"]["Town"]);
            Assert.Equal("String", result["Address"]["Postcode"]);
            Assert.Equal("String", result["Address"]["PlaceRef"]);
            Assert.Equal("Boolean", result["Address"]["IsAutomaticallyFound"]);
            Assert.Equal("String", result["Street"]["SelectedAddress"]);
            Assert.Equal("String", result["Street"]["AddressLine1"]);
            Assert.Equal("String", result["Street"]["AddressLine2"]);
            Assert.Equal("String", result["Street"]["Town"]);
            Assert.Equal("String", result["Street"]["Postcode"]);
            Assert.Equal("String", result["Street"]["PlaceRef"]);
            Assert.Equal("Boolean", result["Street"]["IsAutomaticallyFound"]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldSetDataTypes_ForOrganisation()
        {
            // Arrange
            var organisation = new ElementBuilder()
                .WithType(EElementType.Organisation)
                .WithQuestionId("organisation")
                .Build();

            var page = new PageBuilder()
                .WithElement(organisation)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.Equal("String", result["Organisation"]["Reference"]);
            Assert.Equal("String", result["Organisation"]["Description"]);
            Assert.Equal("String", result["Organisation"]["Name"]);
            Assert.Equal("String", result["Organisation"]["Telephone"]);
            Assert.Equal("String", result["Organisation"]["Email"]);
            Assert.Equal("String", result["Organisation"]["Address"]["Reference"]);
            Assert.Equal("String", result["Organisation"]["Address"]["Description"]);
            Assert.Equal("String", result["Organisation"]["Address"]["Number"]);
            Assert.Equal("String", result["Organisation"]["Address"]["AddressLine1"]);
            Assert.Equal("String", result["Organisation"]["Address"]["AddressLine2"]);
            Assert.Equal("String", result["Organisation"]["Address"]["AddressLine3"]);
            Assert.Equal("String", result["Organisation"]["Address"]["City"]);
            Assert.Equal("String", result["Organisation"]["Address"]["Postcode"]);
            Assert.Equal("String", result["Organisation"]["Address"]["UPRN"]);
            Assert.Equal("String", result["Organisation"]["Address"]["USRN"]);
            Assert.Equal("String", result["Organisation"]["Address"]["PropertyId"]);
            Assert.Equal("String", result["Organisation"]["Address"]["BusinessName"]);
            Assert.Equal("Nullable[Boolean]", result["Organisation"]["Address"]["FromCookie"]);
            Assert.Equal("String", result["Organisation"]["Address"]["Easting"]);
            Assert.Equal("String", result["Organisation"]["Address"]["Northing"]);
            Assert.Equal("Boolean", result["Organisation"]["Address"]["IsFullyResolved"]);
            Assert.Equal("String", result["Organisation"]["SocialContacts"][0]["Type"]);
            Assert.Equal("String", result["Organisation"]["SocialContacts"][0]["Value"]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldSetDataTypes_ForBooking()
        {
            // Arrange
            var booking = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .Build();

            var page = new PageBuilder()
                .WithElement(booking)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.Equal("Guid", result["Booking"]["Id"]);
            Assert.Equal("String", result["Booking"]["HashedId"]);
            Assert.Equal("DateTime", result["Booking"]["Date"]);
            Assert.Equal("DateTime", result["Booking"]["StartTime"]);
            Assert.Equal("DateTime", result["Booking"]["EndTime"]);
            Assert.Equal("String", result["Booking"]["Location"]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldSetDataTypes_ForMap()
        {
            // Arrange
            var map = new ElementBuilder()
                .WithType(EElementType.Map)
                .WithQuestionId("map")
                .Build();

            var page = new PageBuilder()
                .WithElement(map)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.IsType<ExpandoObject>(result["Map"]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldSetDataTypes_ForFileUpload_AndMultipleFileUpload()
        {
            // Arrange
            var fileUpload = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId("fileUpload")
                .Build();

            var multipleFileUpload = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("multipleFileUpload")
                .Build();

            var page = new PageBuilder()
                .WithElement(fileUpload)
                .WithElement(multipleFileUpload)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.Equal("String", result["FileUpload"][0]["TrustedOriginalFileName"]);
            Assert.Equal("String", result["FileUpload"][0]["Content"]);
            Assert.Equal("String", result["FileUpload"][0]["UntrustedOriginalFileName"]);
            Assert.Equal("String", result["FileUpload"][0]["KeyName"]);
            Assert.Equal("String", result["MultipleFileUpload"][0]["TrustedOriginalFileName"]);
            Assert.Equal("String", result["MultipleFileUpload"][0]["Content"]);
            Assert.Equal("String", result["MultipleFileUpload"][0]["UntrustedOriginalFileName"]);
            Assert.Equal("String", result["MultipleFileUpload"][0]["KeyName"]);
        }

        [Fact]
        public async Task CreateBaseFormDataStructure_ShouldSetDataTypeToString_ForRemainingElementTypes()
        {
            // Arrange
            var declaration = new ElementBuilder()
                .WithType(EElementType.Declaration)
                .WithQuestionId("declaration")
                .Build();

            var radio = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("radio")
                .Build();

            var select = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithQuestionId("select")
                .Build();

            var textarea = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("textarea")
                .Build();

            var textbox = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("textbox")
                .Build();

            var page = new PageBuilder()
                .WithElement(declaration)
                .WithElement(radio)
                .WithElement(select)
                .WithElement(textarea)
                .WithElement(textbox)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(formSchema);

            // Act
            var result = (IDictionary<string, dynamic>)await _structureMapper.CreateBaseFormDataStructure("test-form");

            // Assert
            Assert.Equal("String", result["Declaration"]);
            Assert.Equal("String", result["Radio"]);
            Assert.Equal("String", result["Select"]);
            Assert.Equal("String", result["Textarea"]);
            Assert.Equal("String", result["Textbox"]);
        }
    }
}
