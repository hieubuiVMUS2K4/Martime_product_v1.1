const fs = require('fs');
const file = 'e:/NCKH/Martime_product_v1.1/edge_product/frontend-edge/src/pages/Crew/CrewPage.tsx';
let c = fs.readFileSync(file, 'utf8');

// Diagnose remaining problematic strings
const tests = [
  'Qu',  // Quốc tịch
  'Kh',  // Không có
  'thuy', // thuyền viên)
  'T\u00e1', // Tất cả
  '\u00e2', // pagination arrows
  '\u00c4', // Đến trang / removeAccents
  '\u0393', // chevron
];

for (const t of tests) {
  const idx = c.indexOf(t);
  if (idx > -1) {
    const sample = c.substring(idx, idx + 30);
    const hex = Buffer.from(sample, 'utf8').toString('hex').match(/.{2}/g).join(' ');
    console.log(`Found "${t}" at ${idx}: ${JSON.stringify(sample)}`);
    console.log(`  Hex: ${hex}\n`);
  } else {
    console.log(`NOT FOUND: "${t}"\n`);
  }
}

const file = 'e:/NCKH/Martime_product_v1.1/edge_product/frontend-edge/src/pages/Crew/CrewPage.tsx';
let c = fs.readFileSync(file, 'utf8');

// The file has triple-encoded mojibake characters.
// Each mojibake char is encoded as UTF-8. We replace them with correct Unicode.

// Arrow → (U+2192): appears as U+00E2 + U+2020 + U+2019 in file
const ARROW = '\u00e2\u2020\u2019';  // â†'

// All replacements: [bad_string, good_string]
const fixes = [
  // Sort labels
  [`A ${ARROW} Z`, 'A \u2192 Z'],
  [`Z ${ARROW} A`, 'Z \u2192 A'],
  // Search placeholder arrow
  [`${ARROW} T\u00c3\u00acm ki\u00e1\u00ba\u00bfm`, '\u2192 T\u00ecm ki\u1ebfm'],
  // Column headers
  ['H\u00e1\u00bb\u008d v\u00c3\u00a0 t\u00c3\u00aan', 'H\u1ecd v\u00e0 t\u00ean'],
  ['Qu\u00e1\u00bb\u2019c t\u00e1\u00bb\u2039ch', 'Qu\u1ed1c t\u1ecbch'],
  ['Ng\u00c3\u00a0y l\u00c3\u00aan t\u00c3\u00a0u', 'Ng\u00e0y l\u00ean t\u00e0u'],
  // Empty state + pagination
  ['Kh\u00c3\u00b4ng c\u00c3\u00b3 thuy\u00e1\u00bb\u00b9n vi\u00c3\u00aan n\u00c3\u00a0o', 'Kh\u00f4ng c\u00f3 thuy\u1ec1n vi\u00ean n\u00e0o'],
  ['thuy\u00e1\u00bb\u00b9n vi\u00c3\u00aan)', 'thuy\u1ec1n vi\u00ean)'],
  // Dropdown option
  ['T\u00e1\u00ba\u00a5t c\u00e1\u00ba\u00a3 qu\u00e1\u00bb\u2019c t\u00e1\u00bb\u2039ch', 'T\u1ea5t c\u1ea3 qu\u1ed1c t\u1ecbch'],
  // Pagination buttons
  ['\u00e2\u20ac\u00b9', '\u2039'],
  ['\u00e2\u20ac\u00ba', '\u203a'],
  // Go to page
  ['\u00c4\u00e1\u00ba\u00bfn trang', '\u0110\u1ebfn trang'],
  // InlinePendingReviewSection chevron (shows as garbled in various ways)
  ['\u0393\u00fb\u255d', '\u25bc'],
  // removeAccents regex
  ["replace(/\u00c4'/g, 'd').replace(/\u00c4/g, 'D')", "replace(/\u0111/g, 'd').replace(/\u0110/g, 'D')"],
];

let count = 0;
for (const [bad, good] of fixes) {
  if (c.includes(bad)) {
    c = c.split(bad).join(good);
    console.log(`Fixed: ${JSON.stringify(bad)} -> ${JSON.stringify(good)}`);
    count++;
  } else {
    console.log(`NOT FOUND: ${JSON.stringify(bad)}`);
  }
}

fs.writeFileSync(file, c, 'utf8');
console.log(`\nDone: ${count}/${fixes.length} replacements applied.`);
