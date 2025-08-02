// Test native library loading
console.log('Testing native library loading...');

// Note: This test will fail unless the mathlib native library is compiled
// Build instructions:
// Linux: cd native-libraries/mathlib && gcc -shared -fPIC -o libmathlib.so mathlib.c -lm
// Windows: cd native-libraries/mathlib && gcc -shared -o mathlib.dll mathlib.c -lm

import mathlib from 'native:mathlib';

console.log('\nTesting basic math operations:');
console.log('add(5, 3) =', mathlib.add(5, 3));
console.log('subtract(10, 4) =', mathlib.subtract(10, 4));
console.log('multiplyDouble(3.5, 2.0) =', mathlib.multiplyDouble(3.5, 2.0));
console.log('divideDouble(10.0, 2.5) =', mathlib.divideDouble(10.0, 2.5));

console.log('\nTesting string operations:');
console.log('getVersion() =', mathlib.getVersion());
console.log('stringLength("Hello World") =', mathlib.stringLength("Hello World"));

console.log('\nTesting struct operations:');
const p1 = new mathlib.Point({ x: 0, y: 0 });
const p2 = new mathlib.Point({ x: 3, y: 4 });
console.log('distance(p1, p2) =', mathlib.distance(p1, p2));

console.log('\nTesting array operations:');
const arr = [1, 2, 3, 4, 5];
console.log('sumArray([1,2,3,4,5]) =', mathlib.sumArray(arr, arr.length));

console.log('\nNative library test completed!');