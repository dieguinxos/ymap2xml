# ymap2xml

Convert binary GTA V / FiveM `.ymap` files to human-readable `.xml` — no OpenIV or CodeWalker GUI required.

## Features

- Converts individual `.ymap` files or entire directories at once
- Uses [CodeWalker.Core](https://www.nuget.org/packages/CodeWalker.Core/) — the same engine behind the official CodeWalker tool
- No GUI needed — run it from the terminal, integrate it into scripts
- Valid XML output with all entities, positions, rotations, LOD data, and metadata
- Handles corrupted files gracefully without stopping batch processing

## Usage

```bash
ymap2xml.exe <path> [--outdir <dir>]
```

| Argument | Description |
|----------|-------------|
| `<path>` | Path to a `.ymap` file or a folder containing `.ymap` files |
| `--outdir` | Optional output directory (default: same folder as the source) |

### Examples

```bash
# Convert a single file — saves next to the source
ymap2xml.exe city.ymap

# Convert an entire stream folder — saves alongside the .ymap files
ymap2xml.exe resources\[mapas]\my_map\stream\

# Convert and output to a separate directory
ymap2xml.exe stream\ --outdir xml\

# batch processing (PowerShell)
Get-ChildItem -Recurse -Filter *.ymap | ForEach-Object {
    ymap2xml.exe $_.FullName --outdir "C:\ymap_xml"
}
```

## Installation

### Download (easiest)

Download the latest `ymap2xml.exe` from the [Releases](https://github.com/dieguinxo/ymap2xml/releases) page. No installation required.

### Build from source

Requires [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

```bash
git clone https://github.com/dieguinxo/ymap2xml.git
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
  <name>Main_Road</name>
  <streamingExtentsMin x="-3665.4072" y="-778.2665" z="-480.1723" />
  <streamingExtentsMax x="-1783.6318" y="2648.4155" z="551.0064" />
  <entities>
    <Item type="CEntityDef">
      <archetypeName>hash_F5AF9D4A</archetypeName>
      <position x="-2221.0586" y="-344.7843" z="12.281154" />
      <rotation x="0" y="-0" z="-0.78420985" w="0.6204957" />
      <lodLevel>LODTYPES_DEPTH_ORPHANHD</lodLevel>
    </Item>
  </entities>
</CMapData>
```

## How it works

1. Reads the binary `.ymap` file (RSC7 container format)
2. Deserializes the internal `CMapData` structure using CodeWalker.Core
3. Serializes the structure to a standard XML document
4. Writes the `.ymap.xml` output file

The tool does **not** need OpenIV, CodeWalker GUI, or any external dependencies — it uses the same Core library internally.

## Acknowledgments

- [CodeWalker](https://github.com/dexyfex/CodeWalker) by dexyfex — the engine behind this tool
- [CodeWalker.Core](https://www.nuget.org/packages/CodeWalker.Core/) NuGet package

## License

[MIT](LICENSE)
