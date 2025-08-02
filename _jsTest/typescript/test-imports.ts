// TypeScript with ES modules
import fs from 'ekko:fs';
import { join, dirname } from 'ekko:path';

interface FileInfo {
    path: string;
    content: string;
}

function writeFile(info: FileInfo): void {
    const dir = dirname(info.path);
    console.log(`Writing to directory: ${dir}`);
    fs.writeFileSync(info.path, info.content);
}

function readFile(path: string): string {
    return fs.readFileSync(path);
}

// Test the functions
const testFile: FileInfo = {
    path: join("test", "typescript-test.txt"),
    content: "Hello from TypeScript!"
};

console.log("Testing TypeScript with ekko modules...");

// Create test directory
fs.mkdirSync("test");

// Write file
writeFile(testFile);

// Read it back
const content = readFile(testFile.path);
console.log("Read content:", content);

// Clean up
fs.rmSync("test", { recursive: true });
console.log("Test completed successfully!");