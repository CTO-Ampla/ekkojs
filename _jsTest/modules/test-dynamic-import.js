// Test dynamic imports
console.log('Testing dynamic imports...');

async function testDynamicImports() {
    try {
        // Dynamic import of fs module
        console.log('\nDynamically importing ekko:fs...');
        const fs = await import('ekko:fs');
        console.log('fs module loaded:', typeof fs);
        
        // Use the dynamically imported module
        fs.default.writeFileSync('dynamic-test.txt', 'Dynamic import works!');
        const content = fs.default.readFileSync('dynamic-test.txt');
        console.log('Content from dynamic import:', content);
        
        // Dynamic import of path module
        console.log('\nDynamically importing ekko:path...');
        const path = await import('ekko:path');
        
        const joined = path.default.join('dynamic', 'path', 'test.js');
        console.log('Joined path:', joined);
        
        // Clean up
        fs.default.rmSync('dynamic-test.txt');
        console.log('\nDynamic import test completed successfully!');
    } catch (error) {
        console.error('Dynamic import error:', error.message);
    }
}

// Run the test
testDynamicImports().catch(console.error);