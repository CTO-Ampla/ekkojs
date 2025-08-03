// Test string utilities
import cli from 'ekko:cli';

console.log('=== String Utilities Test ===\n');

// Test stringWidth
console.log('1. String Width:');
console.log(`  "Hello": ${cli.stringWidth("Hello")}`);
console.log(`  "Hello 世界": ${cli.stringWidth("Hello 世界")}`);
console.log(`  styled text: ${cli.stringWidth(cli.red("Hello"))}`);

// Test truncate
console.log('\n2. Truncate:');
const longText = "This is a very long string that needs to be truncated";
console.log(`  Original: "${longText}"`);
console.log(`  End (20): "${cli.truncate(longText, 20)}"`);
console.log(`  Start (20): "${cli.truncateStart(longText, 20)}"`);
console.log(`  Middle (20): "${cli.truncateMiddle(longText, 20)}"`);

// Test padding
console.log('\n3. Padding:');
const text = "Hello";
console.log(`  Original: "${text}"`);
console.log(`  Pad (15): "${cli.pad(text, 15)}"`);
console.log(`  PadLeft (15): "${cli.padLeft(text, 15)}"`);
console.log(`  PadRight (15): "${cli.padRight(text, 15)}"`);
console.log(`  Center (15): "${cli.center(text, 15)}"`);
console.log(`  Right (15): "${cli.right(text, 15)}"`);

// Test wrap
console.log('\n4. Wrap:');
const wrapText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
console.log(`  Original: "${wrapText}"`);
console.log('\n  Wrapped (30):');
console.log(cli.wrap(wrapText, 30));
console.log('\n  Wrapped (30, indent 4):');
console.log(cli.wrapIndent(wrapText, 30, 4));

// Test with CJK characters
console.log('\n5. CJK Characters:');
const cjkText = "こんにちは世界";
console.log(`  CJK text: "${cjkText}"`);
console.log(`  Width: ${cli.stringWidth(cjkText)}`);
console.log(`  Truncated (10): "${cli.truncate(cjkText, 10)}"`);
console.log(`  Padded (20): "${cli.pad(cjkText, 20)}"`);

console.log('\n=== Test Complete ===');