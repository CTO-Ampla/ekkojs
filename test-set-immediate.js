// Test if we need setImmediate
console.log('Testing promise continuations...');

// Check if setImmediate exists
if (typeof setImmediate === 'undefined') {
    console.log('setImmediate is not defined');
    
    // Polyfill setImmediate using Promise
    global.setImmediate = (fn) => Promise.resolve().then(fn);
}

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

async function test() {
    for (let i = 0; i < 5; i++) {
        console.log(`Iteration ${i}`);
        await sleep(100);
        // Force a microtask to keep the event loop alive
        await new Promise(resolve => setImmediate(resolve));
    }
    console.log('Complete!');
}

test();