// Open a MOCA package, search it and cite what you find, with the .NET Reader.
//
//   dotnet run --project Moca.Examples.Cli -- open   <package>
//   dotnet run --project Moca.Examples.Cli -- search <package> <query...>
//   dotnet run --project Moca.Examples.Cli -- get    <package> <node-ref>
//
// <package> is a folder with a moca.json, or a .moca archive.
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenMoca.Reader;
using OpenMoca.Reader.Search;

if (args.Length < 2 || args[0] is not ("open" or "search" or "get") || (args[0] != "open" && args.Length < 3))
{
    Console.Error.WriteLine("usage: moca-cli open <package> | search <package> <query...> | get <package> <node-ref>");
    return 2;
}

var (command, target, rest) = (args[0], args[1], args[2..]);

// Reading checks the package: its digest, its files, its evidence. A broken
// package is not an exception; it comes back with Valid = false and diagnostics.
var package = await MocaReader.ReadPackageAsync(PackageTarget.FromPath(target));
if (!package.Valid)
{
    foreach (var d in package.Diagnostics) Console.Error.WriteLine($"{d.Severity} {d.Code} {d.File} {d.Message}");
    return 1;
}

var library = new Library().Add(package);

switch (command)
{
    case "open":
        Console.WriteLine($"{package.Manifest?["id"]} {package.Manifest?["version"]}");
        Console.WriteLine($"digest        {package.Digest}");
        Console.WriteLine($"capabilities  {string.Join(", ", package.Capabilities)}");
        Console.WriteLine($"nodes         {library.Citations().Count}");
        foreach (var d in package.Diagnostics) Console.WriteLine($"{d.Severity,-13} {d.Code} {d.Message}");
        break;

    case "search":
        // One search entry point. The Reader applies the default retrieval policy
        // to every hit (nothing out of force, superseded or deprecated) before it
        // returns, whatever the backend found.
        var records = await library.SearchAsync(string.Join(' ', rest), new SearchOptions { Limit = 5 });
        if (records.Count == 0) Console.WriteLine("No results.");
        for (var i = 0; i < records.Count; i++)
        {
            var r = records[i];
            Console.WriteLine($"[{i + 1}] {r.Node.Title ?? r.Node.Path}  ({r.Node.Ref})  score {r.Score:0.00}");
        }
        break;

    case "get":
        // A citation record: the text, plus where it came from and what vouches for it.
        var record = library.Get(rest[0]);
        if (record is null)
        {
            Console.Error.WriteLine($"No node {rest[0]} in this package.");
            return 1;
        }
        Console.WriteLine(JsonSerializer.Serialize(record, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        }));
        break;
}

return 0;
