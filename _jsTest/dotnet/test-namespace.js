// Test namespace properties directly
globalThis.import('dotnet:TestLibrary/Calculator').then(calc => {
    console.log('Module loaded');
    console.log('Type of module:', typeof calc);
    console.log('Module toString:', calc.toString());
    console.log('Has default?', 'default' in calc);
    console.log('Has add?', 'add' in calc);
    console.log('Keys:', Object.keys(calc));
    console.log('Own property names:', Object.getOwnPropertyNames(calc));
    
    // Try to access properties directly
    console.log('\nDirect access:');
    console.log('calc.default:', calc.default);
    console.log('calc.add:', calc.add);
    
    // If default exists, check its properties
    if (calc.default) {
        console.log('\nDefault object:');
        console.log('Type:', typeof calc.default);
        console.log('Keys:', Object.keys(calc.default));
        console.log('calc.default.add:', calc.default.add);
    }
}).catch(err => console.error('Error:', err));