export interface StatCard {
  title: string;
  value: number;
  icon?: string;
}

export interface ModuleItem {
  title: string;
  items: string[];
}

export interface DashboardData {
  stats: StatCard[];
  modules: ModuleItem[];
}
