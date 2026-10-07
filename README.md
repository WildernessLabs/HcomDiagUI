# HcomDiagUI
This C# application is a WinForms GUI that allows the user to send commands
to HCOM. It was created because I needed a tool for trying new and different
protocol changes to HCOM without having to hack the Meadow CLI.
This tool will never be finished. And it is not yet able to send/receive all
HCOM protocol message types.

## Building
Targets .NET 10 (`net10.0-windows`). Open `HcomDiagUi.sln` in Visual Studio 2026 or
later, or run `dotnet build` from the repository root (this also works from
WSL/Linux, as a compile check only). The executable is written to
`bin/Debug/net10.0-windows/HcomDiagUi.exe` and needs the .NET 10 Desktop Runtime.

The form is laid out at 100% scaling and the app runs DPI-unaware (Program.cs).
The project forces the WinForms Designer to 100% as well
(`ForceDesignerDPIUnaware` in HcomDiagUi.csproj), so the layout is not re-scaled
when the form is saved on a scaled display; Visual Studio's notice about display
scaling can be ignored. If the app ever reports at startup that the form layout
was saved at another scaling, revert HcomDiagUi.Designer.cs and redo the edit.
