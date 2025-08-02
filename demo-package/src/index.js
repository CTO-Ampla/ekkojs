// Demo package main entry point
export function greet(name = 'World') {
    return `Hello, ${name}!`;
}

export function add(a, b) {
    return a + b;
}

export const version = '1.0.0';

export default {
    greet,
    add,
    version
};