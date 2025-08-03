// Simple test to verify timer tracking works
console.log('Starting timer test...');

setTimeout(() => {
    console.log('Timer 1 fired after 6 seconds');
}, 6000);

setTimeout(() => {
    console.log('Timer 2 fired after 8 seconds');
}, 8000);

console.log('Timers scheduled. Runtime should wait...');