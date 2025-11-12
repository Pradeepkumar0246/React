using HRPayrollSystem_Payslip.DTOs.LeaveRequestDTO;
using HRPayrollSystem_Payslip.Enums;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly ILeaveBalanceService _leaveBalanceService;

        public LeaveRequestService(ILeaveRequestRepository leaveRequestRepository, ILeaveBalanceService leaveBalanceService)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _leaveBalanceService = leaveBalanceService;
        }

        public async Task<IEnumerable<LeaveRequestResponseDto>> GetAllLeaveRequestsAsync()
        {
            var leaveRequests = await _leaveRequestRepository.GetAllAsync();
            return leaveRequests.Select(MapToResponseDto);
        }

        public async Task<LeaveRequestResponseDto?> GetLeaveRequestByIdAsync(int id)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
            return leaveRequest == null ? null : MapToResponseDto(leaveRequest);
        }

        public async Task<IEnumerable<LeaveRequestResponseDto>> GetLeaveRequestsByEmployeeIdAsync(string employeeId)
        {
            if (!await _leaveRequestRepository.EmployeeExistsAsync(employeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var leaveRequests = await _leaveRequestRepository.GetByEmployeeIdAsync(employeeId);
            return leaveRequests.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<LeaveRequestResponseDto>> GetLeaveRequestsByStatusAsync(string status)
        {
            var leaveRequests = await _leaveRequestRepository.GetByStatusAsync(status);
            return leaveRequests.Select(MapToResponseDto);
        }

        public async Task<LeaveRequestResponseDto> CreateLeaveRequestAsync(LeaveRequestCreateDto leaveRequestCreateDto)
        {
            if (!await _leaveRequestRepository.EmployeeExistsAsync(leaveRequestCreateDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            if (leaveRequestCreateDto.FromDate > leaveRequestCreateDto.ToDate)
            {
                throw new ArgumentException("From date must be less than or equal to to date.");
            }

            // Date validation: No past dates except for Sick and Emergency
            if (leaveRequestCreateDto.FromDate.Date < DateTime.Today && 
                leaveRequestCreateDto.LeaveType != LeaveType.Sick && 
                leaveRequestCreateDto.LeaveType != LeaveType.Emergency)
            {
                throw new ArgumentException("Leave cannot be applied for past dates except for Sick and Emergency leave.");
            }

            // Auto-generate reason for non-emergency leaves
            var reason = leaveRequestCreateDto.Reason;
            if (leaveRequestCreateDto.LeaveType != LeaveType.Emergency && string.IsNullOrWhiteSpace(reason))
            {
                reason = leaveRequestCreateDto.LeaveType.ToString();
            }
            else if (leaveRequestCreateDto.LeaveType == LeaveType.Emergency && string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Reason is required for Emergency leave.");
            }

            // Auto-calculate number of days
            var numberOfDays = CalculateNumberOfDays(leaveRequestCreateDto.FromDate, leaveRequestCreateDto.ToDate, leaveRequestCreateDto.IsHalfDay);

            // Check leave balance before creating request
            var currentYear = leaveRequestCreateDto.FromDate.Year;
            if (!await _leaveBalanceService.HasSufficientBalanceAsync(leaveRequestCreateDto.EmployeeId, leaveRequestCreateDto.LeaveType, numberOfDays, currentYear))
            {
                throw new InvalidOperationException($"Insufficient leave balance. You don't have enough {leaveRequestCreateDto.LeaveType} leave days.");
            }

            if (await _leaveRequestRepository.HasOverlappingLeaveAsync(leaveRequestCreateDto.EmployeeId, leaveRequestCreateDto.FromDate, leaveRequestCreateDto.ToDate))
            {
                throw new InvalidOperationException("Leave request overlaps with existing approved leave.");
            }

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = leaveRequestCreateDto.EmployeeId,
                LeaveType = leaveRequestCreateDto.LeaveType,
                FromDate = leaveRequestCreateDto.FromDate,
                ToDate = leaveRequestCreateDto.ToDate,
                NumberOfDays = numberOfDays,
                IsHalfDay = leaveRequestCreateDto.IsHalfDay,
                HalfDayPeriod = leaveRequestCreateDto.HalfDayPeriod,
                Reason = reason,
                Status = LeaveStatus.Pending
            };

            var createdLeaveRequest = await _leaveRequestRepository.CreateAsync(leaveRequest);
            var result = await _leaveRequestRepository.GetByIdAsync(createdLeaveRequest.LeaveRequestId);
            return MapToResponseDto(result!);
        }

        public async Task<LeaveRequestResponseDto> UpdateLeaveRequestAsync(LeaveRequestUpdateDto leaveRequestUpdateDto)
        {
            if (!await _leaveRequestRepository.ExistsAsync(leaveRequestUpdateDto.LeaveRequestId))
            {
                throw new KeyNotFoundException("Leave request not found.");
            }

            if (!await _leaveRequestRepository.EmployeeExistsAsync(leaveRequestUpdateDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            if (leaveRequestUpdateDto.FromDate > leaveRequestUpdateDto.ToDate)
            {
                throw new ArgumentException("From date must be less than or equal to to date.");
            }

            // Auto-calculate number of days
            var numberOfDays = CalculateNumberOfDays(leaveRequestUpdateDto.FromDate, leaveRequestUpdateDto.ToDate, leaveRequestUpdateDto.IsHalfDay);

            if (await _leaveRequestRepository.HasOverlappingLeaveAsync(leaveRequestUpdateDto.EmployeeId, leaveRequestUpdateDto.FromDate, leaveRequestUpdateDto.ToDate, leaveRequestUpdateDto.LeaveRequestId))
            {
                throw new InvalidOperationException("Leave request overlaps with existing approved leave.");
            }

            var leaveRequest = new LeaveRequest
            {
                LeaveRequestId = leaveRequestUpdateDto.LeaveRequestId,
                EmployeeId = leaveRequestUpdateDto.EmployeeId,
                LeaveType = leaveRequestUpdateDto.LeaveType,
                FromDate = leaveRequestUpdateDto.FromDate,
                ToDate = leaveRequestUpdateDto.ToDate,
                NumberOfDays = numberOfDays,
                IsHalfDay = leaveRequestUpdateDto.IsHalfDay,
                HalfDayPeriod = leaveRequestUpdateDto.HalfDayPeriod,
                Reason = leaveRequestUpdateDto.Reason,
                Status = leaveRequestUpdateDto.Status,
                ApprovedBy = leaveRequestUpdateDto.ApprovedBy
            };

            var updatedLeaveRequest = await _leaveRequestRepository.UpdateAsync(leaveRequest);
            var result = await _leaveRequestRepository.GetByIdAsync(updatedLeaveRequest.LeaveRequestId);
            return MapToResponseDto(result!);
        }



        public async Task<LeaveRequestResponseDto> UpdateLeaveRequestStatusAsync(int id, LeaveStatus status, string? approvedBy = null)
        {
            if (!await _leaveRequestRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException("Leave request not found.");
            }

            var existingLeaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
            if (existingLeaveRequest == null)
            {
                throw new KeyNotFoundException("Leave request not found.");
            }

            var previousStatus = existingLeaveRequest.Status;
            existingLeaveRequest.Status = status;
            if (!string.IsNullOrWhiteSpace(approvedBy))
            {
                existingLeaveRequest.ApprovedBy = approvedBy;
            }

            var updatedLeaveRequest = await _leaveRequestRepository.UpdateAsync(existingLeaveRequest);
            
            // Update leave balance when leave is approved
            if (status == LeaveStatus.Approved && previousStatus != LeaveStatus.Approved)
            {
                await _leaveBalanceService.UpdateLeaveBalanceAsync(
                    existingLeaveRequest.EmployeeId, 
                    existingLeaveRequest.LeaveType, 
                    existingLeaveRequest.NumberOfDays, 
                    existingLeaveRequest.FromDate.Year);
            }
            
            return MapToResponseDto(updatedLeaveRequest);
        }

        private static decimal CalculateNumberOfDays(DateTime fromDate, DateTime toDate, bool isHalfDay)
        {
            if (isHalfDay)
            {
                // For half day, check if it's a weekday
                return (fromDate.DayOfWeek != DayOfWeek.Saturday && fromDate.DayOfWeek != DayOfWeek.Sunday) ? 0.5m : 0;
            }

            var totalDays = 0;
            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                // Count only weekdays (Monday to Friday)
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    totalDays++;
                }
            }
            return totalDays;
        }

        private static LeaveRequestResponseDto MapToResponseDto(LeaveRequest leaveRequest)
        {
            return new LeaveRequestResponseDto
            {
                LeaveRequestId = leaveRequest.LeaveRequestId,
                EmployeeId = leaveRequest.EmployeeId,
                EmployeeName = $"{leaveRequest.Employee?.FirstName} {leaveRequest.Employee?.LastName}".Trim(),
                LeaveType = leaveRequest.LeaveType,
                FromDate = leaveRequest.FromDate,
                ToDate = leaveRequest.ToDate,
                NumberOfDays = leaveRequest.NumberOfDays,
                IsHalfDay = leaveRequest.IsHalfDay,
                HalfDayPeriod = leaveRequest.HalfDayPeriod,
                Reason = leaveRequest.Reason,
                Status = leaveRequest.Status,
                ApprovedBy = leaveRequest.ApprovedBy,
                ApproverName = leaveRequest.Approver != null ? $"{leaveRequest.Approver.FirstName} {leaveRequest.Approver.LastName}".Trim() : null
            };
        }
    }
}