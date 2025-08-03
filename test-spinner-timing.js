import cli from 'ekko:cli';

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

console.log('Testing spinner with exact timing from demo...\n');

try {
    // First spinner - 2000ms (works)
    console.log('1. Starting first spinner (2000ms)...');
    const spinner1 = cli.createSpinner('Loading data...');
    spinner1.start();
    await sleep(2000);
    spinner1.succeed('Data loaded successfully!');
    console.log('✓ First spinner completed\n');
    
    // Wait 500ms
    console.log('2. Waiting 500ms...');
    await sleep(500);
    console.log('✓ Wait completed\n');
    
    // Second spinner - 1500ms (where it fails)
    console.log('3. Starting second spinner (1500ms)...');
    const spinner2 = cli.createSpinner('Processing...');
    spinner2.start();
    console.log('   Spinner started, waiting 1500ms...');
    await sleep(1500);
    console.log('   Sleep completed, calling warn()...');
    spinner2.warn('Completed with warnings');
    console.log('✓ Second spinner completed\n');
    
    // If we get here, the issue is later
    console.log('4. Waiting another 500ms...');
    await sleep(500);
    console.log('✓ Wait completed\n');
    
    console.log('5. Testing continues...');
    console.log('If you see this, the issue is not with the spinner timing.');
    
} catch (error) {
    console.error('Error:', error.message);
    console.error('Stack:', error.stack);
}