// Test .NET assembly loading
import Calculator from 'dotnet:TestLibrary/Calculator';
import { StringHelper, Constants } from 'dotnet:TestLibrary';

console.log('Testing .NET assembly loading...');

// Test static methods
console.log('\nTesting Calculator (static methods):');
console.log('5 + 3 =', Calculator.add(5, 3));
console.log('10 - 4 =', Calculator.subtract(10, 4));
console.log('6 * 7 =', Calculator.multiply(6, 7));
console.log('20 / 4 =', Calculator.divide(20, 4));

// Test constants
console.log('\nTesting Constants:');
console.log('Version:', Constants.version);
console.log('Max Length:', Constants.maxLength);
console.log('Build Date:', Constants.buildDate);

// Test instance creation
console.log('\nTesting StringHelper (instance):');
const helper = new StringHelper('[PREFIX] ');
console.log('Prefix:', helper.prefix);
console.log('Formatted text:', helper.format('Hello World'));

// Test async method
console.log('\nTesting async method:');
helper.processAsync('test').then(result => {
    console.log('Async result:', result);
});

// Test property setter
helper.prefix = '[NEW] ';
console.log('Updated prefix:', helper.prefix);
console.log('Formatted with new prefix:', helper.format('Hello Again'));

console.log('\n.NET assembly test completed!');