// Simple dynamic import test
console.log('Testing if import function exists...');
console.log('typeof globalThis.import:', typeof globalThis.import);

if (typeof globalThis.import === 'function') {
    console.log('import function is available!');
    
    globalThis.import('ekko:fs').then(fs => {
        console.log('Successfully imported fs:', fs);
        console.log('fs methods:', Object.keys(fs));
    }).catch(error => {
        console.error('Import failed:', error.message);
        console.error('Stack:', error.stack);
    });
} else {
    console.error('import function is not available');
}