// Test native library loading - string with parameters
console.log('Testing native library loading (strings)...');

import mathlib from 'native:mathlib';

console.log('\nTesting string operations:');
console.log('getVersion() =', mathlib.getVersion());
console.log('stringLength("Hello World") =', mathlib.stringLength("Hello World"));