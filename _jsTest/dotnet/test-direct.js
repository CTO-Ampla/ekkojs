// Test direct access to wrapped exports
console.log('Testing direct import...');

globalThis.import('dotnet:TestLibrary/Calculator').then(calc => {
    console.log('Module loaded');
    console.log('Module keys:', Object.keys(calc));
    console.log('Default export:', calc.default);
    
    // Try to access the static methods directly
    try {
        if (calc.default && typeof calc.default === 'function') {
            console.log('Default is a function (constructor)');
            console.log('Static method add:', calc.default.add);
            if (calc.default.add) {
                console.log('5 + 3 =', calc.default.add(5, 3));
            }
        } else if (calc.default && typeof calc.default === 'object') {
            console.log('Default is an object');
            console.log('Methods:', Object.keys(calc.default));
        }
    } catch (e) {
        console.error('Error accessing methods:', e.message);
    }
}).catch(err => console.error('Import error:', err));