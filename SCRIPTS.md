# EkkoJS Launch Scripts

## Available Scripts

### Development Scripts (uses `dotnet run`)
- **Linux/macOS**: `./ekko`
- **Windows CMD**: `ekko.cmd`
- **Windows PowerShell**: `./ekko.ps1`

These scripts compile and run EkkoJS on each execution. Good for development when code changes frequently.

### Fast Scripts (uses compiled binary)
- **Linux/macOS**: `./ekko-fast`
- **Windows**: `ekko-fast.cmd`

These scripts use the pre-compiled binary for faster startup. They automatically build if the binary doesn't exist.

## Usage Examples

```bash
# Show version
./ekko version

# Run a JavaScript file
./ekko run _jsTest/test.js

# Start REPL
./ekko repl

# Show help
./ekko help
```

## Windows Examples

```cmd
REM Using CMD
ekko run _jsTest\test.js
ekko-fast version

REM Using PowerShell
.\ekko.ps1 run _jsTest\test.js
```

## Adding to PATH

### Linux/macOS
```bash
# Add to your ~/.bashrc or ~/.zshrc
export PATH="$PATH:/path/to/ekkojs"
```

### Windows
Add the EkkoJS directory to your system PATH environment variable, then you can use `ekko` from anywhere.

## Performance Note

- `ekko` scripts: ~2-4 seconds startup (compiles on each run)
- `ekko-fast` scripts: ~100-200ms startup (uses pre-compiled binary)