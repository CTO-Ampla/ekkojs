# Cross-platform Makefile for mathlib

CC = gcc
CFLAGS = -Wall -Wextra -fPIC -O2
LDFLAGS = -shared

# Platform detection
UNAME_S := $(shell uname -s)
ifeq ($(UNAME_S),Linux)
    TARGET = libmathlib.so
    CFLAGS += -DMATHLIB_EXPORTS
endif
ifeq ($(UNAME_S),Darwin)
    TARGET = libmathlib.dylib
    CFLAGS += -DMATHLIB_EXPORTS
    LDFLAGS += -dynamiclib
endif

# Windows cross-compilation
MINGW_CC = x86_64-w64-mingw32-gcc
MINGW_CFLAGS = -Wall -Wextra -O2 -DMATHLIB_EXPORTS
MINGW_LDFLAGS = -shared -Wl,--out-implib,libmathlib.dll.a

# Source files
SOURCES = mathlib.c
OBJECTS = $(SOURCES:.c=.o)
MINGW_OBJECTS = $(SOURCES:.c=.obj)

# Default target
all: $(TARGET)

# Linux/macOS shared library
$(TARGET): $(OBJECTS)
	$(CC) $(LDFLAGS) -o $@ $^ -lm

%.o: %.c
	$(CC) $(CFLAGS) -c -o $@ $<

# Windows DLL (cross-compiled from Linux)
windows: mathlib.dll

mathlib.dll: $(MINGW_OBJECTS)
	$(MINGW_CC) $(MINGW_LDFLAGS) -o $@ $^ -lm

%.obj: %.c
	$(MINGW_CC) $(MINGW_CFLAGS) -c -o $@ $<

# Build all platforms
all-platforms: $(TARGET) mathlib.dll

# Clean
clean:
	rm -f $(OBJECTS) $(MINGW_OBJECTS) $(TARGET) mathlib.dll libmathlib.dll.a

# Install (Linux/macOS)
install: $(TARGET)
	@echo "Installing $(TARGET) to /usr/local/lib"
	@sudo cp $(TARGET) /usr/local/lib/
	@sudo ldconfig 2>/dev/null || true

.PHONY: all windows all-platforms clean install