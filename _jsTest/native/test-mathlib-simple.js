// Test native library loading - numbers only
console.log('Testing native library loading (numbers only)...');

import mathlib from 'native:mathlib';

console.log('\nTesting basic math operations:');
console.log('add(5, 3) =', mathlib.add(5, 3));
console.log('subtract(10, 4) =', mathlib.subtract(10, 4));
console.log('multiplyDouble(3.5, 2.0) =', mathlib.multiplyDouble(3.5, 2.0));
console.log('divideDouble(10.0, 2.5) =', mathlib.divideDouble(10.0, 2.5));

console.log('\nBasic math test completed!');