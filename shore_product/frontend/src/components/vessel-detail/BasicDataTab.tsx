import React from 'react';
import { SectionCard, ReadOnlyField } from '../vessel/VesselDataFields';

interface Vessel {
  id?: string;
  imo?: string | null;
  officialNumber?: string | null;
  callSign?: string | null;
  mmsiNumber?: string | null;
  name?: string;
  flag?: string | null;
  portOfRegistry?: string | null;
  previousName?: string | null;
  previousFlag?: string | null;
  vesselType?: string | null;
  serviceSpeedKts?: number | null;
  classNotation?: string | null;
  classRegisterNumber?: string | null;
  shipyardCountry?: string | null;
  shipyardName?: string | null;
  yardNo?: string | null;
  yearBuilt?: number | null;
  keelLaidDate?: string | Date | null;
  dateOfRegistry?: string | Date | null;
  companyImoNumber?: string | null;
  ownerImoNumber?: string | null;
  suezCanalIdNumber?: string | null;
  panamaCanalIdNumber?: string | null;
  vrpNumber?: string | null;
  vrpType?: string | null;
  maxPersonsAllowedOB?: number | null;
  maxPassengersAllowedOB?: number | null;
  noOfCrewSafeManning?: number | null;
  lastEdgeSyncAt?: string | Date | null;
}

interface BasicDataTabProps {
  vessel: Vessel;
}

// Edge sync badge component
const EdgeSyncBadge: React.FC<{ lastSync?: string | Date | null }> = ({ lastSync }) => {
  if (!lastSync) return null;
  
  const syncDate = new Date(lastSync);
  const isRecent = (Date.now() - syncDate.getTime()) < 24 * 60 * 60 * 1000;
  
  return (
    <div
      style={{
        padding: '8px 12px',
        fontSize: '13px',
        fontWeight: 500,
        color: isRecent ? '#065f46' : '#6b7280',
        backgroundColor: isRecent ? '#d1fae5' : '#f3f4f6',
        borderRadius: '6px',
        width: 'fit-content',
        marginBottom: '12px'
      }}
    >
      🔄 Đồng bộ lần cuối từ Edge: {syncDate.toLocaleString('vi-VN')}
    </div>
  );
};

export function BasicDataTab({ vessel }: BasicDataTabProps) {
  return (
    <div className="space-y-4">
      <EdgeSyncBadge lastSync={vessel.lastEdgeSyncAt} />
      
      {/* Identification */}
      <SectionCard title="Nhận dạng">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Số IMO" value={vessel.imo} />
          <ReadOnlyField label="Số đăng ký chính thức" value={vessel.officialNumber} />
          <ReadOnlyField label="Hô hiệu" value={vessel.callSign} />
          <ReadOnlyField label="Số MMSI" value={vessel.mmsiNumber} />
        </div>
      </SectionCard>

      {/* Ship Details */}
      <SectionCard title="Thông tin tàu">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Tên tàu" value={vessel.name} className="col-span-2" />
          <ReadOnlyField label="Quốc kỳ" value={vessel.flag} />
          <ReadOnlyField label="Cảng đăng ký" value={vessel.portOfRegistry} />
          <ReadOnlyField label="Tên cũ" value={vessel.previousName} />
          <ReadOnlyField label="Quốc kỳ cũ" value={vessel.previousFlag} />
          <ReadOnlyField label="Loại tàu" value={vessel.vesselType} />
          <ReadOnlyField label="Tốc độ khai thác" value={vessel.serviceSpeedKts} suffix="kts" />
        </div>
      </SectionCard>

      {/* Classification */}
      <SectionCard title="Phân cấp">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Ký hiệu phân cấp" value={vessel.classNotation} className="col-span-2" />
          <ReadOnlyField label="Số đăng bạ phân cấp" value={vessel.classRegisterNumber} />
        </div>
      </SectionCard>

      {/* Construction */}
      <SectionCard title="Đóng tàu">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Quốc gia đóng tàu" value={vessel.shipyardCountry} />
          <ReadOnlyField label="Nhà máy đóng tàu" value={vessel.shipyardName} />
          <ReadOnlyField label="Số xưởng" value={vessel.yardNo} />
          <ReadOnlyField label="Năm đóng" value={vessel.yearBuilt} />
          <ReadOnlyField label="Ngày đặt ky" value={vessel.keelLaidDate?.toString().substring(0, 10)} />
          <ReadOnlyField label="Ngày đăng ký" value={vessel.dateOfRegistry?.toString().substring(0, 10)} />
        </div>
      </SectionCard>

      {/* Company & Registration */}
      <SectionCard title="Công ty & Đăng ký">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Số IMO công ty" value={vessel.companyImoNumber} />
          <ReadOnlyField label="Số IMO chủ tàu" value={vessel.ownerImoNumber} />
          <ReadOnlyField label="Số ID kênh đào Suez" value={vessel.suezCanalIdNumber} />
          <ReadOnlyField label="Số ID kênh đào Panama" value={vessel.panamaCanalIdNumber} />
          <ReadOnlyField label="Số VRP" value={vessel.vrpNumber} />
          <ReadOnlyField label="Loại VRP" value={vessel.vrpType} />
        </div>
      </SectionCard>

      {/* Manning */}
      <SectionCard title="Định biên">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Số người tối đa trên tàu" value={vessel.maxPersonsAllowedOB} />
          <ReadOnlyField label="Số hành khách tối đa" value={vessel.maxPassengersAllowedOB} />
          <ReadOnlyField label="Số thuyền viên (định biên an toàn)" value={vessel.noOfCrewSafeManning} />
        </div>
      </SectionCard>
    </div>
  );
}
