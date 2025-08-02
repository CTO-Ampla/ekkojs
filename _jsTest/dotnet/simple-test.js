// Simple test without async/await
console.log('Testing .NET assembly loading...');

// Use the synchronous import function directly
const Calculator = globalThis.import('dotnet:TestLibrary/Calculator');
console.log('Calculator:', Calculator);

// Note: Since import returns a promise, we need to handle it
Calculator.then(calc => {
    console.log('Calculator loaded:', calc);
    console.log('Calculator type:', typeof calc);
    console.log('Calculator keys:', Object.keys(calc));
    
    if (calc.default) {
        console.log('Using default export...');
        const CalcClass = calc.default;
        console.log('5 + 3 =', CalcClass.add(5, 3));
    }
}).catch(err => console.error('Error loading Calculator:', err));