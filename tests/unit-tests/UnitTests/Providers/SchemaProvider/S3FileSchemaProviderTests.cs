using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using form_builder.Configuration;
using form_builder.Gateways;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.SchemaProvider
{
    public class S3FileSchemaProviderTests
    {
        private readonly S3FileSchemaProvider _s3Schema;
        private readonly Mock<IS3Gateway> _mockS3Gateway = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();
        private readonly Mock<IOptions<S3SchemaProviderConfiguration>> _mocks3SchemaConfiguration = new();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCacheWrapper = new();
        private readonly Mock<IOptions<DistributedCacheConfiguration>> _mockDistributedCacheConfiguration = new();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistributedCacheExpirationConfiguration = new();
        private readonly Mock<ILogger<ISchemaProvider>> _mockLogger = new();
        private const int _indexExpiry = 10;
        private const string _s3SchemaFolderName =  "test-folder-name";


        public S3FileSchemaProviderTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("uitest");
            _mocks3SchemaConfiguration.Setup(_ => _.Value)
                .Returns(new S3SchemaProviderConfiguration {
                    S3BucketKey = "forms-storage",
                    S3BucketFolderName = _s3SchemaFolderName
                });
            _mockDistributedCacheConfiguration.Setup(_ => _.Value).Returns(new DistributedCacheConfiguration { UseDistributedCache = true });
            _mockDistributedCacheExpirationConfiguration.Setup(_ => _.Value).Returns(new DistributedCacheExpirationConfiguration { Index = _indexExpiry });
            _s3Schema = new S3FileSchemaProvider(_mockS3Gateway.Object, _mockHostingEnv.Object, _mockDistributedCacheWrapper.Object, _mocks3SchemaConfiguration.Object, _mockDistributedCacheConfiguration.Object, _mockDistributedCacheExpirationConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Get_ShouldThrowExceptionWhenGateway_ThrowsAmazonException()
        {
            _mockS3Gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AmazonS3Exception("amazon exception"));

            var result = await Assert.ThrowsAsync<Exception>(() => _s3Schema.Get<string>("name"));
            _mockS3Gateway.Verify(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.StartsWith("S3FileSchemaProvider: An error has occured while attempting to get S3 Object, Exception:", result.Message);
        }

        [Fact]
        public async Task Get_ShouldThrowExceptionWhenGateway_ThrowsException()
        {
            _mockS3Gateway.Setup(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("an exception"));

            var result = await Assert.ThrowsAsync<Exception>(() => _s3Schema.Get<string>("name"));
            _mockS3Gateway.Verify(_ => _.GetObject(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.StartsWith("S3FileSchemaProvider: An error has occured while attempting to deserialize object, Exception:", result.Message);
        }

        [Fact]
        public async Task ValidateSchemaName_ShouldReturnTrue_AndMatch_Schema_ToRequested_SchemaName()
        {
            var schemaName = "test-form-name";
            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new List<string>
                {
                    $"{schemaName}.json"
                }));
            var result = await _s3Schema.ValidateSchemaName(schemaName);
            
            Assert.True(result);
        }
        
        [Theory]
        [InlineData("test-form-nam")]
        [InlineData("est-form-name")]
        [InlineData("test")]
        [InlineData("form-name")]
        public async Task ValidateSchemaName_ShouldReturnFalse_When_Requested_SchemaName_IsNot_In_Index(string formName)
        {
            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new List<string>
                {
                    $"test-form-name.json"
                }));
            var result = await _s3Schema.ValidateSchemaName(formName);
            
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateSchemaName_Should_Call_ToIndex_Schema_WhenNotCurrently_InCache()
        {
            var schemaName = "test-form-name";
            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(string.Empty);

            _mockS3Gateway.Setup(_ => _.ListObjectsV2(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ListObjectsV2Response{ S3Objects = new List<S3Object>{ new S3Object { Key = $"Int/{_s3SchemaFolderName}/testform.json" } } });

            var result = await _s3Schema.ValidateSchemaName(schemaName);
            
            _mockS3Gateway.Verify(_ => _.ListObjectsV2(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockDistributedCacheWrapper.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<int>(_ => _.Equals(_indexExpiry)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ValidateSchemaName_Should_Remove_BucketPrefix_WhenIndexing_List()
        {
            var testIndexKey = $"Int/{_s3SchemaFolderName}/test-form-name.json";
            var expected = "test-form-name.json";
            _mockS3Gateway.Setup(_ => _.ListObjectsV2(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ListObjectsV2Response{ S3Objects = new List<S3Object>{ new S3Object { Key = testIndexKey } } });

            var result = await _s3Schema.IndexSchema();
            
            Assert.Single(result);
            Assert.Equal(expected, result.First());
        }
    }
}
