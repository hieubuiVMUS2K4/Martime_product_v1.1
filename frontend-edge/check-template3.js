import ExcelJS from 'exceljs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function checkTemplate() {
  const workbook = new ExcelJS.Workbook();
  await workbook.xlsx.readFile(path.join(__dirname, '..', 'template1.xlsx'));
  
  const worksheet = workbook.worksheets[0];
  
  // Check specific data cells (cells right after label merges)
  const checkCells = [
    // Row 4 data cells
    'K4', 'L4', 'M4', // after "Crew code" G4:J4
    'T4', 'U4',        // after "Present Rank" Q4:S4 (T4:X4 has "WPR")
    'AD4', 'AE4',      // after "Prepared by" Z4:AC4
    'AN4', 'AO4',      // after "Date Prepared" AK4:AM4
    
    // Row 8-9 data cells  
    'L8', 'L9',        // "Full name" label is at L8, but L8:Z8 is merged
    'AA8', 'AE8', 'AM8', // labels
    
    // Row 9 - likely the DATA row for row 8 labels
    'H9', 'K9', 'L9', 'M9', 'AA9', 'AE9', 'AM9',
    
    // Row 10 data cells
    'L10', 'V10', 'X10',
    
    // Row 11 data cells
    'L11', 'S11', 'X11', 'AM11',
    
    // Row 12 data cells
    'S12', 'X12', 'AE12', 'AK12', 'AP12',
    
    // Row 13 data cells
    'P13', 'X13',  'AK13', 'AP13',
    
    // Row 14 data cells
    'P14', 'X14',
  ];
  
  console.log('=== CHECKING SPECIFIC CELLS FOR DATA POSITIONS ===\n');
  
  for (const addr of checkCells) {
    const cell = worksheet.getCell(addr);
    const master = cell.master ? cell.master.address : addr;
    const isMerged = master !== addr;
    const val = cell.value;
    console.log(`${addr}: value=${JSON.stringify(val)} | master=${master} | merged=${isMerged}`);
  }
  
  // Now check Row 9 comprehensively
  console.log('\n=== ROW 9 ALL CELLS ===');
  for (let col = 1; col <= 45; col++) {
    const cell = worksheet.getCell(9, col);
    const addr = cell.address;
    const master = cell.master ? cell.master.address : addr;
    const isMaster = master === addr;
    if (cell.value || isMaster) {
      console.log(`Col ${col} (${addr}): value=${JSON.stringify(cell.value)} master=${master}`);
    }
  }
  
  // Check Row 10 comprehensively  
  console.log('\n=== ROW 10 ALL NON-EMPTY MASTER CELLS ===');
  for (let col = 1; col <= 45; col++) {
    const cell = worksheet.getCell(10, col);
    const addr = cell.address;
    const master = cell.master ? cell.master.address : addr;
    if (master === addr) {
      console.log(`Col ${col} (${addr}): value=${JSON.stringify(cell.value)} isMergedMaster=${cell.isMerged}`);
    }
  }
}

checkTemplate().catch(console.error);
