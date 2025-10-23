export interface Category {
  code: string;
  name: string;
  description: string;
}

export type CategoryType =
  | 'loai-tau'
  | 'loai-vat-tu'
  | 'chuc-vu'
  | 'loai-bao-cao'
  | 'loai-chung-chi'
  | 'loai-bao-tri'
  | 'loai-chi-phi';

export interface CategoryCard {
  type: CategoryType;
  title: string;
  count: number;
}
