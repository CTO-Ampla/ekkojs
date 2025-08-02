// Debug ES module execution
console.log('Starting ES module test...');

import fs from 'ekko:fs';

console.log('After import statement');
console.log('fs:', fs);
console.log('fs type:', typeof fs);
console.log('fs keys:', Object.keys(fs));

// Try using fs
fs.writeFileSync('es-test.txt', 'Hello from ES modules!');
const content = fs.readFileSync('es-test.txt');
console.log('File content:', content);
fs.rmSync('es-test.txt');

console.log('Done!');