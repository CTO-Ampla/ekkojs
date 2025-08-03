// Test just tree display
import cli from 'ekko:cli';

console.log('Testing tree display...\n');

cli.tree({
    label: 'Project Root',
    children: [
        {
            label: 'src',
            children: [
                { label: 'index.js' },
                { label: 'utils.js' },
                { label: 'components', children: [
                    { label: 'Button.js' },
                    { label: 'Input.js' }
                ]}
            ]
        },
        {
            label: 'tests',
            children: [
                { label: 'unit.test.js' },
                { label: 'integration.test.js' }
            ]
        },
        { label: 'package.json' },
        { label: 'README.md' }
    ]
});

console.log('\nTree display complete.');