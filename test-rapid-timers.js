// Test rapid timer creation
console.log('Testing rapid timer creation...');

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

async function test() {
    console.log('Starting rapid timer test...');
    
    // Create many timers quickly
    for (let i = 0; i <= 20; i++) {
        console.log(`Iteration ${i}`);
        await sleep(100);
    }
    
    console.log('All iterations complete!');
}

test().then(() => {
    console.log('Test finished successfully');
}).catch(err => {
    console.error('Test failed:', err);
});