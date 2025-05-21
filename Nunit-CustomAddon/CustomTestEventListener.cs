using NUnit.Engine;
using NUnit.Engine.Extensibility;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;

namespace NUnit.Custom.TestEventListener
{
    [Extension]
    public class CustomTestEventListener : ITestEventListener
    {
        public static List<TestResultCustom> TestResults { get; } = new();

        public static readonly ManualResetEvent AllTestsFinished = new(false);

        public void OnTestEvent(string report)
        {
            XElement? reportElement = ParseXmlReport(report);

            if (reportElement?.Name.LocalName == "test-run" && reportElement.Attribute("result") != null)
            {
                AllTestsFinished.Set();
            }

            if (reportElement?.Name.LocalName == "test-case")
            {
                TestResultCustom testCaseResult = CreateTestResultCustom(reportElement);
                UpdateOrAddTestResult(testCaseResult);
                //WriteToDummyLog(reportElement.ToString());
                WriteToDummyLog($"TestCase Result:\n {testCaseResult}");
            }
        }

        private static XElement? ParseXmlReport(string report)
        {
            try
            {
                return XDocument.Parse(report).Root;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error parsing XML TestCase report in NUnit Custom TestEvent Listener: {ex.Message}");
            }
        }

        private TestResultCustom CreateTestResultCustom(XElement testCaseElement)
        {
            var testCase = new TestResultCustom
            {
                TestCaseName = testCaseElement.Attribute("name")?.Value,
                Outcome = testCaseElement.Attribute("result")?.Value,
                StartTime = testCaseElement.Attribute("start-time")?.Value,
                EndTime = testCaseElement.Attribute("end-time")?.Value,
                Duration = (int)Math.Round(double.Parse(testCaseElement.Attribute("duration")?.Value, CultureInfo.InvariantCulture)),
                TestCaseId = testCaseElement.Element("properties")?.Elements("property")
                    .FirstOrDefault(p => p.Attribute("name")?.Value == "ZephyrTestId")?.Attribute("value")?.Value,
                TestRailId = testCaseElement.Element("properties")?.Elements("property")
                    .FirstOrDefault(p => p.Attribute("name")?.Value == "TestRailId")?.Attribute("value")?.Value,
                FailureMessage = testCaseElement.Element("failure")?.Element("message")?.Value,
                StackTrace = testCaseElement.Element("failure")?.Element("stack-trace")?.Value
            };

            return testCase;
        }

        /// <summary>
        /// Update or Add new TestCase Result in the Final Test Result List.
        /// This Logic is to handle the Retry Concept as Retrying the Failed Test Might Result is Success.
        /// So the final Result should produce the End of the final Result.
        /// </summary>
        /// <param name="newTestResult"></param>
        private void UpdateOrAddTestResult(TestResultCustom newTestResult)
        {
            // Remove the existing TestResultCustom object and Add new object
            TestResults.RemoveAll(tr => tr.TestCaseName == newTestResult.TestCaseName);
            TestResults.Add(newTestResult);
        }

        /// <summary>
        /// Write the DummyLogs for the Test results.
        /// 'TestEventListener.log' file will be crated in Bin Directory.
        /// </summary>
        /// <param name="resultLog"></param>
        private static void WriteToDummyLog(string resultLog)
        {
            File.AppendAllLines(Path.Combine(Environment.CurrentDirectory, "TestEventListener.log"),
                new[] { resultLog });
        }
    }

    /// <summary>
    /// Class as model to store the values of the TestCase as Result.
    /// This will hold our custom result for the single Test.
    /// </summary>
    public class TestResultCustom
    {
        public string? TestCaseName { get; set; }
        public string? TestRailId { get; set; }
        public string? TestCaseId { get; set; }
        public string? Outcome { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int Duration { get; set; } //In Seconds
        public string? Description { get; set; }
        public string? FailureMessage { get; set; }
        public string? StackTrace { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
