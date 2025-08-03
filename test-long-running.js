// Test that scripts can run longer than 30 seconds
console.log('Starting long-running test...');
const start = Date.now();

// Set a timer for 35 seconds
setTimeout(() => {
    const elapsed = Math.round((Date.now() - start) / 1000);
    console.log(`Timer fired after ${elapsed} seconds`);
    console.log('Test completed successfully!');
}, 35000);

console.log('Waiting 35 seconds...');