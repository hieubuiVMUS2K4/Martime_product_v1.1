const fs = require('fs');
const file = 'e:/NCKH/Martime_product_v1.1/edge_product/frontend-edge/src/pages/Crew/CrewPage.tsx';
let c = fs.readFileSync(file, 'utf8');

// Search for substrings around line numbers
function showAround(marker, context) {
  const idx = c.indexOf(marker);
  if (idx > -1) {
    const s = c.substring(idx, idx + context);
    console.log('Found "' + marker.substring(0,10) + '" -> ' + JSON.stringify(s));
    console.log('  hex: ' + Buffer.from(s,'utf8').toString('hex').match(/.{2}/g).join(' ') + '\n');
  } else {
    console.log('NOT FOUND: "' + marker.substring(0,10) + '"\n');
  }
}

// Pagination prev/next buttons (‹ and ›) - search by context
showAround('disabled:opacity-40\r\n            >', 30); // prev button end
showAround('disabled={onboardPage === 1}', 200); // prev button full
// Đến trang
showAround('\u00c4', 20); // Any Ä remaining
showAround('n trang', 20); // "n trang" - end of Đến trang
// Chevron in InlinePendingReviewSection
showAround('rotate(180deg)', 100);
// removeAccents
showAround("replace(/", 60);
