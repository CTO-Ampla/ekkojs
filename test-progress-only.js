// Test just the progress indicators section
import cli from 'ekko:cli';

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

async function testProgress() {
    console.log('Testing Progress Indicators...\n');
    
    // Progress Bar Demo
    console.log('Progress Bar Demo:');
    const progressBar = cli.createProgressBar(100, 40);
    
    for (let i = 0; i <= 100; i += 5) {
        progressBar.update(i);
        await sleep(100);
    }
    progressBar.complete();
    console.log(cli.green('✓ Progress bar completed!\n'));
    
    // Spinner Demo
    console.log('Spinner Demo:');
    const spinner = cli.createSpinner('Quick demo of spinner states...');
    spinner.start();
    await sleep(500);
    spinner.succeed('Success state!');
    
    const spinner2 = cli.createSpinner('Warning example...');
    spinner2.start();
    await sleep(500);
    spinner2.warn('Warning state!');
    
    const spinner3 = cli.createSpinner('Info example...');
    spinner3.start();
    await sleep(500);
    spinner3.info('Info state!');
    
    console.log('\n✅ All progress indicators tested successfully!');
}

testProgress().catch(err => {
    console.error('Error:', err);
});