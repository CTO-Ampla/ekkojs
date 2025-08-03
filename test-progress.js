import cli from 'ekko:cli';

console.log('Testing Progress Indicators\n');

try {
    // Test Progress Bar
    console.log('Creating progress bar...');
    const progressBar = cli.createProgressBar(100, 40);
    console.log('Progress bar created:', progressBar);
    console.log('Type:', typeof progressBar);
    console.log('Properties:', Object.keys(progressBar));
    
    console.log('\nTesting update method...');
    progressBar.update(50);
    console.log('Update successful');
    
    progressBar.complete();
    console.log('Progress bar completed\n');
    
    // Test Spinner
    console.log('Creating spinner...');
    const spinner = cli.createSpinner('Testing...');
    console.log('Spinner created:', spinner);
    console.log('Type:', typeof spinner);
    console.log('Properties:', Object.keys(spinner));
    
    console.log('\nStarting spinner...');
    spinner.start();
    console.log('Spinner started');
    
    // Wait a bit
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    spinner.succeed('Test completed!');
    console.log('Spinner completed');
    
} catch (error) {
    console.error('Error:', error.message);
    console.error('Stack:', error.stack);
}