// Fix double-encoded UTF-8 in files (Windows-1252 variant)
// Double encoding: UTF-8 bytes → interpreted as Windows-1252 → re-encoded as UTF-8
// This handles the special 0x80-0x9F range that Windows-1252 maps differently from Latin-1

const fs = require('fs');

// Windows-1252 to byte mapping (reverse): Unicode code point → original Win-1252 byte
const WIN1252_REVERSE = new Map();
const WIN1252_MAP = {
  0x80: 0x20AC, 0x82: 0x201A, 0x83: 0x0192, 0x84: 0x201E, 0x85: 0x2026,
  0x86: 0x2020, 0x87: 0x2021, 0x88: 0x02C6, 0x89: 0x2030, 0x8A: 0x0160,
  0x8B: 0x2039, 0x8C: 0x0152, 0x8E: 0x017D, 0x91: 0x2018, 0x92: 0x2019,
  0x93: 0x201C, 0x94: 0x201D, 0x95: 0x2022, 0x96: 0x2013, 0x97: 0x2014,
  0x98: 0x02DC, 0x99: 0x2122, 0x9A: 0x0161, 0x9B: 0x203A, 0x9C: 0x0153,
  0x9E: 0x017E, 0x9F: 0x0178
};
for (const [byte, cp] of Object.entries(WIN1252_MAP)) {
  WIN1252_REVERSE.set(Number(cp), Number(byte));
}
// Map 0x80-0x9F C1 control chars to themselves (Latin-1 fallback for undefined Win-1252 bytes)
for (let b = 0x80; b <= 0x9F; b++) {
  if (!WIN1252_REVERSE.has(b)) {
    WIN1252_REVERSE.set(b, b);
  }
}
// Also map A0-FF directly (same as Latin-1)
for (let b = 0xA0; b <= 0xFF; b++) {
  WIN1252_REVERSE.set(b, b);
}

function unicodeCharToWin1252Byte(codePoint) {
  if (codePoint < 0x80) return codePoint; // ASCII
  if (WIN1252_REVERSE.has(codePoint)) return WIN1252_REVERSE.get(codePoint);
  return -1; // Not a Win-1252 character
}

function fixDoubleEncodedUTF8(filePath) {
  const text = fs.readFileSync(filePath, 'utf8');
  const chars = [...text]; // Split into Unicode characters
  const result = [];
  let fixCount = 0;
  let i = 0;

  while (i < chars.length) {
    const cp = chars[i].codePointAt(0);
    
    // Check if this could be the start of a double-encoded sequence
    // In double encoding, the original UTF-8 bytes (0x80-0xFF) become Win-1252 characters
    // A double-encoded sequence looks like: [Win1252 char for byte1][Win1252 char for byte2]...
    // where byte1 byte2... form a valid original UTF-8 sequence
    
    const b1 = unicodeCharToWin1252Byte(cp);
    
    if (b1 >= 0xC0 && b1 <= 0xF4 && i + 1 < chars.length) {
      // Potential start of a double-encoded UTF-8 sequence
      // Determine expected length based on first byte
      let expectedBytes;
      if (b1 >= 0xC0 && b1 <= 0xDF) expectedBytes = 2;
      else if (b1 >= 0xE0 && b1 <= 0xEF) expectedBytes = 3;
      else if (b1 >= 0xF0 && b1 <= 0xF4) expectedBytes = 4;
      else { result.push(chars[i]); i++; continue; }
      
      // Try to read the expected number of continuation "bytes"
      const bytes = [b1];
      let valid = true;
      for (let j = 1; j < expectedBytes && i + j < chars.length; j++) {
        const bj = unicodeCharToWin1252Byte(chars[i + j].codePointAt(0));
        if (bj >= 0x80 && bj <= 0xBF) {
          bytes.push(bj);
        } else {
          valid = false;
          break;
        }
      }
      
      if (valid && bytes.length === expectedBytes) {
        // Reconstruct the original UTF-8 bytes and decode
        try {
          const originalBuf = Buffer.from(bytes);
          const decoded = originalBuf.toString('utf8');
          // Verify it decoded to a valid character (not replacement char)
          if (!decoded.includes('\uFFFD') && decoded.length > 0) {
            result.push(decoded);
            fixCount++;
            i += expectedBytes;
            continue;
          }
        } catch (e) {
          // Fall through
        }
      }
    }
    
    // No double-encoding detected, keep as-is
    result.push(chars[i]);
    i++;
  }

  const fixed = result.join('');
  console.log(`Fixed ${fixCount} double-encoded sequences in ${filePath}`);
  
  fs.writeFileSync(filePath, fixed, 'utf8');
  console.log('File saved successfully.');
  
  // Show samples of fixed text
  const lines = fixed.split('\n');
  for (let l = 0; l < lines.length; l++) {
    const line = lines[l];
    if (line.includes('xếp') || line.includes('lượng') || line.includes('ời gian') || 
        line.includes('ăng d') || line.includes('ảm d') || line.includes('Previous') ||
        line.includes('arrow')) {
      console.log('  Line ' + (l+1) + ': ' + line.trim().substring(0, 100));
    }
  }
}

const file = process.argv[2];
if (!file) {
  console.log('Usage: node fix-double-encoding.js <file>');
  process.exit(1);
}
fixDoubleEncodedUTF8(file);