@echo off
REM EkkoJS CLI launcher for Windows (using compiled binary)

REM Get the directory where this script is located
set SCRIPT_DIR=%~dp0

REM Path to the compiled binary
set EKKO_BIN=%SCRIPT_DIR%EkkoJS.CLI\bin\Debug\net9.0\EkkoJS.CLI.exe

REM Check if binary exists
if not exist "%EKKO_BIN%" (
    echo Building EkkoJS...
    dotnet build "%SCRIPT_DIR%EkkoJS.CLI\EkkoJS.CLI.csproj" -c Debug
)

REM Run the compiled binary with all arguments
"%EKKO_BIN%" %*