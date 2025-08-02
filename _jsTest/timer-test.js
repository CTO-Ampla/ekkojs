console.log("Starting timer tests...");

// Test 1: Simple setTimeout
console.log("1. Testing setTimeout");
setTimeout(() => {
    console.log("  ✓ setTimeout fired after 100ms");
}, 100);

// Test 2: setTimeout with arguments
setTimeout((name, value) => {
    console.log(`  ✓ setTimeout with args: ${name} = ${value}`);
}, 200, "test", 42);

// Test 3: setInterval
console.log("2. Testing setInterval");
let intervalCount = 0;
const intervalId = setInterval(() => {
    intervalCount++;
    console.log(`  ✓ setInterval tick ${intervalCount}`);
    if (intervalCount >= 3) {
        clearInterval(intervalId);
        console.log("  ✓ clearInterval worked");
    }
}, 150);

// Test 4: clearTimeout
console.log("3. Testing clearTimeout");
const timeoutId = setTimeout(() => {
    console.log("  ✗ This should not appear!");
}, 500);
clearTimeout(timeoutId);
console.log("  ✓ clearTimeout called");

// Test 5: Nested timers
setTimeout(() => {
    console.log("4. Testing nested timers");
    console.log("  ✓ Outer timeout fired");
    setTimeout(() => {
        console.log("  ✓ Inner timeout fired");
    }, 100);
}, 300);

// Test 6: Zero delay
setTimeout(() => {
    console.log("5. Testing zero delay");
    console.log("  ✓ Zero delay timeout fired");
}, 0);

// Test 7: Multiple timers
console.log("6. Testing multiple simultaneous timers");
setTimeout(() => console.log("  ✓ Timer A (50ms)"), 50);
setTimeout(() => console.log("  ✓ Timer B (75ms)"), 75);
setTimeout(() => console.log("  ✓ Timer C (25ms)"), 25);

// Keep the process alive for 1 second to see all results
setTimeout(() => {
    console.log("\nAll timer tests completed!");
}, 1000);