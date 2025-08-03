// Test timer tracking implementation
console.log('Testing timer tracking...');

console.log('1. Setting timeout for 3 seconds');
setTimeout(() => {
    console.log('2. Timeout fired after 3 seconds');
}, 3000);

console.log('3. Setting timeout for 7 seconds');
setTimeout(() => {
    console.log('4. Timeout fired after 7 seconds');
    
    // Set another timeout inside
    console.log('5. Setting nested timeout for 2 seconds');
    setTimeout(() => {
        console.log('6. Nested timeout fired');
    }, 2000);
}, 7000);

console.log('7. Script execution continues...');
console.log('8. Runtime should now wait for all timers to complete');