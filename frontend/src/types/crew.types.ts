export interface CrewMember {
  id: string;
  code: string;
  name: string;
  birthDate: string;
  gender: 'Nam' | 'Nữ';
  position: string;
  phone: string;
  email: string;
  address: string;
  avatar?: string;
  joinDate: string;
  status: 'active' | 'inactive';
}

export interface CrewFormData {
  code: string;
  name: string;
  birthDate: string;
  gender: 'Nam' | 'Nữ';
  position: string;
  phone: string;
  email: string;
  address: string;
  avatar?: File | string;
}

export interface CrewFilter {
  search: string;
  position: string;
}
