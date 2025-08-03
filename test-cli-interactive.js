import cli from 'ekko:cli';

console.log('Testing Interactive CLI Components\n');

// Test 1: Interactive Select
console.log(cli.blue('Test 1: Interactive Select (with arrow keys)'));
const color = await cli.select('Choose your favorite color:', [
    'Red', 'Green', 'Blue', 'Yellow', 'Purple', 'Orange'
]);
console.log(cli.green(`✓ Selected: ${color}\n`));

// Test 2: MultiSelect
console.log(cli.blue('Test 2: MultiSelect (spacebar to toggle, enter to confirm)'));
const hobbies = await cli.multiSelect('Select your hobbies:', [
    'Reading', 'Gaming', 'Cooking', 'Sports', 'Music', 'Travel'
]);
console.log(cli.green(`✓ Selected: ${hobbies.join(', ') || 'None'}\n`));

// Test 3: Large MultiSelect with scrolling
console.log(cli.blue('Test 3: Large MultiSelect with scrolling'));
const languages = await cli.multiSelect('Select programming languages you know:', [
    'JavaScript', 'TypeScript', 'Python', 'Java', 'C#', 'C++', 'Go', 'Rust',
    'Ruby', 'PHP', 'Swift', 'Kotlin', 'Scala', 'Haskell', 'Elixir', 'Clojure',
    'F#', 'OCaml', 'R', 'Julia', 'Dart', 'Lua', 'Perl', 'Shell/Bash'
]);
console.log(cli.green(`✓ Selected: ${languages.join(', ') || 'None'}\n`));

console.log(cli.bold(cli.green('All tests completed!')));