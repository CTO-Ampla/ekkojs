// Test .NET assembly loading - working version
console.log('Testing .NET assembly loading...');

// Test Calculator methods directly
globalThis.import('dotnet:TestLibrary/Calculator').then(calc => {
    console.log('\nTesting Calculator (static methods):');
    console.log('5 + 3 =', calc.add(5, 3));
    console.log('10 - 4 =', calc.subtract(10, 4));
    console.log('6 * 7 =', calc.multiply(6, 7));
    console.log('20 / 4 =', calc.divide(20, 4));
    
    // Load all exports for other tests
    return globalThis.import('dotnet:TestLibrary');
}).then(allExports => {
    const { Calculator, StringHelper, Constants } = allExports.default;
    
    // Test Constants
    console.log('\nTesting Constants:');
    console.log('Version:', Constants.version);
    console.log('Max Length:', Constants.maxLength);
    console.log('Build Date:', Constants.buildDate);
    
    // Test StringHelper instance
    console.log('\nTesting StringHelper (instance):');
    if (StringHelper && StringHelper.new) {
        const helper = StringHelper.new('[PREFIX] ');
        console.log('Created instance');
        console.log('Formatted text:', helper.format('Hello World'));
        
        // Test property setter
        helper.prefix = '[NEW] ';
        console.log('Updated prefix:', helper.prefix);
        console.log('Formatted with new prefix:', helper.format('Hello Again'));
        
        // Test async method
        console.log('\nTesting async method:');
        helper.processAsync('test').then(result => {
            console.log('Async result:', result);
        }).catch(err => console.error('Async error:', err));
    } else {
        console.error('StringHelper.new not found');
    }
    
    console.log('\n.NET assembly test completed!');
}).catch(err => console.error('Error:', err));