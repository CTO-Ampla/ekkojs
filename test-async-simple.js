// Simple async test
console.log('1. Start');

async function test() {
    console.log('2. In async function');
    await new Promise(resolve => {
        console.log('3. Promise executor');
        setTimeout(() => {
            console.log('4. Timer callback');
            resolve();
        }, 1000);
    });
    console.log('5. After await');
}

test().then(() => {
    console.log('6. Test completed');
});

console.log('7. End of script');