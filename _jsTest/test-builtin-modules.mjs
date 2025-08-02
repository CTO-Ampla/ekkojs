// Test built-in modules with static imports
import fs from "ekko:fs";
import path from "ekko:path";

console.log("Testing built-in modules...");

// Test fs module
console.log("\n=== Testing ekko:fs ===");
const files = fs.readdir(".");
console.log("Type of files:", typeof files);
console.log("Files:", files);
if (Array.isArray(files)) {
    console.log("Files in current directory:", files.slice(0, 5), "...");
}

// Test path module
console.log("\n=== Testing ekko:path ===");
console.log("Path separator:", path.sep);
console.log("Join paths:", path.join("folder", "subfolder", "file.txt"));
console.log("Dirname:", path.dirname("/home/user/file.txt"));
console.log("Basename:", path.basename("/home/user/file.txt"));
console.log("Extension:", path.extname("file.txt"));

// Test absolute path
const absPath = path.resolve("test.js");
console.log("Absolute path:", absPath);
console.log("Is absolute?", path.isAbsolute(absPath));

console.log("\nAll built-in modules working correctly!");