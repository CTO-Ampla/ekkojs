#!/usr/bin/env node

// Script to extract Mermaid diagrams and convert them to images

const fs = require('fs');
const path = require('path');
const https = require('https');

// Extract Mermaid diagrams from markdown content
function extractMermaidDiagrams(content, fileName) {
    const diagrams = [];
    const regex = /```mermaid\n([\s\S]*?)```/g;
    let match;
    let index = 0;
    
    while ((match = regex.exec(content)) !== null) {
        index++;
        diagrams.push({
            content: match[1].trim(),
            fileName: fileName,
            index: index,
            startLine: content.substring(0, match.index).split('\n').length,
            endLine: content.substring(0, match.index + match[0].length).split('\n').length
        });
    }
    
    return diagrams;
}

// Encode diagram to Base64
function encodeToBase64(diagramText) {
    const buffer = Buffer.from(diagramText, 'utf-8');
    return buffer.toString('base64');
}

// Create Mermaid Ink URL
function createMermaidInkUrl(base64) {
    return `https://mermaid.ink/img/${base64}`;
}

// Download image from URL
async function downloadImage(url, outputPath) {
    return new Promise((resolve, reject) => {
        const file = fs.createWriteStream(outputPath);
        
        https.get(url, (response) => {
            if (response.statusCode === 200) {
                response.pipe(file);
                file.on('finish', () => {
                    file.close();
                    resolve(outputPath);
                });
            } else {
                reject(new Error(`Failed to download: ${response.statusCode}`));
            }
        }).on('error', (err) => {
            fs.unlink(outputPath, () => {}); // Delete the file on error
            reject(err);
        });
    });
}

// Process a single diagram
async function processDiagram(diagram, outputDir) {
    try {
        console.log(`Processing diagram ${diagram.index} from ${diagram.fileName}...`);
        
        // Encode to Base64
        const base64 = encodeToBase64(diagram.content);
        const url = createMermaidInkUrl(base64);
        
        // Create output filename
        const baseName = path.basename(diagram.fileName, '.md');
        const outputFileName = `${baseName}_diagram_${diagram.index}.png`;
        const outputPath = path.join(outputDir, outputFileName);
        
        // Download the image
        await downloadImage(url, outputPath);
        console.log(`✓ Saved: ${outputFileName}`);
        
        return {
            ...diagram,
            base64: base64,
            url: url,
            outputPath: outputPath,
            outputFileName: outputFileName
        };
    } catch (error) {
        console.error(`✗ Error processing diagram ${diagram.index}: ${error.message}`);
        return null;
    }
}

// Update markdown file with image links
function updateMarkdownWithImages(filePath, processedDiagrams) {
    let content = fs.readFileSync(filePath, 'utf8');
    let offset = 0;
    
    // Sort diagrams by position in file (reverse order to maintain positions)
    const sortedDiagrams = processedDiagrams
        .filter(d => d !== null)
        .sort((a, b) => b.endLine - a.endLine);
    
    for (const diagram of sortedDiagrams) {
        const lines = content.split('\n');
        
        // Find the mermaid block
        let mermaidStart = -1;
        let mermaidEnd = -1;
        let currentLine = 0;
        
        for (let i = 0; i < lines.length; i++) {
            if (lines[i].trim() === '```mermaid') {
                currentLine++;
                if (currentLine === diagram.index) {
                    mermaidStart = i;
                    // Find the closing ```
                    for (let j = i + 1; j < lines.length; j++) {
                        if (lines[j].trim() === '```') {
                            mermaidEnd = j;
                            break;
                        }
                    }
                    break;
                }
            }
        }
        
        if (mermaidStart !== -1 && mermaidEnd !== -1) {
            // Insert image link after the mermaid block
            const imageLink = `\n![${path.basename(diagram.fileName, '.md')} Diagram ${diagram.index}](${diagram.url})`;
            lines.splice(mermaidEnd + 1, 0, imageLink);
            content = lines.join('\n');
        }
    }
    
    // Write the updated content back
    const backupPath = filePath + '.backup';
    fs.copyFileSync(filePath, backupPath);
    fs.writeFileSync(filePath, content, 'utf8');
    console.log(`✓ Updated ${filePath} with image links (backup saved as ${path.basename(backupPath)})`);
}

// Main processing function
async function processMarkdownFiles() {
    const docsDir = '/mnt/d/git/ekkojs/docs';
    const outputDir = path.join(docsDir, 'diagrams');
    
    // Create output directory if it doesn't exist
    if (!fs.existsSync(outputDir)) {
        fs.mkdirSync(outputDir, { recursive: true });
    }
    
    // Files to process
    const files = [
        'TUI_MODULE_DESIGN.md',
        'CLI_MODULE_DESIGN.md',
        'ARCHITECTURE.md'
    ];
    
    for (const fileName of files) {
        const filePath = path.join(docsDir, fileName);
        
        if (!fs.existsSync(filePath)) {
            console.error(`File not found: ${filePath}`);
            continue;
        }
        
        console.log(`\nProcessing ${fileName}...`);
        
        // Read file content
        const content = fs.readFileSync(filePath, 'utf8');
        
        // Extract diagrams
        const diagrams = extractMermaidDiagrams(content, fileName);
        console.log(`Found ${diagrams.length} Mermaid diagrams`);
        
        if (diagrams.length === 0) continue;
        
        // Process each diagram
        const processedDiagrams = [];
        for (const diagram of diagrams) {
            const processed = await processDiagram(diagram, outputDir);
            processedDiagrams.push(processed);
            
            // Add a small delay to avoid rate limiting
            await new Promise(resolve => setTimeout(resolve, 1000));
        }
        
        // Update the markdown file with image links
        updateMarkdownWithImages(filePath, processedDiagrams);
    }
    
    console.log('\n✓ All diagrams processed successfully!');
}

// Run the script
processMarkdownFiles().catch(console.error);