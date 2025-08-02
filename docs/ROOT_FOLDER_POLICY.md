# EkkoJS Root Folder Policy

## Strict Root Directory Rules

When a project contains a `.ekko/` directory, it enters the EkkoJS ecosystem and must follow strict root folder rules. **No files should exist directly in the root folder** except those explicitly allowed.

## Default Allowed Files

Only these files are allowed in the root by default:

```
project/
├── .ekko/          # EkkoJS configuration directory
├── README.md       # Project documentation
├── LICENSE         # License file
├── .gitignore      # Git ignore file
├── .gitattributes  # Git attributes
└── [folders only]  # All other content must be in folders
```

## Enforcement and Warnings

### Validation Command

```bash
$ ekko validate

❌ Root folder compliance check failed:
   - Unexpected file: package.json (move to .ekko/ekko.json)
   - Unexpected file: index.js (move to src/)
   - Unexpected file: test.ts (move to tests/)
   - Unexpected file: config.yaml (move to .ekko/ or declare in exceptions)

✅ To fix: Run 'ekko fix-structure' to automatically reorganize files
```

### Runtime Warnings

```bash
$ ekko run src/main.ts

⚠️  Warning: Non-compliant root folder structure detected
   - Unexpected files in root: server.js, utils.ts
   - Run 'ekko validate' for details
   
[Continue running...]
```

## Configuring Exceptions

If you need additional files in the root, declare them in `.ekko/ekko.json`:

```json
{
  "name": "my-project",
  "version": "1.0.0",
  "type": "application",
  
  "rootPolicy": {
    "allowedFiles": [
      "README.md",        // Default
      "LICENSE",          // Default
      ".gitignore",       // Default
      ".gitattributes",   // Default
      ".editorconfig",    // Additional allowed
      "Dockerfile",       // Additional allowed
      ".env.example"      // Additional allowed
    ],
    "strict": true,       // Enable strict mode (default: true)
    "warnOnly": false     // Set true to warn instead of error
  }
}
```

## Automatic Structure Fixing

The `ekko fix-structure` command automatically reorganizes non-compliant projects:

```bash
$ ekko fix-structure

🔧 Fixing project structure...

Moving files:
  ✓ package.json → .ekko/ekko.json (converted)
  ✓ tsconfig.json → .ekko/tsconfig.json
  ✓ index.ts → src/index.ts
  ✓ utils.ts → src/utils.ts
  ✓ test.spec.ts → tests/test.spec.ts
  ✓ logo.png → assets/logo.png

Creating folders:
  ✓ Created src/
  ✓ Created tests/
  ✓ Created assets/

✅ Project structure fixed! 
   Run 'ekko validate' to confirm compliance.
```

## Migration Helper

For existing projects, use the migration command:

```bash
$ ekko init --migrate

🔍 Analyzing existing project structure...

Found:
  - Node.js project (package.json)
  - TypeScript configuration
  - 15 source files
  - 8 test files

📋 Migration plan:
  1. Create .ekko/ekko.json from package.json
  2. Move source files to src/
  3. Move test files to tests/
  4. Create .ekko/ configuration files

Proceed with migration? (y/n): y

✅ Migration complete!
```

## Validation Rules

### 1. File Rules

```typescript
// Default allowed files (case-sensitive)
const DEFAULT_ALLOWED_FILES = [
  'README.md',
  'LICENSE',
  'LICENSE.md',
  'LICENSE.txt',
  '.gitignore',
  '.gitattributes'
];

// Never allowed in root (must be in folders)
const NEVER_IN_ROOT = [
  '*.ts',
  '*.js',
  '*.tsx',
  '*.jsx',
  '*.css',
  '*.html',
  '*.json' // except .ekko/ekko.json
];
```

### 2. Folder Rules

```typescript
// Required folders (created if missing)
const REQUIRED_FOLDERS = [
  '.ekko',
  'src'
];

// Recommended folders (warning if missing)
const RECOMMENDED_FOLDERS = [
  'tests',
  'docs'
];
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: EkkoJS Validation
on: [push, pull_request]

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: ekkojs/validate-action@v1
        with:
          strict: true
          fix: false  # Don't auto-fix in CI
```

### Pre-commit Hook

```bash
#!/bin/sh
# .git/hooks/pre-commit

# Run EkkoJS validation
ekko validate --quiet

if [ $? -ne 0 ]; then
  echo "❌ Commit blocked: Project structure is not compliant"
  echo "   Run 'ekko validate' for details"
  echo "   Run 'ekko fix-structure' to fix automatically"
  exit 1
fi
```

## Benefits of Strict Root Policy

1. **Instant Recognition**: `.ekko/` immediately identifies an EkkoJS project
2. **Clean Structure**: No clutter in the root directory
3. **Consistency**: All EkkoJS projects look the same
4. **Tooling**: IDEs and tools know exactly where to find things
5. **Migration Path**: Easy to adopt with automated tools

## Examples

### ✅ Compliant Project

```
my-app/
├── .ekko/
│   ├── ekko.json
│   └── build.json
├── src/
│   └── main.ts
├── tests/
├── assets/
├── README.md
└── .gitignore
```

### ❌ Non-Compliant Project

```
my-app/
├── .ekko/
│   └── ekko.json
├── src/
├── index.ts          ❌ Should be in src/
├── package.json      ❌ Should be .ekko/ekko.json
├── config.yaml       ❌ Should be in .ekko/ or declared
├── utils.ts          ❌ Should be in src/
├── logo.png          ❌ Should be in assets/
└── README.md         ✅ Allowed
```