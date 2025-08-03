console.log('Testing setTimeout...');

console.log('1. Setting timeout for 1 second');
setTimeout(() => {
    console.log('2. Timeout fired after 1 second');
}, 1000);

console.log('3. Setting timeout for 2 seconds');
setTimeout(() => {
    console.log('4. Timeout fired after 2 seconds');
}, 2000);

console.log('5. Script continues...');

// Keep process alive for 3 seconds
const start = Date.now();
while (Date.now() - start < 3000) {
    // busy wait
}