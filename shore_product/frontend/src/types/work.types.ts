export type WorkStatus = 
  | 'pending'      // Chờ duyệt
  | 'approved'     // Đã duyệt
  | 'in-progress'  // Đang thực hiện
  | 'completed'    // Đã hoàn thành
  | 'overdue';     // Quá hạn

export interface WorkPlan {
  id: string;
  stt: number;
  createdDate: string;        // Ngày lập
  title: string;              // Nội dung
  planType: string;           // Loại kế hoạch
  startDate: string;          // Thời gian bắt đầu
  endDate: string;            // Thời gian yêu cầu hoàn thành
  status: WorkStatus;
  assignedTo?: string[];      // Danh sách thuyền viên được phân công
  description?: string;
  priority?: 'low' | 'medium' | 'high';
}

export interface WorkAssignment {
  id: string;
  workPlanId: string;
  crewMemberId: string;
  crewMemberName: string;
  assignedDate: string;
  completedDate?: string;
  notes?: string;
  status: WorkStatus;
}

export interface WorkFilter {
  search: string;
  status: WorkStatus | '';
  dateFrom: string;
  dateTo: string;
}
