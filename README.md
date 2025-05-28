# GhidraZipCleaner
A small program to "clean" potentially coprighted ROM information from .gzf files.

This can be used so .gzf files for decompiling games can be safely shared without distributing copyrighted material.

## Instructions
### GUI mode (Windows only)
1. Download and run the latest <a href="https://github.com/GrasonHumphrey/GhidraZipCleaner/tree/master/Release">release</a> (without _CLI).
2. Select the import/export tab depending on what function you want, select the .gzf or .cgzf, and the ROM corresponding to it.
3. Import/Export.  The "cleaned" .cgzf can only be imported with an identical ROM file as the one used to export it.
4. The generated .cgzf can now be safely shared, or the generated .gzf can be imported to Ghidra.

### CLI mode (Windows only)
1. Download and run the latest <a href="https://github.com/GrasonHumphrey/GhidraZipCleaner/tree/master/Release">_CLI release</a>.
2. Run from command line with the following syntax: GhidraZipCleaner_CLI_202xxxxx.exe [import/export] [GZF/CGZF file location] [ROM file location] [output folder]