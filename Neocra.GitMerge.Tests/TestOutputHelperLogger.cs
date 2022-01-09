using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Neocra.GitMerge.Tests
{
    public class TestOutputHelperLogger : ILogger, IDisposable
    {
        private ITestOutputHelper testOutputHelper;

        public TestOutputHelperLogger(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var formatted = formatter(state, exception);
            
            this.testOutputHelper.WriteLine($"[{logLevel}] {formatted}");

            if (exception != null)
            {
                this.testOutputHelper.WriteLine(exception.ToString());
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose()
        {
            
        }
    }
}