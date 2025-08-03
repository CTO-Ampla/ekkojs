import cli from 'ekko:cli';

console.log('Testing ClearScript async array handling\n');

// Test what multiSelect actually returns
const result = await cli.multiSelect('Test:', ['A', 'B', 'C']);

console.log('Result:', result);
console.log('Type:', typeof result);
console.log('Constructor name:', result?.constructor?.name);
console.log('Is Array:', Array.isArray(result));
console.log('Has join method:', typeof result?.join);

// Check if it's a Task object
if (result && typeof result === 'object') {
    console.log('\nObject properties:');
    for (const key in result) {
        console.log(`  ${key}:`, result[key]);
    }
    
    // Check prototype
    const proto = Object.getPrototypeOf(result);
    console.log('\nPrototype:', proto);
    console.log('Prototype constructor:', proto?.constructor?.name);
}

// Try accessing as array
try {
    console.log('\nTrying array access:');
    console.log('result[0]:', result[0]);
    console.log('result.length:', result.length);
} catch (e) {
    console.log('Array access error:', e.message);
}