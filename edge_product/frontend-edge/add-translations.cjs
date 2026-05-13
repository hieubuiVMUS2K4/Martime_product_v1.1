const fs = require('fs');

const enPath = 'f:/NCKH/Product/Martime_product_v1.1/edge_product/frontend-edge/src/locales/en.json';
const viPath = 'f:/NCKH/Product/Martime_product_v1.1/edge_product/frontend-edge/src/locales/vi.json';

const enData = JSON.parse(fs.readFileSync(enPath, 'utf8'));
const viData = JSON.parse(fs.readFileSync(viPath, 'utf8'));

const conningEn = {
  "title": "Bridge Conning Display",
  "local": "Local",
  "positioning": "Positioning",
  "gpsFix": "GPS Fix",
  "dgpsFix": "DGPS Fix",
  "noFix": "No Fix",
  "edgeNode": "Edge Node",
  "queue": "Queue",
  "lastSync": "Last Sync",
  "hdg": "HDG (Heading)",
  "magnetic": "Magnetic",
  "cog": "COG (Course)",
  "drift": "Drift",
  "sog": "SOG (Seabed)",
  "gpsDerived": "GPS Derived",
  "stw": "STW (Water)",
  "tide": "Tide",
  "activeAlarms": "Active Alarms",
  "critical": "Critical",
  "total": "Total",
  "attitude": "Vessel Attitude",
  "pitch": "Pitch",
  "roll": "Roll",
  "draftFore": "Draft Fore",
  "draftMid": "Draft Mid",
  "draftAft": "Draft Aft",
  "ukc": "Echo Sounder UKC",
  "wind": "Wind",
  "bowThr": "Bow Thruster",
  "sternThr": "Stern Thruster",
  "rudder": "Rudder",
  "stbd": "STBD",
  "port": "PORT",
  "mid": "MID",
  "rot": "R.O.T",
  "aisTargets": "AIS Targets",
  "cpa": "CPA",
  "tcpa": "TCPA",
  "propulsion": "Main Propulsion (CPP)",
  "rpm": "RPM",
  "propPitch": "Propeller Pitch",
  "engineLoad": "Engine Load",
  "fuelRate": "Fuel Rate",
  "status": "Status",
  "ahead": "AHEAD",
  "stopped": "STOPPED"
};

const conningVi = {
  "title": "Trực ca Động lực (Conning)",
  "local": "Địa phương",
  "positioning": "Định vị",
  "gpsFix": "Cố định GPS",
  "dgpsFix": "Cố định DGPS",
  "noFix": "Mất tín hiệu",
  "edgeNode": "Edge Node",
  "queue": "Hàng chờ",
  "lastSync": "Đồng bộ cuối",
  "hdg": "Mũi Tàu (HDG)",
  "magnetic": "Từ tính",
  "cog": "Hướng Đi (COG)",
  "drift": "Góc dạt",
  "sog": "Tốc Độ Đáy (SOG)",
  "gpsDerived": "Từ GPS",
  "stw": "Tốc Độ Nước (STW)",
  "tide": "Dòng triều",
  "activeAlarms": "Báo động hiện tại",
  "critical": "Nghiêm trọng",
  "total": "Tổng",
  "attitude": "Động lực học Tàu",
  "pitch": "Chúi (Pitch)",
  "roll": "Nghiêng (Roll)",
  "draftFore": "Mớn nước mũi",
  "draftMid": "Mớn nước giữa",
  "draftAft": "Mớn nước lái",
  "ukc": "Sâu Kế (UKC)",
  "wind": "Gió TĐ",
  "bowThr": "CV Mũi",
  "sternThr": "CV Lái",
  "rudder": "Bánh Lái",
  "stbd": "Phải",
  "port": "Trái",
  "mid": "Giữa",
  "rot": "Quay trở",
  "aisTargets": "Mục tiêu AIS",
  "cpa": "Khoảng cách tránh va",
  "tcpa": "TG tránh va",
  "propulsion": "Hệ thống Lực Đẩy (CPP)",
  "rpm": "Vòng Tua",
  "propPitch": "Bước Chân Vịt",
  "engineLoad": "Tải động cơ",
  "fuelRate": "Tiêu thụ",
  "status": "Trạng thái",
  "ahead": "CHẠY TỚI",
  "stopped": "DỪNG"
};

enData.conning = conningEn;
viData.conning = conningVi;

fs.writeFileSync(enPath, JSON.stringify(enData, null, 2));
fs.writeFileSync(viPath, JSON.stringify(viData, null, 2));

console.log('Successfully added conning translations.');
