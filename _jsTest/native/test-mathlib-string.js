// Test native library loading - string only
console.log('Testing native library loading (string only)...');

import mathlib from 'native:mathlib';

console.log('\nTesting string operations:');
console.log('getVersion() =', mathlib.getVersion());