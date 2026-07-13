using CodeWalker.GameFiles;

var version = "1.0.0";
Console.Error.WriteLine($"ymap2xml v{version} — Converts binary YMAP to XML");
Console.Error.WriteLine();

var argsList = new List<string>(args);
string? inputPath = null;
string? outputDir = null;

for (int i = 0; i < argsList.Count; i++)
{
    if (argsList[i] == "--outdir" && i + 1 < argsList.Count)
    {
        outputDir = argsList[i + 1];
        i++;
    }
    else if (!argsList[i].StartsWith("--"))
    {
        inputPath = argsList[i];
    }
}

if (inputPath == null)
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine($"  {AppDomain.CurrentDomain.FriendlyName} <path> [--outdir <dir>]");
    Console.Error.WriteLine();
    Console.Error.WriteLine("<path>   .ymap file or folder containing .ymap files");
    Console.Error.WriteLine("--outdir  Output directory (default: same folder as source)");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Examples:");
    Console.Error.WriteLine($"  {AppDomain.CurrentDomain.FriendlyName} stream\\");
    Console.Error.WriteLine($"  {AppDomain.CurrentDomain.FriendlyName} city.ymap --outdir xml\\");
    return;
}

if (!Path.Exists(inputPath))
{
    Console.Error.WriteLine($"Error: path not found — {inputPath}");
    return;
}

if (outputDir != null)
{
    Directory.CreateDirectory(outputDir);
}

List<string> files;
if (File.Exists(inputPath) && Path.GetExtension(inputPath).Equals(".ymap", StringComparison.OrdinalIgnoreCase))
{
    files = [inputPath];
}
else if (Directory.Exists(inputPath))
{
    files = Directory.GetFiles(inputPath, "*.ymap", SearchOption.TopDirectoryOnly).ToList();
    if (files.Count == 0)
    {
        Console.Error.WriteLine($"No .ymap files found in {inputPath}");
        return;
    }
}
else
{
    Console.Error.WriteLine("Error: path must be a .ymap file or a directory.");
    return;
}

Console.Error.WriteLine($"Found {files.Count} .ymap file{(files.Count == 1 ? "" : "s")}");
Console.Error.WriteLine();

int ok = 0, fail = 0;

foreach (var ymapPath in files)
{
    try
    {
        var ymapName = Path.GetFileName(ymapPath);
        Console.Error.Write($"  {ymapName} ... ");

        var data = File.ReadAllBytes(ymapPath);

        var ymap = new YmapFile();
        ymap.Load(data);

        var xml = MetaXml.GetXml(ymap, out _);
        if (string.IsNullOrWhiteSpace(xml))
        {
            Console.Error.WriteLine("FAIL (null/empty XML)");
            fail++;
            continue;
        }

        var baseName = Path.GetFileNameWithoutExtension(ymapPath);
        var outDir = outputDir ?? Path.GetDirectoryName(ymapPath)!;
        var outPath = Path.Combine(outDir, baseName + ".ymap.xml");

        File.WriteAllText(outPath, xml);

        Console.Error.WriteLine($"OK → {baseName}.ymap.xml");
        ok++;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"FAIL ({ex.GetType().Name}: {ex.Message})");
        fail++;
    }
}

Console.Error.WriteLine();
Console.Error.WriteLine($"Done: {ok} converted, {fail} failed.");
Environment.Exit(fail > 0 ? 1 : 0);
