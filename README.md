# HcomDiagUI
This C# application is a WinForms GUI that allows the user to send commands
to HCOM. It was created because I needed a tool for trying new and different
protocol changes to HCOM without having to hack the Meadow CLI.
This tool will never be finished. And it is not yet able to send/receive all
HCOM protocol message types.

## Building
Open `HcomDiagUi.sln` in Visual Studio 2022 or later, or run `dotnet build` from the
repository root (this also works from WSL/Linux, as a compile check only).
The executable is written to `bin/Debug/net472/HcomDiagUi.exe`.
