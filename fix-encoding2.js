const fs = require('fs');
const file = 'e:/NCKH/Martime_product_v1.1/edge_product/frontend-edge/src/pages/Crew/CrewPage.tsx';
let c = fs.readFileSync(file, 'utf8');

// Diagnose remaining problematic strings by searching for partial matches
function findAndShow(searchStr) {
  const idx = c.indexOf(searchStr);
  if (idx > -1) {
    const sample = c.substring(idx, idx + 30);
    const hex = Buffer.from(sample, 'utf8').toString('hex').match(/.{2}/g).join(' ');
    console.log('Found "' + searchStr + '" -> ' + JSON.stringify(sample));
    console.log('  hex: ' + hex);
  } else {
    console.log('NOT FOUND: "' + searchStr + '"');
  }
}

// Quốc tịch column header
findAndShow('Qu\u00e1\u00bb');
// Không có
findAndShow('Kh\u00c3');
// thuyền viên
findAndShow('thuy\u00e1\u00bb');
// Tất cả quốc tịch dropdown
findAndShow('T\u00e1\u00ba\u00a5t c');
// Pagination prev button contains â€
findAndShow('\u00e2\u20ac');
// Đến trang
findAndShow('\u00c4\u00e1');
// chevron ▼ / Γû╝
findAndShow('\u0393');
// removeAccents regex: Ä' pattern
findAndShow('replace(/\u00c4');
