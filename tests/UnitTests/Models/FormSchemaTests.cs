using form_builder.Enum;
using form_builder.Models;
using form_builder_tests.Builders;
using System;
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

            var result = formSchema.IsAvailabile("Int");

            Assert.True(result);
        }
        
        [Fact]
        public void IsAvailable_ShouldReturn_True_WhenRequestedEnvironmentAvailabilitiesIsNotSpecified()
        {
            var formSchema = new FormSchema{
                EnvironmentAvailabilities = new List<EnvironmentAvailability>
                {
                    new EnvironmentAvailability {
                        Environment: "Prod",
                        IsAvailabile: false
                    };
                };
            };

            var result = formSchema.IsAvailabile("Int");

            Assert.True(result);
        }

        [Fact]
        public void IsAvailable_ShouldReturn_True_WhenRequestedEnvironmentAvailabilitiesIsSpecified_And_IsAvailableEqualsTrue()
        {
            var formSchema = new FormSchema{
                EnvironmentAvailabilities = new List<EnvironmentAvailability>
                {
                    new EnvironmentAvailability {
                        Environment: "Int",
                        IsAvailabile: true
                    };
                };
            };

            var result = formSchema.IsAvailabile("Int");

            Assert.True(result);
        }

        
        [Fact]
        public void IsAvailable_ShouldReturn_False_WhenRequestedEnvironmentAvailabilitiesIsSpecified_And_IsAvailableEqualsFalse()
        {
            var formSchema = new FormSchema{
                EnvironmentAvailabilities = new List<EnvironmentAvailability>
                {
                    new EnvironmentAvailability {
                        Environment: "Int",
                        IsAvailabile: false
                    };
                };
            };

            var result = formSchema.IsAvailabile("Int");

            Assert.False(result);
        }
    }
}
