export type LeaveBalance = {
  leaveType: string;
  allocatedDays: number;
  usedDays: number;
  remainingDays: number;
  year: number;
};

export type LeaveBalanceWithStats = LeaveBalance & {
  usedThisMonth: number;
  pendingRequests: number;
};