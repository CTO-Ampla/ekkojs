// Complete CLI Module Showcase - Non-interactive version
import cli from 'ekko:cli';

// Helper function for delays
const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

// Helper function to create section headers
function section(title) {
    console.log('\n' + '='.repeat(60));
    console.log(cli.bold(cli.cyan(`  ${title}  `)));
    console.log('='.repeat(60) + '\n');
}

// Main showcase function
async function showcase() {
    cli.clear();
    console.log(cli.bold(cli.green('╔═══════════════════════════════════════════════════════╗')));
    console.log(cli.bold(cli.green('║     EkkoJS CLI Module - Complete Feature Showcase     ║')));
    console.log(cli.bold(cli.green('╚═══════════════════════════════════════════════════════╝')));
    console.log(cli.dim('\nShowcasing all implemented CLI components\n'));
    
    // ========================================
    // 1. COLOR SYSTEM
    // ========================================
    section('COLOR SYSTEM');
    
    // Basic colors
    console.log(cli.bold('Basic Colors:'));
    console.log(cli.red('█ Red') + ' ' + cli.green('█ Green') + ' ' + cli.yellow('█ Yellow') + ' ' + cli.blue('█ Blue'));
    console.log(cli.magenta('█ Magenta') + ' ' + cli.cyan('█ Cyan') + ' ' + cli.white('█ White') + ' ' + cli.gray('█ Gray'));
    
    // Bright colors
    console.log('\n' + cli.bold('Bright Colors:'));
    console.log(cli.brightRed('█ Bright Red') + ' ' + cli.brightGreen('█ Bright Green') + ' ' + cli.brightYellow('█ Bright Yellow'));
    console.log(cli.brightBlue('█ Bright Blue') + ' ' + cli.brightMagenta('█ Bright Magenta') + ' ' + cli.brightCyan('█ Bright Cyan'));
    
    // Background colors
    console.log('\n' + cli.bold('Background Colors:'));
    console.log(cli.bgRed('  RED  ') + ' ' + cli.bgGreen('  GREEN  ') + ' ' + cli.bgYellow('  YELLOW  ') + ' ' + cli.bgBlue('  BLUE  '));
    console.log(cli.bgMagenta('  MAGENTA  ') + ' ' + cli.bgCyan('  CYAN  ') + ' ' + cli.bgWhite(cli.black('  WHITE  ')) + ' ' + cli.bgGray('  GRAY  '));
    
    // 256 color palette
    console.log('\n' + cli.bold('256 Color Palette:'));
    for (let row = 0; row < 8; row++) {
        let line = '';
        for (let col = 0; col < 32; col++) {
            const color = row * 32 + col;
            line += cli.bgColor(color)(' ');
        }
        console.log(line);
    }
    
    // RGB gradient
    console.log('\n' + cli.bold('RGB Color Gradient:'));
    let gradient = '';
    for (let i = 0; i < 60; i++) {
        const hue = i * 6;
        const r = Math.floor(128 + 127 * Math.sin(hue * Math.PI / 180));
        const g = Math.floor(128 + 127 * Math.sin((hue + 120) * Math.PI / 180));
        const b = Math.floor(128 + 127 * Math.sin((hue + 240) * Math.PI / 180));
        gradient += cli.rgb(r, g, b)('█');
    }
    console.log(gradient);
    
    // Hex colors
    console.log('\n' + cli.bold('Hex Color Examples:'));
    console.log(cli.hex('#FF5733')('█ Coral #FF5733') + '  ' + cli.hex('#C70039')('█ Red #C70039') + '  ' + cli.hex('#900C3F')('█ Wine #900C3F'));
    console.log(cli.hex('#581845')('█ Purple #581845') + '  ' + cli.hex('#1ABC9C')('█ Turquoise #1ABC9C') + '  ' + cli.hex('#F39C12')('█ Orange #F39C12'));
    
    // ========================================
    // 2. TEXT STYLES
    // ========================================
    section('TEXT STYLES');
    
    console.log(cli.bold('Bold text for emphasis'));
    console.log(cli.italic('Italic text for style'));
    console.log(cli.underline('Underlined text for importance'));
    console.log(cli.strikethrough('Strikethrough for deprecated items'));
    console.log(cli.inverse('Inverse for highlighting'));
    console.log(cli.dim('Dim text for secondary information'));
    console.log('Hidden text example: ' + cli.hidden('This text is hidden') + ' (between the colons)');
    
    // Style combinations
    console.log('\n' + cli.bold('Style Combinations:'));
    console.log(cli.bold(cli.red('Bold + Red')));
    console.log(cli.underline(cli.blue('Underline + Blue')));
    console.log(cli.italic(cli.green('Italic + Green')));
    console.log(cli.bold(cli.underline(cli.yellow('Bold + Underline + Yellow'))));
    console.log(cli.inverse(cli.magenta('Inverse + Magenta')));
    
    // ========================================
    // 3. CURSOR & SCREEN CONTROL
    // ========================================
    section('CURSOR & SCREEN CONTROL');
    
    console.log(cli.bold('Screen Information:'));
    const screen = cli.getScreenSize();
    console.log(`  Terminal size: ${screen.width}×${screen.height}`);
    
    console.log('\n' + cli.bold('Cursor Movement Demo:'));
    cli.write('Start→');
    await sleep(200);
    cli.cursorBackward();
    cli.cursorBackward();
    cli.write('|');
    await sleep(200);
    cli.cursorForward();
    cli.cursorForward();
    cli.writeLine(' End');
    
    console.log('\n' + cli.bold('Clear Line Demo:'));
    cli.write('This line will be partially cleared...');
    await sleep(500);
    cli.cursorBackward();
    cli.cursorBackward();
    cli.cursorBackward();
    cli.cursorBackward();
    cli.cursorBackward();
    cli.clearLineRight();
    cli.writeLine('[CLEARED]');
    
    // ========================================
    // 4. PROGRESS INDICATORS
    // ========================================
    section('PROGRESS INDICATORS');
    
    console.log(cli.bold('Progress Bar:'));
    const progress = cli.createProgressBar(50, 30);
    for (let i = 0; i <= 50; i++) {
        progress.update(i);
        await sleep(20);
    }
    progress.complete();
    
    console.log('\n' + cli.bold('Spinner States:'));
    
    const spinner1 = cli.createSpinner('Loading resources...');
    spinner1.start();
    await sleep(1000);
    spinner1.succeed('Resources loaded successfully!');
    
    const spinner2 = cli.createSpinner('Connecting to server...');
    spinner2.start();
    await sleep(800);
    spinner2.fail('Failed to connect to server');
    
    const spinner3 = cli.createSpinner('Checking dependencies...');
    spinner3.start();
    await sleep(800);
    spinner3.warn('Some dependencies are outdated');
    
    const spinner4 = cli.createSpinner('Gathering information...');
    spinner4.start();
    await sleep(800);
    spinner4.info('Found 42 items');
    
    // ========================================
    // 5. TERMINAL CAPABILITIES
    // ========================================
    section('TERMINAL CAPABILITIES');
    
    const caps = cli.getCapabilities();
    console.log(cli.bold('System Information:'));
    console.log(`  • Color support: ${cli.green(caps.colors + ' colors')}`);
    console.log(`  • Unicode: ${caps.unicode ? cli.green('✓ Supported') : cli.red('✗ Not supported')}`);
    console.log(`  • Interactive: ${caps.interactive ? cli.green('✓ Yes') : cli.yellow('○ No')}`);
    console.log(`  • Platform: ${caps.isWindows ? 'Windows' : caps.isMac ? 'macOS' : caps.isLinux ? 'Linux' : 'Unknown'}`);
    console.log(`  • Terminal: ${caps.program}`);
    
    console.log('\n' + cli.bold('Feature Detection:'));
    console.log(`  • Colors: ${cli.supportsColor() ? cli.green('✓ Supported') : cli.red('✗ Not supported')}`);
    console.log(`  • Unicode: ${cli.supportsUnicode() ? cli.green('✓ Supported') : cli.red('✗ Not supported')}`);
    
    // ========================================
    // 6. SPECIAL FEATURES
    // ========================================
    section('SPECIAL FEATURES');
    
    console.log(cli.bold('ANSI Stripping:'));
    const styled = cli.bold(cli.red('This is bold red text'));
    console.log('  Original: ' + styled);
    console.log('  Stripped: ' + cli.stripAnsi(styled));
    
    console.log('\n' + cli.bold('Unicode Examples:'));
    if (cli.supportsUnicode()) {
        console.log('  Symbols: ✓ ✗ ⚠ ℹ ▶ ▼ ◆ ●');
        console.log('  Arrows: → ← ↑ ↓ ⇒ ⇐ ⇑ ⇓');
        console.log('  Box Drawing: ┌─┬─┐ │ │ │ ├─┼─┤ │ │ │ └─┴─┘');
        console.log('  Blocks: ▀▄█▌▐░▒▓');
        console.log('  Emoji: 🚀 🎨 ⚡ 🔥 💡 🎯 ✨ 🌟');
    } else {
        console.log('  Unicode not supported in this terminal');
    }
    
    console.log('\n' + cli.bold('Input Methods Available:'));
    console.log('  • ' + cli.green('cli.input()') + ' - Text input');
    console.log('  • ' + cli.green('cli.password()') + ' - Hidden password input');
    console.log('  • ' + cli.green('cli.confirm()') + ' - Yes/no confirmation');
    console.log('  • ' + cli.green('cli.select()') + ' - Single selection menu');
    
    console.log('\n' + cli.bold('System Beep:'));
    console.log('  Playing system beep...');
    cli.beep();
    
    // ========================================
    // FEATURE MATRIX
    // ========================================
    section('IMPLEMENTATION STATUS');
    
    console.log(cli.bold(cli.green('✓ Implemented Features:')));
    console.log('  ✓ Basic colors (red, green, blue, etc.)');
    console.log('  ✓ Bright color variants');
    console.log('  ✓ Background colors');
    console.log('  ✓ 256 color support');
    console.log('  ✓ RGB true color support');
    console.log('  ✓ Hex color support');
    console.log('  ✓ Text styles (bold, italic, underline, etc.)');
    console.log('  ✓ Cursor positioning and movement');
    console.log('  ✓ Screen clearing operations');
    console.log('  ✓ Progress bars');
    console.log('  ✓ Spinners with states');
    console.log('  ✓ Basic input (text, password, confirm, select)');
    console.log('  ✓ Terminal capability detection');
    console.log('  ✓ ANSI code stripping');
    console.log('  ✓ System beep');
    
    console.log('\n' + cli.bold(cli.yellow('○ Planned Features:')));
    console.log('  ○ Tables with borders and alignment');
    console.log('  ○ Box drawing with styles');
    console.log('  ○ Tree structure display');
    console.log('  ○ Multi-select menus');
    console.log('  ○ Autocomplete input');
    console.log('  ○ Number/date input');
    console.log('  ○ Form builder');
    console.log('  ○ String measurement utilities');
    console.log('  ○ Text wrapping and padding');
    console.log('  ○ Hyperlink support');
    console.log('  ○ Multi-progress bars');
    console.log('  ○ Custom spinner frames');
    console.log('  ○ Alternative screen buffer');
    
    // ========================================
    // CLOSING
    // ========================================
    console.log('\n' + cli.bold(cli.green('═'.repeat(60))));
    console.log(cli.bold(cli.green('  Demo Complete! The CLI module is ready for use.  ')));
    console.log(cli.bold(cli.green('═'.repeat(60))));
    
    console.log('\n' + cli.dim('EkkoJS CLI Module v1.0'));
    console.log(cli.dim('For more information, see docs/CLI_MODULE_DESIGN.md'));
}

// Run the showcase
showcase().catch(console.error);