// Basic TypeScript test
interface Person {
    name: string;
    age: number;
}

function greet(person: Person): string {
    return `Hello, ${person.name}! You are ${person.age} years old.`;
}

const user: Person = {
    name: "Alice",
    age: 30
};

console.log(greet(user));

// Test type inference
const numbers = [1, 2, 3, 4, 5];
const doubled = numbers.map(n => n * 2);
console.log("Doubled numbers:", doubled);

// Test ES6 features
const message = `TypeScript is working!`;
console.log(message);