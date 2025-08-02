// Test instance creation
globalThis.import('dotnet:TestLibrary').then(allExports => {
    console.log('All exports:', allExports);
    console.log('Default:', allExports.default);
    console.log('StringHelper:', allExports.default.StringHelper);
    
    const StringHelper = allExports.default.StringHelper;
    console.log('StringHelper type:', typeof StringHelper);
    console.log('StringHelper.new:', StringHelper.new);
    console.log('StringHelper.new type:', typeof StringHelper.new);
    
    // Try different ways to create instance
    try {
        console.log('\nTrying StringHelper.new([\'[PREFIX] \'])...');
        const helper1 = StringHelper.new(['[PREFIX] ']);
        console.log('Success:', helper1);
    } catch (e) {
        console.error('Error 1:', e.message);
    }
    
    try {
        console.log('\nTrying StringHelper.new(\'[PREFIX] \')...');
        const helper2 = StringHelper.new('[PREFIX] ');
        console.log('Success:', helper2);
    } catch (e) {
        console.error('Error 2:', e.message);
    }
    
    try {
        console.log('\nTrying StringHelper.new()...');
        const helper3 = StringHelper.new();
        console.log('Success:', helper3);
    } catch (e) {
        console.error('Error 3:', e.message);
    }
}).catch(err => console.error('Import error:', err));