using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Neocra.GitMerge.Tests;

public class AutomaticTests : BaseTests
{
    public AutomaticTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }
        
    [Theory(DisplayName = "Files used to test use case merge")]
    [MemberData(nameof(GetData), "automatic")]
    [MemberData(nameof(GetData), "automatic-ignore")]
    public async Task Should_merge_automatic_directory(string directory, string ancestor, string current, string other, string expected, string extension)
    {
        var ancestorContent = await File.ReadAllTextAsync(ancestor);
        var currentContent = await File.ReadAllTextAsync(current);
        var otherContent = await File.ReadAllTextAsync(other);
        var result =
            File.Exists(expected) ? await File.ReadAllTextAsync(expected) : "File result not found";

            
        await this.Merge(
            ancestorContent,
            currentContent,
            otherContent,
            result,
            extension);
    }
        
    public static IEnumerable<object[]> GetData(string directoryName)
    {
        foreach (var directory in Directory.GetDirectories(directoryName))
        {
            var ancestor = Path.Combine(directory, "ancestor");

            if (File.Exists(ancestor))
            {
                yield return new[]
                {
                    directory,
                    ancestor,
                    Path.Combine(directory, "current"),
                    Path.Combine(directory, "other"),
                    Path.Combine(directory, "result"),
                    directory.Split(".").Last()
                };
            }
        }
    }
}