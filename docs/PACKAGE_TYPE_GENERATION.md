# Type Generation for JavaScript Packages

## Overview

EkkoJS needs to provide TypeScript type definitions for all packages, including those written in pure JavaScript. This document outlines strategies for generating types.

## TypeScript Packages (Easy Case)

For packages written in TypeScript, type generation is straightforward:

```json
// .ekko/ekko.json
{
  "name": "@mycompany/utils",
  "type": "package",
  "main": "src/index.ts",
  "build": {
    "generateTypes": true,  // Generate .d.ts files
    "typeOutput": "types/"  // Where to put them
  }
}
```

During package build:
```bash
$ ekko build package
✓ Compiling TypeScript...
✓ Generating type declarations...
✓ Embedding types/index.d.ts
✓ Package built: utils.dll
```

## JavaScript Packages (Complex Case)

For JavaScript packages, we have several options:

### Option 1: JSDoc Type Inference

JavaScript with JSDoc comments can generate decent types:

```javascript
// src/math.js

/**
 * Adds two numbers together
 * @param {number} a - First number
 * @param {number} b - Second number
 * @returns {number} The sum
 */
export function add(a, b) {
  return a + b;
}

/**
 * @typedef {Object} Point
 * @property {number} x - X coordinate
 * @property {number} y - Y coordinate
 */

/**
 * Calculate distance between two points
 * @param {Point} p1 - First point
 * @param {Point} p2 - Second point
 * @returns {number} Distance
 */
export function distance(p1, p2) {
  return Math.sqrt((p2.x - p1.x) ** 2 + (p2.y - p1.y) ** 2);
}
```

TypeScript compiler can generate:
```typescript
// Generated: types/math.d.ts
export function add(a: number, b: number): number;

export interface Point {
  x: number;
  y: number;
}

export function distance(p1: Point, p2: Point): number;
```

### Option 2: Type Inference from Code

TypeScript can infer types from JavaScript code structure:

```javascript
// src/user.js
export class User {
  constructor(name, email) {
    this.name = name;
    this.email = email;
    this.createdAt = new Date();
  }

  updateEmail(newEmail) {
    this.email = newEmail;
    this.updatedAt = new Date();
    return this;
  }

  toJSON() {
    return {
      name: this.name,
      email: this.email,
      createdAt: this.createdAt.toISOString()
    };
  }
}
```

TypeScript inference generates:
```typescript
// Generated: types/user.d.ts
export class User {
  constructor(name: any, email: any);
  name: any;
  email: any;
  createdAt: Date;
  updatedAt: Date | undefined;
  updateEmail(newEmail: any): this;
  toJSON(): {
    name: any;
    email: any;
    createdAt: string;
  };
}
```

### Option 3: Manual Type Annotations

Provide separate `.d.ts` files alongside JavaScript:

```
my-js-package/
├── .ekko/
│   └── ekko.json
├── src/
│   ├── index.js
│   └── utils.js
└── types/              # Manual type definitions
    ├── index.d.ts
    └── utils.d.ts
```

### Option 4: Hybrid Approach (Recommended)

Combine JSDoc with manual overrides:

```json
// .ekko/ekko.json
{
  "name": "@mycompany/js-lib",
  "type": "package",
  "main": "src/index.js",
  "types": {
    "generate": true,
    "strategy": "jsdoc",      // jsdoc | inference | manual
    "input": "src/**/*.js",
    "output": "types/",
    "overrides": "types/overrides.d.ts",
    "tsconfig": {
      "allowJs": true,
      "checkJs": true,
      "declaration": true,
      "emitDeclarationOnly": true,
      "strict": false
    }
  }
}
```

## EkkoJS Type Generation Pipeline

### 1. Analysis Phase

```typescript
class TypeGenerator {
  async analyzePackage(packagePath: string): Promise<TypeInfo> {
    const files = await this.findJavaScriptFiles(packagePath);
    const analysis = {
      hasJSDoc: false,
      hasTypeScriptConfig: false,
      hasManualTypes: false,
      inferenceQuality: 'unknown'
    };

    for (const file of files) {
      const content = await fs.readFile(file, 'utf8');
      
      // Check for JSDoc
      if (/@param|@returns|@typedef/.test(content)) {
        analysis.hasJSDoc = true;
      }
      
      // Analyze code patterns
      analysis.inferenceQuality = this.assessInferenceQuality(content);
    }
    
    return analysis;
  }
}
```

