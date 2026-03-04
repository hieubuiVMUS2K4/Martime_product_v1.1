import ExcelJS from 'exceljs';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function createTemplate() {
  // Read the user's template.csv
  const csvPath = path.join(__dirname, '..', 'template.csv');
  const csvContent = fs.readFileSync(csvPath, 'utf-8');
  
  // Parse CSV
  const rows = csvContent.split('\n').map(line => {
    const cells = [];
    let current = '';
    let inQuotes = false;
    
    for (let i = 0; i < line.length; i++) {
      const char = line[i];
      if (char === '"') {
        inQuotes = !inQuotes;
      } else if (char === ',' && !inQuotes) {
        cells.push(current.trim());
        current = '';
      } else {
        current += char;
      }
    }
    cells.push(current.trim());
    return cells;
  });
  
  const workbook = new ExcelJS.Workbook();
  const worksheet = workbook.addWorksheet('BIO-DATA', {
    pageSetup: { paperSize: 9, orientation: 'portrait' }
  });
  
  // Set reasonable column widths for all 45 columns (not too narrow)
  for (let i = 1; i <= 45; i++) {
    worksheet.getColumn(i).width = 5; // 5 characters wide, not 3
  }
  
  const borderStyle = {
    top: { style: 'thin' },
    left: { style: 'thin' },
    bottom: { style: 'thin' },
    right: { style: 'thin' }
  };
  
  // Load all rows from CSV
  rows.forEach((rowData, rowIndex) => {
    const row = worksheet.getRow(rowIndex + 1);
    row.height = 20;
    
    rowData.forEach((cellValue, colIndex) => {
      if (cellValue) {
        const cell = row.getCell(colIndex + 1);
        cell.value = cellValue;
        cell.border = borderStyle;
        
        // Special formatting for section headers (rows starting with "1.", "2.", etc)
        if (cellValue.match(/^\d+\.\s+\w/)) {
          cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFD3D3D3' } };
          cell.font = { bold: true };
        }
        
        // Title row (BIO - DATA)
        if (cellValue === 'BIO - DATA') {
          cell.font = { size: 16, bold: true };
          cell.alignment = { horizontal: 'center', vertical: 'middle' };
        }
      }
    });
  });
  
  // Save template
  const outputPath = path.join(__dirname, 'public', 'template.xlsx');
  await workbook.xlsx.writeFile(outputPath);
  console.log(`✅ Template created from user's CSV: ${outputPath}`);
}

createTemplate().catch(console.error);
