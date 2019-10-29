using System;
using Newtonsoft.Json.Linq;
using WireMock.Handlers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace form_builder_tests_ui
{
    public class MockConfiguration
    {
        public static FluentMockServer Server;
        public static bool IsRecordMode;

        public MockConfiguration(bool isRecordMode)
        {
            if (Server == null)
            {
                IsRecordMode = isRecordMode;
                Start();
            }
        }

        private static void Start()
        {
            if (IsRecordMode)
            {
                Server = FluentMockServer.Start(new FluentMockServerSettings
                {
                    StartAdminInterface = true,
                    ProxyAndRecordSettings = new ProxyAndRecordSettings
                    {
                        Url = "https://localhost:44360/",
                        SaveMapping = true,
                        BlackListedHeaders = new[] { "X-ClientId", "Request-Id", "Authorization", "Host" },
                    },
                    Port = 8080
                });
            }
            else
            {
                Server = FluentMockServer.Start(new FluentMockServerSettings
                {
                    Urls = new[] { "http://localhost:8080/" }
                });

                var path = AppDomain.CurrentDomain.BaseDirectory;
                path = path.Remove(path.IndexOf("bin", StringComparison.Ordinal));

                Server.ReadStaticMappings(path + "SavedResponses");
                Server.ReadStaticMappings(path + "SavedResponses/Articles");
                Server.ReadStaticMappings(path + "SavedResponses/Atoz");
                Server.ReadStaticMappings(path + "SavedResponses/Events");
                Server.ReadStaticMappings(path + "SavedResponses/Footer");
                Server.ReadStaticMappings(path + "SavedResponses/Groups");
                Server.ReadStaticMappings(path + "SavedResponses/Homepage");
                Server.ReadStaticMappings(path + "SavedResponses/News");
                Server.ReadStaticMappings(path + "SavedResponses/Showcase");
                Server.ReadStaticMappings(path + "SavedResponses/Topics");
                Server.ReadStaticMappings(path + "SavedResponses/ContactUsArea");

            }
        }
    }
}
