// Complete Demo of EkkoJS CLI Components (Fast Version)
import cli from 'ekko:cli';

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

async function runDemo() {
    cli.clear();
    
    console.log(cli.bold(cli.blue('╔═══════════════════════════════════════════════════════════╗')));
    console.log(cli.bold(cli.blue('║            EkkoJS CLI - Complete Demo (Fast)             ║')));
    console.log(cli.bold(cli.blue('╚═══════════════════════════════════════════════════════════╝')));
    console.log();

    // 1. Colors & Styles
    console.log(cli.bold(cli.cyan('1. Colors & Styles')));
    console.log(`  ${cli.red('Red')} ${cli.green('Green')} ${cli.blue('Blue')} ${cli.yellow('Yellow')}`);
    console.log(`  ${cli.bold('Bold')} ${cli.italic('Italic')} ${cli.underline('Underline')}`);
    console.log();

    // 2. Progress Bar (Fast)
    console.log(cli.bold(cli.cyan('2. Progress Bar')));
    const progressBar = cli.createProgressBar(100, 30);
    for (let i = 0; i <= 100; i += 20) {
        progressBar.update(i);
        await sleep(200);
    }
    progressBar.complete();
    console.log();

    // 3. Spinner (Fast)
    console.log(cli.bold(cli.cyan('3. Spinner')));
    const spinner = cli.createSpinner('Processing...');
    spinner.start();
    await sleep(1000);
    spinner.succeed('Complete!');
    console.log();

    // 4. Table
    console.log(cli.bold(cli.cyan('4. Table')));
    cli.table([
        ['Name', 'Status', 'Score'],
        ['Alice', cli.green('Active'), '95'],
        ['Bob', cli.yellow('Pending'), '82'],
        ['Charlie', cli.red('Inactive'), '73']
    ]);
    console.log();

    // 5. Box
    console.log(cli.bold(cli.cyan('5. Box Drawing')));
    cli.box('Welcome to EkkoJS CLI!\nFull-featured terminal UI.');
    console.log();

    // 6. Tree
    console.log(cli.bold(cli.cyan('6. Tree Display')));
    cli.tree({
        label: 'Project',
        children: [
            { label: 'src', children: [
                { label: 'index.js' },
                { label: 'utils.js' }
            ]},
            { label: 'package.json' }
        ]
    });
    console.log();

    // 7. String Utils
    console.log(cli.bold(cli.cyan('7. String Utilities')));
    const text = 'Hello World! 世界';
    console.log(`  Width: ${cli.stringWidth(text)}`);
    console.log(`  Truncated: ${cli.truncate(text, 10)}`);
    console.log(`  Centered: [${cli.center('Hi', 10)}]`);
    console.log();

    console.log(cli.bold(cli.green('✨ All Components Demonstrated!')));
}

// Run the demo
runDemo().catch(error => {
    console.error(cli.red('Error:'), error.message);
});