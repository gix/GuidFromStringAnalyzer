# GuidFromStringAnalyzer

## Overview

*GuidFromStringAnalyzer* is a small [Roslyn](https://github.com/dotnet/roslyn) analyzer that detects instantiations of
`System.Guid` with a string literal:

```cs
new Guid("26B5DB35-C401-4DE3-B3D8-203FF9A0CA91");
```

and provides the `Construct from integers` fix-it to change them to the more efficient constructor using integers:
​
```cs
new Guid(0x26B5DB35, 0xC401, 0x4DE3, 0xB3, 0xD8, 0x20, 0x3F, 0xF9, 0xA0, 0xCA, 0x91);
```
​
The latter is directly embedded into the resulting assembly as a simple byte-array and requires no run-time parsing of the
GUID string. Roslyn currently does not optimize the string literals away on its own, see
[the relevant issue](https://github.com/dotnet/roslyn/issues/1387).
​
## Installing
​
Download the `GuidFromStringAnalyzer` Visual Studio extension from the
[VS Marketplace](https://marketplace.visualstudio.com/items?itemName=nrieck.GuidFromStringAnalyzer).
​
## License
​
Code licensed under the [MIT License](LICENSE.txt).
