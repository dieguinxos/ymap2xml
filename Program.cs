using CodeWalker.GameFiles;

var version = "1.1.0";
Console.Error.WriteLine($"ymap2xml v{version} — Converts binary YMAP to XML");
Console.Error.WriteLine();

var argsList = new List<string>(args);
string? inputPath = null;
string? outputDir = null;
string? gtaPath = null;
string? nametableFile = null;

for (int i = 0; i < argsList.Count; i++)
{
    if (argsList[i] == "--outdir" && i + 1 < argsList.Count)
    {
        outputDir = argsList[i + 1];
        i++;
    }
    else if (argsList[i] == "--gta-path" && i + 1 < argsList.Count)
    {
        gtaPath = argsList[i + 1];
        i++;
    }
    else if (argsList[i] == "--nametable" && i + 1 < argsList.Count)
    {
        nametableFile = argsList[i + 1];
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
    Console.Error.WriteLine($"  {AppDomain.CurrentDomain.FriendlyName} <path> [options]");
    Console.Error.WriteLine();
    Console.Error.WriteLine("<path>   .ymap file or folder containing .ymap files");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Options:");
    Console.Error.WriteLine("  --outdir <dir>      Output directory (default: same folder as source)");
    Console.Error.WriteLine("  --gta-path <dir>    GTA V installation folder (resolves hashes to names)");
    Console.Error.WriteLine("  --nametable <file>  Text file with one model name per line");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Examples:");
    Console.Error.WriteLine($"  {AppDomain.CurrentDomain.FriendlyName} stream\\ --outdir xml\\");
    Console.Error.WriteLine($"  {AppDomain.CurrentDomain.FriendlyName} city.ymap --gta-path \"C:\\Games\\GTAV\"");
    Console.Error.WriteLine($"  {AppDomain.CurrentDomain.FriendlyName} city.ymap --nametable allnames.txt");
    return;
}

if (!Path.Exists(inputPath))
{
    Console.Error.WriteLine($"Error: path not found — {inputPath}");
    return;
}

// --- Populate JenkIndex from GTA V game folder ---
if (gtaPath != null)
{
    if (!Directory.Exists(gtaPath))
    {
        Console.Error.WriteLine($"Error: GTA V path not found — {gtaPath}");
        return;
    }
    Console.Error.Write("Scanning GTA V RPFs... ");
    try
    {
        var rpfMan = new RpfManager();
        rpfMan.Init(gtaPath, _ => { }, msg => Console.Error.WriteLine("  [RpfManager] " + msg));
        Console.Error.WriteLine("OK (hash index populated)");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"FAILED ({ex.GetType().Name}: {ex.Message})");
        Console.Error.WriteLine("Continuing without hash resolution...");
    }
    Console.Error.WriteLine();
}

// --- Populate JenkIndex from nametable file ---
if (nametableFile != null)
{
    if (!File.Exists(nametableFile))
    {
        Console.Error.WriteLine($"Error: nametable not found — {nametableFile}");
        return;
    }
    Console.Error.Write("Loading nametable... ");
    try
    {
        int count = 0;
        foreach (var line in File.ReadLines(nametableFile))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0)
            {
                JenkIndex.Ensure(trimmed);
                count++;
            }
        }
        Console.Error.WriteLine($"OK ({count} names loaded)");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"FAILED ({ex.GetType().Name}: {ex.Message})");
        return;
    }
    Console.Error.WriteLine();
}

// --- Output dir ---
if (outputDir != null)
{
    Directory.CreateDirectory(outputDir);
}

// --- Collect input files ---
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

// --- Convert ---
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
