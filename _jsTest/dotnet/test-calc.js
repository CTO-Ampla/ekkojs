// Test Calculator static methods
import CalcModule from 'dotnet:TestLibrary/Calculator';

console.log('CalcModule:', CalcModule);
console.log('CalcModule.default:', CalcModule.default);

const Calculator = CalcModule.default;
console.log('Calculator:', Calculator);
console.log('Calculator.add:', Calculator.add);

if (Calculator.add) {
    console.log('5 + 3 =', Calculator.add(5, 3));
}