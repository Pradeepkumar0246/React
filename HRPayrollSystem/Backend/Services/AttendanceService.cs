using HRPayrollSystem_Payslip.DTOs.AttendanceDTO;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;

        public AttendanceService(IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        public async Task<IEnumerable<AttendanceResponseDto>> GetAllAttendanceAsync()
        {
            var attendances = await _attendanceRepository.GetAllAsync();
            return attendances.Select(MapToResponseDto);
        }

        public async Task<AttendanceResponseDto?> GetAttendanceByIdAsync(int id)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(id);
            return attendance == null ? null : MapToResponseDto(attendance);
        }

        public async Task<IEnumerable<AttendanceResponseDto>> GetAttendanceByEmployeeIdAsync(string employeeId)
        {
            if (!await _attendanceRepository.EmployeeExistsAsync(employeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var attendances = await _attendanceRepository.GetByEmployeeIdAsync(employeeId);
            return attendances.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<AttendanceResponseDto>> GetAttendanceByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
            {
                throw new ArgumentException("From date cannot be greater than to date.");
            }

            var attendances = await _attendanceRepository.GetByDateRangeAsync(fromDate, toDate);
            return attendances.Select(MapToResponseDto);
        }

        public async Task<AttendanceResponseDto> CreateAttendanceAsync(AttendanceCreateDto attendanceCreateDto)
        {
            if (!await _attendanceRepository.EmployeeExistsAsync(attendanceCreateDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            if (attendanceCreateDto.Date > DateTime.Today)
            {
                throw new ArgumentException("Attendance date cannot exceed current date.");
            }

            if (await _attendanceRepository.AttendanceExistsForDateAsync(attendanceCreateDto.EmployeeId, attendanceCreateDto.Date))
            {
                throw new InvalidOperationException("Attendance already exists for this employee on this date.");
            }

            // Validate times only if both are provided
            if (attendanceCreateDto.LogoutTime.HasValue && attendanceCreateDto.LoginTime >= attendanceCreateDto.LogoutTime.Value)
            {
                throw new ArgumentException("Login time must be before logout time.");
            }

            // Calculate working hours only if logout time is provided
            var workingHours = attendanceCreateDto.LogoutTime.HasValue 
                ? (attendanceCreateDto.LogoutTime.Value - attendanceCreateDto.LoginTime).TotalHours 
                : 0;

            var attendance = new Attendance
            {
                EmployeeId = attendanceCreateDto.EmployeeId,
                Date = attendanceCreateDto.Date,
                LoginTime = attendanceCreateDto.LoginTime,
                LogoutTime = attendanceCreateDto.LogoutTime,
                WorkingHours = workingHours,
                Status = attendanceCreateDto.Status
            };

            var createdAttendance = await _attendanceRepository.CreateAsync(attendance);
            var result = await _attendanceRepository.GetByIdAsync(createdAttendance.AttendanceId);
            return MapToResponseDto(result!);
        }

        public async Task<AttendanceResponseDto> UpdateAttendanceAsync(AttendanceUpdateDto attendanceUpdateDto)
        {
            if (!await _attendanceRepository.ExistsAsync(attendanceUpdateDto.AttendanceId))
            {
                throw new KeyNotFoundException("Attendance record not found.");
            }

            if (!await _attendanceRepository.EmployeeExistsAsync(attendanceUpdateDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            if (attendanceUpdateDto.Date > DateTime.Today)
            {
                throw new ArgumentException("Attendance date cannot exceed current date.");
            }

            if (await _attendanceRepository.AttendanceExistsForDateAsync(attendanceUpdateDto.EmployeeId, attendanceUpdateDto.Date, attendanceUpdateDto.AttendanceId))
            {
                throw new InvalidOperationException("Attendance already exists for this employee on this date.");
            }

            // Validate times only if both are provided
            if (attendanceUpdateDto.LogoutTime.HasValue && attendanceUpdateDto.LoginTime >= attendanceUpdateDto.LogoutTime.Value)
            {
                throw new ArgumentException("Login time must be before logout time.");
            }

            // Calculate working hours only if logout time is provided
            var workingHours = attendanceUpdateDto.LogoutTime.HasValue 
                ? (attendanceUpdateDto.LogoutTime.Value - attendanceUpdateDto.LoginTime).TotalHours 
                : 0;

            var attendance = new Attendance
            {
                AttendanceId = attendanceUpdateDto.AttendanceId,
                EmployeeId = attendanceUpdateDto.EmployeeId,
                Date = attendanceUpdateDto.Date,
                LoginTime = attendanceUpdateDto.LoginTime,
                LogoutTime = attendanceUpdateDto.LogoutTime,
                WorkingHours = workingHours,
                Status = attendanceUpdateDto.Status
            };

            var updatedAttendance = await _attendanceRepository.UpdateAsync(attendance);
            var result = await _attendanceRepository.GetByIdAsync(updatedAttendance.AttendanceId);
            return MapToResponseDto(result!);
        }



        public async Task<AttendanceResponseDto> CheckInAsync(string employeeId)
        {
            if (!await _attendanceRepository.EmployeeExistsAsync(employeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var today = DateTime.Today;
            var existingAttendance = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId, today);
            
            // Allow check-in if no attendance today OR if already checked out
            if (existingAttendance != null && !existingAttendance.LogoutTime.HasValue)
            {
                throw new InvalidOperationException("Employee is already checked in.");
            }

            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                Date = today,
                LoginTime = DateTime.Now.TimeOfDay,
                LogoutTime = null,
                WorkingHours = 0,
                Status = AttendanceStatus.Present
            };

            var createdAttendance = await _attendanceRepository.CreateAsync(attendance);
            var result = await _attendanceRepository.GetByIdAsync(createdAttendance.AttendanceId);
            return MapToResponseDto(result!);
        }

        public async Task<AttendanceResponseDto> CheckOutAsync(string employeeId)
        {
            if (!await _attendanceRepository.EmployeeExistsAsync(employeeId))
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var today = DateTime.Today;
            var existingAttendance = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId, today);
            
            if (existingAttendance == null)
            {
                throw new InvalidOperationException("Employee has not checked in today.");
            }

            if (existingAttendance.LogoutTime.HasValue)
            {
                throw new InvalidOperationException("Employee has already checked out today.");
            }

            var currentTime = DateTime.Now.TimeOfDay;
            var workingHours = (currentTime - existingAttendance.LoginTime).TotalHours;

            existingAttendance.LogoutTime = currentTime;
            existingAttendance.WorkingHours = workingHours;

            var updatedAttendance = await _attendanceRepository.UpdateAsync(existingAttendance);
            return MapToResponseDto(updatedAttendance);
        }

        private static AttendanceResponseDto MapToResponseDto(Attendance attendance)
        {
            return new AttendanceResponseDto
            {
                AttendanceId = attendance.AttendanceId,
                EmployeeId = attendance.EmployeeId,
                EmployeeName = $"{attendance.Employee?.FirstName} {attendance.Employee?.LastName}".Trim(),
                Date = attendance.Date,
                LoginTime = attendance.LoginTime,
                LogoutTime = attendance.LogoutTime,
                WorkingHours = attendance.WorkingHours,
                Status = attendance.Status
            };
        }
    }
}