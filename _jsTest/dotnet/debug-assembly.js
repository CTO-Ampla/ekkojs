// Debug .NET assembly loading
console.log('Starting debug test...');

async function test() {
    console.log('Importing Calculator...');
    const Calculator = await import('dotnet:TestLibrary/Calculator');
    console.log('Calculator imported:', Calculator);
    console.log('Calculator type:', typeof Calculator);
    console.log('Calculator keys:', Object.keys(Calculator));
    console.log('Calculator.default:', Calculator.default);
    
    if (Calculator.default) {
        console.log('Calculator.default type:', typeof Calculator.default);
        console.log('Calculator.default keys:', Object.keys(Calculator.default));
        console.log('Calculator.default.add:', Calculator.default.add);
    }
}

test().catch(err => console.error('Error:', err));