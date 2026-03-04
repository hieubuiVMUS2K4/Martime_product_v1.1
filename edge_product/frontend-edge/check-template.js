import ExcelJS from 'exceljs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function checkTemplate() {
  const workbook = new ExcelJS.Workbook();
  await workbook.xlsx.readFile(path.join(__dirname, '..', 'template1.xlsx'));
  
  const worksheet = workbook.worksheets[0];
  
  console.log('=== Checking first rows for any content ===');
  for (let row = 1; row <= 15; row++) {
    console.log(`\nRow ${row}:`);
    for (let col = 1; col <= 45; col++) {
      const cell = worksheet.getCell(row, col);
      if (cell.value) {
        const colLetter = String.fromCharCode(64 + (col > 26 ? Math.floor((col-1)/26) + 64 : 0)) + String.fromCharCode(65 + ((col-1) % 26));
        console.log(`  Col ${col}: "${cell.value}" ${cell.master && cell.master.address !== cell.address ? '(merged to ' + cell.master.address + ')' : ''}`);
      }
    }
  }
}

checkTemplate().catch(console.error);
