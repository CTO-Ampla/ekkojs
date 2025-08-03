// Debug tree parsing
import cli from 'ekko:cli';

console.log('=== Tree Debug ===\n');

// Test very simple tree
const simpleTree = {
    label: 'root',
    children: [
        { label: 'child1' },
        { label: 'child2' }
    ]
};

console.log('Tree structure:');
console.log(JSON.stringify(simpleTree, null, 2));
console.log('\nRendering tree:');
cli.tree(simpleTree);
console.log('Done.');