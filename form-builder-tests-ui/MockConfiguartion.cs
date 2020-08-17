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
        }
    }
}
