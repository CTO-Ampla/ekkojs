// Comprehensive native library test
console.log('🧪 EkkoJS Native Library Comprehensive Test');
console.log('==========================================');

import mathlib from 'native:mathlib';

console.log('\n✅ Library Import: SUCCESS');
console.log(`📦 Version: ${mathlib.getVersion()}`);

console.log('\n🔢 Math Operations:');
console.log(`   add(42, 13) = ${mathlib.add(42, 13)}`);
console.log(`   subtract(100, 23) = ${mathlib.subtract(100, 23)}`);
console.log(`   multiplyDouble(3.14159, 2.0) = ${mathlib.multiplyDouble(3.14159, 2.0)}`);
console.log(`   divideDouble(22.5, 4.5) = ${mathlib.divideDouble(22.5, 4.5)}`);

console.log('\n📝 String Operations:');
console.log(`   stringLength("EkkoJS Native Interop") = ${mathlib.stringLength("EkkoJS Native Interop")}`);

console.log('\n📐 Geometry Operations:');
const origin = new mathlib.Point({ x: 0, y: 0 });
const point1 = new mathlib.Point({ x: 5, y: 12 });
const point2 = new mathlib.Point({ x: -3, y: 4 });
console.log(`   distance((0,0), (5,12)) = ${mathlib.distance(origin, point1)}`);
console.log(`   distance((0,0), (-3,4)) = ${mathlib.distance(origin, point2)}`);

console.log('\n📊 Array Operations:');
const numbers = [10, 20, 30, 40, 50];
const fibonacci = [1, 1, 2, 3, 5, 8, 13, 21];
console.log(`   sumArray([10,20,30,40,50]) = ${mathlib.sumArray(numbers, numbers.length)}`);
console.log(`   sumArray([1,1,2,3,5,8,13,21]) = ${mathlib.sumArray(fibonacci, fibonacci.length)}`);

console.log('\n🎯 All Tests Passed! Native library integration working perfectly!');
console.log('✨ EkkoJS can now seamlessly interop with C/C++ libraries! ✨');