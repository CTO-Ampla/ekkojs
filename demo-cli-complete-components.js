// Complete CLI Module Demo - Every Component from Design Document
import cli from 'ekko:cli';

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

function section(title) {
    console.log('\n' + '='.repeat(60));
    console.log(cli.bold(cli.cyan(`  ${title}  `)));
    console.log('='.repeat(60) + '\n');
}

async function runCompleteDemo() {
    cli.clear();
    console.log(cli.bold(cli.green('EkkoJS CLI Module - Complete Component Demo')));
    console.log(cli.dim('Comprehensive demo showcasing ALL components from CLI_MODULE_DESIGN.md'));
    console.log();
    
    // ========================================
    // 1. COLOR AND STYLE SYSTEM (COMPLETE)
    // ========================================
    section('1. COLOR AND STYLE SYSTEM');
    
    // Basic colors
    console.log(cli.bold('Basic Colors:'));
    console.log(cli.red('Red') + ' ' + cli.green('Green') + ' ' + cli.yellow('Yellow') + ' ' + cli.blue('Blue'));
    console.log(cli.magenta('Magenta') + ' ' + cli.cyan('Cyan') + ' ' + cli.white('White') + ' ' + cli.gray('Gray') + ' ' + cli.black(cli.bgWhite('Black')));
    
    // Bright variants
    console.log('\n' + cli.bold('Bright Colors:'));
    console.log(cli.brightRed('BrightRed') + ' ' + cli.brightGreen('BrightGreen') + ' ' + cli.brightYellow('BrightYellow'));
    console.log(cli.brightBlue('BrightBlue') + ' ' + cli.brightMagenta('BrightMagenta') + ' ' + cli.brightCyan('BrightCyan') + ' ' + cli.brightWhite('BrightWhite'));
    
    // Background colors
    console.log('\n' + cli.bold('Background Colors:'));
    console.log(cli.bgRed('  BgRed  ') + ' ' + cli.bgGreen('  BgGreen  ') + ' ' + cli.bgYellow('  BgYellow  ') + ' ' + cli.bgBlue('  BgBlue  '));
    console.log(cli.bgMagenta('  BgMagenta  ') + ' ' + cli.bgCyan('  BgCyan  ') + ' ' + cli.bgWhite(cli.black('  BgWhite  ')) + ' ' + cli.bgGray('  BgGray  '));
    
    // Text styles
    console.log('\n' + cli.bold('Text Styles:'));
    console.log(cli.bold('Bold') + ' ' + cli.italic('Italic') + ' ' + cli.underline('Underline') + ' ' + cli.strikethrough('Strikethrough'));
    console.log(cli.inverse('Inverse') + ' ' + cli.dim('Dim') + ' ' + cli.hidden('Hidden'));
    
    // 256 color support
    console.log('\n' + cli.bold('256 Color Support (first 32 colors):'));
    let colorLine = '';
    for (let i = 0; i < 32; i++) {
        colorLine += cli.bgColor(i)('  ');
    }
    console.log(colorLine);
    
    // RGB support
    console.log('\n' + cli.bold('RGB Support:'));
    console.log(cli.rgb(255, 128, 0)('Orange RGB(255,128,0)') + ' ' + cli.bgRgb(0, 128, 255)('  Blue BG  '));
    
    // Hex support
    console.log('\n' + cli.bold('Hex Support:'));
    console.log(cli.hex('#FF8800')('Orange #FF8800') + ' ' + cli.bgHex('#8800FF')('  Purple BG  '));
    
    // Style combinations
    console.log('\n' + cli.bold('Style Combinations:'));
    console.log(cli.bold(cli.red('Bold + Red')));
    console.log(cli.underline(cli.blue('Underline + Blue')));
    console.log(cli.italic(cli.bgYellow(cli.black('Italic + Yellow BG'))));
    
    // Strip ANSI
    console.log('\n' + cli.bold('Strip ANSI:'));
    const styledText = cli.red(cli.bold('Styled text'));
    console.log('With style: ' + styledText);
    console.log('Without style: ' + cli.stripAnsi(styledText));
    
    await cli.input('\\nPress Enter to continue to Screen & Cursor Control...');
    
    // ========================================
    // 2. SCREEN AND CURSOR CONTROL (COMPLETE)
    // ========================================
    cli.clear();
    section('2. SCREEN AND CURSOR CONTROL');
    
    // Screen information
    console.log(cli.bold('Screen Information:'));
    const screenSize = cli.getScreenSize();
    console.log(`Terminal size: ${screenSize.width}×${screenSize.height}`);
    
    // Clear operations
    console.log('\n' + cli.bold('Clear Operations Demo:'));
    cli.writeLine('Line 1 - This will remain');
    cli.write('Line 2 - This will be partially cleared→');
    await sleep(1000);
    cli.cursorBackward(); cli.cursorBackward(); cli.cursorBackward();
    cli.clearLineRight();
    cli.writeLine('[CLEARED]');
    
    // Cursor movement
    console.log('\n' + cli.bold('Cursor Movement:'));
    cli.write('Start→');
    await sleep(500);
    cli.cursorForward(); cli.cursorForward(); cli.cursorForward();
    cli.write('Forward→');
    await sleep(500);
    cli.cursorBackward(); cli.cursorBackward();
    cli.write('←Back');
    cli.cursorToColumn(0);
    cli.writeLine('\\nCursor positioned');
    
    // Save/restore cursor
    console.log('\n' + cli.bold('Save/Restore Cursor:'));
    cli.write('Position saved→');
    cli.saveCursor();
    cli.cursorDown();
    cli.writeLine('Moved down...');
    await sleep(1000);
    cli.restoreCursor();
    cli.writeLine(' [RESTORED]');
    
    // Hide/show cursor
    console.log('\n' + cli.bold('Cursor Visibility:'));
    console.log('Hiding cursor for 2 seconds...');
    cli.hideCursor();
    await sleep(2000);
    cli.showCursor();
    console.log('Cursor restored!');
    
    await cli.input('\\nPress Enter to continue to Input System...');
    
    // ========================================
    // 3. INPUT SYSTEM
    // ========================================
    cli.clear();
    section('3. INPUT SYSTEM');
    
    console.log(cli.bold('Basic Input Methods:'));
    
    // Text input
    const name = await cli.input('Enter your name: ');
    console.log(cli.green(`Hello, ${name}!`));
    
    // Password input
    const password = await cli.password('Enter a password (hidden): ');
    console.log(cli.dim(`Password entered (${password.length} characters)`));
    
    // Confirmation
    const proceed = await cli.confirm('Continue with advanced demos?');
    console.log(proceed ? cli.green('Great! Continuing...') : cli.red('Skipping advanced features...'));
    
    // Selection menu
    console.log('\\n' + cli.bold('Selection Menu:'));
    const color = await cli.select('Choose your favorite color:', [
        'Red', 'Green', 'Blue', 'Yellow', 'Purple', 'Orange'
    ]);
    console.log(cli.green(`You selected: ${color}`));
    
    // Note about missing advanced input features
    console.log('\\n' + cli.bold(cli.yellow('Advanced Input Features (Not Yet Implemented):')));
    console.log(cli.dim('The following input features from the design document are planned:'));
    console.log(cli.dim('  • cli.multiSelect() - Multiple choice selection'));
    console.log(cli.dim('  • cli.autocomplete() - Autocomplete input'));
    console.log(cli.dim('  • cli.number() - Number input with validation'));
    console.log(cli.dim('  • cli.date() - Date picker'));
    console.log(cli.dim('  • cli.editor() - External editor integration'));
    console.log(cli.dim('  • cli.form() - Multi-field forms'));
    
    await cli.input('\\nPress Enter to continue to Progress Indicators...');
    
    // ========================================
    // 4. PROGRESS INDICATORS (PARTIAL)
    // ========================================
    cli.clear();
    section('4. PROGRESS INDICATORS');
    
    // Basic progress bar
    console.log(cli.bold('Progress Bar:'));
    const progress = cli.createProgressBar(100, 40);
    for (let i = 0; i <= 100; i += 5) {
        progress.update(i);
        await sleep(50);
    }
    progress.complete();
    console.log(cli.green('Progress complete!'));
    
    // Spinner states
    console.log('\\n' + cli.bold('Spinner States:'));
    
    const spinner1 = cli.createSpinner('Loading...');
    spinner1.start();
    await sleep(1000);
    spinner1.succeed('Load successful!');
    
    const spinner2 = cli.createSpinner('Processing...');
    spinner2.start();
    await sleep(800);
    spinner2.fail('Process failed!');
    
    const spinner3 = cli.createSpinner('Checking...');
    spinner3.start();
    await sleep(800);
    spinner3.warn('Warning found!');
    
    const spinner4 = cli.createSpinner('Analyzing...');
    spinner4.start();
    await sleep(800);
    spinner4.info('Analysis complete');
    
    // Note about missing progress features
    console.log('\\n' + cli.bold(cli.yellow('Advanced Progress Features (Not Yet Implemented):')));
    console.log(cli.dim('  • cli.createMultiProgress() - Multiple progress bars'));
    console.log(cli.dim('  • Custom progress bar formatting'));
    console.log(cli.dim('  • Custom spinner frames and intervals'));
    console.log(cli.dim('  • Progress bar with custom tokens'));
    
    await cli.input('\\nPress Enter to continue to Output Formatting...');
    
    // ========================================
    // 5. OUTPUT FORMATTING (NOT IMPLEMENTED)
    // ========================================
    cli.clear();
    section('5. OUTPUT FORMATTING');
    
    console.log(cli.bold(cli.yellow('Output Formatting Components (Not Yet Implemented):')));
    console.log('\\nThe following components are specified in the design document but not yet implemented:');
    
    console.log('\\n' + cli.bold('Tables:'));
    console.log('  • cli.table() - Simple and advanced table rendering');
    console.log('  • Column configuration with width, alignment, formatting');
    console.log('  • Border styles (single, double, round, bold, ascii)');
    console.log('  • Header styling and cell formatting');
    
    console.log('\\n' + cli.bold('Box Drawing:'));
    console.log('  • cli.box() - Draw boxes around content');
    console.log('  • Multiple border styles and colors');
    console.log('  • Padding, margins, and title support');
    console.log('  • Horizontal and vertical box layouts');
    
    console.log('\\n' + cli.bold('Tree Display:'));
    console.log('  • cli.tree() - Hierarchical tree structures');
    console.log('  • Expandable/collapsible nodes');
    console.log('  • Custom icons and branch characters');
    console.log('  • File system tree visualization');
    
    // Example of what tables would look like
    console.log('\\n' + cli.bold('Example Table Output (Simulated):'));
    console.log('┌─────┬──────────────┬────────────┬──────────┐');
    console.log('│ ID  │ Name         │ Status     │ Progress │');
    console.log('├─────┼──────────────┼────────────┼──────────┤');
    console.log('│ 001 │ Task 1       │ ' + cli.green('Complete') + '   │ 100%     │');
    console.log('│ 002 │ Task 2       │ ' + cli.yellow('Running') + '    │ 75%      │');
    console.log('│ 003 │ Task 3       │ ' + cli.red('Failed') + '     │ 0%       │');
    console.log('└─────┴──────────────┴────────────┴──────────┘');
    
    console.log('\\n' + cli.bold('Example Box Output (Simulated):'));
    console.log('╔══════════════════════════════════════╗');
    console.log('║                Notice                ║');
    console.log('╠══════════════════════════════════════╣');
    console.log('║  This is an important message that   ║');
    console.log('║  would be displayed inside a box     ║');
    console.log('║  with proper padding and borders.    ║');
    console.log('╚══════════════════════════════════════╝');
    
    console.log('\\n' + cli.bold('Example Tree Output (Simulated):'));
    console.log('📁 project');
    console.log('├── 📁 src');
    console.log('│   ├── 📄 index.js');
    console.log('│   ├── 📄 utils.js');
    console.log('│   └── 📁 components');
    console.log('│       ├── ⚛️ Button.jsx');
    console.log('│       └── ⚛️ Input.jsx');
    console.log('└── 📁 tests');
    console.log('    └── 🧪 test.spec.js');
    
    await cli.input('\\nPress Enter to continue to String Utilities...');
    
    // ========================================
    // 6. STRING UTILITIES (NOT IMPLEMENTED)
    // ========================================
    cli.clear();
    section('6. STRING UTILITIES');
    
    console.log(cli.bold(cli.yellow('String Utilities (Not Yet Implemented):')));
    console.log('\\nThe following string utility functions are specified in the design document:');
    
    console.log('\\n' + cli.bold('Measurement Functions:'));
    console.log('  • cli.stringWidth() - Measure display width (accounting for ANSI and Unicode)');
    console.log('  • Example: cli.stringWidth("Hello 世界") → 10');
    console.log('  • Example: cli.stringWidth(cli.red("Hello")) → 5 (ignores ANSI)');
    
    console.log('\\n' + cli.bold('Text Manipulation:'));
    console.log('  • cli.truncate() - Truncate with ellipsis');
    console.log('  • cli.wrap() - Word wrapping');
    console.log('  • cli.pad() - Text padding');
    console.log('  • cli.padLeft() - Left padding');
    console.log('  • cli.padRight() - Right padding');
    console.log('  • cli.center() - Center alignment');
    console.log('  • cli.right() - Right alignment');
    
    console.log('\\n' + cli.bold('Example Usage (Simulated):'));
    console.log('  truncate("Very long string", 10) → "Very lo..."');
    console.log('  pad("Hello", 10) → "  Hello   "');
    console.log('  center("Title", 20) → "       Title        "');
    console.log('  wrap("Long text...", 20) → Multi-line output');
    
    await cli.input('\\nPress Enter to continue to Terminal Capabilities...');
    
    // ========================================
    // 7. TERMINAL CAPABILITIES (IMPLEMENTED)
    // ========================================
    cli.clear();
    section('7. TERMINAL CAPABILITIES');
    
    console.log(cli.bold('Terminal Detection:'));
    const caps = cli.getCapabilities();
    console.log(`  • Color support: ${caps.colors} colors`);
    console.log(`  • Unicode: ${caps.unicode ? cli.green('✓ Supported') : cli.red('✗ Not supported')}`);
    console.log(`  • Interactive: ${caps.interactive ? cli.green('✓ Yes') : cli.yellow('○ No')}`);
    console.log(`  • Width: ${caps.width} columns`);
    console.log(`  • Height: ${caps.height} rows`);
    console.log(`  • Platform: ${caps.isWindows ? 'Windows' : caps.isMac ? 'macOS' : caps.isLinux ? 'Linux' : 'Unknown'}`);
    console.log(`  • Terminal: ${caps.program}`);
    
    console.log('\\n' + cli.bold('Feature Support:'));
    console.log(`  • Colors: ${cli.supportsColor() ? cli.green('✓ Supported') : cli.red('✗ Not supported')}`);
    console.log(`  • Unicode: ${cli.supportsUnicode() ? cli.green('✓ Supported') : cli.red('✗ Not supported')}`);
    
    if (cli.supportsUnicode()) {
        console.log('\\n' + cli.bold('Unicode Examples:'));
        console.log('  Symbols: ✓ ✗ ⚠ ℹ ▶ ▼ ◆ ● ◯ ◐ ◑ ◒ ◓');
        console.log('  Arrows: → ← ↑ ↓ ⇒ ⇐ ⇑ ⇓ ⬆ ⬇ ⬅ ➡');
        console.log('  Blocks: ▀ ▄ █ ▌ ▐ ░ ▒ ▓ ▔ ▕');
        console.log('  Box drawing: ┌─┬─┐ │ │ │ ├─┼─┤ │ │ │ └─┴─┘');
        console.log('  Emoji: 🚀 🎨 ⚡ 🔥 💡 🎯 ✨ 🌟 📊 📈');
    }
    
    await cli.input('\\nPress Enter to continue to Special Features...');
    
    // ========================================
    // 8. SPECIAL FEATURES AND MISSING COMPONENTS
    // ========================================
    cli.clear();
    section('8. SPECIAL FEATURES');
    
    console.log(cli.bold('Implemented Special Features:'));
    console.log('  • System beep (you should hear it):');
    cli.beep();
    await sleep(500);
    
    console.log('\\n' + cli.bold(cli.yellow('Missing Special Features (Not Yet Implemented):')));
    
    console.log('\\n' + cli.bold('Hyperlinks and Media:'));
    console.log('  • cli.link() - Clickable hyperlinks');
    console.log('  • cli.fileLink() - File system links');
    console.log('  • cli.image() - Terminal image display');
    
    console.log('\\n' + cli.bold('Advanced Styling:'));
    console.log('  • Style chaining: cli.bold.red("text")');
    console.log('  • Template literals: cli.template`Status: ${cli.green("OK")}`');
    console.log('  • Conditional styling with style objects');
    
    console.log('\\n' + cli.bold('Screen Management:'));
    console.log('  • cli.enterAlternateScreen() - Full-screen applications');
    console.log('  • cli.exitAlternateScreen() - Return to normal screen');
    console.log('  • cli.scrollUp() / cli.scrollDown() - Manual scrolling');
    
    console.log('\\n' + cli.bold('Color Management:'));
    console.log('  • cli.forceColor() - Force color output');
    console.log('  • cli.forceNoColor() - Disable all colors');
    
    console.log('\\n' + cli.bold('Advanced Cursor:'));
    console.log('  • cli.getCursorPosition() - Get current cursor position');
    console.log('  • cli.setCursorStyle() - Change cursor appearance');
    
    // ========================================
    // IMPLEMENTATION STATUS SUMMARY
    // ========================================
    console.log('\\n' + '='.repeat(60));
    console.log(cli.bold(cli.cyan('  IMPLEMENTATION STATUS SUMMARY  ')));
    console.log('='.repeat(60));
    
    console.log('\\n' + cli.bold(cli.green('✓ FULLY IMPLEMENTED COMPONENTS:')));
    console.log('  ✓ Complete color system (basic, bright, 256, RGB, hex)');
    console.log('  ✓ All text styles (bold, italic, underline, etc.)');
    console.log('  ✓ Background colors');
    console.log('  ✓ Screen and cursor control');
    console.log('  ✓ Basic input (text, password, confirm, select)');
    console.log('  ✓ Progress bars');
    console.log('  ✓ Spinners with state changes');
    console.log('  ✓ Terminal capability detection');
    console.log('  ✓ ANSI code stripping');
    console.log('  ✓ System beep');
    console.log('  ✓ Screen size detection');
    console.log('  ✓ Cursor save/restore');
    console.log('  ✓ Cursor visibility control');
    
    console.log('\\n' + cli.bold(cli.yellow('○ PARTIALLY IMPLEMENTED:')));
    console.log('  ○ Progress indicators (basic only, missing multi-progress)');
    console.log('  ○ Input system (basic only, missing advanced inputs)');
    
    console.log('\\n' + cli.bold(cli.red('✗ NOT YET IMPLEMENTED:')));
    console.log('  ✗ Advanced input (multiSelect, autocomplete, number, date, editor, form)');
    console.log('  ✗ Output formatting (tables, boxes, trees)');
    console.log('  ✗ String utilities (stringWidth, truncate, wrap, pad, center)');
    console.log('  ✗ Hyperlinks and file links');
    console.log('  ✗ Image display');
    console.log('  ✗ Style chaining (cli.bold.red())');
    console.log('  ✗ Template literal support');
    console.log('  ✗ Alternative screen buffer');
    console.log('  ✗ Multi-progress bars');
    console.log('  ✗ Custom spinner configurations');
    console.log('  ✗ Force color settings');
    console.log('  ✗ Cursor position querying');
    console.log('  ✗ Cursor style changes');
    console.log('  ✗ Manual scrolling');
    
    // Calculate implementation percentage
    const totalComponents = 13 + 2 + 13; // fully + partial + missing
    const implementedWeight = 13 * 1.0 + 2 * 0.5; // full weight + partial weight
    const percentage = Math.round((implementedWeight / totalComponents) * 100);
    
    console.log('\\n' + cli.bold(`Implementation Status: ${cli.cyan(percentage + '%')} of design document components`));
    
    console.log('\\n' + cli.bold(cli.green('Demo Complete!')));
    console.log(cli.dim('This demonstrates all currently implemented CLI module features.'));
    console.log(cli.dim('For the complete feature set, see docs/CLI_MODULE_DESIGN.md'));
}

// Run the complete demo
runCompleteDemo().catch(console.error);