import cli from 'ekko:cli';

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

console.log('Testing spinner methods individually...\n');

try {
    console.log('1. Testing succeed method:');
    const spinner1 = cli.createSpinner('Test 1');
    spinner1.start();
    await sleep(1000);
    spinner1.succeed('Success!');
    console.log('✓ succeed() worked\n');
    
    console.log('2. Testing warn method:');
    const spinner2 = cli.createSpinner('Test 2');
    spinner2.start();
    await sleep(1000);
    console.log('About to call warn()...');
    spinner2.warn('Warning!');
    console.log('✓ warn() worked\n');
    
    console.log('3. Testing info method:');
    const spinner3 = cli.createSpinner('Test 3');
    spinner3.start();
    await sleep(1000);
    spinner3.info('Info!');
    console.log('✓ info() worked\n');
    
    console.log('4. Testing fail method:');
    const spinner4 = cli.createSpinner('Test 4');
    spinner4.start();
    await sleep(1000);
    spinner4.fail('Failed!');
    console.log('✓ fail() worked\n');
    
    console.log('All spinner methods tested successfully!');
    
} catch (error) {
    console.error('Error:', error.message);
    console.error('Stack:', error.stack);
}