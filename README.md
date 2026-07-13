# ymap2xml

Convert binary GTA V / FiveM `.ymap` files to human-readable `.xml` — no OpenIV or CodeWalker GUI required.

## Features

- Converts individual `.ymap` files or entire directories at once
- Uses [CodeWalker.Core](https://www.nuget.org/packages/CodeWalker.Core/) — the same engine behind the official CodeWalker tool
- No GUI needed — run it from the terminal, integrate it into scripts
- Valid XML output with all entities, positions, rotations, LOD data, and metadata
- Handles corrupted files gracefully without stopping batch processing
- **Hash resolution**: resolves hashes to readable model names via GTA V game files or a nametable

## Usage

```bash
ymap2xml.exe <path> [options]
```

| Argument | Description |
|----------|-------------|
| `<path>` | Path to a `.ymap` file or a folder containing `.ymap` files |
| `--outdir <dir>` | Output directory (default: same folder as the source) |
| `--gta-path <dir>` | GTA V installation folder — scans RPFs to resolve hashes to names |
| `--nametable <file>` | Text file with one model name per line (e.g., DurtyFree's ObjectList.ini) |

### Examples

```bash
# Convert a single file — saves next to the source
ymap2xml.exe city.ymap

# Convert an entire stream folder
ymap2xml.exe resources\[mapas]\my_map\stream\

# Convert and output to a separate directory
ymap2xml.exe stream\ --outdir xml\

# Resolve names using GTA V game files
ymap2xml.exe city.ymap --gta-path "C:\Program Files\Rockstar Games\Grand Theft Auto V"

# Resolve names using a nametable file
ymap2xml.exe city.ymap --nametable ObjectList.ini

# Full: batch with GTA V path + custom output
ymap2xml.exe stream\ --gta-path "C:\Games\GTAV" --outdir xml\

# Batch processing (PowerShell)
Get-ChildItem -Recurse -Filter *.ymap | ForEach-Object {
    ymap2xml.exe $_.FullName --gta-path "C:\Games\GTAV" --outdir "C:\ymap_xml"
}
```

### Hash resolution

Binary YMAPs store archetype names as uint32 hashes. To get readable names:

1. **`--gta-path`** — scans all `.rpf` files in your GTA V folder using `RpfManager`, building the same hash index used by CodeWalker Explorer
2. **`--nametable`** — provide a text file with one model name per line (e.g., [DurtyFree's ObjectList.ini](https://github.com/DurtyFree/gta-v-data-dumps/blob/master/ObjectList.ini))
3. **Neither** — names appear as `hash_XXXXXXXX` (raw hash output)

You can combine both flags for maximum coverage.

## Installation

### Download (easiest)

Download the latest `ymap2xml.exe` from the [Releases](https://github.com/dieguinxos/ymap2xml/releases) page. No installation required.

### Build from source

Requires [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

```bash
git clone https://github.com/dieguinxos/ymap2xml.git
cd ymap2xml
dotnet publish -c Release -r win-x64 --self-contained false -o dist
```

For a portable exe that runs without .NET runtime:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -o dist
```

## Sample XML output

```xml
<?xml version="1.0" encoding="UTF-8"?>
<CMapData>
  <name>City_wreks1</name>
  <entities>
    <Item type="CEntityDef">
      <archetypeName>prop_rub_carwreck_12</archetypeName>
      <position x="210.682" y="-788.6428" z="29.96371" />
      <rotation x="0" y="0" z="0.8191411" w="0.5735921" />
      <lodLevel>LODTYPES_DEPTH_ORPHANHD</lodLevel>
    </Item>
  </entities>
</CMapData>
```

## How it works

1. Reads the binary `.ymap` file (RSC7 container format)
2. If `--gta-path` is provided, scans all `.rpf` files using `RpfManager` to populate `JenkIndex`
3. If `--nametable` is provided, reads model names and feeds them into `JenkIndex.Ensure()`
4. Deserializes the internal `CMapData` structure using CodeWalker.Core
5. Serializes the structure to a standard XML document — hashes are resolved to names via `JenkIndex`
6. Writes the `.ymap.xml` output file

The tool does **not** need OpenIV, CodeWalker GUI, or any external dependencies — it uses the same Core library internally.

## Acknowledgments

- [CodeWalker](https://github.com/dexyfex/CodeWalker) by dexyfex — the engine behind this tool
- [CodeWalker.Core](https://www.nuget.org/packages/CodeWalker.Core/) NuGet package

## License

[MIT](LICENSE)
