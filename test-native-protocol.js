// Test importing from native library using native: protocol
import mathlib from 'native:mathlib';

console.log('Testing native: protocol integration...');

try {
    // Test add function
    const sum = mathlib.add(10, 20);
    console.log(`10 + 20 = ${sum}`);
    
    // Test multiplyDouble function
    const product = mathlib.multiplyDouble(5.0, 6.0);
    console.log(`5.0 * 6.0 = ${product}`);
    
    console.log('native: protocol test passed!');
} catch (error) {
    console.error('Error testing native: protocol:', error);
}