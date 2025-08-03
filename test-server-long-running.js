// Simulated server that runs indefinitely
console.log('Starting server simulation...');

let requestCount = 0;
const startTime = Date.now();

// Simulate incoming requests every 10 seconds
const requestHandler = setInterval(() => {
    requestCount++;
    const elapsed = Math.round((Date.now() - startTime) / 1000);
    console.log(`[${elapsed}s] Handling request #${requestCount}`);
    
    // Show that we're still running after 5 minutes
    if (elapsed > 300) {
        console.log('  --> Still running after 5 minutes!');
    }
}, 10000);

// Simulate server listening
console.log('Server listening on port 3000...');
console.log('This will run forever until you stop it with Ctrl+C');

// The script never ends - the interval keeps it alive