#!/usr/bin/env node

// Script to update image links to use local files instead of mermaid.ink URLs

const fs = require('fs');
const path = require('path');

// Files to process
const files = [
    'TUI_MODULE_DESIGN.md',
    'CLI_MODULE_DESIGN.md',
    'ARCHITECTURE.md'
];

const docsDir = '/mnt/d/git/ekkojs/docs';

for (const fileName of files) {
    const filePath = path.join(docsDir, fileName);
    console.log(`Processing ${fileName}...`);
    
    // Read the file
    let content = fs.readFileSync(filePath, 'utf8');
    
    // Replace mermaid.ink URLs with local image paths
    // Pattern: ![XXX Diagram N](https://mermaid.ink/img/...)
    const pattern = /!\[([^\]]+) Diagram (\d+)\]\(https:\/\/mermaid\.ink\/img\/[^)]+\)/g;
    
    content = content.replace(pattern, (match, docName, diagramNum) => {
        // Construct the local image filename
        const imageFileName = `${fileName.replace('.md', '')}_diagram_${diagramNum}.png`;
        const relativePath = `diagrams/${imageFileName}`;
        
        console.log(`  Replacing diagram ${diagramNum} with local image: ${relativePath}`);
        
        return `![${docName} Diagram ${diagramNum}](${relativePath})`;
    });
    
    // Write the updated content back
    fs.writeFileSync(filePath, content, 'utf8');
    console.log(`✓ Updated ${fileName}`);
}

console.log('\n✓ All files updated to use local images!');