namespace HRPayrollSystem_Payslip.DTOs.PayslipDTO
{
    public class PayslipResponseDto
    {
        public int PayslipId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int PayrollId { get; set; }
        public DateTime PayrollMonth { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public DateTime GeneratedDate { get; set; }
        public decimal NetPay { get; set; }
    }
}