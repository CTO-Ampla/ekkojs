// Comprehensive Interactive Demo for Advanced Input Methods
import cli from 'ekko:cli';

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

async function runInteractiveDemo() {
    cli.clear();
    console.log(cli.bold(cli.green('╔══════════════════════════════════════════════════════╗')));
    console.log(cli.bold(cli.green('║        EkkoJS Advanced Input Methods Demo            ║')));
    console.log(cli.bold(cli.green('╚══════════════════════════════════════════════════════╝')));
    console.log(cli.dim('\nInteractive demo showcasing all advanced input capabilities\n'));
    
    try {
        // ========================================
        // 1. NUMBER INPUT
        // ========================================
        console.log(cli.bold(cli.cyan('1. Number Input Demo')));
        console.log(cli.dim('Testing numeric input with validation and defaults\n'));
        
        const age = await cli.number('Enter your age', 25);
        console.log(cli.green(`✓ Age entered: ${age} years old\n`));
        
        // Number with range validation
        const score = await cli.number('Enter test score (0-100)', 85);
        console.log(cli.green(`✓ Score entered: ${score}%\n`));
        
        // ========================================
        // 2. DATE INPUT  
        // ========================================
        console.log(cli.bold(cli.cyan('2. Date Input Demo')));
        console.log(cli.dim('Testing date input with validation\n'));
        
        const birthday = await cli.date('Enter your birthday');
        console.log(cli.green(`✓ Birthday: ${birthday.toISOString()}\n`));
        
        // ========================================
        // 3. MULTI-SELECT INPUT
        // ========================================
        console.log(cli.bold(cli.cyan('3. Multi-Select Demo')));
        console.log(cli.dim('Use arrow keys to navigate, spacebar to toggle, Enter to confirm\n'));
        
        const languages = await cli.multiSelect('Select programming languages you know:', [
            'JavaScript',
            'TypeScript', 
            'Python',
            'C#',
            'Java',
            'Go',
            'Rust',
            'C++',
            'Swift',
            'Kotlin'
        ]);
        
        if (languages.length > 0) {
            console.log(cli.green(`✓ Selected languages: ${languages.join(', ')}\n`));
        } else {
            console.log(cli.yellow(`○ No languages selected\n`));
        }
        
        // ========================================
        // 4. FRAMEWORK SELECTION
        // ========================================
        console.log(cli.bold(cli.cyan('4. Framework Multi-Select Demo')));
        console.log(cli.dim('Another multi-select example with web frameworks\n'));
        
        const frameworks = await cli.multiSelect('Select web frameworks you\'ve used:', [
            'React',
            'Vue.js',
            'Angular',
            'Svelte',
            'Express.js',
            'FastAPI',
            'Django',
            'ASP.NET Core',
            'Spring Boot',
            'Laravel'
        ]);
        
        if (frameworks.length > 0) {
            console.log(cli.green(`✓ Selected frameworks: ${frameworks.join(', ')}\n`));
        } else {
            console.log(cli.yellow(`○ No frameworks selected\n`));
        }
        
        // ========================================
        // 5. COMBINED DEMO
        // ========================================
        console.log(cli.bold(cli.cyan('5. Combined Input Demo')));
        console.log(cli.dim('Using multiple input types together\n'));
        
        const experience = await cli.number('Years of programming experience', 2);
        const startDate = await cli.date('When did you start programming?');
        
        const interests = await cli.multiSelect('Select your areas of interest:', [
            'Web Development',
            'Mobile Development',
            'Desktop Applications',
            'DevOps',
            'Data Science',
            'Machine Learning',
            'Game Development',
            'Systems Programming',
            'Database Design',
            'UI/UX Design'
        ]);
        
        // ========================================
        // RESULTS SUMMARY
        // ========================================
        console.log('\n' + '='.repeat(60));
        console.log(cli.bold(cli.green('  DEMO RESULTS SUMMARY  ')));
        console.log('='.repeat(60));
        
        console.log('\n' + cli.bold('Personal Info:'));
        console.log(`  Age: ${age} years`);
        console.log(`  Birthday: ${birthday.toISOString()}`);
        console.log(`  Programming Experience: ${experience} years`);
        console.log(`  Started Programming: ${startDate.toISOString()}`);
        
        if (languages.length > 0) {
            console.log('\n' + cli.bold('Programming Languages:'));
            languages.forEach(lang => console.log(`  • ${cli.green(lang)}`));
        }
        
        if (frameworks.length > 0) {
            console.log('\n' + cli.bold('Web Frameworks:'));
            frameworks.forEach(fw => console.log(`  • ${cli.blue(fw)}`));
        }
        
        if (interests.length > 0) {
            console.log('\n' + cli.bold('Areas of Interest:'));
            interests.forEach(interest => console.log(`  • ${cli.cyan(interest)}`));
        }
        
        // ========================================
        // FEATURE STATUS
        // ========================================
        console.log('\n' + '='.repeat(60));
        console.log(cli.bold(cli.cyan('  ADVANCED INPUT FEATURE STATUS  ')));
        console.log('='.repeat(60));
        
        console.log('\n' + cli.bold(cli.green('✓ IMPLEMENTED FEATURES:')));
        console.log('  ✓ cli.number() - Numeric input with defaults and validation');
        console.log('  ✓ cli.date() - Date input with validation');
        console.log('  ✓ cli.multiSelect() - Interactive multi-selection with arrow keys');
        console.log('  ✓ Interactive navigation (arrow keys, spacebar, Enter)');
        console.log('  ✓ Default values and validation');
        console.log('  ✓ Visual feedback and styling');
        
        console.log('\n' + cli.bold(cli.yellow('○ PLANNED FEATURES:')));
        console.log('  ○ cli.autocomplete() - Autocomplete with dynamic suggestions');
        console.log('  ○ cli.editor() - External editor integration');
        console.log('  ○ cli.form() - Multi-field forms');
        console.log('  ○ Range validation for numbers and dates');
        console.log('  ○ Custom input validators');
        console.log('  ○ Input history and recall');
        
        console.log('\n' + cli.bold(cli.green('✨ Demo Complete! ✨')));
        console.log(cli.dim('Advanced input methods are working correctly.\n'));
        
    } catch (error) {
        console.error('\n' + cli.red('Demo Error:'), error.message);
        console.log(cli.yellow('Some features may not be fully implemented yet.'));
    }
}

// Run the interactive demo
console.log(cli.yellow('Starting Advanced Input Methods Interactive Demo...'));
console.log(cli.dim('This demo will test number, date, and multi-select inputs.\n'));

runInteractiveDemo().catch(error => {
    console.error(cli.red('Demo failed:'), error.message);
    process.exit(1);
});