iiNairyt
=========

C# library for interacting with files from the 1995 game Tyrian, developed by Eclipse Software.

## Usage

To edit a file you should instantiate the relevant class and call the `Read` method passing the filename.

```csharp
using ii.Nairyt;
using SixLabors.ImageSharp;

var exports = ShpProcessor.Read(@"D:\games\Tyrian\tyrian.SHP", @"D:\games\Tyrian\PALETTE.DAT");

var writtenFiles = new List<string>(exports.Count);

foreach (var export in exports)
{
    var outputPath = Path.Combine(@"X:\data\Tyrian\", $"{export.Name}.png");
    export.Image.SaveAsPng(outputPath);
    writtenFiles.Add(outputPath);
}
```

## Compiling

To clone and run this repository you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/iiNairyt

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

## Licencing

iiNairyt is licenced under the GPL 2.0 License. Full licence details are available in licence.md


## References
https://github.com/opentyrian/opentyrian