using Microsoft.EntityFrameworkCore;
using HRPayrollSystem_Payslip.Models;
using HRPayrollSystem_Payslip.Enums;
using System.Text.Json;
using System.Security.Claims;
namespace HRPayrollSystem_Payslip.Data
{
    public class HRPayrollDbContext: DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HRPayrollDbContext(DbContextOptions<HRPayrollDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<SalaryStructure> SalaryStructures { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Payslip> Payslips { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.SalaryStructure)
                .WithOne(s => s.Employee)
                .HasForeignKey<SalaryStructure>(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.ReportingManager)
                .WithMany()
                .HasForeignKey(e => e.ReportingManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.Approver)
                .WithMany()
                .HasForeignKey(l => l.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Payrolls)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payslip>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Payslips)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payslip>()
                .HasOne(p => p.Payroll)
                .WithMany(pr => pr.Payslips)
                .HasForeignKey(p => p.PayrollId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeDocument>()
                .HasOne(d => d.Employee)
                .WithMany(e => e.Documents)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraints
            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.OfficeEmail)
                .IsUnique();

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentId = 1, DepartmentName = "Human Resources", Description = "HR Department" },
                new Department { DepartmentId = 2, DepartmentName = "Information Technology", Description = "IT Department" },
                new Department { DepartmentId = 3, DepartmentName = "Finance", Description = "Finance Department" },
                new Department { DepartmentId = 4, DepartmentName = "Marketing", Description = "Marketing Department" },
                new Department { DepartmentId = 5, DepartmentName = "Operations", Description = "Operations Department" }
            );

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "HR Manager", Description = "Human Resources Manager", DepartmentId = 1 },
                new Role { RoleId = 2, RoleName = "Software Developer", Description = "Software Developer", DepartmentId = 2 },
                new Role { RoleId = 3, RoleName = "Senior Developer", Description = "Senior Software Developer", DepartmentId = 2 },
                new Role { RoleId = 4, RoleName = "Finance Manager", Description = "Finance Manager", DepartmentId = 3 },
                new Role { RoleId = 5, RoleName = "Marketing Executive", Description = "Marketing Executive", DepartmentId = 4 },
                new Role { RoleId = 6, RoleName = "Operations Manager", Description = "Operations Manager", DepartmentId = 5 },
                new Role { RoleId = 7, RoleName = "Team Lead", Description = "Team Lead", DepartmentId = 2 },
                new Role { RoleId = 8, RoleName = "Accountant", Description = "Accountant", DepartmentId = 3 }
            );

            // Seed Employees (3 employees) with hashed passwords
            modelBuilder.Entity<Employee>().HasData(
                new Employee { EmployeeId = "EMP001", FirstName = "John", LastName = "Doe", MobileNumber = "9876543210", PersonalEmail = "john.doe@personal.com", OfficeEmail = "john.doe@company.com", PasswordHash = "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", DepartmentId = 1, RoleId = 1, EmploymentType = EmploymentType.FullTime, DateOfJoining = new DateTime(2020, 1, 15), Status = EmployeeStatus.Active },
                new Employee { EmployeeId = "EMP002", FirstName = "Jane", LastName = "Smith", MobileNumber = "9876543211", PersonalEmail = "jane.smith@personal.com", OfficeEmail = "jane.smith@company.com", PasswordHash = "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", DepartmentId = 2, RoleId = 3, EmploymentType = EmploymentType.FullTime, DateOfJoining = new DateTime(2019, 3, 10), ReportingManagerId = "EMP001", Status = EmployeeStatus.Active },
                new Employee { EmployeeId = "EMP003", FirstName = "Mike", LastName = "Johnson", MobileNumber = "9876543212", PersonalEmail = "mike.johnson@personal.com", OfficeEmail = "mike.johnson@company.com", PasswordHash = "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", DepartmentId = 2, RoleId = 2, EmploymentType = EmploymentType.FullTime, DateOfJoining = new DateTime(2021, 6, 20), ReportingManagerId = "EMP002", Status = EmployeeStatus.Active }
            );

            // Seed Salary Structures
            modelBuilder.Entity<SalaryStructure>().HasData(
                new SalaryStructure { SalaryStructureId = 1, EmployeeId = "EMP001", BasicSalary = 80000, HRA = 16000, Allowances = 8000, Deductions = 2000, PF = 9600, Tax = 12000, NetSalary = 80400 },
                new SalaryStructure { SalaryStructureId = 2, EmployeeId = "EMP002", BasicSalary = 90000, HRA = 18000, Allowances = 9000, Deductions = 2500, PF = 10800, Tax = 15000, NetSalary = 88500 },
                new SalaryStructure { SalaryStructureId = 3, EmployeeId = "EMP003", BasicSalary = 60000, HRA = 12000, Allowances = 6000, Deductions = 1500, PF = 7200, Tax = 8000, NetSalary = 61300 }
            );
        }

        public override int SaveChanges()
        {
            LogChanges();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            LogChanges();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void LogChanges()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity.GetType() != typeof(AuditLog) && 
                           (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            foreach (var entry in entries)
            {
                var auditLog = new AuditLog
                {
                    EmployeeId = GetCurrentUserId(),
                    Action = GetActionType(entry.State),
                    TableName = entry.Entity.GetType().Name,
                    RecordId = GetPrimaryKeyValue(entry),
                    OldValues = entry.State == EntityState.Modified ? JsonSerializer.Serialize(GetOriginalValues(entry)) : null,
                    NewValues = entry.State == EntityState.Deleted ? null : JsonSerializer.Serialize(GetCurrentValues(entry)),
                    Timestamp = DateTime.UtcNow
                };

                AuditLogs.Add(auditLog);
            }
        }

        private string GetCurrentUserId()
        {
            // Try to get from JWT token claims, fallback to system user
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // Get EmployeeId from NameIdentifier claim (set in AuthService)
                var employeeIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(employeeIdClaim))
                {
                    return employeeIdClaim;
                }
            }
            return "SYSTEM";
        }

        private ActionType GetActionType(EntityState state)
        {
            return state switch
            {
                EntityState.Added => ActionType.Created,
                EntityState.Modified => ActionType.Updated,
                EntityState.Deleted => ActionType.Deleted,
                _ => ActionType.Created
            };
        }

        private string GetPrimaryKeyValue(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var keyName = Model.FindEntityType(entry.Entity.GetType())?.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
            return keyName != null ? entry.Property(keyName).CurrentValue?.ToString() ?? "" : "";
        }

        private Dictionary<string, object?> GetOriginalValues(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            return entry.OriginalValues.Properties.ToDictionary(
                p => p.Name,
                p => entry.OriginalValues[p]
            );
        }

        private Dictionary<string, object?> GetCurrentValues(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            return entry.CurrentValues.Properties.ToDictionary(
                p => p.Name,
                p => entry.CurrentValues[p]
            );
        }
    }
}
