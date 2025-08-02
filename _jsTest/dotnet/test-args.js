// Test how arguments are passed
globalThis.import('dotnet:TestLibrary/Calculator').then(calc => {
    console.log('Testing argument passing...');
    
    // Create a test function to see how ClearScript passes args
    const testFunc = new Function('console.log("Arguments:", arguments); console.log("Args length:", arguments.length); console.log("First arg:", arguments[0]); return arguments;');
    calc.testFunc = testFunc;
    
    console.log('\nCalling testFunc(5, 3):');
    const result = calc.testFunc(5, 3);
    console.log('Result:', result);
    
    // Now let's see what the actual add function receives
    console.log('\nTrying calc.add with array:');
    try {
        // Maybe it needs an array?
        const result2 = calc.add([5, 3]);
        console.log('Result with array:', result2);
    } catch (e) {
        console.error('Error with array:', e.message);
    }
}).catch(err => console.error('Import error:', err));