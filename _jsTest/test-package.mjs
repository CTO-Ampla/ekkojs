// Test loading a compiled package with static import
import demo from "package:@demo/hello-world";
import { greet, add, version } from "package:@demo/hello-world";

console.log("Testing package loading...");
console.log("Package imported successfully!");
console.log("Demo default export:", demo);
console.log("Version:", version);

// Test default export
if (demo.greet) {
    const greeting = demo.greet("EkkoJS Package System");
    console.log("Default greeting:", greeting);
}

if (demo.add) {
    const result = demo.add(10, 20);
    console.log("Default add result:", result);
}

// Test named exports
const greeting2 = greet("Named Export");
console.log("Named greeting:", greeting2);

const result2 = add(5, 7);
console.log("Named add result:", result2);