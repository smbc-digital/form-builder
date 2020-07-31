using form_builder.Models;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Models
{
    public class FormSchemaTests
    {

        [Fact]
        public void IsAvailable_ShouldReturn_True_WhenNoEnvironmentAvailabilitiesAreSpecified()
        {
            var formSchema = new FormSchema();

            var result = formSchema.IsAvailable("Int");

            Assert.True(result);
        }

        [Fact]
        public void IsAvailable_ShouldReturn_True_WhenRequestedEnvironmentAvailabilitiesIsNotSpecified()
        {
            var formSchema = new FormSchema
            {
                EnvironmentAvailabilities = new List<EnvironmentAvailability>
                {
                    new EnvironmentAvailability {
                        Environment = "Prod",
                        IsAvailable = false
                    }
                }
            };

            var result = formSchema.IsAvailable("Int");

            Assert.True(result);
        }

        [Fact]
        public void IsAvailable_ShouldReturn_True_WhenRequestedEnvironmentAvailabilitiesIsSpecified_And_IsAvailableEqualsTrue()
        {
            var formSchema = new FormSchema
            {
                EnvironmentAvailabilities = new List<EnvironmentAvailability>
                {
                    new EnvironmentAvailability(){
                        Environment = "Int",
                        IsAvailable = true
                    }
                }
            };

            var result = formSchema.IsAvailable("Int");

            Assert.True(result);
        }

        [Fact]
        public void IsAvailable_ShouldReturn_False_WhenRequestedEnvironmentAvailabilitiesIsSpecified_And_IsAvailableEqualsFalse()
        {
            var formSchema = new FormSchema
            {
                EnvironmentAvailabilities = new List<EnvironmentAvailability>
                {
                    new EnvironmentAvailability {
                        Environment = "Int",
                        IsAvailable = false
                    }
                }
            };

            var result = formSchema.IsAvailable("Int");

            Assert.False(result);
        }
    }
}