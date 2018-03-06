# i3blocks-multicore
A multicore CPU monitor blocklet for i3blocks.

## Installation
1. Install F#
2. Clone this repository
3. run `fsharpc multicore.fsx` obtaining multicore.exe and a copy of FSharp.Core.dll.
4. Add to your i3blocks config. You will need to enable pango markup, with the `markup=pango` property. If your shell automatically executes .exe files with mono, then you can place multicore.exe and FSharp.Core.dll in your i3blocks script folder, and the blocklet will work if the title is `[multicore.exe]`. Otherwise, use the `command=mono multicore.exe`, replacing the paths for both mono and multicore.exe as necessary.
