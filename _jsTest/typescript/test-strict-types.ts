// Test strict TypeScript with type definitions
import fs from 'ekko:fs';
import path, { ParsedPath } from 'ekko:path';

// This should have proper type inference
const testPath = path.join('test', 'dir', 'file.txt');
console.log('Test path:', testPath);

// Test parsed path type
const parsed: ParsedPath = path.parse(testPath);
console.log('Parsed path:', parsed);
console.log('Directory:', parsed.dir);
console.log('Extension:', parsed.ext);

// Test fs types
interface Config {
    name: string;
    version: number;
    features: string[];
}

const config: Config = {
    name: 'EkkoJS',
    version: 1,
    features: ['TypeScript', 'ES Modules', 'V8 Engine']
};

// Write typed data
fs.writeFileSync('config.json', JSON.stringify(config, null, 2));

// Read and parse with types
const rawData = fs.readFileSync('config.json');
const loadedConfig: Config = JSON.parse(rawData);

console.log('Loaded config:', loadedConfig);
console.log('Features:', loadedConfig.features.join(', '));

// Test type errors would be caught
// This would error in a real TypeScript compiler:
// fs.writeFileSync(123, 'test'); // Error: Argument of type 'number' is not assignable to parameter of type 'string'

// Clean up
fs.rmSync('config.json');
console.log('TypeScript with strict types working!');