// Debug method access
globalThis.import('dotnet:TestLibrary/Calculator').then(calc => {
    console.log('Module loaded');
    console.log('calc.add:', calc.add);
    console.log('typeof calc.add:', typeof calc.add);
    
    // Try calling directly
    try {
        console.log('Trying to call add...');
        const result = calc.add(5, 3);
        console.log('Result:', result);
    } catch (e) {
        console.error('Error:', e.message);
        console.error('Stack:', e.stack);
    }
}).catch(err => console.error('Import error:', err));