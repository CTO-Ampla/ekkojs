// Test CLI module functionality
import cli from 'ekko:cli';

console.log('Testing CLI module...\n');

// Test basic colors
console.log(cli.red('This is red text'));
console.log(cli.green('This is green text'));
console.log(cli.yellow('This is yellow text'));
console.log(cli.blue('This is blue text'));
console.log(cli.magenta('This is magenta text'));
console.log(cli.cyan('This is cyan text'));
console.log(cli.white('This is white text'));
console.log(cli.gray('This is gray text'));

// Test bright colors
console.log('\nBright colors:');
console.log(cli.brightRed('This is bright red'));
console.log(cli.brightGreen('This is bright green'));
console.log(cli.brightYellow('This is bright yellow'));
console.log(cli.brightBlue('This is bright blue'));

// Test background colors
console.log('\nBackground colors:');
console.log(cli.bgRed('Red background'));
console.log(cli.bgGreen('Green background'));
console.log(cli.bgYellow('Yellow background'));
console.log(cli.bgBlue('Blue background'));

// Test styles
console.log('\nText styles:');
console.log(cli.bold('This is bold text'));
console.log(cli.italic('This is italic text'));
console.log(cli.underline('This is underlined text'));
console.log(cli.strikethrough('This is strikethrough text'));
console.log(cli.inverse('This is inverse text'));
console.log(cli.dim('This is dim text'));

// Test 256 colors
console.log('\n256 Colors:');
console.log(cli.color(196)('This is color 196 (red)'));
console.log(cli.color(46)('This is color 46 (green)'));
console.log(cli.color(21)('This is color 21 (blue)'));
console.log(cli.bgColor(226)('This has background color 226 (yellow)'));

// Test RGB colors
console.log('\nRGB Colors:');
console.log(cli.rgb(255, 128, 0)('This is orange (RGB)'));
console.log(cli.rgb(128, 0, 255)('This is purple (RGB)'));
console.log(cli.bgRgb(0, 128, 128)('This has teal background (RGB)'));

// Test hex colors
console.log('\nHex Colors:');
console.log(cli.hex('#FF8800')('This is orange from hex'));
console.log(cli.hex('#8800FF')('This is purple from hex'));
console.log(cli.bgHex('#008080')('This has teal background from hex'));

// Test strip ANSI
console.log('\nStrip ANSI test:');
const coloredText = cli.red('This is colored');
console.log('With color:', coloredText);
console.log('Stripped:', cli.stripAnsi(coloredText));

// Test screen capabilities
console.log('\nScreen info:');
const screenSize = cli.getScreenSize();
console.log('Screen size:', screenSize);

const capabilities = cli.getCapabilities();
console.log('Terminal capabilities:', capabilities);

console.log('Supports color:', cli.supportsColor());
console.log('Supports unicode:', cli.supportsUnicode());

// Test cursor operations
console.log('\nCursor operations (visual test):');
cli.write('Start');
cli.cursorForward(5);
cli.write('Middle');
cli.cursorBackward(3);
cli.write('X');
cli.writeLine(' <- X should be in the middle of "Middle"');

// Save and restore cursor
cli.write('Before save...');
cli.saveCursor();
cli.cursorForward(10);
cli.write('After move');
cli.restoreCursor();
cli.writeLine(' <- Back to saved position');

// Test progress bar
console.log('\nProgress bar test:');
const progress = cli.createProgressBar(100, 40);
for (let i = 0; i <= 100; i += 10) {
    progress.update(i);
    await new Promise(resolve => setTimeout(resolve, 200));
}
progress.complete();

// Test spinner
console.log('\nSpinner test:');
const spinner = cli.createSpinner('Loading...');
spinner.start();
await new Promise(resolve => setTimeout(resolve, 2000));
spinner.succeed('Loading complete!');

// Test basic input/output
console.log('\nBasic I/O test:');
cli.writeLine('This is a line written with writeLine');
cli.write('This is written with write (no newline)');
cli.writeLine(' <- continued on same line');

// Test beep (may not work in all terminals)
console.log('\nTesting beep...');
cli.beep();

// Test input methods (interactive)
console.log('\nInteractive tests:');
const name = await cli.input('Enter your name: ');
console.log(`Hello, ${name}!`);

const password = await cli.password('Enter a password: ', '*');
console.log(`Password length: ${password.length}`);

const confirmed = await cli.confirm('Do you want to continue?', true);
console.log(`Confirmed: ${confirmed}`);

const choice = await cli.select('Choose your favorite color:', ['Red', 'Green', 'Blue', 'Yellow']);
console.log(`You chose: ${choice}`);

// Clear operations demo
console.log('\nPress Enter to test clear operations...');
await cli.input('');

cli.writeLine('Line 1');
cli.writeLine('Line 2');
cli.writeLine('Line 3');
await new Promise(resolve => setTimeout(resolve, 1000));

cli.cursorUp(2);
cli.clearLineRight();
cli.writeLine('Line 2 - cleared and replaced');

await new Promise(resolve => setTimeout(resolve, 1000));
cli.clearDown();
cli.writeLine('Everything below was cleared');

console.log('\nCLI module test complete!');