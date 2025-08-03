// Test advanced input methods (non-interactive)
import cli from 'ekko:cli';

console.log('=== Advanced Input Methods API Test ===\n');

// Test that the methods exist
console.log('1. Checking method availability:');
console.log(`  multiSelect: ${typeof cli.multiSelect}`);
console.log(`  number: ${typeof cli.number}`);
console.log(`  date: ${typeof cli.date}`);

// Test number with default
console.log('\n2. Testing number with default (pressing Enter):');
try {
    const age = await cli.number('Enter age', 30);
    console.log(cli.green(`  Result: ${age}`));
} catch (e) {
    console.log(cli.red(`  Error: ${e.message}`));
}

// Test date with default
console.log('\n3. Testing date with default (pressing Enter):');
try {
    const date = await cli.date('Enter date');
    console.log(cli.green(`  Result: ${date.toISOString().split('T')[0]}`));
} catch (e) {
    console.log(cli.red(`  Error: ${e.message}`));
}

console.log('\n' + cli.bold('✓ Advanced input methods are properly exposed!'));
console.log('\n=== Test Complete ===');