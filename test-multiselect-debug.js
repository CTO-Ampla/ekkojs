import cli from 'ekko:cli';

console.log('Testing multiSelect return value...\n');

const result = await cli.multiSelect('Select items:', ['A', 'B', 'C']);

console.log('Result type:', typeof result);
console.log('Result is array:', Array.isArray(result));
console.log('Result:', result);
console.log('Result keys:', Object.keys(result));

if (result && typeof result === 'object') {
    console.log('Result properties:');
    for (const key in result) {
        console.log(`  ${key}:`, result[key]);
    }
}

try {
    console.log('Trying join:', result.join(', '));
} catch (e) {
    console.log('Join error:', e.message);
}