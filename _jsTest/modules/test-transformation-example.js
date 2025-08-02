// Original ES Module Code
import fs from 'ekko:fs';
import { join, resolve } from 'ekko:path';

console.log('Hello from ES module');
const content = fs.readFileSync('test.txt');
const fullPath = join('dir', 'file.js');