console.log("Starting countdown from 5...\n");

let count = 5;

const countdownInterval = setInterval(() => {
    console.log(`  ${count}...`);
    count--;
    
    if (count === 0) {
        clearInterval(countdownInterval);
        console.log("\n🚀 Blast off!");
        
        // Celebrate with some delayed messages
        setTimeout(() => console.log("✨ EkkoJS timers are working!"), 100);
        setTimeout(() => console.log("🎉 Great job!"), 200);
    }
}, 500);