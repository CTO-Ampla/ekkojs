// Test importing from .NET assembly using dotnet: protocol
import Calculator from 'dotnet:TestLibrary/Calculator';

console.log('Testing dotnet: protocol integration...');

try {
    // Calculator has static methods according to the mapping
    const result = Calculator.add(5, 3);
    console.log(`5 + 3 = ${result}`);
    
    const result2 = Calculator.multiply(4, 7);
    console.log(`4 * 7 = ${result2}`);
    
    console.log('dotnet: protocol test passed!');
} catch (error) {
    console.error('Error testing dotnet: protocol:', error);
}