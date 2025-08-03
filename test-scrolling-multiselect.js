// Test scrolling multiSelect with exactly 8 visible items max
import cli from 'ekko:cli';

console.log(cli.bold(cli.green('=== Scrolling MultiSelect Test ===')));
console.log('Testing multiSelect with scrolling window (max 8 visible items)\n');

// Create a list with more than 8 items to test scrolling
const techStack = [
    'JavaScript',
    'TypeScript', 
    'Python',
    'Java',
    'C#',
    'Go',
    'Rust',
    'C++',
    'Swift',
    'Kotlin',
    'PHP',
    'Ruby',
    'Elixir',
    'Scala',
    'Clojure',
    'Haskell',
    'F#',
    'Dart',
    'Lua',
    'Perl'
];

console.log(`Testing with ${techStack.length} programming languages:`);
console.log(cli.dim('Note: Only 8 items will be visible at a time. Use arrow keys to scroll through the list.'));
console.log(cli.dim('Watch for "↑ More items above" and "↓ More items below" indicators.\n'));

try {
    const selected = await cli.multiSelect('Select programming languages you know:', techStack);
    
    console.log('\n' + cli.green('✓ Scrolling MultiSelect completed successfully!'));
    
    if (selected.length > 0) {
        console.log(cli.cyan('\nSelected programming languages:'));
        selected.forEach(lang => console.log(`  • ${cli.green(lang)}`));
        console.log(cli.dim(`\nTotal selected: ${selected.length} out of ${techStack.length}`));
    } else {
        console.log(cli.yellow('\nNo programming languages selected.'));
    }
    
} catch (error) {
    console.error(cli.red('\n✗ MultiSelect error:'), error.message);
    console.error('Stack:', error.stack);
}

console.log('\n' + cli.bold(cli.green('=== Scrolling MultiSelect Test Complete ===')));
console.log(cli.dim('The list should have scrolled smoothly with up/down arrows showing only 8 items at a time.'));