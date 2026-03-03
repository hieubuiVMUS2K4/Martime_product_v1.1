import React, { useState, useEffect, useCallback } from 'react';
import { X, User, Phone, MapPin, GraduationCap, Heart, Ship } from 'lucide-react';
import { useReferenceData } from '../../hooks/useCrew';
import type { CrewMember, CreateCrewRequest, UpdateCrewRequest } from '../../types/crew.types';
import './CrewFormModal.css';

interface Props {
  crew: CrewMember | null;
  onClose: () => void;
  onSubmit: (data: CreateCrewRequest | Partial<CreateCrewRequest>) => void | Promise<void>;
  saving: boolean;
}

type Section = 'basic' | 'contact' | 'personal' | 'education' | 'kin' | 'vessel';

const sections: { key: Section; label: string; icon: React.ReactNode }[] = [
  { key: 'basic', label: 'Thông tin cơ bản', icon: <User size={16} /> },
  { key: 'contact', label: 'Liên lạc', icon: <Phone size={16} /> },
  { key: 'vessel', label: 'Tàu & hợp đồng', icon: <Ship size={16} /> },
  { key: 'personal', label: 'Cá nhân', icon: <Heart size={16} /> },
  { key: 'education', label: 'Học vấn', icon: <GraduationCap size={16} /> },
  { key: 'kin', label: 'Người thân', icon: <MapPin size={16} /> },
];

