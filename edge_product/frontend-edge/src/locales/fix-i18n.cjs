const fs = require('fs');
const path = require('path');

const localesPath = 'd:/PROJECT/Martime_product_v1.1/edge_product/frontend-edge/src/locales';
const viPath = path.join(localesPath, 'vi.json');

function updateJson(filePath) {
  if (!fs.existsSync(filePath)) return;
  const data = JSON.parse(fs.readFileSync(filePath, 'utf8'));
  
  if (data.pms && data.pms.assets) {
    data.pms.assets.confirmDelete = 'Bạn có chắc chắn muốn xóa thiết bị này?';
    data.pms.assets.cancel = 'Hủy';
  }
  
  if (data.pms && data.pms.workPlanning && data.pms.workPlanning.toast) {
    data.pms.workPlanning.toast.confirmDeleteTask = 'Bạn có muốn xóa công việc này?';
    data.pms.workPlanning.toast.confirmDeleteConfig = 'Xác nhận xóa cấu hình bảo trì';
  }
  
  if (data.pms && data.pms.workPlanning && data.pms.workPlanning.config) {
    data.pms.workPlanning.config.cancel = 'Hủy';
  }
  
  if (data.materials && data.materials.page) {
    data.materials.page.cancel = 'Hủy';
  }
  
  if (data.storeLocations) {
    data.storeLocations.confirmDelete = 'Bạn có chắc muốn xóa vị trí "{{name}}"?';
    data.storeLocations.confirmBulkDelete = 'Xác nhận xóa {{count}} vị trí kho?';
  }

  fs.writeFileSync(filePath, JSON.stringify(data, null, 2), 'utf8');
  console.log(`Fixed ${filePath}`);
}

updateJson(viPath);
