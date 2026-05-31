iiNairyt
=========

C# library for interacting with files from the 1995 game Tyrian, developed by Eclipse Software.

| Name   | Read | Write | Comment
|--------|:----:|-------|--------
| DAT    | ✔   |   ✗   | Graphics
| DAT    | ✔   |   ✗   | Text
| LVL    | ✔   |   ✗   | Level
| PCX    | ✗   |   ✗   | 
| PIC    | ✗   |   ✗   | 
| SAV    | ✗   |   ✗   | 
| SHP    | ✔   |   ✗   | Graphics
| SND    | ✗   |   ✗   | 

## Usage

To edit a file you should instantiate the relevant class and call the `Read` method passing the filename.

```csharp
using ii.Nairyt;
using SixLabors.ImageSharp;

// encrypted text
var txt = DatProcessor.Read(@"D:\games\tyrian\cubetxt1.dat");


// Levels
var graphics = new List<string>() {
@"D:\games\tyrian\shapes).dat",
@"D:\games\tyrian\shapesw.dat",
@"D:\games\tyrian\shapesx.dat",
@"D:\games\tyrian\shapesy.dat",
@"D:\games\tyrian\shapesz.dat" };

var levels = LvlProcessor.Read(@"D:\games\tyrian\tyrian.lvl", @"D:\games\tyrian\palette.dat", graphics);
foreach (var level in levels)
{
    File.WriteAllText($@"D:\data\Tyrian\{level.Name}.tmx", level.Content);

    if (level.Tileset is not null)
    {
        level.Tileset.Image.SaveAsPng($@"D:\data\Tyrian\{level.Tileset.Name}.png");
    }
}


// Graphics
var exports = ShpProcessor.Read(@"D:\games\tyrian\tyrian.shp", @"D:\games\tyrian\palette.dat");
var writtenFiles = new List<string>(exports.Count);
foreach (var export in exports)
{
    var outputPath = Path.Combine(@"X:\data\tyrian\", $"{export.Name}.png");
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