// Test async loop
console.log('Testing async loop...');

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

async function test() {
    console.log('Starting loop...');
    
    for (let i = 0; i < 5; i++) {
        console.log(`Iteration ${i}`);
        await sleep(100);
    }
    
    console.log('Loop complete!');
}

console.log('Before test()');
test().then(() => {
    console.log('Test finished');
}).catch(err => {
    console.error('Test error:', err);
});
console.log('After test() call');