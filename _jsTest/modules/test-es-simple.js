// Very simple ES module test
import fs from 'ekko:fs';

console.log('ES module import worked!');
console.log('fs type:', typeof fs);
console.log('fs.writeFileSync:', typeof fs.writeFileSync);