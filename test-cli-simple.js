// Simple test for CLI module
import cli from 'ekko:cli';

console.log('CLI module loaded');
console.log('cli object:', cli);
console.log('cli.red:', cli.red);

// Try a simple color test
try {
    const redText = cli.red('This is red');
    console.log('Red text:', redText);
} catch (e) {
    console.error('Error:', e);
}