// Test with explicit keep-alive
console.log('Testing with keep-alive...');

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

// Keep-alive timer
let keepAlive = setInterval(() => {}, 1000);

async function test() {
    try {
        for (let i = 0; i < 10; i++) {
            console.log(`Iteration ${i}`);
            await sleep(100);
        }
        console.log('Complete!');
    } finally {
        // Clear keep-alive when done
        clearInterval(keepAlive);
        console.log('Keep-alive cleared');
    }
}

test().catch(err => {
    console.error('Error:', err);
    clearInterval(keepAlive);
});