### 2. Generation Strategies

```typescript
interface GenerationStrategy {
  generate(files: string[]): Promise<TypeFiles>;
}

class JSDocStrategy implements GenerationStrategy {
  async generate(files: string[]): Promise<TypeFiles> {
    // Use TypeScript compiler API with JSDoc parsing
    const program = ts.createProgram(files, {
      allowJs: true,
      declaration: true,
      emitDeclarationOnly: true,
      checkJs: true
    });
    
    // Emit .d.ts files
    return this.emitDeclarations(program);
  }
}

class InferenceStrategy implements GenerationStrategy {
  async generate(files: string[]): Promise<TypeFiles> {
    // Use TypeScript's type inference
    const program = ts.createProgram(files, {
      allowJs: true,
      declaration: true,
      emitDeclarationOnly: true,
      strict: false,  // Less strict for inference
      noImplicitAny: false
    });
    
    return this.emitDeclarations(program);
  }
}

class HybridStrategy implements GenerationStrategy {
  async generate(files: string[]): Promise<TypeFiles> {
    // 1. Generate from JSDoc
    const jsdocTypes = await new JSDocStrategy().generate(files);
    
    // 2. Generate from inference
    const inferredTypes = await new InferenceStrategy().generate(files);
    
    // 3. Merge intelligently
    return this.mergeTypes(jsdocTypes, inferredTypes);
  }
}
```

### 3. Type Quality Assessment

```typescript
class TypeQualityAnalyzer {
  assess(generatedTypes: string): QualityReport {
    const report = {
      score: 0,
      issues: [],
      suggestions: []
    };
    
    // Check for 'any' types
    const anyCount = (generatedTypes.match(/: any/g) || []).length;
    if (anyCount > 0) {
      report.issues.push(`Found ${anyCount} 'any' types`);
      report.score -= anyCount * 5;
    }
    
    // Check for missing return types
    const missingReturns = (generatedTypes.match(/\): void/g) || []).length;
    
    // Provide suggestions
    if (report.score < 50) {
      report.suggestions.push('Consider adding JSDoc comments');
      report.suggestions.push('Use TypeScript for better type safety');
    }
    
    return report;
  }
}
```

## Package Build Integration

```bash
$ ekko build package

📦 Building JavaScript package...

📝 Type Generation:
   Strategy: JSDoc + Inference
   Files analyzed: 12
   
⚠️  Type Quality Report:
   Score: 65/100
   Issues:
   - Found 8 'any' types
   - Missing JSDoc for 3 exported functions
   
   Suggestions:
   - Add @param and @returns to: calculateTotal, processUser
   - Consider TypeScript migration for complex types

✓ Generated types/index.d.ts
✓ Package built with type definitions

💡 Tip: Run 'ekko types improve' for interactive type improvement
```

## Interactive Type Improvement

```bash
$ ekko types improve

🔍 Type Improvement Assistant

Found: export function processUser(data) { ... }

Inferred type: function processUser(data: any): any

Suggested JSDoc:
/**
 * @param {Object} data - User data
 * @param {string} data.name - User name
 * @param {string} data.email - User email
 * @returns {User} Processed user object
 */

Accept suggestion? (y/n/edit): y
✓ Updated src/user.js

Next: export function calculateTotal(items) { ... }
```

## Type Definition Priority System

EkkoJS uses a priority system for type definitions:

### Priority Order (highest to lowest)

1. **Manual type definitions** (`types/` folder)
2. **Generated + overrides** (hybrid approach)
3. **Pure generated** (JSDoc/inference)
4. **No types** (JavaScript without types)

### Configuration Examples

#### Example 1: Manual Types Only
```json
// .ekko/ekko.json
{
  "name": "@mycompany/complex-lib",
  "type": "package",
  "main": "src/index.js",
  "types": {
    "source": "manual",
    "path": "types/index.d.ts"
  }
}
```

