iiNairyt
=========

C# library for interacting with files from the 1995 game Tyrian, developed by Eclipse Software.

| Name   | Read | Write | Comment
|--------|:----:|-------|--------
| CDT    | ✔   |   ✔   | Encrypted text
| DAT    | ✔   |   ✗   | Graphics
| DAT    | ✔   |   ✔   | Encrypted text
| HDT    | ✔   |   ✔   | Configuration info
| LVL    | ✔   |   ✗   | Level
| MUS    | ✗   |   ✗   | Music
| PCX    | ✗   |   ✗   | 
| PIC    | ✔   |   ✗   | Fullscreen backgrounds
| SAV    | ✗   |   ✗   | 
| SHP    | ✔   |   ✔   | Graphics
| SND    | ✔   |   ✗   | 

## Usage

To edit a file, call the relevant processor's `Read` method, change the result, and pass it to `Write`. `ShpProcessor.Export` turns a loaded SHP file into images.

```csharp
using ii.Nairyt;
using ii.Nairyt.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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


// Fullscreen backgrounds (13 images in tyrian.pic)
var pics = PicProcessor.Read(@"D:\games\tyrian\tyrian.pic", @"D:\games\tyrian\palette.dat");
foreach (var pic in pics)
{
    pic.Image.SaveAsPng($@"D:\data\Tyrian\{pic.Name}.png");
}

// Graphics
var shp = ShpProcessor.Read(@"D:\games\tyrian\tyrian.shp");
var exports = ShpProcessor.Export(shp, @"D:\games\tyrian\palette.dat", "tyrian");
var writtenFiles = new List<string>(exports.Count);
foreach (var export in exports)
{
    var outputPath = Path.Combine(@"X:\data\tyrian\", $"{export.Name}.png");
    export.Image.SaveAsPng(outputPath);
    writtenFiles.Add(outputPath);
}

var shp = ShpProcessor.Read(@"D:\games\Tyrian\tyrian.shp");
var palette = TyrianPalette.Load(@"D:\games\Tyrian\palette.dat");
using var edited = Image.Load<Rgba32>(@"D:\data\Tyrian\tyrian-0-0.png");

const int bankIndex = 0;
const int spriteIndex = 0;

var banks = shp.Banks.ToArray();
var sprites = banks[bankIndex].Sprites.ToArray();
sprites[spriteIndex] = ShpSprite.FromRgba(edited, palette);
banks[bankIndex] = banks[bankIndex] with { Sprites = sprites };
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