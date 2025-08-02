// Test native library loading - struct only
console.log('Testing native library loading (struct only)...');

import mathlib from 'native:mathlib';

console.log('\nTesting struct operations:');
const p1 = new mathlib.Point({ x: 0, y: 0 });
const p2 = new mathlib.Point({ x: 3, y: 4 });
console.log('p1:', p1);
console.log('p2:', p2);
console.log('distance(p1, p2) =', mathlib.distance(p1, p2));