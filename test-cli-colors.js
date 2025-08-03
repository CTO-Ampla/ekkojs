// Test CLI module colors and styles
import cli from 'ekko:cli';

console.log('Testing CLI colors and styles...\n');

// Basic colors
console.log(cli.red('This is red text'));
console.log(cli.green('This is green text'));
console.log(cli.yellow('This is yellow text'));
console.log(cli.blue('This is blue text'));
console.log(cli.magenta('This is magenta text'));
console.log(cli.cyan('This is cyan text'));
console.log(cli.white('This is white text'));
console.log(cli.gray('This is gray text'));

console.log('\nBright colors:');
console.log(cli.brightRed('This is bright red'));
console.log(cli.brightGreen('This is bright green'));
console.log(cli.brightYellow('This is bright yellow'));
console.log(cli.brightBlue('This is bright blue'));

console.log('\nBackground colors:');
console.log(cli.bgRed('Red background'));
console.log(cli.bgGreen('Green background'));
console.log(cli.bgYellow('Yellow background'));
console.log(cli.bgBlue('Blue background'));

console.log('\nText styles:');
console.log(cli.bold('This is bold text'));
console.log(cli.italic('This is italic text'));
console.log(cli.underline('This is underlined text'));
console.log(cli.strikethrough('This is strikethrough text'));
console.log(cli.inverse('This is inverse text'));
console.log(cli.dim('This is dim text'));

console.log('\n256 Colors:');
console.log(cli.color(196)('This is color 196 (red)'));
console.log(cli.color(46)('This is color 46 (green)'));
console.log(cli.color(21)('This is color 21 (blue)'));
console.log(cli.bgColor(226)('This has background color 226 (yellow)'));

console.log('\nRGB Colors:');
console.log(cli.rgb(255, 128, 0)('This is orange (RGB)'));
console.log(cli.rgb(128, 0, 255)('This is purple (RGB)'));
console.log(cli.bgRgb(0, 128, 128)('This has teal background (RGB)'));

console.log('\nHex Colors:');
console.log(cli.hex('#FF8800')('This is orange from hex'));
console.log(cli.hex('#8800FF')('This is purple from hex'));
console.log(cli.bgHex('#008080')('This has teal background from hex'));

console.log('\nStrip ANSI test:');
const coloredText = cli.red('This is colored');
console.log('With color:', coloredText);
console.log('Stripped:', cli.stripAnsi(coloredText));

console.log('\nTerminal capabilities:');
const capabilities = cli.getCapabilities();
console.log('Terminal capabilities:', JSON.stringify(capabilities, null, 2));
console.log('Supports color:', cli.supportsColor());
console.log('Supports unicode:', cli.supportsUnicode());

console.log('\nScreen info:');
const screenSize = cli.getScreenSize();
console.log('Screen size:', JSON.stringify(screenSize, null, 2));

console.log('\nBeep test (you should hear a beep):');
cli.beep();