// Test progress bar stopping at 60%
import cli from 'ekko:cli';

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

async function test() {
    console.log('Starting progress bar test...');
    const progressBar = cli.createProgressBar(100, 40);
    
    for (let i = 0; i <= 100; i += 5) {
        console.log(`Update ${i}%`);
        progressBar.update(i);
        await sleep(100);
        
        if (i === 60) {
            console.log('Reached 60% - this is where it usually stops');
        }
    }
    
    progressBar.complete();
    console.log('Progress bar completed!');
}

test().catch(err => console.error('Error:', err));