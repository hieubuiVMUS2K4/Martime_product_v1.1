const fs = require('fs');
const { execSync } = require('child_process');

// Read markdown file
const mdContent = fs.readFileSync('PHAN_TICH_TONG_QUAN_DU_AN.md', 'utf-8');

// Find all mermaid blocks (with multiline support)
const mermaidRegex = /```mermaid([\s\S]*?)```/gm;
let processedContent = mdContent;
const matches = [...mdContent.matchAll(mermaidRegex)];

console.log(`🔍 Found ${matches.length} Mermaid diagrams`);

// Convert each mermaid block to PNG (better for PDF)
for (let i = 0; i < matches.length; i++) {
  const match = matches[i];
  const mermaidCode = match[1].trim();
  const mmdFile = `temp_diagram_${i}.mmd`;
  const pngFile = `diagram_${i}.png`;
  
  try {
    // Write mermaid code to temp file
    fs.writeFileSync(mmdFile, mermaidCode);
    
    // Convert to PNG using mmdc with higher resolution
    console.log(`⚙️  Converting diagram ${i + 1}/${matches.length} to PNG...`);
    execSync(`mmdc -i ${mmdFile} -o ${pngFile} -t default -b white -w 2400 -s 2`, { 
      stdio: 'pipe'
    });
    
    // Replace mermaid block with image reference
    processedContent = processedContent.replace(
      match[0],
      `\n<div style="text-align: center; padding: 20px; page-break-inside: avoid;">\n<img src="${pngFile}" alt="Diagram ${i + 1}" style="max-width: 100%; height: auto;" />\n</div>\n`
    );
    
    // Clean up temp file
    fs.unlinkSync(mmdFile);
    
    console.log(`✅ Diagram ${i + 1} converted to PNG successfully`);
  } catch (error) {
    console.error(`❌ Error converting diagram ${i}:`, error.message);
  }
}

// Write processed markdown
const outputFile = 'PHAN_TICH_TONG_QUAN_DU_AN_PROCESSED.md';
fs.writeFileSync(outputFile, processedContent);

console.log(`\n✨ Processed markdown saved to: ${outputFile}`);
console.log('📄 Now converting to PDF...\n');

// Convert to PDF
try {
  execSync(`md-to-pdf "${outputFile}" --config-file ".md2pdf.config.js"`, {
    stdio: 'inherit'
  });
  
  // Rename the output
  const pdfFile = outputFile.replace('.md', '.pdf');
  if (fs.existsSync(pdfFile)) {
    fs.renameSync(pdfFile, 'PHAN_TICH_TONG_QUAN_DU_AN.pdf');
    console.log('\n✅ PDF generated successfully with rendered Mermaid diagrams!');
    console.log('📄 File: PHAN_TICH_TONG_QUAN_DU_AN.pdf');
    
    // Clean up temporary files
    fs.unlinkSync(outputFile);
    console.log('\n🧹 Cleaned up temporary files');
  }
} catch (error) {
  console.error('❌ Error generating PDF:', error.message);
}
