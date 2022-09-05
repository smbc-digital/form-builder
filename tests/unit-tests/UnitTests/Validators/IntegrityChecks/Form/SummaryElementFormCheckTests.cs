using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class SummaryElementFormCheckTests
    {
        private readonly SummaryElementFormCheck _validator = new SummaryElementFormCheck();

        [Fact]
        public void Validate_Should_Return_Error_WhenSummarySection_IsMissingTitle()
        {
            // Arrange
            var section = new Section
            {
                Pages = new List<string> { "test-page" }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Summary)
                .withSummarySection(section)
                .Build();

            var page1 = new PageBuilder()
                .WithPageSlug("test-page")
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .Build();

            // Act
            var result = _validator.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Messages);
            Assert.Equal($"{IntegrityChecksConstants.FAILURE}FormSchemaIntegrityCheck::SummaryFormCheck, Summary section is defined but section title is empty. Please add a title for the section.", result.Messages.First());
        }

        [Fact]
        public void Validate_Should_Return_Error_WhenSummarySection_Has_NoPages_Defined_InSection()
        {
            // Arrange
            var section = new Section
            {
                Title = "section title"
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Summary)
                .withSummarySection(section)
                .Build();

            var page1 = new PageBuilder()
                .WithPageSlug("test-page")
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .Build();

            // Act
            var result = _validator.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Messages);
            Assert.Equal($"{IntegrityChecksConstants.FAILURE}FormSchemaIntegrityCheck::SummaryFormCheck, Summary section is defined but no pages have been specified to appear in the section.", result.Messages.First());
        }

        [Fact]
        public void Validate_Should_Return_Error_WhenOne_SectionIsValid_AndOther_IsNotValid()
        {
            // Arrange
            var sectionValid = new Section
            {
                Title = "section title",
                Pages = new List<string> {
                    "test-page"
                }
            };

            var sectionInvalid = new Section
            {
                Title = "section title"
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Summary)
                .withSummarySection(sectionValid)
                .withSummarySection(sectionInvalid)
                .Build();

            var page1 = new PageBuilder()
                .WithPageSlug("test-page")
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .Build();

            // Act
            var result = _validator.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Messages);
            Assert.Equal($"{IntegrityChecksConstants.FAILURE}FormSchemaIntegrityCheck::SummaryFormCheck, Summary section is defined but no pages have been specified to appear in the section.", result.Messages.First());
        }

        [Fact]
        public void Validate_Should_Return_Error_WhenSummarySection_Has_PageSlug_Which_Is_Unknown()
        {
            // Arrange
            var pageSlug = "unknown-page";
            var section = new Section
            {
                Title = "section title",
                Pages = new List<string>
                {
                   pageSlug
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Summary)
                .withSummarySection(section)
                .Build();

            var page1 = new PageBuilder()
                .WithPageSlug("test-page")
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .Build();

            // Act
            var result = _validator.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Messages);
            Assert.Equal($"{IntegrityChecksConstants.FAILURE}FormSchemaIntegrityCheck::SummaryFormCheck, Summary section has a page slug defined which cannot be found. Verify '{pageSlug}' is a valid page slug within the schema.", result.Messages.First());
        }

        [Fact]
        public void Validate_Should_Return_Success_WhenSumary_Element_IsValid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Summary)
                .Build();

            var page1 = new PageBuilder()
                .WithPageSlug("test-page")
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .Build();

            // Act
            var result = _validator.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Messages);
        }

        [Fact]
        public void Validate_Should_Return_Success_WhenSumary_Element_WithSections_IsValid()
        {
            // Arrange
            var section = new Section
            {
                Title = "section title",
                Pages = new List<string>
                {
                   "page-one",
                   "page-two"
                }
            };

            var sectionTwo = new Section
            {
                Title = "section title two",
                Pages = new List<string>
                {
                   "page-three"
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Summary)
                .withSummarySection(section)
                .withSummarySection(sectionTwo)
                .Build();

            var page1 = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(element)
                .Build();

            var page2 = new PageBuilder()
                .WithPageSlug("page-two")
                .WithElement(element)
                .Build();

            var page3 = new PageBuilder()
                .WithPageSlug("page-three")
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .WithPage(page2)
                .WithPage(page3)
                .Build();

            // Act
            var result = _validator.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Messages);
        }
    }
}
