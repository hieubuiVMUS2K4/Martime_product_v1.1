import ExcelJS from 'exceljs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function checkTemplate() {
  const workbook = new ExcelJS.Workbook();
  await workbook.xlsx.readFile(path.join(__dirname, '..', 'template1.xlsx'));
  
  const worksheet = workbook.worksheets[0];
  console.log('Sheet name:', worksheet.name);
  console.log('Total rows:', worksheet.rowCount);
  console.log('Total cols:', worksheet.columnCount);
  
  // Check all merged cells
  console.log('\n=== MERGED CELLS ===');
  const merges = worksheet.model.merges || [];
  merges.forEach(m => console.log(m));
  
  // Check all rows with content
  console.log('\n=== ALL ROWS WITH CONTENT (master cells only) ===');
  for (let row = 1; row <= 55; row++) {
    const rowCells = [];
    for (let col = 1; col <= 45; col++) {
      const cell = worksheet.getCell(row, col);
      if (cell.value !== null && cell.value !== undefined) {
        // Only show master cells (not merged copies)
        const addr = cell.address;
        const isMaster = !cell.master || cell.master.address === addr;
        if (isMaster) {
          rowCells.push(`${addr}="${cell.value}"`);
        }
      }
    }
    if (rowCells.length > 0) {
      console.log(`Row ${row}: ${rowCells.join(' | ')}`);
    }
  }
}

checkTemplate().catch(console.error);
