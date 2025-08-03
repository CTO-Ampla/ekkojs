import cli from 'ekko:cli';

console.log('Testing MultiSelect Return Value\n');

try {
    const result = await cli.multiSelect('Select items:', ['A', 'B', 'C']);
    
    console.log('typeof result:', typeof result);
    console.log('Array.isArray(result):', Array.isArray(result));
    console.log('result:', result);
    
    // Try to access it different ways
    console.log('\nTrying different access methods:');
    
    // Check if it's an object with array-like properties
    if (result && typeof result === 'object') {
        console.log('Object.keys(result):', Object.keys(result));
        console.log('result.length:', result.length);
        
        // Try to convert to array
        try {
            const arr = Array.from(result);
            console.log('Array.from(result):', arr);
        } catch (e) {
            console.log('Array.from failed:', e.message);
        }
        
        // Try spreading
        try {
            const arr = [...result];
            console.log('Spread operator [...result]:', arr);
        } catch (e) {
            console.log('Spread failed:', e.message);
        }
    }
    
} catch (error) {
    console.error('Error:', error.message);
    console.error('Stack:', error.stack);
}