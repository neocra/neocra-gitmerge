using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Neocra.GitMerge.Tests
{
    public class BaseTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly string extension;

        public BaseTests(ITestOutputHelper testOutputHelper, string extension = null)
        {
            this.testOutputHelper = testOutputHelper;
            this.extension = extension;
        }
        
        protected async Task MergeConflict(string ancestor, string current, string other)
        {
            var test = Guid.NewGuid();

            var ancestorFile = $"{test}-ancestor.{extension}";
            File.WriteAllText(ancestorFile, ancestor);
            var currentFile = $"{test}-current.{extension}";
            File.WriteAllText(currentFile, current);
            var otherFile = $"{test}-other.{extension}";
            File.WriteAllText(otherFile, other);

            Program.LoggerProvider = new TestOUtputHelperLoggerProvider(this.testOutputHelper);
            var result = await Program.Main(new string[]
            {
                "merge",
                "--current",
                currentFile,
                "--ancestor",
                ancestorFile,
                "--other",
                otherFile,
                "-v"
            });

            Assert.Equal(1, result);
        }
        
        protected async Task Merge(string ancestor, string current, string other, string expected, string extension = null)
        {
            var test = Guid.NewGuid();
            testOutputHelper.WriteLine($"Test numéro : {test}");
          
            extension ??= this.extension;
            
            var ancestorFile = $"{test}-ancestor.{extension}";
            await File.WriteAllTextAsync(ancestorFile, ancestor);
            var currentFile = $"{test}-current.{extension}";
            await File.WriteAllTextAsync(currentFile, current);
            var otherFile = $"{test}-other.{extension}";
            await File.WriteAllTextAsync(otherFile, other);

            Program.LoggerProvider = new TestOUtputHelperLoggerProvider(this.testOutputHelper);
            var resultStatus = await Program.Main(new string[]
            {
                "merge",
                "--current",
                currentFile,
                "--ancestor",
                ancestorFile,
                "--other",
                otherFile,
                "-v"
            });
            
            var result = await File.ReadAllTextAsync(currentFile);

            Assert.Equal( 0, resultStatus);
            Assert.Equal(expected, result);
        }
    }
}