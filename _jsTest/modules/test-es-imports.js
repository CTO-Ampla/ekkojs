// Test ES module imports
import fs from 'ekko:fs';
import path from 'ekko:path';

console.log('Testing ES module imports...');

// Test ekko:fs module
console.log('\nTesting ekko:fs module...');
console.log('Writing test file...');
fs.writeFileSync('test-es.txt', 'Hello from EkkoJS ES modules!');

console.log('Reading test file...');
const content = fs.readFileSync('test-es.txt');
console.log('Content:', content);

// Test ekko:path module
console.log('\nTesting ekko:path module...');
const filePath = path.join('folder', 'subfolder', 'file.js');
console.log('Joined path:', filePath);

const parsed = path.parse('/home/user/documents/file.txt');
console.log('Parsed path:', JSON.stringify(parsed, null, 2));

console.log('Extension:', path.extname('script.js'));
console.log('Basename:', path.basename('/path/to/file.txt', '.txt'));
console.log('Directory:', path.dirname('/path/to/file.txt'));

// Test named imports
console.log('\nTesting named imports...');
import { join, resolve } from 'ekko:path';
const resolvedPath = resolve(filePath);
console.log('Resolved path:', resolvedPath);

// Clean up
fs.rmSync('test-es.txt');
console.log('\nTest file cleaned up.');