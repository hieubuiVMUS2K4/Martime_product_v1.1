const ExcelJS = require('exceljs');
const fs = require('fs');
const path = require('path');

async function createTemplate() {
  const workbook = new ExcelJS.Workbook();
  const worksheet = workbook.addWorksheet('BIO-DATA', {
    pageSetup: { paperSize: 9, orientation: 'portrait' }
  });

  // Set column widths (10 main columns, reasonable widths)
  worksheet.getColumn(1).width = 12;
  worksheet.getColumn(2).width = 18;
  worksheet.getColumn(3).width = 15;
  worksheet.getColumn(4).width = 12;
  worksheet.getColumn(5).width = 18;
  worksheet.getColumn(6).width = 15;
  worksheet.getColumn(7).width = 12;
  worksheet.getColumn(8).width = 18;
  worksheet.getColumn(9).width = 10;
  worksheet.getColumn(10).width = 10;

  const borderStyle = {
    top: { style: 'thin' },
    left: { style: 'thin' },
    bottom: { style: 'thin' },
    right: { style: 'thin' }
  };

  // Row 1: Empty
  worksheet.getRow(1).height = 20;

  // Row 2: Title "BIO - DATA"
  worksheet.mergeCells('A2:J2');
  const titleCell = worksheet.getCell('A2');
  titleCell.value = 'BIO - DATA';
  titleCell.font = { size: 16, bold: true };
  titleCell.alignment = { horizontal: 'center', vertical: 'middle' };
  titleCell.border = borderStyle;
  worksheet.getRow(2).height = 25;

  // Row 3: Empty
  worksheet.getRow(3).height = 15;

  // Row 4: Header info row
  worksheet.getRow(4).height = 20;
  worksheet.mergeCells('A4:B4');
  worksheet.getCell('A4').value = 'Crew code:';
  worksheet.getCell('A4').font = { bold: true };
  worksheet.getCell('A4').border = borderStyle;
  
  worksheet.mergeCells('C4:D4');
  worksheet.getCell('C4').value = '';
  worksheet.getCell('C4').border = borderStyle;
  
  worksheet.mergeCells('E4:F4');
  worksheet.getCell('E4').value = 'Present Rank:';
  worksheet.getCell('E4').font = { bold: true };
  worksheet.getCell('E4').border = borderStyle;
  
  worksheet.mergeCells('G4:H4');
  worksheet.getCell('G4').value = '';
  worksheet.getCell('G4').border = borderStyle;

  // Row 5: Prepared by/Date
  worksheet.getRow(5).height = 20;
  worksheet.mergeCells('A5:B5');
  worksheet.getCell('A5').value = 'Prepared by:';
  worksheet.getCell('A5').font = { bold: true };
  worksheet.getCell('A5').border = borderStyle;
  
  worksheet.mergeCells('C5:F5');
  worksheet.getCell('C5').value = '';
  worksheet.getCell('C5').border = borderStyle;
  
  worksheet.mergeCells('G5:H5');
  worksheet.getCell('G5').value = 'Date Prepared:';
  worksheet.getCell('G5').font = { bold: true };
  worksheet.getCell('G5').border = borderStyle;
  
  worksheet.mergeCells('I5:J5');
  worksheet.getCell('I5').value = '';
  worksheet.getCell('I5').border = borderStyle;

  // Row 6: Empty
  worksheet.getRow(6).height = 15;

  // Row 7: Section 1 header
  worksheet.getRow(7).height = 22;
  worksheet.mergeCells('A7:H7');
  const section1 = worksheet.getCell('A7');
  section1.value = '1. Personal Particular';
  section1.font = { bold: true, size: 11 };
  section1.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
  section1.border = borderStyle;
  section1.alignment = { horizontal: 'left', vertical: 'middle' };
  
  // Photo placeholder (merged cells I7:J12)
  worksheet.mergeCells('I7:J12');
  const photoCell = worksheet.getCell('I7');
  photoCell.value = 'PHOTO';
  photoCell.alignment = { horizontal: 'center', vertical: 'middle' };
  photoCell.border = borderStyle;
  for (let r = 7; r <= 12; r++) {
    worksheet.getRow(r).height = 22;
  }

  // Row 8: Name fields
  worksheet.getRow(8).height = 20;
  worksheet.getCell('A8').value = 'Name:';
  worksheet.getCell('A8').font = { bold: true };
  worksheet.getCell('A8').border = borderStyle;
  
  worksheet.mergeCells('B8:C8');
  worksheet.getCell('B8').value = '';
  worksheet.getCell('B8').border = borderStyle;
  
  worksheet.getCell('D8').value = 'Full name:';
  worksheet.getCell('D8').font = { bold: true };
  worksheet.getCell('D8').border = borderStyle;
  
  worksheet.mergeCells('E8:H8');
  worksheet.getCell('E8').value = '';
  worksheet.getCell('E8').border = borderStyle;

  // Row 9: DOB, POB
  worksheet.getRow(9).height = 20;
  worksheet.getCell('A9').value = 'Date of Birth:';
  worksheet.getCell('A9').font = { bold: true };
  worksheet.getCell('A9').border = borderStyle;
  
  worksheet.mergeCells('B9:C9');
  worksheet.getCell('B9').value = '';
  worksheet.getCell('B9').border = borderStyle;
  
  worksheet.getCell('D9').value = 'Place of Birth:';
  worksheet.getCell('D9').font = { bold: true };
  worksheet.getCell('D9').border = borderStyle;
  
  worksheet.mergeCells('E9:H9');
  worksheet.getCell('E9').value = '';
  worksheet.getCell('E9').border = borderStyle;

  // Row 10: ID, Nationality
  worksheet.getRow(10).height = 20;
  worksheet.getCell('A10').value = 'ID Card Number:';
  worksheet.getCell('A10').font = { bold: true };
  worksheet.getCell('A10').border = borderStyle;
  
  worksheet.mergeCells('B10:C10');
  worksheet.getCell('B10').value = '';
  worksheet.getCell('B10').border = borderStyle;
  
  worksheet.getCell('D10').value = 'Nationality:';
  worksheet.getCell('D10').font = { bold: true };
  worksheet.getCell('D10').border = borderStyle;
  
  worksheet.mergeCells('E10:H10');
  worksheet.getCell('E10').value = '';
  worksheet.getCell('E10').border = borderStyle;

  // Row 11: Address
  worksheet.getRow(11).height = 20;
  worksheet.getCell('A11').value = 'Permanent Address:';
  worksheet.getCell('A11').font = { bold: true };
  worksheet.getCell('A11').border = borderStyle;
  
  worksheet.mergeCells('B11:H11');
  worksheet.getCell('B11').value = '';
  worksheet.getCell('B11').border = borderStyle;

  // Row 12: Phone, Email
  worksheet.getRow(12).height = 20;
  worksheet.getCell('A12').value = 'Phone:';
  worksheet.getCell('A12').font = { bold: true };
  worksheet.getCell('A12').border = borderStyle;
  
  worksheet.mergeCells('B12:C12');
  worksheet.getCell('B12').value = '';
  worksheet.getCell('B12').border = borderStyle;
  
  worksheet.getCell('D12').value = 'Email:';
  worksheet.getCell('D12').font = { bold: true };
  worksheet.getCell('D12').border = borderStyle;
  
  worksheet.mergeCells('E12:H12');
  worksheet.getCell('E12').value = '';
  worksheet.getCell('E12').border = borderStyle;

  // Row 13: Empty
  worksheet.getRow(13).height = 15;

  // Row 14: Height, Weight
  worksheet.getRow(14).height = 20;
  worksheet.mergeCells('A14:J14');
  const section2 = worksheet.getCell('A14');
  section2.value = '2. Physical Details';
  section2.font = { bold: true, size: 11 };
  section2.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
  section2.border = borderStyle;

  // Row 15: Height, Weight values
  worksheet.getRow(15).height = 20;
  worksheet.getCell('A15').value = 'Height:';
  worksheet.getCell('A15').font = { bold: true };
  worksheet.getCell('A15').border = borderStyle;
  
  worksheet.mergeCells('B15:C15');
  worksheet.getCell('B15').value = '';
  worksheet.getCell('B15').border = borderStyle;
  
  worksheet.getCell('D15').value = 'Weight:';
  worksheet.getCell('D15').font = { bold: true };
  worksheet.getCell('D15').border = borderStyle;
  
  worksheet.mergeCells('E15:F15');
  worksheet.getCell('E15').value = '';
  worksheet.getCell('E15').border = borderStyle;
  
  worksheet.getCell('G15').value = 'Eye Color:';
  worksheet.getCell('G15').font = { bold: true };
  worksheet.getCell('G15').border = borderStyle;
  
  worksheet.mergeCells('H15:J15');
  worksheet.getCell('H15').value = '';
  worksheet.getCell('H15').border = borderStyle;

  // Row 16: Empty
  worksheet.getRow(16).height = 15;

  // Row 17: Next of Kin section
  worksheet.getRow(17).height = 20;
  worksheet.mergeCells('A17:J17');
  const section3 = worksheet.getCell('A17');
  section3.value = '3. Next of Kin';
  section3.font = { bold: true, size: 11 };
  section3.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
  section3.border = borderStyle;

  // Row 18: Next of Kin Name
  worksheet.getRow(18).height = 20;
  worksheet.getCell('A18').value = 'Name:';
  worksheet.getCell('A18').font = { bold: true };
  worksheet.getCell('A18').border = borderStyle;
  
  worksheet.mergeCells('B18:E18');
  worksheet.getCell('B18').value = '';
  worksheet.getCell('B18').border = borderStyle;
  
  worksheet.getCell('F18').value = 'Relationship:';
  worksheet.getCell('F18').font = { bold: true };
  worksheet.getCell('F18').border = borderStyle;
  
  worksheet.mergeCells('G18:J18');
  worksheet.getCell('G18').value = '';
  worksheet.getCell('G18').border = borderStyle;

  // Row 19: Next of Kin Phone
  worksheet.getRow(19).height = 20;
  worksheet.getCell('A19').value = 'Phone:';
  worksheet.getCell('A19').font = { bold: true };
  worksheet.getCell('A19').border = borderStyle;
  
  worksheet.mergeCells('B19:E19');
  worksheet.getCell('B19').value = '';
  worksheet.getCell('B19').border = borderStyle;
  
  worksheet.getCell('F19').value = 'Address:';
  worksheet.getCell('F19').font = { bold: true };
  worksheet.getCell('F19').border = borderStyle;
  
  worksheet.mergeCells('G19:J19');
  worksheet.getCell('G19').value = '';
  worksheet.getCell('G19').border = borderStyle;

  // Additional sections placeholder
  const sections = [
    { row: 21, name: '4. Education' },
    { row: 26, name: '5. Licenses & Certificates' },
    { row: 31, name: '6. Training Record' },
    { row: 36, name: '7. Service Record' },
    { row: 41, name: '8. Remarks' }
  ];

  sections.forEach(section => {
    worksheet.getRow(section.row).height = 20;
    worksheet.mergeCells(`A${section.row}:J${section.row}`);
    const cell = worksheet.getCell(`A${section.row}`);
    cell.value = section.name;
    cell.font = { bold: true, size: 11 };
    cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
    cell.border = borderStyle;
  });

  // Save template
  const outputPath = path.join(__dirname, 'public', 'template.xlsx');
  await workbook.xlsx.writeFile(outputPath);
  console.log(`Template created successfully at: ${outputPath}`);
}

createTemplate().catch(console.error);
