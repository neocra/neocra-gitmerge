using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Neocra.GitMerge;

public class InstallCommand : AsyncCommand<InstallSettings>
{
    private IAnsiConsole _console;
    private ILogger<InstallCommand> _logger;

    public InstallCommand(IAnsiConsole console, ILogger<InstallCommand> logger)
    {
        this._console = console;
        this._logger = logger;
    }

    private string? FindGitRepository()
    {
        var current = Directory.GetCurrentDirectory();
        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current, ".git")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        return null;
    }
    
    public override Task<int> ExecuteAsync(CommandContext context, InstallSettings settings)
    {
        var directory = this.FindGitRepository();
        if (directory == null)
        {
            _logger.LogError("This command must be run in a git repository");
            return Task.FromResult(1);
        }
        
        var choices = this.GetChoices();

        ReadAttribute(directory, choices);

        var value = this._console.Prompt(
            new SelectionPrompt<string>()
                .Title("Select the language you want to activate")
                .AddChoices(choices.Select(c => c.Display)));
        
        var choice = choices.FirstOrDefault(c => c.Display == value);

        if (choice != null)
        {
            choice.IsActivated = !choice.IsActivated;
            this.WriteAttributes(directory, choice);

            WriteGitConfig(directory, choices.Any(c => c.IsActivated));
        }
        
        return Task.FromResult(0);
    }

    private void WriteGitConfig(string directory, bool add)
    {
        var iniFilePath = Path.Combine(directory, ".git/config");
        var parser = new FileIniDataParser();
        var ini = parser.ReadFile(iniFilePath);
        ini.Configuration.AssigmentSpacer = " ";
      
        if (add)
        {
            ini["merge \"neocra-gitmerge\""]["name"] = "Neocra gitmerge driver";
            ini["merge \"neocra-gitmerge\""]["driver"] = "neocra-gitmerge merge --ancestor %O --current %A --other %B --path-name %P";
        }
        else
        {
            ini.Sections.RemoveSection("merge \"neocra-gitmerge\"");
        }
        
        parser.WriteFile(iniFilePath, ini);
    }

    private void WriteAttributes(string directory, InstallChoice choice)
    {
        var gitInfoAttributes = Path.Combine(directory, ".git/info/attributes");
        if (!File.Exists(gitInfoAttributes))
        {
            File.Create(gitInfoAttributes).Close();
        }
        
        // Read file attributes
        var lines = File.ReadAllLines(gitInfoAttributes);

        if (!choice.IsActivated)
        {
            lines = lines
                .Where(l => !l.StartsWith($"*.{choice.Extension} merge=neocra-gitmerge"))
                .ToArray();
        }
        else
        {
            lines = lines
                .Where(l => !l.StartsWith($"*.{choice.Extension} merge=neocra-gitmerge"))
                .Concat(new[] { $"*.{choice.Extension} merge=neocra-gitmerge" })
                .ToArray();
        }

        File.WriteAllLines(gitInfoAttributes, lines);
    }

    private static void ReadAttribute(string directory, InstallChoice[] choices)
    {
        var gitInfoAttributes = Path.Combine(directory, ".git/info/attributes");
        if (File.Exists(gitInfoAttributes))
        {
            // Read file attributes
            var lines = File.ReadAllLines(gitInfoAttributes);
            foreach (var line in lines)
            {
                var c = choices
                    .FirstOrDefault(c => line.StartsWith($"*.{c.Extension} merge=neocra-gitmerge"));

                if (c == null)
                {
                    continue;
                }

                c.IsActivated = true;
            }
        }
    }

    private InstallChoice[] GetChoices()
    {
        return new[]
        {
            new InstallChoice("Charp", "cs"),
            new InstallChoice("CsProj", "csproj"),
            new InstallChoice("XML", "xml"),
        };
    }
}

public class InstallChoice
{
    public InstallChoice(string name, string extension)
    {
        this.Name = name;
        this.Extension = extension;
    }

    public string Extension { get; }

    public string Name { get; }
    public string Display => (this.IsActivated ? "*" : " ") + $"{this.Name}";
    public bool IsActivated { get; set; }
}