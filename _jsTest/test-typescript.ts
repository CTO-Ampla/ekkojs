// Test TypeScript with ES modules
import fs from "ekko:fs";
import path from "ekko:path";

interface FileInfo {
    name: string;
    size: number;
    isDirectory: boolean;
}

function getFileInfo(filePath: string): FileInfo {
    const stats = fs.statSync(filePath);
    return {
        name: path.basename(filePath),
        size: stats.size || 0,
        isDirectory: stats.isDirectory || false
    };
}

console.log("Testing TypeScript with ES modules...");

// Test TypeScript features
const testFile: string = "test-ts.txt";
fs.writeFileSync(testFile, "TypeScript ES module test");

const info: FileInfo = getFileInfo(testFile);
console.log("File info:", info);

// Clean up
fs.rmSync(testFile);
console.log("TypeScript test completed!");