// Test what actually works
globalThis.import('dotnet:TestLibrary/Calculator').then(calc => {
    console.log('Testing .NET assembly loading...');
    
    // Access methods directly from the module
    console.log('\nTesting Calculator (static methods):');
    console.log('5 + 3 =', calc.add(5, 3));
    console.log('10 - 4 =', calc.subtract(10, 4));
    console.log('6 * 7 =', calc.multiply(6, 7));
    console.log('20 / 4 =', calc.divide(20, 4));
    
    // Now test importing all exports
    return globalThis.import('dotnet:TestLibrary');
}).then(allExports => {
    console.log('\nAll exports loaded');
    console.log('Export keys:', Object.keys(allExports.default));
    
    // Test Constants
    if (allExports.default.Constants) {
        console.log('\nTesting Constants:');
        console.log('Version:', allExports.default.Constants.version);
        console.log('Max Length:', allExports.default.Constants.maxLength);
    }
    
    // Test StringHelper with instance
    if (allExports.default.StringHelper) {
        console.log('\nTesting StringHelper (instance):');
        const StringHelper = allExports.default.StringHelper;
        console.log('StringHelper:', StringHelper);
        console.log('StringHelper.new:', StringHelper.new);
        
        if (StringHelper.new) {
            const helper = StringHelper.new(['[PREFIX] ']);
            console.log('Created instance:', helper);
            console.log('Formatted text:', helper.format('Hello World'));
        }
    }
}).catch(err => console.error('Error:', err));