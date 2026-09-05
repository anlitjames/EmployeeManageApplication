using EmployeeLeaveApplication.Data;
using EmployeeLeaveApplication.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveApplication.Services
{
    public class LeaveApprovalService : ILeaveApprovalService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILeaveApplicationService _leaveAppService;
        private readonly ILogger<LeaveApprovalService> _logger;

        public LeaveApprovalService(
            ApplicationDbContext context,
            ILeaveApplicationService leaveAppService,
            ILogger<LeaveApprovalService> logger)
        {
            _context = context;
            _leaveAppService = leaveAppService;
            _logger = logger;
        }

        public async Task<LeaveApprovalListViewModel> GetPendingApplicationsAsync(int userId, string role, int? managerEmployeeId)
        {
            var query = _context.LeaveApplications
                .AsNoTracking()
                .Include(la => la.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(la => la.LeaveType)
                .Where(la => la.Status == "Pending");

            // Reporting Manager can only see applications for employees who report to them
            if (role == "Manager" && managerEmployeeId.HasValue)
            {
                query = query.Where(la => la.Employee != null && la.Employee.ReportingManagerId == managerEmployeeId.Value);
            }

            var pendingList = await query
                .OrderBy(la => la.AppliedDate)
                .ToListAsync();

            var currentYear = DateTime.Today.Year;
            var items = new List<PendingLeaveItemViewModel>();

            foreach (var la in pendingList)
            {
                var balance = await _leaveAppService.GetEmployeeLeaveBalanceAsync(la.EmployeeId, la.LeaveTypeId, la.FromDate.Year);

                items.Add(new PendingLeaveItemViewModel
                {
                    Id = la.Id,
                    EmployeeId = la.EmployeeId,
                    EmployeeName = la.Employee?.EmployeeName ?? "Unknown",
                    EmployeeCode = la.Employee?.EmployeeCode ?? "Unknown",
                    DepartmentName = la.Employee?.Department?.DepartmentName ?? "—",
                    LeaveTypeName = la.LeaveType?.LeaveTypeName ?? "—",
                    FromDate = la.FromDate,
                    ToDate = la.ToDate,
                    NumberOfDays = la.NumberOfDays,
                    Reason = la.Reason,
                    AppliedDate = la.AppliedDate,
                    AvailableBalance = balance.Available
                });
            }

            return new LeaveApprovalListViewModel
            {
                PendingApplications = items,
                UserRole = role
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> ApproveApplicationAsync(
            int applicationId,
            int approvedByUserId,
            string role,
            int? managerEmployeeId)
        {
            var app = await _context.LeaveApplications
                .Include(la => la.Employee)
                .FirstOrDefaultAsync(la => la.Id == applicationId);

            if (app == null)
            {
                return (false, "Leave application not found.");
            }

            if (app.Status != "Pending")
            {
                return (false, "Only pending leave applications can be approved.");
            }

            // Authorization: Manager can only approve their subordinates
            if (role == "Manager")
            {
                if (!managerEmployeeId.HasValue || app.Employee?.ReportingManagerId != managerEmployeeId.Value)
                {
                    _logger.LogWarning("Unauthorized approval attempt by Manager {MgrId} for Application {AppId}.", managerEmployeeId, applicationId);
                    return (false, "You are only authorized to approve leave applications for your direct subordinates.");
                }
            }

            try
            {
                // Execute the concurrency-safe stored procedure
                var appIdParam = new SqlParameter("@LeaveApplicationId", applicationId);
                var approverParam = new SqlParameter("@ApprovedBy", approvedByUserId);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[sp_ApproveLeaveApplication] @LeaveApplicationId, @ApprovedBy",
                    appIdParam, approverParam);

                _logger.LogInformation("Leave application {AppId} successfully approved by user {UserId}.", applicationId, approvedByUserId);
                return (true, null);
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "Approval rejected by database logic for Application {AppId}: {Message}", applicationId, ex.Message);
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during leave approval for Application {AppId}.", applicationId);
                return (false, "An unexpected error occurred during approval. Please try again.");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RejectApplicationAsync(
            int applicationId,
            string rejectionRemarks,
            int approvedByUserId,
            string role,
            int? managerEmployeeId)
        {
            if (string.IsNullOrWhiteSpace(rejectionRemarks))
            {
                return (false, "Rejection remarks are mandatory.");
            }

            var app = await _context.LeaveApplications
                .Include(la => la.Employee)
                .FirstOrDefaultAsync(la => la.Id == applicationId);

            if (app == null)
            {
                return (false, "Leave application not found.");
            }

            if (app.Status != "Pending")
            {
                return (false, "Only pending leave applications can be rejected.");
            }

            // Authorization: Manager can only reject their subordinates
            if (role == "Manager")
            {
                if (!managerEmployeeId.HasValue || app.Employee?.ReportingManagerId != managerEmployeeId.Value)
                {
                    return (false, "You are only authorized to reject leave applications for your direct subordinates.");
                }
            }

            app.Status = "Rejected";
            app.RejectionRemarks = rejectionRemarks.Trim();
            app.ApprovedBy = approvedByUserId;
            app.ApprovedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Leave application {AppId} rejected by user {UserId}.", applicationId, approvedByUserId);
            return (true, null);
        }

        public async Task EnsureConcurrencySafeStoredProcedureAsync()
        {
            var sql = @"
CREATE OR ALTER PROCEDURE [dbo].[sp_ApproveLeaveApplication]
(
    @LeaveApplicationId INT,
    @ApprovedBy INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE
            @EmployeeId INT,
            @LeaveTypeId INT,
            @FromDate DATE,
            @ToDate DATE,
            @NumberOfDays DECIMAL(5,2),
            @Status NVARCHAR(20),
            @Available DECIMAL(10,2),
            @AnnualAlloc DECIMAL(5,2),
            @UsedDays DECIMAL(10,2),
            @Year INT,
            @LockResource NVARCHAR(255),
            @LockResult INT;

        -- 1. Lock and retrieve the target application row
        SELECT
            @EmployeeId = EmployeeId,
            @LeaveTypeId = LeaveTypeId,
            @FromDate = FromDate,
            @ToDate = ToDate,
            @NumberOfDays = NumberOfDays,
            @Status = Status
        FROM LeaveApplications WITH (UPDLOCK, ROWLOCK)
        WHERE Id = @LeaveApplicationId;

        IF @Status IS NULL
        BEGIN
            THROW 50001, 'Leave application not found.', 1;
        END;

        IF @Status <> 'Pending'
        BEGIN
            THROW 50002, 'Only pending leave applications can be approved.', 1;
        END;

        SET @Year = YEAR(@FromDate);

        -- 2. ACQUIRE EXCLUSIVE APPLICATION LOCK PER EMPLOYEE, LEAVE TYPE, AND YEAR
        -- This guarantees that concurrent approvals for the same employee, leave type and year run sequentially!
        SET @LockResource = 'LeaveApprove_Emp_' + CAST(@EmployeeId AS NVARCHAR(10)) + '_Type_' + CAST(@LeaveTypeId AS NVARCHAR(10)) + '_Year_' + CAST(@Year AS NVARCHAR(10));
        
        EXEC @LockResult = sp_getapplock 
            @Resource = @LockResource, 
            @LockMode = 'Exclusive', 
            @LockOwner = 'Transaction', 
            @LockTimeout = 15000;

        IF @LockResult < 0
        BEGIN
            THROW 50004, 'Could not acquire concurrency lock for leave approval. Please try again.', 1;
        END;

        -- 3. Check for any overlapping APPROVED leave for the same employee
        IF EXISTS (
            SELECT 1 
            FROM LeaveApplications WITH (HOLDLOCK)
            WHERE EmployeeId = @EmployeeId
              AND Id <> @LeaveApplicationId
              AND Status = 'Approved'
              AND FromDate <= @ToDate
              AND ToDate >= @FromDate
        )
        BEGIN
            THROW 50005, 'Cannot approve: An approved leave application already exists for this overlapping date range.', 1;
        END;

        -- 4. Get Annual Allocation
        SELECT @AnnualAlloc = AnnualAllocation
        FROM LeaveTypes WITH (HOLDLOCK)
        WHERE Id = @LeaveTypeId AND IsActive = 1;

        IF @AnnualAlloc IS NULL
        BEGIN
            THROW 50006, 'Leave type is inactive or does not exist.', 1;
        END;

        -- 5. Calculate Used Days in the specific year (HOLDLOCK prevents concurrent insertions/updates during calculation)
        SELECT @UsedDays = ISNULL(SUM(NumberOfDays), 0)
        FROM LeaveApplications WITH (HOLDLOCK)
        WHERE EmployeeId = @EmployeeId
          AND LeaveTypeId = @LeaveTypeId
          AND Status = 'Approved'
          AND YEAR(FromDate) = @Year;

        SET @Available = @AnnualAlloc - @UsedDays;

        -- 6. Enforce Available Balance check
        IF @NumberOfDays > @Available
        BEGIN
            THROW 50003, 'Insufficient leave balance. Approval would result in a negative balance.', 1;
        END;

        -- 7. Perform Atomic Update
        UPDATE LeaveApplications
        SET
            Status = 'Approved',
            ApprovedDate = GETDATE(),
            ApprovedBy = @ApprovedBy,
            RejectionRemarks = NULL
        WHERE Id = @LeaveApplicationId;

        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
";
            await _context.Database.ExecuteSqlRawAsync(sql);
            _logger.LogInformation("Concurrency-safe sp_ApproveLeaveApplication verified/updated in database.");
        }
    }
}
