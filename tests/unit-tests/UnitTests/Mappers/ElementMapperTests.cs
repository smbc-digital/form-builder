using System;
using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Utils.Extensions;
using form_builder.Utils.Hash;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Booking;
using StockportGovUK.NetStandard.Models.FileManagement;
using Xunit;

namespace form_builder_tests.UnitTests.Mappers
{
    public class ElementMapperTests
    {
        private readonly ElementMapper _elementMapper;
        private readonly Mock<IDistributedCacheWrapper> _wrapper = new ();
        private readonly Mock<IHashUtil> _mockHashUtil = new ();

        public ElementMapperTests()
        {
            _mockHashUtil
                .Setup(_ => _.Hash(It.IsAny<string>()))
                .Returns("hashedId");

            _elementMapper = new ElementMapper(_wrapper.Object, _mockHashUtil.Object);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnIntWhenNumericIsTrue()
        {
            // Arrange
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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            Assert.IsType<int>(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnSingleDateWhenElementIsDatePicker()
        {
            // Arrange
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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            Assert.IsType<DateTime>(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnNullWhenResponseIsEmpty_WhenElementIsDatePicker()
        {
            // Arrange
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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnNullWhenResponseIsEmpty_AndNumeric()
        {
            // Arrange
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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnArrayOfStrings_WhenElementIsCheckbox()
        {
            // Arrange
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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            var type = Assert.IsType<List<string>>(result);
            Assert.Equal("option1", type[0]);
            Assert.Equal("option2", type[1]);
            Assert.Equal("option3", type[2]);
            Assert.Equal(3, type.Count);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnEmptyArray_WhenResponseIsEmpty_ForCheckbox()
        {
            // Arrange
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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            var type = Assert.IsType<List<string>>(result);
            Assert.Empty(type);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnDateValue_WhenElementIsDateInput()
        {
            // Arrange
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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            Assert.IsType<DateTime>(result);
        }
        [Fact]
        public void GetAnswerValue_ShouldReturnNullIfDayIsEmpty_WhenElementIsDateInput()
        {
            // Arrange
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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnNullIfHourIsEmpty_WhenElementIsTimeInput()
        {
            // Arrange
            var element = new ElementBuilder()
                   .WithQuestionId("testTime")
                   .WithType(EElementType.TimeInput)
                   .Build();

            var timeMinutesKey = $"testTime{TimeConstants.MINUTES_SUFFIX}";
            var timeHoursKey = $"testTime{TimeConstants.HOURS_SUFFIX}";
            var timeAmPmKey = $"testTime{TimeConstants.AM_PM_SUFFIX}";

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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnTimeSpan_WhenElementIsTimeInput()
        {
            // Arrange
            var element = new ElementBuilder()
                  .WithQuestionId("testTime")
                  .WithType(EElementType.TimeInput)
                  .Build();

            var timeMinutesKey = $"testTime{TimeConstants.MINUTES_SUFFIX}";
            var timeHoursKey = $"testTime{TimeConstants.HOURS_SUFFIX}";
            var timeAmPmKey = $"testTime{TimeConstants.AM_PM_SUFFIX}";

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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            Assert.IsType<TimeSpan>(result);
        }
        [Fact]
        public void GetAnswerValue_ShouldReturnAddress_WhenElementIsAddress_Automatic()
        {
            // Arrange
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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            var type = Assert.IsType<Address>(result);
            Assert.Equal("1001254222", type.PlaceRef);
            Assert.Equal(description, type.SelectedAddress);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnAddress_WhenElementIsAddress_Manual()
        {
            // Arrange
            var element = new ElementBuilder()
                  .WithQuestionId("testAddress")
                  .WithType(EElementType.Address)
                  .Build();

            var manualAddressLineOne = $"testAddress-{AddressManualConstants.ADDRESS_LINE_1}";
            var manualAddressLineTwo = $"testAddress-{AddressManualConstants.ADDRESS_LINE_2}";
            var manualAddressLineTown = $"testAddress-{AddressManualConstants.TOWN}";
            var manualAddressLinePostcode = $"testAddress-{AddressManualConstants.POSTCODE}";

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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            var type = Assert.IsType<Address>(result);
            Assert.Equal("line1", type.AddressLine1);
            Assert.Equal("line2", type.AddressLine2);
            Assert.Equal("town", type.Town);
            Assert.Equal("sk13xe", type.Postcode);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnAddress_WhenElementIsStreet()
        {
            // Arrange
            var element = new ElementBuilder()
                  .WithQuestionId("testStreetAddress")
                  .WithType(EElementType.Street)
                  .Build();

            var streetUspr = "testStreetAddress-street";
            var streetDescription = "testStreetAddress-street-description";

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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            var type = Assert.IsType<Address>(result);
            Assert.Equal("0101010101", type.PlaceRef);
            Assert.Equal("im a street", type.SelectedAddress);
        }


        [Fact]
        public void GetAnswerValue_ShouldReturnBookingModel_WhenElementIsBooking()
        {
            // Arrange
            var questionId = "testbookingId";
            var date = DateTime.Today;
            var startTime = DateTime.Today.Add(new TimeSpan(22, 0, 0));
            var id = Guid.NewGuid();
            var hashedId = "hashedId";
            var location = "location";
            var element = new ElementBuilder()
                  .WithQuestionId(questionId)
                  .WithType(EElementType.Booking)
                  .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers { QuestionId = $"{questionId}-{BookingConstants.RESERVED_BOOKING_DATE}", Response = date.ToString() },
                            new Answers { QuestionId = $"{questionId}-{BookingConstants.RESERVED_BOOKING_START_TIME}", Response = startTime.ToString() },
                            new Answers { QuestionId = $"{questionId}-{BookingConstants.RESERVED_BOOKING_ID}", Response = id.ToString() },
                            new Answers { QuestionId = $"{questionId}-{BookingConstants.APPOINTMENT_LOCATION}", Response = location }
                        }
                    }
                }
            };

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            var bookingResult = Assert.IsType<Booking>(result);
            Assert.Equal(id, bookingResult.Id);
            Assert.Equal(location, bookingResult.Location);
            Assert.Equal(hashedId, bookingResult.HashedId);
            Assert.Equal(date, bookingResult.Date);
            Assert.Equal(startTime, bookingResult.StartTime);
        }

        [Fact]
        public void GetAnswerValue_ShouldCallHashUtil_WhenElementIsBooking()
        {
            // Arrange
            var questionId = "testbookingId";
            var date = DateTime.Today;
            var startTime = DateTime.Today.Add(new TimeSpan(22, 0, 0));
            var id = Guid.NewGuid();
            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Booking)
                .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers { QuestionId = $"{questionId}-{BookingConstants.RESERVED_BOOKING_DATE}", Response = date.ToString() },
                            new Answers { QuestionId = $"{questionId}-{BookingConstants.RESERVED_BOOKING_START_TIME}", Response = startTime.ToString() },
                            new Answers { QuestionId = $"{questionId}-{BookingConstants.RESERVED_BOOKING_ID}", Response = id.ToString() }
                        }
                    }
                }
            };

            // Act
            _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            _mockHashUtil.Verify(_ => _.Hash(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnNull_WhenElementIsBooking_AndValueNotFound()
        {
            // Arrange
            var dateTime = DateTime.Now.ToString();
            var element = new ElementBuilder()
                  .WithQuestionId("testbookingId")
                  .WithType(EElementType.Booking)
                  .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers>()
                    }
                }
            };

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            Assert.Null(result);
        }


        [Fact]
        public void GetAnswerValue_ShouldReturnOrganisation_WhenElementIsOrganisation()
        {
            // Arrange
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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            var type = Assert.IsType<StockportGovUK.NetStandard.Models.Verint.Organisation>(result);
            Assert.Equal("0101010101", type.Reference);
            Assert.Equal("im an organisation", type.Name);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnValue()
        {
            // Arrange
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

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            Assert.IsType<string>(result);
        }

        [Theory]
        [InlineData(EElementType.FileUpload)]
        [InlineData(EElementType.MultipleFileUpload)]
        public void GetAnswerValue_ShouldCall_DistributedCache_ToGetFileUploadContent(EElementType type)
        {
            // Arrange
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
                                Response = JsonConvert.SerializeObject(new List<FileUploadModel>{ new FileUploadModel{ UntrustedOriginalFileName = key, TrustedOriginalFileName = key } })
                            }
                        }
                    }
                }
            };

            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId(key)
                .Build();

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            _wrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            var model = Assert.IsType<List<File>>(result);
            Assert.Equal("testfile", model[0].Content);
        }

