using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Neocra.GitMerge.Tests
{
    public class TestOUtputHelperLoggerProvider : ILoggerProvider
    {
        private ITestOutputHelper testOutputHelper;

        public TestOUtputHelperLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputHelperLogger(testOutputHelper);
        }
    }
}