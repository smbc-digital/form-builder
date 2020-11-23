﻿using form_builder.Mappers;
using form_builder.Models;
using form_builder_tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using Xunit;
using form_builder.Providers.StorageProvider;
using form_builder.Enum;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.FileManagement;
using form_builder.Builders;

namespace form_builder_tests.UnitTests.Mappers
{
    public class ElementMapperTests
    {
        private readonly ElementMapper _elementMapper;
        private readonly Mock<ILogger<ElementMapper>> _logger = new Mock<ILogger<ElementMapper>>();
        private readonly Mock<IDistributedCacheWrapper> _wrapper = new Mock<IDistributedCacheWrapper>();

        public ElementMapperTests()
        {
            _elementMapper = new ElementMapper(_logger.Object, _wrapper.Object);
        }

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
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            Assert.IsType<int>(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnSingleDateWhenElementIsDatePicker()
        {
            var element = new ElementBuilder()
            .WithType(EElementType.DatePicker)
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

            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            Assert.IsType<DateTime>(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnNullWhenResponseIsEmpty_WhenElementIsDatePicker()
        {
            var element = new ElementBuilder()
            .WithType(EElementType.DatePicker)
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

            var result = _elementMapper.GetAnswerValue(element, formAnswers);

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

            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            Assert.Null(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnArrayOfStrings_WhenElementIsCheckbox()
        {
            var element = new ElementBuilder()
           .WithQuestionId("testCheckbox")
           .WithType(EElementType.Checkbox)
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

            var result = _elementMapper.GetAnswerValue(element, formAnswers);

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
            .WithType(EElementType.Checkbox)
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

            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            var type = Assert.IsType<List<string>>(result);
            Assert.Empty(type);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnDateValue_WhenElementIsDateInput()
        {
            var element = new ElementBuilder()
            .WithQuestionId("testDate")
            .WithType(EElementType.DateInput)
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
            var result = _elementMapper.GetAnswerValue(element, formAnswers);
            Assert.IsType<DateTime>(result);
        }
        [Fact]
        public void GetAnswerValue_ShouldReturnNullIfDayIsEmpty_WhenElementIsDateInput()
        {
            var element = new ElementBuilder()
            .WithQuestionId("testDate")
            .WithType(EElementType.DateInput)
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

            var result = _elementMapper.GetAnswerValue(element, formAnswers);
            Assert.Null(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnNullIfHourIsEmpty_WhenElementIsTimeInput()
        {
            var element = new ElementBuilder()
                   .WithQuestionId("testTime")
                   .WithType(EElementType.TimeInput)
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
            var result = _elementMapper.GetAnswerValue(element, formAnswers);
            Assert.Null(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnTimeSpan_WhenElementIsTimeInput()
        {
            var element = new ElementBuilder()
                  .WithQuestionId("testTime")
                  .WithType(EElementType.TimeInput)
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
            var result = _elementMapper.GetAnswerValue(element, formAnswers);
            Assert.IsType<TimeSpan>(result);
        }
        [Fact]
        public void GetAnswerValue_ShouldReturnAddress_WhenElementIsAddress_Automatic()
        {
            var element = new ElementBuilder()
                  .WithQuestionId("testAddress")
                  .WithType(EElementType.Address)
                  .Build();

            var uprn = "testAddress-address";
            var addressDescription = "testAddress-address-description";
            var description = "line1,line2,town,sk11aa";

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
                            },
                            new Answers
                            {
                                QuestionId = addressDescription,
                                Response = description
                            }
                        }
                    }
                }
            };
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            var type = Assert.IsType<Address>(result);
            Assert.Equal("1001254222", type.PlaceRef);
            Assert.Equal(description, type.SelectedAddress);
        }
        [Fact]
        public void GetAnswerValue_ShouldReturnAddress_WhenElementIsAddress_Manual()
        {
            var element = new ElementBuilder()
                  .WithQuestionId("testAddress")
                  .WithType(EElementType.Address)
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
            var result = _elementMapper.GetAnswerValue(element, formAnswers);
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
                  .WithType(EElementType.Street)
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
            var result = _elementMapper.GetAnswerValue(element, formAnswers);
            var type = Assert.IsType<Address>(result);

            Assert.Equal("0101010101", type.PlaceRef);
            Assert.Equal("im a street", type.SelectedAddress);
        }
        [Fact]
        public void GetAnswerValue_ShouldReturnOrganisation_WhenElementIsOrganisation()
        {
            var element = new ElementBuilder()
                  .WithQuestionId("testOrganisation")
                  .WithType(EElementType.Organisation)
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
            var result = _elementMapper.GetAnswerValue(element, formAnswers);
            var type = Assert.IsType<StockportGovUK.NetStandard.Models.Verint.Organisation>(result);

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

            var result = _elementMapper.GetAnswerValue(element, formAnswers);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldCall_DistributedCache_ToGetFileUploadContent()
        {
            var key = "fileUpload_fileUploadTestKey";
            _wrapper.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns("testfile");

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = "fileUpload_fileUploadTestKey-fileupload",
                                Response = JsonConvert.SerializeObject(new FileUploadModel{ UntrustedOriginalFileName = key })
                            }
                        }
                    }
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId(key)
                .Build();

            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            _wrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);

            var model = Assert.IsType<File>(result);
            Assert.Equal("testfile", model.Content);
        }

        [Fact]
        public void GetAnswerValue_ShouldCall_ThrowExceptionWhenDistributedCacheThrows()
        {
            var key = "fileUploadTestKey";
            _wrapper.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(() => null);

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = $"{key}-fileupload",
                                Response = JsonConvert.SerializeObject(new FileUploadModel
                                {
                                    Key = key
                                })
                            }
                        }
                    }
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId(key)
                .Build();