        [Fact]
        public void GetAnswerValue_ShouldCall_DistributedCache_MultipleTimes_ToGet_All_FileUploadContent()
        {
            // Arrange
            var key = "fileUpload_fileUploadTestKey";
            _wrapper.Setup(_ => _.GetString(It.Is<string>(_ => _ == "datakeyfile1")))
                .Returns("file1content");

            _wrapper.Setup(_ => _.GetString(It.Is<string>(_ => _ == "datakeyfile2")))
                .Returns("file2content");

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = "fileUpload_fileUploadTestKey-fileupload",
                                Response = JsonConvert.SerializeObject(new List<FileUploadModel>
                                {
                                    new FileUploadModel{  UntrustedOriginalFileName ="file1.txt",  TrustedOriginalFileName = "file1.txt", Key = "datakeyfile1" },
                                    new FileUploadModel{  UntrustedOriginalFileName ="file2.txt", TrustedOriginalFileName = "file2.txt", Key = "datakeyfile2"  }
                                })
                            }
                        }
                    }
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId(key)
                .Build();

            // Act
            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            _wrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Exactly(2));
            var model = Assert.IsType<List<File>>(result);
            Assert.Equal("file1content", model[0].Content);
            Assert.Equal("file2content", model[1].Content);
            Assert.Equal("file1.txt", model[0].TrustedOriginalFileName);
            Assert.Equal("file2.txt", model[1].TrustedOriginalFileName);
        }

        [Theory]
        [InlineData(EElementType.FileUpload)]
        [InlineData(EElementType.MultipleFileUpload)]
        public void GetAnswerValue_Should_ThrowException_WhenDistributedCacheThrows(EElementType type)
        {
            // Arrange
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
                                Response = JsonConvert.SerializeObject(
                                    new List<FileUploadModel>
                                    {
                                        new FileUploadModel
                                        {
                                            Key = key
                                        }
                                    }
                                )
                            }
                        }
                    }
                }
            };

            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId(key)
                .Build();

            // Act
            var result = Assert.Throws<Exception>(() => _elementMapper.GetAnswerValue(element, formAnswers));

            // Assert
            Assert.Equal($"ElementMapper::GetFileUploadElementValue: An error has occurred while attempting to retrieve an uploaded file with key: {key} from the distributed cache", result.Message);
        }

        [Fact]
        public void GetAnswerValue_ShouldNotCall_DistributedCache_WhenNoFileWithinAnswers()
        {
            // Arrange
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

            // Act
            _elementMapper.GetAnswerValue(element, formAnswers);

            // Assert
            _wrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnDateTime_WithValidTime()
        {
            // Arrange
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
                                QuestionId = $"timeInput{TimeConstants.MINUTES_SUFFIX}",
                                Response = elementMinutes
                            },
                            new Answers
                            {
                                QuestionId = $"timeInput{TimeConstants.HOURS_SUFFIX}",
                                Response = elementHours
                            },
                            new Answers
                            {
                                QuestionId = $"timeInput{TimeConstants.AM_PM_SUFFIX}",
                                Response = elementAmPm
                            }
                        }
                    }
                }
            };


            var result = _elementMapper.GetAnswerValue(element, formAnswers);

            var resultData = Assert.IsType<TimeSpan>(result);
            Assert.Equal("3", resultData.Hours.ToString());
            Assert.Equal(elementMinutes, resultData.Minutes.ToString());
        }


        [Fact]
        public void Map_ShouldReturnExpandoObject_WhenFormContains_MultipleValidatableElementsWithTargetMapping_WithValues()
        {
            // Arrange
            var elementOneAnswer = "01/01/2000";
            var element3 = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test")
                .WithTargetMapping("customer.datepicker.date")
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

            var result = _elementMapper.GetAnswerValue(element3, formAnswers);

            var resultData = Assert.IsType<DateTime>(result);
            Assert.Equal($"{elementOneAnswer} 00:00:00", resultData.ToString());
        }

        [Theory]
        [InlineData(EElementType.Textbox, "test")]
        [InlineData(EElementType.Textarea, "textAreaValue")]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForElements(EElementType type, string value)
        {
            // Arrange
            var questionId = "test-questionID";
            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId, Response = value } } } } };

            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId(questionId)
                .Build();

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForDatePickerElement()
        {
            // Arrange
            var questionId = "test-questionID";
            var labelText = "Enter the date";
            var value = new DateTime(2000, 01, 01);
            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId, Response = value.ToString() } } } } };

            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .Build();

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal(value.ToString("dd/MM/yyyy"), result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForCheckboxElement()
        {
            // Arrange
            var questionId = "test-questionID";
            var labelText = "Checkbox label";
            var labelValue = "Yes Text";
            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId, Response = "yes" } } } } };

            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .WithOptions(new List<Option> { new Option { Text = labelValue, Value = "yes" }, new Option { Text = "No Text", Value = "n" } })
                .Build();

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal(labelValue, result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForRadioElement()
        {
            // Arrange
            var labelText = "Radio radio";
            var labelValue = "No Text";
            var questionId = "test-questionID";

            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId, Response = "n" } } } } };

            var element = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .WithOptions(new List<Option> { new Option { Text = "Yes Text", Value = "yes" }, new Option { Text = labelValue, Value = "n" } })
                .Build();

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal(labelValue, result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForStreetElement()
        {
            var questionId = "test-questionID";
            var labelText = "Enter the Street";
            var usrn = $"{questionId}{StreetConstants.SELECT_SUFFIX}";
            var addressDescription = $"{questionId}{StreetConstants.DESCRIPTION_SUFFIX}";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = usrn,
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
                .WithLabel(labelText)
                .Build();

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal("street, city, postcode, uk", result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForAddressElement()
        {
            // Arrange
            var value = new Address { SelectedAddress = "11 road, city, postcode, uk" };
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

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal(value.SelectedAddress, result);
        }

        [Theory]
        [InlineData(EElementType.FileUpload)]
        [InlineData(EElementType.MultipleFileUpload)]
        public void GetAnswerStringValue_ShouldReturnCorrectValue_ForFileUpload(EElementType type)
        {
            // Arrange
            var questionId = "test-questionID";
            var labelText = "Evidence file";
            var value = new List<FileUploadModel> { new FileUploadModel { TrustedOriginalFileName = "your_upload_file.txt" } };
            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-fileupload", Response = Newtonsoft.Json.JsonConvert.SerializeObject(value) } } } } };

            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .Build();

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal(value[0].TrustedOriginalFileName, result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldReturn_MultipleStrings_WhenMultipleFiles_AreWithinAnswerValue()
        {
            // Arrange
            var questionId = "test-questionID";
            var labelText = "Evidence file";
            var value = new List<FileUploadModel> { new FileUploadModel { TrustedOriginalFileName = "your_upload_file.txt" }, new FileUploadModel { TrustedOriginalFileName = "filetwo.jpg" } };
            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-fileupload", Response = Newtonsoft.Json.JsonConvert.SerializeObject(value) } } } } };

            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .Build();

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal($"{value[1].TrustedOriginalFileName} \\r\\n\\ {value[0].TrustedOriginalFileName}", result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldGenerateCorrectValue_ForDateInput()
        {
            // Arrange
            var questionId = "test-questionID";
            var labelText = "What Date do you like";
            var valueDay = "01";
            var valueMonth = "02";
            var valueYear = "2010";
            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-day", Response = valueDay }, new Answers { QuestionId = $"{questionId}-month", Response = valueMonth }, new Answers { QuestionId = $"{questionId}-year", Response = valueYear } } } } };

            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .Build();

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal($"{valueDay}/{valueMonth}/{valueYear}", result);
        }

        [Theory]
        [InlineData("03", "10", "PM")]
        [InlineData("12", "54", "PM")]
        [InlineData("12", "11", "AM")]
        [InlineData("05", "23", "AM")]
        public void GetAnswerStringValue_ShouldGenerateCorrectValue_ForTimeInput(string hour, string min, string amPm)
        {
            // Arrange
            var questionId = "test-questionID";
            var labelText = "What Time do you like";
            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}{TimeConstants.HOURS_SUFFIX}", Response = hour }, new Answers { QuestionId = $"{questionId}{TimeConstants.MINUTES_SUFFIX}", Response = min }, new Answers { QuestionId = $"{questionId}{TimeConstants.AM_PM_SUFFIX}", Response = amPm } } } } };

            var element = new ElementBuilder()
                .WithType(EElementType.TimeInput)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .Build();

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal($"{hour}:{min} {amPm}", result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldGenerateCorrectValue_ForBooking()
        {
            // Arrange
            var questionId = "test-bookingID";
            var time = new TimeSpan(4, 30, 0);
            var date = DateTime.Now;
            var startTime = DateTime.Today.Add(time);
            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers> { new PageAnswers { Answers = new List<Answers>
                {
                    new Answers { QuestionId = $"{questionId}-{BookingConstants.RESERVED_BOOKING_DATE}", Response = date.ToString() },
                    new Answers { QuestionId = $"{questionId}-{BookingConstants.RESERVED_BOOKING_START_TIME}", Response = startTime.ToString() }
                }
            }
            }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId(questionId)
                .Build();

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal($"{date.ToFullDateFormat()} at 4:30am to 12am", result);
        }

        [Fact]
        public void GetAnswerStringValue_ShouldGenerateCorrectValue_ForBooking_WhenDateTime_IsMinValue()
        {
            // Arrange
            var questionId = "test-bookingID";
            var date = DateTime.MinValue;
            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-{BookingConstants.RESERVED_BOOKING_DATE}", Response = date.ToString() } } } } };

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId(questionId)
                .Build();

            // Act
            var result = _elementMapper.GetAnswerStringValue(element, formAnswers);

            // Assert
            Assert.Equal(string.Empty, result);
        }

    }
}