// Test loading a compiled EkkoJS package
import demoPackage from 'package:@demo/hello-world';

console.log('Testing compiled package...');
console.log('Version:', demoPackage.version);
console.log('Greeting:', demoPackage.greet('EkkoJS'));
console.log('Math:', demoPackage.add(10, 32));