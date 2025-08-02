// Test named imports
import { writeFileSync, readFileSync, rmSync } from 'ekko:fs';
import { join, dirname, basename } from 'ekko:path';

console.log('Testing named imports...');

// Test fs named imports
writeFileSync('named-test.txt', 'Named imports work!');
const content = readFileSync('named-test.txt');
console.log('Content:', content);
rmSync('named-test.txt');

// Test path named imports
const fullPath = join('home', 'user', 'documents', 'file.txt');
console.log('Joined path:', fullPath);
console.log('Directory:', dirname(fullPath));
console.log('Basename:', basename(fullPath));

console.log('Named imports test completed!');