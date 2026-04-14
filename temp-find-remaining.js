const fs = require('fs');
const b = fs.readFileSync('edge_product/frontend-edge/src/pages/Crew/CertificateMonitorView.tsx');
const text = fs.readFileSync('edge_product/frontend-edge/src/pages/Crew/CertificateMonitorView.tsx', 'utf8');
const lines = text.split('\n');

// Find remaining double-encoded 3-byte patterns
for (let i = 0; i < b.length - 5; i++) {
  if (b[i] === 0xC3 && b[i+1] >= 0xA0 && b[i+1] <= 0xAF && b[i+2] === 0xC2) {
    // Find which line this is on
    const before = b.slice(0, i).toString('utf8');
    const lineNum = before.split('\n').length;
    const hex = [];
    for (let j = Math.max(0, i-5); j < Math.min(b.length, i+10); j++) {
      hex.push(b[j].toString(16).padStart(2, '0'));
    }
    console.log(`Offset ${i}, Line ${lineNum}: ${hex.join(' ')}`);
    console.log(`  Line content: ${lines[lineNum-1].trim().substring(0, 120)}`);
  }
}
