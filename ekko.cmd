@echo off
REM EkkoJS CLI launcher for Windows

REM Get the directory where this script is located
set SCRIPT_DIR=%~dp0

REM Run the CLI with all arguments passed through
dotnet run --project "%SCRIPT_DIR%EkkoJS.CLI\EkkoJS.CLI.csproj" -- %*