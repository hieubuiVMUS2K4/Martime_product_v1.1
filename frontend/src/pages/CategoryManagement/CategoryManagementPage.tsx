import React, { useState } from 'react';
import { Card, CardHeader, CardBody, Button, Modal, Input } from '../../components/common';
import type { Category, CategoryType, CategoryCard } from '../../types/category.types';
import './CategoryManagementPage.css';

const categoryCards: CategoryCard[] = [
  { type: 'loai-tau', title: 'Quản lý loại tàu', count: 5 },
  { type: 'loai-vat-tu', title: 'Quản lý loại vật tư', count: 3 },
  { type: 'chuc-vu', title: 'Quản lý chức vụ', count: 8 },
  { type: 'loai-bao-cao', title: 'Quản lý loại báo cáo', count: 4 },
  { type: 'loai-chung-chi', title: 'Quản lý loại chứng chỉ', count: 6 },
  { type: 'loai-bao-tri', title: 'Quản lý loại bảo trì', count: 5 },
  { type: 'loai-chi-phi', title: 'Quản lý loại chi phí', count: 7 },
];

const mockData: Record<CategoryType, Category[]> = {
  'loai-tau': [
    { code: 'LT001', name: 'Tàu chở hàng', description: 'Tàu dùng để vận chuyển hàng hóa' },
    { code: 'LT002', name: 'Tàu container', description: 'Tàu chuyên chở container' },
    { code: 'LT003', name: 'Tàu chở dầu', description: 'Tàu vận chuyển dầu thô' },
    { code: 'LT004', name: 'Tàu cá', description: 'Tàu đánh bắt hải sản' },
    { code: 'LT005', name: 'Tàu du lịch', description: 'Tàu phục vụ du lịch' },
  ],
  'loai-vat-tu': [
    { code: 'VT001', name: 'Dầu máy', description: 'Dầu bôi trơn động cơ' },
    { code: 'VT002', name: 'Phụ tùng động cơ', description: 'Các bộ phận thay thế động cơ' },
    { code: 'VT003', name: 'Sơn chống gỉ', description: 'Sơn bảo vệ thân tàu' },
  ],
  'chuc-vu': [],
  'loai-bao-cao': [],
  'loai-chung-chi': [],
  'loai-bao-tri': [],
  'loai-chi-phi': [],
};

export const CategoryManagementPage: React.FC = () => {
  const [selectedCategory, setSelectedCategory] = useState<CategoryType | null>(null);
  const [isListModalOpen, setIsListModalOpen] = useState(false);
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [formData, setFormData] = useState<Category>({ code: '', name: '', description: '' });
  const [editingCode, setEditingCode] = useState<string | null>(null);

  const handleOpenCategory = (type: CategoryType) => {
    setSelectedCategory(type);
    setIsListModalOpen(true);
    setSearchQuery('');
  };

  const handleAddNew = () => {
    setFormData({ code: '', name: '', description: '' });
    setEditingCode(null);
    setIsFormModalOpen(true);
  };

  const handleEdit = (item: Category) => {
    setFormData(item);
    setEditingCode(item.code);
    setIsFormModalOpen(true);
  };

  const handleDelete = (code: string) => {
    if (window.confirm(`Bạn có chắc chắn muốn xóa mục có mã ${code}?`)) {
      alert(`Đã xóa thành công mục ${code}!`);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    alert(editingCode ? 'Đã cập nhật thành công!' : 'Đã thêm mới thành công!');
    setIsFormModalOpen(false);
    setIsListModalOpen(true);
  };

  const currentData = selectedCategory ? mockData[selectedCategory] : [];
  const filteredData = currentData.filter((item) =>
    item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    item.code.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const selectedCard = categoryCards.find((c) => c.type === selectedCategory);

  return (
    <div className="category-management-page">
      <div className="page-header">
        <h1>Quản lý danh mục</h1>
        <p>Quản lý các danh mục hệ thống</p>
      </div>

      <div className="categories-grid">
        {categoryCards.map((card) => (
          <Card key={card.type} className="category-card" hoverable>
            <CardHeader>
              <h3>{card.title}</h3>
              <span className="category-count">{card.count}</span>
            </CardHeader>
            <CardBody>
              <Button
                variant="primary"
                fullWidth
                onClick={() => handleOpenCategory(card.type)}
              >
                Quản lý
              </Button>
            </CardBody>
          </Card>
        ))}
      </div>

      {/* List Modal */}
      <Modal
        isOpen={isListModalOpen}
        onClose={() => setIsListModalOpen(false)}
        title={selectedCard?.title}
        size="xl"
      >
        <div className="table-toolbar">
          <Input
            type="text"
            placeholder="Tìm kiếm..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
          <Button variant="primary" onClick={handleAddNew}>
            Thêm mới
          </Button>
        </div>

        <table className="data-table">
          <thead>
            <tr>
              <th>STT</th>
              <th>Mã</th>
              <th>Tên</th>
              <th>Mô tả</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {filteredData.map((item, index) => (
              <tr key={item.code}>
                <td>{index + 1}</td>
                <td>{item.code}</td>
                <td>{item.name}</td>
                <td>{item.description}</td>
                <td>
                  <div className="action-buttons">
                    <Button size="sm" onClick={() => handleEdit(item)}>
                      Sửa
                    </Button>
                    <Button size="sm" onClick={() => handleDelete(item.code)}>
                      Xóa
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Modal>

      {/* Form Modal */}
      <Modal
        isOpen={isFormModalOpen}
        onClose={() => {
          setIsFormModalOpen(false);
          setIsListModalOpen(true);
        }}
        title={editingCode ? 'Chỉnh sửa' : 'Thêm mới'}
        size="md"
      >
        <form onSubmit={handleSubmit}>
          <Input
            label="Mã"
            value={formData.code}
            onChange={(e) => setFormData({ ...formData, code: e.target.value })}
            required
            fullWidth
          />
          <Input
            label="Tên"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            required
            fullWidth
          />
          <Input
            label="Mô tả"
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            fullWidth
          />
          <div className="form-actions">
            <Button
              type="button"
              onClick={() => {
                setIsFormModalOpen(false);
                setIsListModalOpen(true);
              }}
            >
              Hủy
            </Button>
            <Button type="submit" variant="primary">
              Lưu
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};
