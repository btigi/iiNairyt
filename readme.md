iiSingleMve
=========

C# library for interacting with Interplay MVE files, as used in Baldur's Gate, Descent, M.A.X and more.

## Usage

Install the [nuget package](https://www.nuget.org/packages/ii.SingleMve/) e.g.

`dotnet add package ii.SingleMve`

To edit a file you should instantiate the relevant class and call the `Read` method passing the filename. This will return an object model, which you can amend, before calling the `Write` method.

```csharp
var mveProcessor = new MveProcessor();
var avi = mveProcessor.Read(@"D:\data\movie.mve");
File.WriteAllBytes(@"D:\dat\movie.avi", avi);
```

## Compiling

To clone and run this repository you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/iiSingleMve

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

## Licencing

iiSingleMve is licenced under the GPL 2.0 License. Full licence details are available in licence.md


## References
https://github.com/DescentNetwork/libmve-lgpl2.1
https://github.com/gemrb/gemrb/