            var result = Assert.Throws<Exception>(() => _elementMapper.GetAnswerValue(element, formAnswers));

            Assert.Equal($"ElementMapper::GetFileUploadElementValue: An error has occurred while attempting to retrieve an uploaded file with key: {key} from the distributed cache", result.Message);
        }

        [Fact]
        public void GetAnswerValue_ShouldNotCall_DistributedCache_WhenNoFileWithinAnswers()
        {
            var key = "fileUploadTestKey";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers>()
                    }
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId(key)
                .Build();

            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            _wrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnDateTime_WithValidTime()
        {
            var elementHours = "03";
            var elementMinutes = "30";
            var elementAmPm = "am";

            var element = new ElementBuilder()
                .WithType(EElementType.TimeInput)
                .WithQuestionId("timeInput")
                .WithTargetMapping("customer.textboxtime")
                .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = "timeInput-minutes",
                                Response = elementMinutes
                            },
                            new Answers
                            {
                                QuestionId = "timeInput-hours",
                                Response = elementHours
                            },
                            new Answers
                            {
                                QuestionId = "timeInput-ampm",
                                Response = elementAmPm
                            }
                        }
                    }
                }
            };

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            var resultData = Assert.IsType<TimeSpan>(result);
            Assert.Equal("3", resultData.Hours.ToString());
            Assert.Equal(elementMinutes, resultData.Minutes.ToString());
        }

        [Fact]
        public void Map_ShouldReturnExpandoObject_WhenFormContains_MultipleValidatableElementsWithTargetMapping_WithValues()
        {
            var elementOneAnswer = "01/01/2000";

            var element3 = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test")
                .WithTargetMapping("customer.datepicker.date")
                .Build();


            var page = new PageBuilder()
                .WithElement(element3)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
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
                                Response = elementOneAnswer
                            }
                        }
                    }
                }
            };

            // Act
            var result = _elementMapper.GetAnswerValue(element3, formAnswers);

            // Assert
            var resultData = Assert.IsType<DateTime>(result);
            Assert.Equal($"{elementOneAnswer} 00:00:00", resultData.ToString());
        }
        
        
        [Theory]
        [InlineData(EElementType.Textbox, "test")]
        [InlineData(EElementType.Textarea, "textAreaValue")]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForElements(EElementType type, string value)
        {
            var questionId = "test-questionID";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId, Response = value } } } }};

            var element = new ElementBuilder()
                            .WithType(type)
                            .WithQuestionId(questionId)
                            .Build();

            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            Assert.Equal(value, result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForDatePickerElement()
        {
            var questionId = "test-questionID";
            var labelText = "Enter the date";
            var value = new DateTime(2000, 01, 01);
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId, Response = value.ToString() } } } }};

            var element = new ElementBuilder()
                            .WithType(EElementType.DatePicker)
                            .WithQuestionId(questionId)
                            .WithLabel(labelText)
                            .Build();

            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            Assert.Equal(value.ToString("dd/MM/yyyy"), result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForCheckboxElement()
        {
            var questionId = "test-questionID";
            var labelText = "Checkbox label";
            var labelValue =  "Yes Text";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId, Response = "yes" } } } }};

            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .WithOptions(new List<Option>{ new Option{ Text = labelValue, Value = "yes" }, new Option{ Text = "No Text", Value = "n" }})
                .Build();

            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            Assert.Equal(labelValue, result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForRadioElement()
        {
             var labelText = "Radio radio";
             var labelValue = "No Text";
            
            var questionId = "test-questionID";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId, Response = "n" } } } }};

            var element = new ElementBuilder()
                            .WithType(EElementType.Radio)
                            .WithQuestionId(questionId)
                            .WithLabel(labelText)
                            .WithOptions(new List<Option>{ new Option{ Text = "Yes Text", Value = "yes" }, new Option{ Text = labelValue, Value = "n" }})
                            .Build();

            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            Assert.Equal(labelValue, result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForStreetElement()
        {
             var value = new StockportGovUK.NetStandard.Models.Addresses.Address{ SelectedAddress = "street, city, postcode, uk", PlaceRef = "1234" };

            var questionId = "test-questionID";
            var labelText = "Enter the Street";

            var uspn = "test-questionID-streetaddress";
            var addressDescription = "test-questionID-streetaddress-description";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = uspn,
                                Response = "1001254222"
                            },
                            new Answers
                            {
                                QuestionId = addressDescription,
                                Response = "street, city, postcode, uk"
                            }
                        }
                    }
                }
            };
            var element = new ElementBuilder()
                            .WithType(EElementType.Street)
                            .WithQuestionId(questionId)
                            .WithStreetLabel(labelText)
                            .Build();

            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            Assert.Equal("street, city, postcode, uk", result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForAddressElement()
        {
            var value = new StockportGovUK.NetStandard.Models.Addresses.Address{ SelectedAddress = "11 road, city, postcode, uk" };

            var questionId = "test-questionID";
            var labelText = "Whats your Address";
            var uprn = "test-questionID-address";
            var addressDescription = "test-questionID-address-description";

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
                            },
                            new Answers
                            {
                                QuestionId = addressDescription,
                                Response = "11 road, city, postcode, uk"
                            }
                        }
                    }
                }
            };

            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .WithAddressLabel(labelText)
                            .Build();

            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            Assert.Equal(value.SelectedAddress, result);
        }
        
        [Fact]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForFileUpload()
        {
            var questionId = "test-questionID";
            var labelText = "Evidence file";
            var value = new FileUploadModel{ TrustedOriginalFileName = "your_upload_file.txt" };
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-fileupload", Response = Newtonsoft.Json.JsonConvert.SerializeObject(value) } } }}};

            var element = new ElementBuilder()
                            .WithType(EElementType.FileUpload)
                            .WithQuestionId(questionId)
                            .WithLabel(labelText)
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            Assert.Equal(value.TrustedOriginalFileName, result);
        }
              
           
        [Fact]
        public void GetAnswerStringValue_ShouldGenerateCorrectValue_ForDateInput()
        {
            var questionId = "test-questionID";
            var labelText = "What Date do you like";
            var valueDay = "01";
            var valueMonth = "02";
            var valueYear = "2010";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-day", Response = valueDay }, new Answers { QuestionId = $"{questionId}-month", Response = valueMonth }, new Answers { QuestionId = $"{questionId}-year", Response = valueYear } } }}};

            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .Build();

            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            Assert.Equal($"{valueDay}/{valueMonth}/{valueYear}", result);
        }

        [Theory]
        [InlineData("03", "10", "PM")]
        [InlineData("12", "54", "PM")]
        [InlineData("12", "11", "AM")]
        [InlineData("05", "23", "AM")]
        public void GetAnswerStringValue_ShouldGenerateCorrectValue_ForTimeInput(string hour, string min, string amPm)
        {
            var dateTime = DateTime.Parse($"{hour}:{min} {amPm}");

            var questionId = "test-questionID";
            var labelText = "What Time do you like";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-hours", Response = hour }, new Answers { QuestionId = $"{questionId}-minutes", Response = min }, new Answers { QuestionId = $"{questionId}-ampm", Response = amPm } } }}};

            var element = new ElementBuilder()
                .WithType(EElementType.TimeInput)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .Build();

            var page = new PageBuilder()
            .WithElement(element)
            .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            Assert.Equal($"{hour}:{min} {amPm}", result);
        }
    }
}