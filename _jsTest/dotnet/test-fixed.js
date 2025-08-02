// Test with correct import patterns
import * as CalcModule from 'dotnet:TestLibrary/Calculator';
import AllExports from 'dotnet:TestLibrary';

console.log('Testing .NET assembly loading...');

// Test static methods directly from named export
console.log('\nTesting Calculator (static methods):');
console.log('5 + 3 =', CalcModule.add(5, 3));
console.log('10 - 4 =', CalcModule.subtract(10, 4));
console.log('6 * 7 =', CalcModule.multiply(6, 7));
console.log('20 / 4 =', CalcModule.divide(20, 4));

// Test accessing via all exports
console.log('\nAll exports:', AllExports);
console.log('AllExports.default:', AllExports.default);

if (AllExports.default && AllExports.default.Constants) {
    console.log('\nTesting Constants:');
    console.log('Version:', AllExports.default.Constants.version);
}