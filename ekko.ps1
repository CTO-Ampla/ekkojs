# EkkoJS CLI launcher for Windows PowerShell

# Get the directory where this script is located
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Run the CLI with all arguments passed through
& dotnet run --project "$scriptDir\EkkoJS.CLI\EkkoJS.CLI.csproj" -- $args