// Test advanced input methods
import cli from 'ekko:cli';

console.log('=== Advanced Input Methods Test ===\n');

try {
    // Test number input
    console.log('1. Number Input:');
    const age = await cli.number('Enter your age', 25);
    console.log(cli.green(`You entered: ${age}`));

    // Test date input
    console.log('\n2. Date Input:');
    const birthday = await cli.date('Enter your birthday');
    console.log(cli.green(`You entered: ${birthday.toISOString().split('T')[0]}`));

    // Test multiSelect
    console.log('\n3. Multi-Select:');
    const languages = await cli.multiSelect('Select programming languages you know:', [
        'JavaScript',
        'TypeScript', 
        'Python',
        'C#',
        'Java',
        'Go',
        'Rust'
    ]);
    console.log(cli.green(`You selected: ${languages.join(', ')}`));

} catch (error) {
    console.error(cli.red('Error:', error.message));
}

console.log('\n=== Test Complete ===');