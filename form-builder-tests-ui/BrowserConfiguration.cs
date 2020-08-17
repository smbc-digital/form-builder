using System;
using System.IO;
using Coypu;
using Coypu.Drivers;
using Coypu.Drivers.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Tracing;

namespace form_builder_tests_ui
{
    [Binding]
    public class BrowserConfiguration
    {
        public static BrowserSession BrowserSession;

        [BeforeTestRun]
        public static void Start()
        {
            var sessionConfiguration = new SessionConfiguration
            {
                Driver = typeof(CustomChromeProfileSeleniumWebDriver),
                AppHost = "http://localhost",
                Browser = Browser.Chrome,
                Port = 5001,
                Timeout = TimeSpan.FromSeconds(4),
                RetryInterval = TimeSpan.FromSeconds(0.5)
            };

            BrowserSession = new BrowserSession(sessionConfiguration);
        }

        [AfterScenario]
        public static void AfterScenario()
        {
            var webDriver = BrowserSession.Native as IWebDriver;

            if (webDriver != null)
            {
                webDriver.Manage().Cookies.DeleteAllCookies();
            }

            if (ScenarioContext.Current.TestError != null)
            {
                TakeScreenshot(webDriver);
            }
        }

        [AfterTestRun]
        public static void Shutdown()
        {
            BrowserSession.Dispose();
        }

        private static void TakeScreenshot(IWebDriver driver)
        {
            var fileNameBase = string.Format("error_{0}_{1}_{2}",
                FeatureContext.Current.FeatureInfo.Title.ToIdentifier(),
                ScenarioContext.Current.ScenarioInfo.Title.ToIdentifier(),
                DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Remove(path.IndexOf("bin", StringComparison.Ordinal)) + "/Screenshots";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            ITakesScreenshot takesScreenshot = driver as ITakesScreenshot;

            if (takesScreenshot != null)
            {
                var screenshot = takesScreenshot.GetScreenshot();

                var screenshotFilePath = Path.Combine(path, fileNameBase + "_screenshot.png");

                screenshot.SaveAsFile(screenshotFilePath, ScreenshotImageFormat.Png);
            }
        }
    }
    public class CustomChromeProfileSeleniumWebDriver : SeleniumWebDriver
    {
        public CustomChromeProfileSeleniumWebDriver(Browser browser)
            : base(CustomProfile(), browser)
        {
        }

        private static RemoteWebDriver CustomProfile()
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AcceptInsecureCertificates = true;



            return new ChromeDriver(chromeOptions);
        }
    }
}
