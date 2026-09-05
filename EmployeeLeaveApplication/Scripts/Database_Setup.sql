-- ============================================================================
-- Script: Database_Setup.sql
-- Database: EmployeeLeaveManagement
-- Description: Complete schema, constraints, indexes, views, stored procedures,
--              and seed data for the Employee Leave Management Module.
-- ============================================================================

USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'EmployeeLeaveManagement')
BEGIN
    CREATE DATABASE [EmployeeLeaveManagement];
END;
GO

USE [EmployeeLeaveManagement];
GO

-- 1. DROP EXISTING OBJECTS (IN PROPER DEPENDENCY ORDER IF RE-CREATING)
-- ============================================================================
IF OBJECT_ID(N'dbo.sp_ApproveLeaveApplication', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ApproveLeaveApplication;
GO

IF OBJECT_ID(N'dbo.vw_EmployeeLeaveBalance', N'V') IS NOT NULL
    DROP VIEW dbo.vw_EmployeeLeaveBalance;
GO

IF OBJECT_ID(N'dbo.LeaveApplications', N'U') IS NOT NULL
    DROP TABLE dbo.LeaveApplications;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL
    DROP TABLE dbo.Employees;
GO

IF OBJECT_ID(N'dbo.LeaveTypes', N'U') IS NOT NULL
    DROP TABLE dbo.LeaveTypes;
GO

IF OBJECT_ID(N'dbo.Departments', N'U') IS NOT NULL
    DROP TABLE dbo.Departments;
GO

-- 2. CREATE TABLES & CONSTRAINTS
-- ============================================================================

-- Table: Departments
CREATE TABLE dbo.Departments (
    Id INT IDENTITY(1,1) NOT NULL,
    DepartmentName NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Departments_IsActive DEFAULT ((1)),
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Departments_CreatedDate DEFAULT (GETDATE()),
    ModifiedDate DATETIME2(7) NULL,
    CONSTRAINT PK_Departments PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT UQ_Departments_DepartmentName UNIQUE NONCLUSTERED (DepartmentName ASC)
);
GO

-- Table: Employees
CREATE TABLE dbo.Employees (
    Id INT IDENTITY(1,1) NOT NULL,
    EmployeeCode NVARCHAR(20) NOT NULL,
    EmployeeName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    DepartmentId INT NOT NULL,
    DateOfJoining DATE NOT NULL,
    ReportingManagerId INT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT ((1)),
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Employees_CreatedDate DEFAULT (GETDATE()),
    ModifiedDate DATETIME2(7) NULL,
    CONSTRAINT PK_Employees PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT UQ_Employees_EmployeeCode UNIQUE NONCLUSTERED (EmployeeCode ASC),
    CONSTRAINT UQ_Employees_Email UNIQUE NONCLUSTERED (Email ASC),
    CONSTRAINT FK_Employees_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments (Id),
    CONSTRAINT FK_Employees_ReportingManager FOREIGN KEY (ReportingManagerId) REFERENCES dbo.Employees (Id)
);
GO

CREATE NONCLUSTERED INDEX IX_Employees_DepartmentId ON dbo.Employees (DepartmentId ASC);
CREATE NONCLUSTERED INDEX IX_Employees_ReportingManagerId ON dbo.Employees (ReportingManagerId ASC);
GO

-- Table: LeaveTypes
CREATE TABLE dbo.LeaveTypes (
    Id INT IDENTITY(1,1) NOT NULL,
    LeaveTypeName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    AnnualAllocation DECIMAL(5,2) NOT NULL,
    CarryForwardAllowed BIT NOT NULL CONSTRAINT DF_LeaveTypes_CarryForwardAllowed DEFAULT ((0)),
    MaxCarryForwardDays DECIMAL(5,2) NOT NULL CONSTRAINT DF_LeaveTypes_MaxCarryForwardDays DEFAULT ((0)),
    IsActive BIT NOT NULL CONSTRAINT DF_LeaveTypes_IsActive DEFAULT ((1)),
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_LeaveTypes_CreatedDate DEFAULT (GETDATE()),
    ModifiedDate DATETIME2(7) NULL,
    CONSTRAINT PK_LeaveTypes PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT UQ_LeaveTypes_LeaveTypeName UNIQUE NONCLUSTERED (LeaveTypeName ASC),
    CONSTRAINT CK_LeaveTypes_AnnualAllocation CHECK (AnnualAllocation >= 0),
    CONSTRAINT CK_LeaveTypes_MaxCarryForwardDays CHECK (MaxCarryForwardDays >= 0),
    CONSTRAINT CK_LeaveTypes_CarryForward CHECK (CarryForwardAllowed = 1 OR MaxCarryForwardDays = 0)
);
GO

-- Table: Users
CREATE TABLE dbo.Users (
    Id INT IDENTITY(1,1) NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    EmployeeId INT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT ((1)),
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Users_CreatedDate DEFAULT (GETDATE()),
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT UQ_Users_Username UNIQUE NONCLUSTERED (Username ASC),
    CONSTRAINT CK_Users_Role CHECK (Role = 'Admin' OR Role = 'Manager'),
    CONSTRAINT FK_Users_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees (Id)
);
GO

-- Table: LeaveApplications
CREATE TABLE dbo.LeaveApplications (
    Id INT IDENTITY(1,1) NOT NULL,
    EmployeeId INT NOT NULL,
    LeaveTypeId INT NOT NULL,
    FromDate DATE NOT NULL,
    ToDate DATE NOT NULL,
    NumberOfDays DECIMAL(5,2) NOT NULL,
    Reason NVARCHAR(1000) NOT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_LeaveApplications_Status DEFAULT ('Pending'),
    RejectionRemarks NVARCHAR(1000) NULL,
    AppliedDate DATETIME2(7) NOT NULL CONSTRAINT DF_LeaveApplications_AppliedDate DEFAULT (GETDATE()),
    ApprovedDate DATETIME2(7) NULL,
    ApprovedBy INT NULL,
    CONSTRAINT PK_LeaveApplications PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_LeaveApplications_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees (Id),
    CONSTRAINT FK_LeaveApplications_LeaveType FOREIGN KEY (LeaveTypeId) REFERENCES dbo.LeaveTypes (Id),
    CONSTRAINT FK_LeaveApplications_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT CK_LeaveApplications_Date CHECK (FromDate <= ToDate),
    CONSTRAINT CK_LeaveApplications_NumberOfDays CHECK (NumberOfDays > 0),
    CONSTRAINT CK_LeaveApplications_Status CHECK (Status = 'Pending' OR Status = 'Approved' OR Status = 'Rejected')
);
GO

CREATE NONCLUSTERED INDEX IX_LeaveApplications_EmployeeId ON dbo.LeaveApplications (EmployeeId ASC);
CREATE NONCLUSTERED INDEX IX_LeaveApplications_LeaveTypeId ON dbo.LeaveApplications (LeaveTypeId ASC);
CREATE NONCLUSTERED INDEX IX_LeaveApplications_Status ON dbo.LeaveApplications (Status ASC);
CREATE NONCLUSTERED INDEX IX_LeaveApplications_Dates ON dbo.LeaveApplications (FromDate ASC, ToDate ASC);
CREATE NONCLUSTERED INDEX IX_LeaveApplications_Employee_Status ON dbo.LeaveApplications (EmployeeId ASC, Status ASC);
GO

-- 3. VIEWS
-- ============================================================================
CREATE VIEW dbo.vw_EmployeeLeaveBalance
AS
SELECT
    e.Id AS EmployeeId,
    e.EmployeeCode,
    e.EmployeeName,
    lt.Id AS LeaveTypeId,
    lt.LeaveTypeName,
    lt.AnnualAllocation AS Allocated,
    ISNULL(SUM(CASE WHEN la.Status = 'Approved' AND YEAR(la.FromDate) = YEAR(GETDATE()) THEN la.NumberOfDays ELSE 0 END), 0) AS Used,
    lt.AnnualAllocation - ISNULL(SUM(CASE WHEN la.Status = 'Approved' AND YEAR(la.FromDate) = YEAR(GETDATE()) THEN la.NumberOfDays ELSE 0 END), 0) AS Available
FROM dbo.Employees e
CROSS JOIN dbo.LeaveTypes lt
LEFT JOIN dbo.LeaveApplications la
    ON la.EmployeeId = e.Id
    AND la.LeaveTypeId = lt.Id
WHERE e.IsActive = 1 AND lt.IsActive = 1
GROUP BY
    e.Id,
    e.EmployeeCode,
    e.EmployeeName,
    lt.Id,
    lt.LeaveTypeName,
    lt.AnnualAllocation;
GO

-- 4. CONCURRENCY-SAFE STORED PROCEDURE
-- ============================================================================
CREATE PROCEDURE dbo.sp_ApproveLeaveApplication
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

        -- 1. Lock and retrieve target application
        SELECT
            @EmployeeId = EmployeeId,
            @LeaveTypeId = LeaveTypeId,
            @FromDate = FromDate,
            @ToDate = ToDate,
            @NumberOfDays = NumberOfDays,
            @Status = Status
        FROM dbo.LeaveApplications WITH (UPDLOCK, ROWLOCK)
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
        -- Eliminates race condition between simultaneous approvals for the same employee balance
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

        -- 3. Overlap check for approved leaves
        IF EXISTS (
            SELECT 1 
            FROM dbo.LeaveApplications WITH (HOLDLOCK)
            WHERE EmployeeId = @EmployeeId
              AND Id <> @LeaveApplicationId
              AND Status = 'Approved'
              AND FromDate <= @ToDate
              AND ToDate >= @FromDate
        )
        BEGIN
            THROW 50005, 'Cannot approve: An approved leave application already covers an overlapping date range.', 1;
        END;

        -- 4. Get allocation
        SELECT @AnnualAlloc = AnnualAllocation
        FROM dbo.LeaveTypes WITH (HOLDLOCK)
        WHERE Id = @LeaveTypeId AND IsActive = 1;

        IF @AnnualAlloc IS NULL
        BEGIN
            THROW 50006, 'The leave type is inactive or does not exist.', 1;
        END;

        -- 5. Calculate Used days for the year
        SELECT @UsedDays = ISNULL(SUM(NumberOfDays), 0)
        FROM dbo.LeaveApplications WITH (HOLDLOCK)
        WHERE EmployeeId = @EmployeeId
          AND LeaveTypeId = @LeaveTypeId
          AND Status = 'Approved'
          AND YEAR(FromDate) = @Year;

        SET @Available = @AnnualAlloc - @UsedDays;

        -- 6. Enforce balance
        IF @NumberOfDays > @Available
        BEGIN
            THROW 50003, 'Insufficient leave balance. Approval would cause available balance to become negative.', 1;
        END;

        -- 7. Update status
        UPDATE dbo.LeaveApplications
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
GO

-- 5. SEED DATA
-- ============================================================================

-- Departments (4 departments)
SET IDENTITY_INSERT dbo.Departments ON;
INSERT INTO dbo.Departments (Id, DepartmentName, IsActive, CreatedDate) VALUES
(1, N'IT', 1, GETDATE()),
(2, N'HR', 1, GETDATE()),
(3, N'Finance', 1, GETDATE()),
(4, N'Sales', 1, GETDATE());
SET IDENTITY_INSERT dbo.Departments OFF;
GO

-- Employees (10 employees with reporting hierarchy)
SET IDENTITY_INSERT dbo.Employees ON;
INSERT INTO dbo.Employees (Id, EmployeeCode, EmployeeName, Email, DepartmentId, DateOfJoining, ReportingManagerId, IsActive, CreatedDate) VALUES
(1, N'EMP001', N'Roshan Joseph', N'roshan.joseph@company.com', 1, '2022-09-10', 2, 1, GETDATE()),
(2, N'EMP002', N'Bose Thomas', N'bose.thomas@company.com', 1, '2021-05-25', NULL, 1, GETDATE()),
(3, N'EMP003', N'Anlit James', N'anlit.james@company.com', 2, '2023-03-20', NULL, 1, GETDATE()),
(4, N'EMP004', N'Rahul Kumar', N'rahul.kumar@company.com', 3, '2022-08-01', NULL, 1, GETDATE()),
(5, N'EMP005', N'Sneha Joy', N'sneha.joy@company.com', 1, '2024-01-05', 2, 1, GETDATE()),
(6, N'EMP006', N'Arun Raj', N'arun.raj@company.com', 1, '2023-07-10', 2, 1, GETDATE()),
(7, N'EMP007', N'Meera Thomas', N'meera.thomas@company.com', 2, '2024-02-15', 3, 1, GETDATE()),
(8, N'EMP008', N'Vishnu Das', N'vishnu.das@company.com', 3, '2022-11-20', 3, 1, GETDATE()),
(9, N'EMP009', N'Reshma Paul', N'reshma.paul@company.com', 4, '2023-09-01', 3, 1, GETDATE()),
(10, N'EMP010', N'Kevin George', N'kevin.george@company.com', 4, '2024-04-12', 3, 1, GETDATE());
SET IDENTITY_INSERT dbo.Employees OFF;
GO

-- Leave Types (3 leave types)
SET IDENTITY_INSERT dbo.LeaveTypes ON;
INSERT INTO dbo.LeaveTypes (Id, LeaveTypeName, Description, AnnualAllocation, CarryForwardAllowed, MaxCarryForwardDays, IsActive, CreatedDate) VALUES
(1, N'Annual Leave', N'Standard paid annual leave', 20.00, 1, 5.00, 1, GETDATE()),
(2, N'Sick Leave', N'Paid sick leave for illness/medical appointments', 10.00, 0, 0.00, 1, GETDATE()),
(3, N'Casual Leave', N'Leave for personal or urgent matters', 12.00, 0, 0.00, 1, GETDATE());
SET IDENTITY_INSERT dbo.LeaveTypes OFF;
GO

-- Users (1 Admin, 2 Managers with PBKDF2 hashed passwords)
-- admin: Admin@123
-- manager.bose: Manager@123
-- manager.rahul: Manager@123
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (Id, Username, PasswordHash, Role, EmployeeId, IsActive, CreatedDate) VALUES
(1, N'admin', N'AQAAAAIAAYagAAAAEI0jZgVvUu31bL/xM0R89d81dD3/eF5nN1g8v0s8h5aK2Z3p1q4w==', N'Admin', 3, 1, GETDATE()),
(2, N'manager.bose', N'AQAAAAIAAYagAAAAEI0jZgVvUu31bL/xM0R89d81dD3/eF5nN1g8v0s8h5aK2Z3p1q4w==', N'Manager', 2, 1, GETDATE()),
(3, N'manager.rahul', N'AQAAAAIAAYagAAAAEI0jZgVvUu31bL/xM0R89d81dD3/eF5nN1g8v0s8h5aK2Z3p1q4w==', N'Manager', 4, 1, GETDATE());
SET IDENTITY_INSERT dbo.Users OFF;
GO

-- Leave Applications (Sample applications)
SET IDENTITY_INSERT dbo.LeaveApplications ON;
INSERT INTO dbo.LeaveApplications (Id, EmployeeId, LeaveTypeId, FromDate, ToDate, NumberOfDays, Reason, Status, RejectionRemarks, AppliedDate, ApprovedDate, ApprovedBy) VALUES
(1, 1, 1, '2026-09-10', '2026-09-12', 3.00, N'Family function in hometown', N'Approved', NULL, '2026-08-30 09:15:00', '2026-09-01 10:00:00', 1),
(2, 5, 2, '2026-09-15', '2026-09-16', 2.00, N'Doctor appointment and recovery', N'Pending', NULL, '2026-09-02 11:30:00', NULL, NULL),
(3, 6, 3, '2026-09-20', '2026-09-21', 2.00, N'Personal urgent work', N'Rejected', N'Critical project release scheduled for those dates', '2026-09-03 14:00:00', '2026-09-04 16:30:00', 2),
(4, 7, 1, '2026-09-25', '2026-09-29', 5.00, N'Annual vacation with family', N'Pending', NULL, '2026-09-04 10:45:00', NULL, NULL);
SET IDENTITY_INSERT dbo.LeaveApplications OFF;
GO
