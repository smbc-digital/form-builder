using form_builder.Mappers;
using form_builder.Models;
using form_builder_tests.Builders;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Mappers
{
    public class ElementMapperTests
    {
        [Fact]
        public void GetAnswerValue_ShouldReturnIntWhenNumericIsTrue()
        {
            var element = new ElementBuilder()
            .WithNumeric(true)
            .WithQuestionId("testNumber")
            .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = "testNumber",
                                Response = "23"
                            }
                        }
                    }
                }
            };

            var result = ElementMapper.GetAnswerValue(element, formAnswers);

            Assert.IsType<int>(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnSingleDateWhenElementIsDatePicker()
        {
            var element = new ElementBuilder()
            .WithType(form_builder.Enum.EElementType.DatePicker)
            .WithQuestionId("testDate")
            .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = "testDate",
                                Response = "21/02/2020"
                            }
                        }
                    }
                }
            };

            var result = ElementMapper.GetAnswerValue(element, formAnswers);

            Assert.IsType<DateTime>(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnNullWhenResponseIsEmpty_WhenElementIsDatePicker()
        {
            var element = new ElementBuilder()
            .WithType(form_builder.Enum.EElementType.DatePicker)
            .WithQuestionId("testNumber")
            .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                    {
                        new PageAnswers {
                            Answers = new List<Answers> {
                                new Answers
                                {
                                    QuestionId = "testNumber"
                                }
                            }
                        }
                    }
            };

            var result = ElementMapper.GetAnswerValue(element, formAnswers);

            Assert.Null(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnNullWhenResponseIsEmpty_AndNumeric()
        {
            var element = new ElementBuilder()
            .WithQuestionId("testNumber")
            .WithNumeric(true)
            .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = "testNumber"
                            }
                        }
                    }
                }
            };

            var result = ElementMapper.GetAnswerValue(element, formAnswers);

            Assert.Null(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnArrayOfStrings_WhenElementIsCheckbox()
        {
            var element = new ElementBuilder()
           .WithQuestionId("testCheckbox")
           .WithType(form_builder.Enum.EElementType.Checkbox)
           .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                    {
                        new PageAnswers {
                            Answers = new List<Answers> {
                                new Answers
                                {
                                    QuestionId = "testCheckbox",
                                    Response = "option1,option2,option3"
                                }
                            }
                        }
                    }
            };

            var result = ElementMapper.GetAnswerValue(element, formAnswers);

            var type = Assert.IsType<List<string>>(result);
            Assert.Equal("option1", type[0]);
            Assert.Equal("option2", type[1]);
            Assert.Equal("option3", type[2]);
            Assert.Equal(3, type.Count);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnEmptyArray_WhenResponseIsEmpty_ForCheckbox()
        {
            var element = new ElementBuilder()
            .WithQuestionId("testCheckbox")
            .WithType(form_builder.Enum.EElementType.Checkbox)
            .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                    {
                        new PageAnswers {
                            Answers = new List<Answers> {
                                new Answers
                                {
                                    QuestionId = "testCheckbox"
                                }
                            }
                        }
                    }
            };

            var result = ElementMapper.GetAnswerValue(element, formAnswers);

            var type = Assert.IsType<List<string>>(result);
            Assert.Empty(type);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnDateValue_WhenElementIsDateInput()
        {
            var element = new ElementBuilder()
            .WithQuestionId("testDate")
            .WithType(form_builder.Enum.EElementType.DateInput)
            .Build();

            var dayKey = "testDate-day";
            var monthKey = "testDate-month";
            var yearKey = "testDate-year";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                    {
                        new PageAnswers {
                            Answers = new List<Answers> {
                                new Answers
                                {
                                    QuestionId = dayKey,
                                    Response = "20"
                                },
                                new Answers
                                {
                                    QuestionId = monthKey,
                                    Response = "05"
                                },
                                new Answers
                                {
                                    QuestionId = yearKey,
                                    Response = "2020"
                                }
                            }
                        }
                }
            };
            var result = ElementMapper.GetAnswerValue(element, formAnswers);
            Assert.IsType<DateTime>(result);
        }
        [Fact]
        public void GetAnswerValue_ShouldReturnNullIfDayIsEmpty_WhenElementIsDateInput()
        {
            var element = new ElementBuilder()
            .WithQuestionId("testDate")
            .WithType(form_builder.Enum.EElementType.DateInput)
            .Build();

            var dayKey = "testDate-day";
            var monthKey = "testDate-month";
            var yearKey = "testDate-year";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = dayKey
                            },
                            new Answers
                            {
                                QuestionId = monthKey,
                                Response = "05"
                            },
                            new Answers
                            {
                                QuestionId = yearKey,
                                Response = "2020"
                            }
                        }
                    }
                }
                    
            };
            var result = ElementMapper.GetAnswerValue(element, formAnswers);
            Assert.Null(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnNullIfHourIsEmpty_WhenElementIsTimeInput()
        {
            var element = new ElementBuilder()
                   .WithQuestionId("testTime")
                   .WithType(form_builder.Enum.EElementType.TimeInput)
                   .Build();

            var timeMinutesKey = "testTime-minutes";
            var timeHoursKey = "testTime-hours";
            var timeAmPmKey = "testTime-ampm";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = timeMinutesKey,
                                Response = "10"
                            },
                            new Answers
                            {
                                QuestionId = timeHoursKey
                            },
                            new Answers
                            {
                                QuestionId = timeAmPmKey,
                                Response = "am"
                            }
                        }
                    }
                }

            };
            var result = ElementMapper.GetAnswerValue(element, formAnswers);
            Assert.Null(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnTimeSpan_WhenElementIsTimeInput()
        {
            var element = new ElementBuilder()
                  .WithQuestionId("testTime")
                  .WithType(form_builder.Enum.EElementType.TimeInput)
                  .Build();

            var timeMinutesKey = "testTime-minutes";
            var timeHoursKey = "testTime-hours";
            var timeAmPmKey = "testTime-ampm";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = timeMinutesKey,
                                Response = "10"
                            },
                            new Answers
                            {
                                QuestionId = timeHoursKey,
                                Response = "10"
                            },
                            new Answers
                            {
                                QuestionId = timeAmPmKey,
                                Response = "am"
                            }
                        }
                    }
                }

            };
            var result = ElementMapper.GetAnswerValue(element, formAnswers);
            Assert.IsType<TimeSpan>(result);
        }
        [Fact]
        public void GetAnswerValue_ShouldReturnAddress_WhenElementIsAddress_Automatic()
        {
            var element = new ElementBuilder()
                  .WithQuestionId("testAddress")
                  .WithType(form_builder.Enum.EElementType.Address)
                  .Build();

            var uprn = "testAddress-address";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = uprn,
                                Response = "1001254222"
                            }
                        }
                    }
                }

            };
            var result = ElementMapper.GetAnswerValue(element, formAnswers);

            var type = Assert.IsType<Address>(result);
            Assert.Equal("1001254222", type.PlaceRef);
        }
        [Fact]
        public void GetAnswerValue_ShouldReturnAddress_WhenElementIsAddress_Manual()
        {
            var element = new ElementBuilder()
                  .WithQuestionId("testAddress")
                  .WithType(form_builder.Enum.EElementType.Address)
                  .Build();

            var manualAddressLineOne = "testAddress-AddressManualAddressLine1";
            var manualAddressLineTwo = "testAddress-AddressManualAddressLine2";
            var manualAddressLineTown = "testAddress-AddressManualAddressTown";
            var manualAddressLinePostcode = "testAddress-AddressManualAddressPostcode";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = manualAddressLineOne,
                                Response = "line1"
                            },
                            new Answers
                            {
                                QuestionId = manualAddressLineTwo,
                                Response = "line2"
                            },
                            new Answers
                            {
                                QuestionId = manualAddressLineTown,
                                Response = "town"
                            },
                            new Answers
                            {
                                QuestionId = manualAddressLinePostcode,
                                Response = "sk13xe"
                            }
                        }
                    }
                }

            };
            var result = ElementMapper.GetAnswerValue(element, formAnswers);
            var type = Assert.IsType<Address>(result);

            Assert.Equal("line1", type.AddressLine1);
            Assert.Equal("line2", type.AddressLine2);
            Assert.Equal("town", type.Town);
            Assert.Equal("sk13xe", type.Postcode);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnAddress_WhenElementIsStreet()
        {
            var element = new ElementBuilder()
                  .WithQuestionId("testStreetAddress")
                  .WithType(form_builder.Enum.EElementType.Street)
                  .Build();

            var streetUspr = "testStreetAddress-streetaddress";
            var streetDescription = "testStreetAddress-streetaddress-description";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = streetUspr,
                                Response = "0101010101"
                            },
                            new Answers
                            {
                                QuestionId = streetDescription,
                                Response = "im a street"
                            }
                        }
                    }
                }

            };
            var result = ElementMapper.GetAnswerValue(element, formAnswers);
            var type = Assert.IsType<Address>(result);

            Assert.Equal("0101010101", type.PlaceRef);
            Assert.Equal("im a street", type.SelectedAddress);
        }
        [Fact]
        public void GetAnswerValue_ShouldReturnOrganisation_WhenElementIsOrganisation()
        {
            var element = new ElementBuilder()
                  .WithQuestionId("testOrganisation")
                  .WithType(form_builder.Enum.EElementType.Organisation)
                  .Build();

            var organisationKey = "testOrganisation-organisation";
            var organisationDescription = "testOrganisation-organisation-description";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = organisationKey,
                                Response = "0101010101"
                            },
                            new Answers
                            {
                                QuestionId = organisationDescription,
                                Response = "im an organisation"
                            }
                        }
                    }
                }

            };
            var result = ElementMapper.GetAnswerValue(element, formAnswers);
            var type = Assert.IsType<StockportGovUK.NetStandard.Models.Models.Verint.Organisation>(result);

            Assert.Equal("0101010101", type.Reference);
            Assert.Equal("im an organisation", type.Name);
        }
        [Fact]
        public void GetAnswerValue_ShouldReturnValue()
        {
            var element = new ElementBuilder()
                  .WithQuestionId("test")
                  .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = "test",
                                Response = "testTwo"
                            }
                        }
                    }
                }

            };
            var result = ElementMapper.GetAnswerValue(element, formAnswers);
            Assert.IsType<string>(result);
        }
    }
}
