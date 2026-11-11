using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace Moca.Examples.Tests;

/// <summary>
/// Runs the built examples against moca-spec's support-kb example package. Needs a moca-spec
/// checkout beside this repository, or <c>MOCA_SPEC_DIR</c>.
/// </summary>
public sealed partial class ExampleTests
{
    private static readonly string Kb = Path.Combine(SpecDir(), "examples", "support-kb");

    [Fact]
    public void OpenPrintsTheDigestAndCapabilities()
    {
        var (status, output, error) = Run("moca-cli", "open", Kb);
        Assert.True(status == 0, error);
        Assert.Matches(@"digest\s+sha256:[a-f0-9]{64}", output);
        Assert.Matches(@"capabilities\s+core", output);
    }

    [Fact]
    public void SearchReturnsCitedResults()
    {
        var (status, output, error) = Run("moca-cli", "search", Kb, "refund", "window");
        Assert.True(status == 0, error);
        Assert.Matches(new Regex(@"^\[1\] .+\(https://.+#.+\)", RegexOptions.Multiline), output);
    }

    [Fact]
    public void ChatHandsTheSourcesToTheModelAsDataNotInstructions()
    {
        var (status, output, error) = Run("moca-chat", Kb, "How", "long", "is", "the", "refund", "window?", "--show-prompt");
        Assert.True(status == 0, error);
        var messages = JsonDocument.Parse(output).RootElement;
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.DoesNotContain("<source", messages[0].GetProperty("content").GetString());
        Assert.Matches("<source n=\"1\" ref=\"https://.+\">", messages[1].GetProperty("content").GetString());
    }

    private static (int Status, string Output, string Error) Run(string example, params string[] args)
    {
        var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, $"{example}.dll"));
        foreach (var a in args) start.ArgumentList.Add(a);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output, error);
    }

    private static string SpecDir()
    {
        var configured = Environment.GetEnvironmentVariable("MOCA_SPEC_DIR");
        if (!string.IsNullOrEmpty(configured)) return Path.GetFullPath(configured);
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "moca-examples.slnx")))
                return Path.GetFullPath(Path.Combine(dir.FullName, "..", "..", "moca-spec"));
        }
        throw new InvalidOperationException("Cannot find moca-spec. Clone it beside this repository or set MOCA_SPEC_DIR.");
    }
}
