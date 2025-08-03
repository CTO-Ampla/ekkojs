// Test multiSelect with large list that might overflow console buffer
import cli from 'ekko:cli';

console.log(cli.bold(cli.green('=== Large MultiSelect Test ===')));
console.log('Testing multiSelect with a large list that might overflow the console buffer\n');

// Create a large list of choices that will likely exceed console buffer height
const largeChoiceList = [
    'React', 'Vue.js', 'Angular', 'Svelte', 'Next.js', 'Nuxt.js', 'Gatsby',
    'Express.js', 'Koa.js', 'Fastify', 'Hapi.js', 'Nest.js', 'Adonis.js',
    'FastAPI', 'Django', 'Flask', 'Tornado', 'Quart', 'Sanic',
    'ASP.NET Core', 'Spring Boot', 'Laravel', 'Symfony', 'CodeIgniter',
    'Ruby on Rails', 'Sinatra', 'Phoenix', 'Cowboy', 'Plug',
    'Gin', 'Echo', 'Fiber', 'Beego', 'Revel',
    'Actix', 'Rocket', 'Warp', 'Axum', 'Tide',
    'Java Servlet', 'Jersey', 'Dropwizard', 'Micronaut', 'Quarkus',
    'Ktor', 'Vert.x', 'Play Framework', 'Akka HTTP',
    'Node.js', 'Deno', 'Bun', 'Python', 'Java', 'C#', 'Go', 'Rust',
    'JavaScript', 'TypeScript', 'PHP', 'Ruby', 'Elixir', 'Kotlin', 'Scala',
    'HTML', 'CSS', 'SASS', 'LESS', 'Stylus', 'PostCSS',
    'Webpack', 'Vite', 'Rollup', 'Parcel', 'ESBuild', 'SWC'
];

console.log(`Testing with ${largeChoiceList.length} choices:`);

try {
    const selected = await cli.multiSelect('Select the technologies you have experience with:', largeChoiceList);
    
    console.log('\n' + cli.green('✓ MultiSelect completed successfully!'));
    if (selected.length > 0) {
        console.log(cli.cyan('Selected technologies:'));
        selected.forEach(tech => console.log(`  • ${tech}`));
    } else {
        console.log(cli.yellow('No technologies selected.'));
    }
    
} catch (error) {
    console.error(cli.red('✗ MultiSelect error:'), error.message);
    console.error('Stack:', error.stack);
}

console.log('\n' + cli.bold(cli.green('=== Large MultiSelect Test Complete ===')));