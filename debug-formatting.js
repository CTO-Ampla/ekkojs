// Debug output formatting
import cli from 'ekko:cli';

console.log('=== Debug Test ===\n');

// Test table with simple array
console.log('Testing simple array table:');
cli.table([
    ['A', 'B'],
    ['1', '2']
]);
console.log('Done.\n');

// Test table with object format
console.log('Testing object table:');
cli.table({
    head: ['Name', 'Status'],
    rows: [
        ['Test', 'OK']
    ]
});
console.log('Done.\n');

// Test tree with simple structure
console.log('Testing simple tree:');
cli.tree({
    label: 'root',
    children: [
        { label: 'child1' },
        { label: 'child2' }
    ]
});
console.log('Done.\n');