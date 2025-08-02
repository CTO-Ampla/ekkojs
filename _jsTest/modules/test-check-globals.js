// Check what global functions are available
console.log('Checking global functions...');
console.log('typeof globalThis.import:', typeof globalThis.import);
console.log('typeof globalThis.__createImportMeta:', typeof globalThis.__createImportMeta);
console.log('typeof globalThis.__ekkoLoadModule:', typeof globalThis.__ekkoLoadModule);

// Try a direct dynamic import
if (typeof globalThis.import === 'function') {
    globalThis.import('ekko:fs')
        .then(module => {
            console.log('Direct import worked!');
            console.log('Module keys:', Object.keys(module));
        })
        .catch(err => {
            console.error('Direct import failed:', err.message);
        });
}