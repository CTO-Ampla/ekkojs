#!/bin/bash

# Build script for mathlib

echo "Building mathlib..."

# Detect OS
OS="$(uname -s)"
case "${OS}" in
    Linux*)     
        echo "Building for Linux..."
        gcc -Wall -Wextra -fPIC -O2 -DMATHLIB_EXPORTS -shared -o libmathlib.so mathlib.c -lm
        echo "Built: libmathlib.so"
        ;;
    Darwin*)    
        echo "Building for macOS..."
        gcc -Wall -Wextra -fPIC -O2 -DMATHLIB_EXPORTS -dynamiclib -o libmathlib.dylib mathlib.c -lm
        echo "Built: libmathlib.dylib"
        ;;
    MINGW*|MSYS*|CYGWIN*)
        echo "Building for Windows (MinGW)..."
        gcc -Wall -Wextra -O2 -DMATHLIB_EXPORTS -shared -o mathlib.dll mathlib.c -lm
        echo "Built: mathlib.dll"
        ;;
    *)
        echo "Unknown OS: ${OS}"
        exit 1
        ;;
esac

# Cross-compile for Windows if mingw is available
if command -v x86_64-w64-mingw32-gcc &> /dev/null; then
    echo "Cross-compiling for Windows x64..."
    x86_64-w64-mingw32-gcc -Wall -Wextra -O2 -DMATHLIB_EXPORTS -shared -o mathlib.dll mathlib.c -lm
    echo "Built: mathlib.dll (Windows x64)"
fi

echo "Build complete!"