Package structure:
```
complex-lib/
├── .ekko/
│   └── ekko.json
├── src/
│   └── index.js         # JavaScript implementation
├── types/
│   ├── index.d.ts      # Manual type definitions
│   ├── utils.d.ts      # Additional types
│   └── internal.d.ts   # Internal type definitions
└── README.md
```

#### Example 2: Generated with Manual Overrides
```json
{
  "types": {
    "source": "hybrid",
    "generate": {
      "from": "src/**/*.js",
      "strategy": "jsdoc"
    },
    "overrides": [
      "types/overrides.d.ts",
      "types/complex-types.d.ts"
    ]
  }
}
```

#### Example 3: Augmenting Generated Types
```typescript
// types/overrides.d.ts

// Augment the generated module
declare module '@mycompany/complex-lib' {
  // Fix function that was poorly inferred
  export function complexFunction<T extends object>(
    data: T,
    options?: ComplexOptions
  ): Promise<ProcessedData<T>>;
  
  // Add missing interface
  export interface ComplexOptions {
    timeout?: number;
    retries?: number;
    transform?: (data: any) => any;
  }
  
  // Fix class that was inferred as 'any'
  export class DataProcessor<T = any> {
    constructor(config: ProcessorConfig);
    process(input: T): Promise<T>;
    validate(input: unknown): input is T;
  }
}
```

## Package Build Type Resolution

During build, EkkoJS resolves types in this order:

```typescript
class TypeResolver {
  async resolveTypes(packageConfig: PackageConfig): Promise<TypeBundle> {
    const { types } = packageConfig;
    
    switch (types.source) {
      case 'manual':
        // Use only manual types
        return this.loadManualTypes(types.path);
        
      case 'generated':
        // Use only generated types
        return this.generateTypes(types.generate);
        
      case 'hybrid':
        // Generate + apply overrides
        const generated = await this.generateTypes(types.generate);
        const overrides = await this.loadOverrides(types.overrides);
        return this.mergeTypes(generated, overrides);
        
      case 'none':
        // No types provided
        return null;
        
      default:
        // Auto-detect best approach
        return this.autoDetectTypes(packageConfig);
    }
  }
}
```

## Type Validation

EkkoJS validates provided types match the implementation:

```bash
$ ekko build package

📦 Building package with manual types...

🔍 Type validation:
   ✓ All exports have type definitions
   ✓ Type signatures match JSDoc comments
   ⚠️  Warning: Function 'processData' returns Promise but type says object
   
❓ Continue with warnings? (y/n): y
```

## Best Practices

### For JavaScript Package Authors

1. **Try auto-generation first**
   ```bash
   ekko types generate --preview
   ```

2. **Provide manual types for complex APIs**
   ```typescript
   // types/index.d.ts
   export function complexAPI<T>(
     data: T,
     options?: Options
   ): Promise<Result<T>>;
   ```

3. **Use hybrid approach for best results**
   - Let EkkoJS generate basic types
   - Override complex ones manually

4. **Test your types**
   ```typescript
   // types/test.ts
   import { complexAPI } from './index';
   
   // This should compile without errors
   const result = await complexAPI({ foo: 'bar' }, {
     timeout: 1000
   });
   ```

### Type Definition Examples

#### Simple Manual Types
```typescript
// types/index.d.ts
export interface User {
  id: string;
  name: string;
  email: string;
}

export function getUser(id: string): Promise<User>;
export function createUser(data: Partial<User>): Promise<User>;
export function updateUser(id: string, data: Partial<User>): Promise<User>;
```

#### Complex Generic Types
```typescript
// types/index.d.ts
export interface Cache<T = any> {
  get<K extends keyof T>(key: K): Promise<T[K] | undefined>;
  set<K extends keyof T>(key: K, value: T[K]): Promise<void>;
  has(key: keyof T): Promise<boolean>;
  clear(): Promise<void>;
}

export function createCache<T>(options?: CacheOptions): Cache<T>;
```

## Conclusion

EkkoJS can generate types for JavaScript packages through:
- **JSDoc parsing** (best for documented code)
- **Type inference** (works for simple patterns)
- **Hybrid approach** (recommended)
- **Manual overrides** (for complex cases)

The build system automatically chooses the best strategy and provides quality feedback to improve type coverage.