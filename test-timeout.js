console.log('Starting timeout test...');
console.log('If this exits after ~30 seconds, there\'s a timeout.');

let counter = 0;
const interval = setInterval(() => {
    counter++;
    console.log(`Still running... ${counter} seconds`);
    
    if (counter >= 60) {
        console.log('Test completed - no timeout after 60 seconds');
        clearInterval(interval);
    }
}, 1000);

// Keep the process alive
await new Promise(() => {});