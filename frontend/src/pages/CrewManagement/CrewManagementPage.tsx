import React, { useState } from 'react';
import { Card, CardBody, CardFooter, Button, Modal, Input, Select } from '../../components/common';
import type { CrewMember, CrewFormData, CrewFilter } from '../../types/crew.types';
import './CrewManagementPage.css';

const mockCrewData: CrewMember[] = [
  {
    id: '1',
    code: 'TV001',
    name: 'Nguyễn Văn A',
    birthDate: '1990-05-15',
    gender: 'Nam',
    position: 'Thuyền trưởng',
    phone: '0901234567',
    email: 'nguyenvana@email.com',
    address: 'Hà Nội',
    joinDate: '2020-01-15',
    status: 'active',
  },
  {
    id: '2',
    code: 'TV002',
    name: 'Trần Thị B',
    birthDate: '1995-08-20',
    gender: 'Nữ',
    position: 'Thủy thủ',
    phone: '0912345678',
    email: 'tranthib@email.com',
    address: 'TP. Hồ Chí Minh',
    joinDate: '2021-03-10',
    status: 'active',
  },
];

const positions = ['Thuyền trưởng', 'Máy trưởng', 'Thủy thủ', 'Thợ máy', 'Đầu bếp'];

export const CrewManagementPage: React.FC = () => {
  const [crewList, setCrewList] = useState<CrewMember[]>(mockCrewData);
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [selectedCrew, setSelectedCrew] = useState<CrewMember | null>(null);
  const [filters, setFilters] = useState<CrewFilter>({ search: '', position: '' });
  const [formData, setFormData] = useState<CrewFormData>({
    code: '',
    name: '',
    birthDate: '',
    gender: 'Nam',
    position: '',
    phone: '',
    email: '',
    address: '',
  });
  const [avatarPreview, setAvatarPreview] = useState<string>('');

  const calculateAge = (birthDate: string): number => {
    const today = new Date();
    const birth = new Date(birthDate);
    let age = today.getFullYear() - birth.getFullYear();
    const monthDiff = today.getMonth() - birth.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
      age--;
    }
    return age;
  };

  const handleAddNew = () => {
    setFormData({
      code: '',
      name: '',
      birthDate: '',
      gender: 'Nam',
      position: '',
      phone: '',
      email: '',
      address: '',
    });
    setAvatarPreview('');
    setSelectedCrew(null);
    setIsFormModalOpen(true);
  };

  const handleEdit = (crew: CrewMember) => {
    setFormData({
      code: crew.code,
      name: crew.name,
      birthDate: crew.birthDate,
      gender: crew.gender,
      position: crew.position,
      phone: crew.phone,
      email: crew.email,
      address: crew.address,
    });
    setAvatarPreview(crew.avatar || '');
    setSelectedCrew(crew);
    setIsFormModalOpen(true);
  };

  const handleView = (crew: CrewMember) => {
    setSelectedCrew(crew);
    setIsDetailModalOpen(true);
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Bạn có chắc chắn muốn xóa?')) {
      setCrewList(crewList.filter((c) => c.id !== id));
      alert('Đã xóa thành công!');
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedCrew) {
      // Update existing
      const updatedCrew: CrewMember = {
        ...selectedCrew,
        code: formData.code,
        name: formData.name,
        birthDate: formData.birthDate,
        gender: formData.gender,
        position: formData.position,
        phone: formData.phone,
        email: formData.email,
        address: formData.address,
        avatar: avatarPreview || selectedCrew.avatar,
      };
      setCrewList(crewList.map((c) => (c.id === selectedCrew.id ? updatedCrew : c)));
      alert('Đã cập nhật thành công!');
    } else {
      // Add new
      const newCrew: CrewMember = {
        id: Date.now().toString(),
        ...formData,
        joinDate: new Date().toISOString().split('T')[0],
        status: 'active',
        avatar: avatarPreview || undefined,
      };
      setCrewList([...crewList, newCrew]);
      alert('Đã thêm mới thành công!');
    }
    setIsFormModalOpen(false);
  };

  const handleAvatarChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setAvatarPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleExcelImport = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      alert('Chức năng import Excel cần thư viện xlsx. Đã import file: ' + file.name);
      // TODO: Implement xlsx parsing
    }
  };

  const filteredCrew = crewList.filter((crew) => {
    const matchSearch =
      crew.name.toLowerCase().includes(filters.search.toLowerCase()) ||
      crew.code.toLowerCase().includes(filters.search.toLowerCase());
    const matchPosition = !filters.position || crew.position === filters.position;
    return matchSearch && matchPosition;
  });

  return (
    <div className="crew-management-page">
      <div className="page-header">
        <h1>Quản lý thuyền viên</h1>
      </div>

      <div className="toolbar">
        <div className="toolbar-left">
          <Input
            type="text"
            placeholder="Tìm kiếm theo tên, mã..."
            value={filters.search}
            onChange={(e) => setFilters({ ...filters, search: e.target.value })}
          />
          <Select
            value={filters.position}
            onChange={(e) => setFilters({ ...filters, position: e.target.value })}
            options={[
              { value: '', label: 'Tất cả chức vụ' },
              ...positions.map((p) => ({ value: p, label: p })),
            ]}
          />
        </div>
        <div className="toolbar-right">
          <Button onClick={handleAddNew} variant="primary">
            Thêm mới
          </Button>
          <label className="file-upload-btn">
            <Button>Import Excel</Button>
            <input
              type="file"
              accept=".xlsx,.xls"
              onChange={handleExcelImport}
              style={{ display: 'none' }}
            />
          </label>
          <Button>Export Excel</Button>
        </div>
      </div>

      <div className="crew-grid">
        {filteredCrew.map((crew) => (
          <Card key={crew.id} className="crew-card">
            <div className="crew-avatar">
              {crew.avatar ? (
                <img src={crew.avatar} alt={crew.name} />
              ) : (
                <div className="avatar-placeholder">👤</div>
              )}
            </div>
            <CardBody>
              <h3 className="crew-name">{crew.name}</h3>
              <div className="crew-detail">
                <strong>Mã:</strong> {crew.code}
              </div>
              <div className="crew-detail">
                <strong>Chức vụ:</strong> {crew.position}
              </div>
              <div className="crew-detail">
                <strong>Tuổi:</strong> {calculateAge(crew.birthDate)}
              </div>
              <div className="crew-detail">
                <strong>SĐT:</strong> {crew.phone}
              </div>
            </CardBody>
            <CardFooter>
              <Button size="sm" onClick={() => handleView(crew)}>
                Xem
              </Button>
              <Button size="sm" onClick={() => handleEdit(crew)}>
                Sửa
              </Button>
              <Button size="sm" onClick={() => handleDelete(crew.id)}>
                Xóa
              </Button>
            </CardFooter>
          </Card>
        ))}
      </div>

      {/* Form Modal */}
      <Modal
        isOpen={isFormModalOpen}
        onClose={() => setIsFormModalOpen(false)}
        title={selectedCrew ? 'Chỉnh sửa thuyền viên' : 'Thêm thuyền viên mới'}
        size="md"
      >
        <form onSubmit={handleSubmit}>
          <div className="avatar-upload">
            <div className="avatar-preview-large">
              {avatarPreview ? (
                <img src={avatarPreview} alt="Preview" />
              ) : (
                <div className="avatar-placeholder">👤</div>
              )}
            </div>
            <label className="file-upload-btn">
              <Button type="button">Chọn ảnh</Button>
              <input
                type="file"
                accept="image/*"
                onChange={handleAvatarChange}
                style={{ display: 'none' }}
              />
            </label>
          </div>

          <div className="form-row">
            <Input
              label="Mã thuyền viên"
              value={formData.code}
              onChange={(e) => setFormData({ ...formData, code: e.target.value })}
              required
              fullWidth
            />
            <Input
              label="Họ và tên"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
              fullWidth
            />
          </div>

          <div className="form-row">
            <Input
              label="Ngày sinh"
              type="date"
              value={formData.birthDate}
              onChange={(e) => setFormData({ ...formData, birthDate: e.target.value })}
              required
              fullWidth
            />
            <Select
              label="Giới tính"
              value={formData.gender}
              onChange={(e) =>
                setFormData({ ...formData, gender: e.target.value as 'Nam' | 'Nữ' })
              }
              options={[
                { value: 'Nam', label: 'Nam' },
                { value: 'Nữ', label: 'Nữ' },
              ]}
              fullWidth
            />
          </div>

          <Select
            label="Chức vụ"
            value={formData.position}
            onChange={(e) => setFormData({ ...formData, position: e.target.value })}
            options={[
              { value: '', label: 'Chọn chức vụ' },
              ...positions.map((p) => ({ value: p, label: p })),
            ]}
            required
            fullWidth
          />

          <div className="form-row">
            <Input
              label="Số điện thoại"
              type="tel"
              value={formData.phone}
              onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
              required
              fullWidth
            />
            <Input
              label="Email"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              required
              fullWidth
            />
          </div>

          <Input
            label="Địa chỉ"
            value={formData.address}
            onChange={(e) => setFormData({ ...formData, address: e.target.value })}
            fullWidth
          />

          <div className="form-actions">
            <Button type="button" onClick={() => setIsFormModalOpen(false)}>
              Hủy
            </Button>
            <Button type="submit" variant="primary">
              Lưu
            </Button>
          </div>
        </form>
      </Modal>

      {/* Detail Modal */}
      {selectedCrew && (
        <Modal
          isOpen={isDetailModalOpen}
          onClose={() => setIsDetailModalOpen(false)}
          title="Thông tin thuyền viên"
          size="md"
        >
          <div className="crew-detail-view">
            <div className="detail-avatar">
              {selectedCrew.avatar ? (
                <img src={selectedCrew.avatar} alt={selectedCrew.name} />
              ) : (
                <div className="avatar-placeholder">👤</div>
              )}
            </div>

            <div className="detail-grid">
              <div className="detail-item">
                <label>Mã thuyền viên</label>
                <div className="value">{selectedCrew.code}</div>
              </div>
              <div className="detail-item">
                <label>Họ và tên</label>
                <div className="value">{selectedCrew.name}</div>
              </div>
              <div className="detail-item">
                <label>Ngày sinh</label>
                <div className="value">{selectedCrew.birthDate}</div>
              </div>
              <div className="detail-item">
                <label>Tuổi</label>
                <div className="value">{calculateAge(selectedCrew.birthDate)}</div>
              </div>
              <div className="detail-item">
                <label>Giới tính</label>
                <div className="value">{selectedCrew.gender}</div>
              </div>
              <div className="detail-item">
                <label>Chức vụ</label>
                <div className="value">{selectedCrew.position}</div>
              </div>
              <div className="detail-item">
                <label>Số điện thoại</label>
                <div className="value">{selectedCrew.phone}</div>
              </div>
              <div className="detail-item">
                <label>Email</label>
                <div className="value">{selectedCrew.email}</div>
              </div>
              <div className="detail-item full-width">
                <label>Địa chỉ</label>
                <div className="value">{selectedCrew.address}</div>
              </div>
              <div className="detail-item">
                <label>Ngày vào làm</label>
                <div className="value">{selectedCrew.joinDate}</div>
              </div>
              <div className="detail-item">
                <label>Trạng thái</label>
                <div className="value">
                  {selectedCrew.status === 'active' ? 'Đang làm việc' : 'Nghỉ việc'}
                </div>
              </div>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
};
