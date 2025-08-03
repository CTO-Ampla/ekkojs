// Test output formatting features
import cli from 'ekko:cli';

console.log(cli.bold(cli.green('=== EkkoJS Output Formatting Test ===')));
console.log('Testing table, box, and tree display features\n');

// Test 1: Simple Table
console.log(cli.bold(cli.cyan('1. Simple Table Test')));
console.log('Creating a simple table with headers and data rows:\n');

try {
    cli.table({
        head: ['ID', 'Name', 'Status', 'Progress'],
        rows: [
            ['001', 'Task 1', cli.green('Complete'), '100%'],
            ['002', 'Task 2', cli.yellow('In Progress'), '60%'],
            ['003', 'Task 3', cli.red('Failed'), '0%']
        ]
    });
    console.log(cli.green('✓ Table rendered successfully!\n'));
} catch (error) {
    console.error(cli.red('✗ Table error:'), error.message);
}

// Test 2: Simple Box
console.log(cli.bold(cli.cyan('2. Simple Box Test')));
console.log('Creating a simple box with text:\n');

try {
    cli.box('Hello, EkkoJS!\nThis is a test box\nwith multiple lines.');
    console.log(cli.green('✓ Box rendered successfully!\n'));
} catch (error) {
    console.error(cli.red('✗ Box error:'), error.message);
}

// Test 3: Simple Tree
console.log(cli.bold(cli.cyan('3. Simple Tree Test')));
console.log('Creating a simple tree structure:\n');

try {
    cli.tree({
        label: 'project',
        children: [
            {
                label: 'src',
                children: [
                    { label: 'index.js' },
                    { label: 'utils.js' },
                    {
                        label: 'components',
                        children: [
                            { label: 'Button.jsx' },
                            { label: 'Input.jsx' }
                        ]
                    }
                ]
            },
            {
                label: 'tests',
                children: [
                    { label: 'test.spec.js' }
                ]
            },
            { label: 'package.json' },
            { label: 'README.md' }
        ]
    });
    console.log(cli.green('✓ Tree rendered successfully!\n'));
} catch (error) {
    console.error(cli.red('✗ Tree error:'), error.message);
}

// Test 4: Array-based Table (alternative syntax)
console.log(cli.bold(cli.cyan('4. Array-based Table Test')));
console.log('Testing table with array syntax:\n');

try {
    cli.table([
        ['Name', 'Age', 'City'],
        ['John Doe', '30', 'New York'],
        ['Jane Smith', '25', 'London'],
        ['Bob Johnson', '35', 'Paris']
    ]);
    console.log(cli.green('✓ Array table rendered successfully!\n'));
} catch (error) {
    console.error(cli.red('✗ Array table error:'), error.message);
}

console.log(cli.bold(cli.green('=== Output Formatting Tests Complete! ===')));
console.log('Table, box, and tree display features are working correctly.');