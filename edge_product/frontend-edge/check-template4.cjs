const ExcelJS = require('exceljs');
const path = require('path');

async function check() {
  const wb = new ExcelJS.Workbook();
  await wb.xlsx.readFile(path.join(__dirname, 'public', 'template1.xlsx'));
  const ws = wb.worksheets[0];

  console.log('=== PHOTO AREA: Check merged cells in rows 1-6, cols 1-7 (A-G) ===');
  const merges = ws.model?.merges || [];
  merges.forEach(m => {
    // Check if merge is in top-left area (photo region)
    if (m.match(/[A-G][1-6]/)) {
      console.log('  Merge:', m);
    }
  });

  console.log('\n=== ROW 1-6 CONTENT (for photo position) ===');
  for (let r = 1; r <= 6; r++) {
    const row = ws.getRow(r);
    for (let c = 1; c <= 45; c++) {
      const cell = row.getCell(c);
      if (cell.value) {
        const colLetter = ExcelJS.utils ? String.fromCharCode(64 + c) : `col${c}`;
        console.log(`  Row ${r}, Col ${c}: "${cell.value}"`);
      }
    }
  }

  console.log('\n=== SERVICE RECORDS AREA (Rows 44-52) ===');
  for (let r = 44; r <= 52; r++) {
    const row = ws.getRow(r);
    const vals = [];
    for (let c = 1; c <= 45; c++) {
      const cell = row.getCell(c);
      if (cell.value) {
        vals.push(`col${c}="${cell.value}"`);
      }
    }
    if (vals.length > 0) {
      console.log(`  Row ${r}: ${vals.join(' | ')}`);
    } else {
      console.log(`  Row ${r}: (empty)`);
    }
  }

  console.log('\n=== MERGES in rows 44-52 ===');
  merges.forEach(m => {
    const match = m.match(/(\d+)/);
    if (match) {
      const rowNum = parseInt(match[1]);
      if (rowNum >= 44 && rowNum <= 52) {
        console.log('  ', m);
      }
    }
  });

  console.log('\n=== MERGES in rows 1-6 ===');
  merges.forEach(m => {
    const match = m.match(/[A-Z]+(\d+)/);
    if (match) {
      const rowNum = parseInt(match[1]);
      if (rowNum >= 1 && rowNum <= 6) {
        console.log('  ', m);
      }
    }
  });

  console.log('\n=== ALL IMAGES in template ===');
  if (ws.getImages) {
    const images = ws.getImages();
    console.log('  Images found:', images.length);
    images.forEach((img, i) => {
      console.log(`  Image ${i}:`, JSON.stringify(img));
    });
  }

  // Check column widths to understand photo placement
  console.log('\n=== COLUMN WIDTHS (cols 1-10 and 38-45) ===');
  for (let c = 1; c <= 10; c++) {
    const col = ws.getColumn(c);
    console.log(`  Col ${c}: width=${col.width}`);
  }
  for (let c = 38; c <= 45; c++) {
    const col = ws.getColumn(c);
    console.log(`  Col ${c}: width=${col.width}`);
  }

  // Check row heights for photo area
  console.log('\n=== ROW HEIGHTS (rows 1-14) ===');
  for (let r = 1; r <= 14; r++) {
    const row = ws.getRow(r);
    console.log(`  Row ${r}: height=${row.height}`);
  }
}

check().catch(console.error);