export const CrewFormModal: React.FC<Props> = ({ crew, onClose, onSubmit, saving }) => {
  const { ranks, countries } = useReferenceData();
  const [activeSection, setActiveSection] = useState<Section>('basic');
  const isEdit = !!crew;

  const [form, setForm] = useState<CreateCrewRequest>({
    crewId: '',
    fullName: '',
  });

  useEffect(() => {
    if (crew) {
      setForm({
        crewId: crew.crewId,
        fullName: crew.fullName,
        rankId: crew.rankId,
        department: crew.department,
        nationality: crew.nationality,
        dateOfBirth: crew.dateOfBirth?.split('T')[0],
        joinDate: crew.joinDate?.split('T')[0],
        embarkDate: crew.embarkDate?.split('T')[0],
        contractEnd: crew.contractEnd?.split('T')[0],
        isOnboard: crew.isOnboard,
        emergencyContact: crew.emergencyContact,
        emailAddress: crew.emailAddress,
        phoneNumber: crew.phoneNumber,
        address: crew.address,
        placeOfBirth: crew.placeOfBirth,
        idCardNumber: crew.idCardNumber,
        maritalStatus: crew.maritalStatus,
        height: crew.height,
        weight: crew.weight,
        bloodGroup: crew.bloodGroup,
        clothingSize: crew.clothingSize,
        shoeSize: crew.shoeSize,
        isSmoker: crew.isSmoker,
        isCovidVaccinated: crew.isCovidVaccinated,
        nextOfKinName: crew.nextOfKinName,
        nextOfKinRelation: crew.nextOfKinRelation,
        nextOfKinPhone: crew.nextOfKinPhone,
        nextOfKinAddress: crew.nextOfKinAddress,
        educationInstitution: crew.educationInstitution,
        educationCourse: crew.educationCourse,
        educationPeriodYears: crew.educationPeriodYears,
        educationGraduationYear: crew.educationGraduationYear,
        notes: crew.notes,
      });
    }
  }, [crew]);

  const set = useCallback((field: keyof CreateCrewRequest, value: unknown) => {
    setForm(prev => ({ ...prev, [field]: value }));
  }, []);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.crewId || !form.fullName) return;
    onSubmit(form);
  };

  // Close on Escape
  useEffect(() => {
    const handleEsc = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); };
    document.addEventListener('keydown', handleEsc);
    document.body.style.overflow = 'hidden';
    return () => {
      document.removeEventListener('keydown', handleEsc);
      document.body.style.overflow = '';
    };
  }, [onClose]);

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="crew-form-modal" onClick={e => e.stopPropagation()}>
        {/* Header */}
        <div className="cfm-header">
          <h2 className="cfm-title">{isEdit ? 'Chỉnh sửa thuyền viên' : 'Thêm thuyền viên mới'}</h2>
          <button className="cfm-close" onClick={onClose}><X size={18} /></button>
        </div>

        {/* Section Nav */}
        <div className="cfm-sections">
          {sections.map(s => (
            <button
              key={s.key}
              className={`cfm-section-btn ${activeSection === s.key ? 'cfm-section-btn--active' : ''}`}
              onClick={() => setActiveSection(s.key)}
              type="button"
            >
              {s.icon}
              <span>{s.label}</span>
            </button>
          ))}
        </div>

        {/* Form Body */}
        <form onSubmit={handleSubmit}>
          <div className="cfm-body">
            {/* Basic Info */}
            {activeSection === 'basic' && (
              <div className="cfm-grid fade-in">
                <div className="cfm-field cfm-field--required">
                  <label>Mã thuyền viên</label>
                  <input value={form.crewId} onChange={e => set('crewId', e.target.value)} placeholder="VD: TV001" required />
                </div>
                <div className="cfm-field cfm-field--required">
                  <label>Họ và tên</label>
                  <input value={form.fullName} onChange={e => set('fullName', e.target.value)} placeholder="Nguyễn Văn A" required />
                </div>
                <div className="cfm-field">
                  <label>Chức danh</label>
                  <select value={form.rankId ?? ''} onChange={e => set('rankId', e.target.value ? Number(e.target.value) : undefined)}>
                    <option value="">— Chọn chức danh —</option>
                    {ranks.map(r => <option key={r.id} value={r.id}>{r.rankName}</option>)}
                  </select>
                </div>
                <div className="cfm-field">
                  <label>Bộ phận</label>
                  <select value={form.department ?? ''} onChange={e => set('department', e.target.value || undefined)}>
                    <option value="">— Chọn —</option>
                    <option value="DECK">Boong (Deck)</option>
                    <option value="ENGINE">Máy (Engine)</option>
                    <option value="CATERING">Bếp (Catering)</option>
                    <option value="RADIO">Radio</option>
                  </select>
                </div>
                <div className="cfm-field">
                  <label>Quốc tịch</label>
                  <input value={form.nationality ?? ''} onChange={e => set('nationality', e.target.value)} placeholder="Vietnam" />
                </div>
                <div className="cfm-field">
                  <label>Ngày sinh</label>
                  <input type="date" value={form.dateOfBirth ?? ''} onChange={e => set('dateOfBirth', e.target.value)} />
                </div>
                <div className="cfm-field">
                  <label>Nơi sinh</label>
                  <input value={form.placeOfBirth ?? ''} onChange={e => set('placeOfBirth', e.target.value)} />
                </div>
                <div className="cfm-field">
                  <label>Số CMND/CCCD</label>
                  <input value={form.idCardNumber ?? ''} onChange={e => set('idCardNumber', e.target.value)} />
                </div>
              </div>
            )}

            {/* Contact */}
            {activeSection === 'contact' && (
              <div className="cfm-grid fade-in">
                <div className="cfm-field">
                  <label>Email</label>
                  <input type="email" value={form.emailAddress ?? ''} onChange={e => set('emailAddress', e.target.value)} />
                </div>
                <div className="cfm-field">
                  <label>Số điện thoại</label>
                  <input value={form.phoneNumber ?? ''} onChange={e => set('phoneNumber', e.target.value)} />
                </div>
                <div className="cfm-field cfm-field--full">
                  <label>Địa chỉ</label>
                  <input value={form.address ?? ''} onChange={e => set('address', e.target.value)} />
                </div>
                <div className="cfm-field cfm-field--full">
                  <label>Liên hệ khẩn cấp</label>
                  <input value={form.emergencyContact ?? ''} onChange={e => set('emergencyContact', e.target.value)} placeholder="Tên — Số điện thoại" />
                </div>
              </div>
            )}

            {/* Vessel & Contract */}
            {activeSection === 'vessel' && (
              <div className="cfm-grid fade-in">
                <div className="cfm-field">
                  <label>Ngày gia nhập</label>
                  <input type="date" value={form.joinDate ?? ''} onChange={e => set('joinDate', e.target.value)} />
                </div>
                <div className="cfm-field">
                  <label>Ngày lên tàu</label>
                  <input type="date" value={form.embarkDate ?? ''} onChange={e => set('embarkDate', e.target.value)} />
                </div>
                <div className="cfm-field">
                  <label>Hết hạn hợp đồng</label>
                  <input type="date" value={form.contractEnd ?? ''} onChange={e => set('contractEnd', e.target.value)} />
                </div>
                <div className="cfm-field">
                  <label>Đang trên tàu?</label>
                  <select value={form.isOnboard ? 'true' : 'false'} onChange={e => set('isOnboard', e.target.value === 'true')}>
                    <option value="true">Có — Trên tàu</option>
                    <option value="false">Không — Ở bờ</option>
                  </select>
                </div>
                <div className="cfm-field cfm-field--full">
                  <label>Ghi chú</label>
                  <textarea rows={3} value={form.notes ?? ''} onChange={e => set('notes', e.target.value)} />
                </div>
              </div>
            )}

            {/* Personal */}
            {activeSection === 'personal' && (
              <div className="cfm-grid fade-in">
                <div className="cfm-field">
                  <label>Tình trạng hôn nhân</label>
                  <select value={form.maritalStatus ?? ''} onChange={e => set('maritalStatus', e.target.value || undefined)}>
                    <option value="">— Chọn —</option>
                    <option value="single">Độc thân</option>
                    <option value="married">Đã kết hôn</option>
                    <option value="divorced">Ly hôn</option>
                  </select>
                </div>
                <div className="cfm-field">
                  <label>Nhóm máu</label>
                  <select value={form.bloodGroup ?? ''} onChange={e => set('bloodGroup', e.target.value || undefined)}>
                    <option value="">— Chọn —</option>
                    <option>A+</option><option>A-</option>
                    <option>B+</option><option>B-</option>
                    <option>AB+</option><option>AB-</option>
                    <option>O+</option><option>O-</option>
                  </select>
                </div>
                <div className="cfm-field">
                  <label>Chiều cao (cm)</label>
                  <input type="number" value={form.height ?? ''} onChange={e => set('height', e.target.value ? Number(e.target.value) : undefined)} />
                </div>
                <div className="cfm-field">
                  <label>Cân nặng (kg)</label>
                  <input type="number" value={form.weight ?? ''} onChange={e => set('weight', e.target.value ? Number(e.target.value) : undefined)} />
                </div>
                <div className="cfm-field">
                  <label>Cỡ giày</label>
                  <input value={form.shoeSize ?? ''} onChange={e => set('shoeSize', e.target.value)} />
                </div>
                <div className="cfm-field">
                  <label>Cỡ áo</label>
                  <input value={form.clothingSize ?? ''} onChange={e => set('clothingSize', e.target.value)} />
                </div>
                <div className="cfm-field">
                  <label>Hút thuốc?</label>
                  <select value={form.isSmoker === true ? 'true' : form.isSmoker === false ? 'false' : ''} onChange={e => set('isSmoker', e.target.value === '' ? undefined : e.target.value === 'true')}>
                    <option value="">— Chọn —</option>
                    <option value="true">Có</option>
                    <option value="false">Không</option>
                  </select>
                </div>
                <div className="cfm-field">
                  <label>Đã tiêm COVID?</label>
                  <select value={form.isCovidVaccinated === true ? 'true' : form.isCovidVaccinated === false ? 'false' : ''} onChange={e => set('isCovidVaccinated', e.target.value === '' ? undefined : e.target.value === 'true')}>
                    <option value="">— Chọn —</option>
                    <option value="true">Có</option>
                    <option value="false">Không</option>
                  </select>
                </div>
              </div>
            )}

            {/* Education */}
            {activeSection === 'education' && (
              <div className="cfm-grid fade-in">
                <div className="cfm-field cfm-field--full">
                  <label>Trường / Cơ sở đào tạo</label>
                  <input value={form.educationInstitution ?? ''} onChange={e => set('educationInstitution', e.target.value)} />
                </div>
                <div className="cfm-field cfm-field--full">
                  <label>Chuyên ngành</label>
                  <input value={form.educationCourse ?? ''} onChange={e => set('educationCourse', e.target.value)} />
                </div>
                <div className="cfm-field">
                  <label>Số năm học</label>
                  <input type="number" value={form.educationPeriodYears ?? ''} onChange={e => set('educationPeriodYears', e.target.value ? Number(e.target.value) : undefined)} />
                </div>
                <div className="cfm-field">
                  <label>Năm tốt nghiệp</label>
                  <input type="number" value={form.educationGraduationYear ?? ''} onChange={e => set('educationGraduationYear', e.target.value ? Number(e.target.value) : undefined)} />
                </div>
              </div>
            )}

            {/* Next of Kin */}
            {activeSection === 'kin' && (
              <div className="cfm-grid fade-in">
                <div className="cfm-field">
                  <label>Họ tên người thân</label>
                  <input value={form.nextOfKinName ?? ''} onChange={e => set('nextOfKinName', e.target.value)} />
                </div>
                <div className="cfm-field">
                  <label>Mối quan hệ</label>
                  <input value={form.nextOfKinRelation ?? ''} onChange={e => set('nextOfKinRelation', e.target.value)} placeholder="VD: Vợ, Cha, Mẹ" />
                </div>
                <div className="cfm-field">
                  <label>Số điện thoại</label>
                  <input value={form.nextOfKinPhone ?? ''} onChange={e => set('nextOfKinPhone', e.target.value)} />
                </div>
                <div className="cfm-field cfm-field--full">
                  <label>Địa chỉ</label>
                  <input value={form.nextOfKinAddress ?? ''} onChange={e => set('nextOfKinAddress', e.target.value)} />
                </div>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="cfm-footer">
            <button type="button" className="btn btn-ghost" onClick={onClose}>Hủy</button>
            <button type="submit" className="btn btn-primary" disabled={saving || !form.crewId || !form.fullName}>
              {saving ? 'Đang lưu...' : (isEdit ? 'Cập nhật' : 'Thêm mới')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
