// Test basic import functionality
console.log('Testing globalThis.import existence:', typeof globalThis.import);

if (typeof globalThis.import === 'function') {
    console.log('import function exists, testing...');
    
    globalThis.import('ekko:fs').then(fs => {
        console.log('Successfully imported ekko:fs');
        console.log('fs module:', fs);
    }).catch(err => {
        console.error('Failed to import ekko:fs:', err);
    });
} else {
    console.error('globalThis.import is not a function!');
}