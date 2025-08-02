# EkkoJS Folder Categories

## Standard Folder Categories

EkkoJS recognizes these folder categories, each serving a specific purpose:

### 🎯 Core Categories (Most Common)

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `.ekko/` | config | EkkoJS configuration files | ✅ Yes |
| `src/` | source | Source code (TS/JS) | ✅ Yes |
| `tests/` | testing | Unit and integration tests | ❌ No |
| `assets/` | assets | Static resources (images, data, templates) | ❌ No |
| `docs/` | documentation | Project documentation | ❌ No |

### 🔧 Build & Development

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `native/` | native | Platform-specific native libraries | ❌ No |
| `scripts/` | scripts | Build, deploy, and utility scripts | ❌ No |
| `tools/` | tools | Development tools and utilities | ❌ No |
| `dist/` | output | Build output (compiled/bundled files) | ❌ No |
| `build/` | output | Alternative build output directory | ❌ No |
| `generated/` | generated | Auto-generated code | ❌ No |

### 📦 Dependencies & Packages

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `packages/` | packages | Local packages (monorepo) | ❌ No |
| `libs/` | libraries | Third-party libraries (non-npm) | ❌ No |
| `vendor/` | vendor | Vendored dependencies | ❌ No |
| `modules/` | modules | Custom module collections | ❌ No |

### 🚀 Deployment & CI/CD

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `.github/` | ci | GitHub Actions workflows | ❌ No |
| `.gitlab/` | ci | GitLab CI configuration | ❌ No |
| `.circleci/` | ci | CircleCI configuration | ❌ No |
| `ci/` | ci | Generic CI/CD scripts | ❌ No |
| `deploy/` | deployment | Deployment configurations | ❌ No |
| `docker/` | containerization | Docker-related files | ❌ No |
| `k8s/` | orchestration | Kubernetes manifests | ❌ No |
| `terraform/` | infrastructure | Infrastructure as Code | ❌ No |

### 🎨 Frontend/UI Specific

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `public/` | public | Static files served as-is | ❌ No |
| `static/` | static | Static assets (alternative to public) | ❌ No |
| `styles/` | styles | CSS/SCSS/style files | ❌ No |
| `components/` | components | UI components | ❌ No |
| `pages/` | pages | Page components/routes | ❌ No |
| `layouts/` | layouts | Layout templates | ❌ No |
| `themes/` | themes | Theme definitions | ❌ No |

### 📊 Data & Content

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `data/` | data | Data files (JSON, CSV, etc.) | ❌ No |
| `content/` | content | Content files (markdown, etc.) | ❌ No |
| `locales/` | localization | i18n translation files | ❌ No |
| `i18n/` | localization | Alternative i18n directory | ❌ No |
| `db/` | database | Database scripts/migrations | ❌ No |
| `migrations/` | database | Database migrations | ❌ No |
| `seeds/` | database | Database seed data | ❌ No |

### 🔌 API & Services

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `api/` | api | API definitions/implementations | ❌ No |
| `services/` | services | Service layer code | ❌ No |
| `controllers/` | controllers | API/MVC controllers | ❌ No |
| `routes/` | routes | Route definitions | ❌ No |
| `middleware/` | middleware | Middleware functions | ❌ No |
| `graphql/` | graphql | GraphQL schemas/resolvers | ❌ No |

### 🧪 Testing & Quality

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `__tests__/` | testing | Jest-style test directory | ❌ No |
| `spec/` | testing | Specification tests | ❌ No |
| `e2e/` | testing | End-to-end tests | ❌ No |
| `integration/` | testing | Integration tests | ❌ No |
| `unit/` | testing | Unit tests | ❌ No |
| `fixtures/` | testing | Test fixtures and mocks | ❌ No |
| `coverage/` | testing | Code coverage reports | ❌ No |
| `benchmarks/` | performance | Performance benchmarks | ❌ No |

### 🛠️ Configuration & Settings

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `config/` | config | Application configuration | ❌ No |
| `settings/` | config | Alternative config directory | ❌ No |
| `environments/` | config | Environment-specific configs | ❌ No |
| `.vscode/` | ide | VS Code settings | ❌ No |
| `.idea/` | ide | JetBrains IDE settings | ❌ No |

### 📚 Resources & Examples

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `examples/` | examples | Usage examples | ❌ No |
| `samples/` | examples | Sample code/data | ❌ No |
| `templates/` | templates | File/project templates | ❌ No |
| `resources/` | resources | Generic resources | ❌ No |

### 🔐 Security & Keys

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `keys/` | security | Encryption keys (git-ignored) | ❌ No |
| `certs/` | security | SSL certificates | ❌ No |
| `secrets/` | security | Secret files (git-ignored) | ❌ No |

### 📱 Platform-Specific

| Folder | Category | Purpose | Required |
|--------|----------|---------|----------|
| `android/` | platform | Android-specific code | ❌ No |
| `ios/` | platform | iOS-specific code | ❌ No |
| `windows/` | platform | Windows-specific code | ❌ No |
| `linux/` | platform | Linux-specific code | ❌ No |
| `macos/` | platform | macOS-specific code | ❌ No |

## Configuration in ekko.json

You can declare your folder structure and their categories:

```json
{
  "name": "my-app",
  "type": "application",
  
  "structure": {
    // Override default folder names
    "source": "src",           // default: src
    "assets": "assets",        // default: assets
    "tests": "tests",          // default: tests
    "output": "dist",          // default: dist
    
    // Declare additional folders and their categories
    "folders": {
      "api": {
        "category": "api",
        "description": "REST API endpoints"
      },
      "shared": {
        "category": "source",
        "description": "Shared utilities"
      },
      "infra": {
        "category": "infrastructure",
        "description": "Infrastructure code"
      },
      ".aws": {
        "category": "deployment",
        "description": "AWS configurations"
      }
    }
  }
}
```

## Category Rules

### 1. Source Code Categories
- `source` - Application/package source code
- `generated` - Auto-generated source code
- `vendor` - Third-party source code

### 2. Resource Categories
- `assets` - Static resources
- `public` - Publicly served files
- `data` - Data files
- `content` - Content files

### 3. Configuration Categories
- `config` - Configuration files
- `ide` - IDE-specific settings
- `security` - Security-related files

### 4. Build/Deploy Categories
- `output` - Build output
- `ci` - CI/CD configurations
- `deployment` - Deployment configs
- `infrastructure` - IaC files

### 5. Testing Categories
- `testing` - Test files
- `performance` - Performance tests
- `fixtures` - Test data

## Validation

EkkoJS can validate folder categories:

```bash
$ ekko validate --folders

✅ Folder structure validation:
   - src/ (source) ✓
   - tests/ (testing) ✓
   - assets/ (assets) ✓
   - api/ (unknown) ⚠️  Add to structure.folders in ekko.json
   
⚠️  Unknown folders detected. Define them in ekko.json
```

## Best Practices

1. **Use standard names** - Prefer `tests/` over `test/`
2. **Declare custom folders** - Add them to `structure.folders`
3. **Group by category** - Keep similar folders together
4. **Avoid deep nesting** - Maximum 3 levels recommended
5. **Git-ignore output** - Don't commit `dist/`, `coverage/`, etc.