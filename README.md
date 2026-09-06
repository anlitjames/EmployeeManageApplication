# Employee Leave Management Application

An enterprise-grade Employee Leave Management System built with **ASP.NET Core 8.0 MVC**, **C#**, **Entity Framework Core 8.0**, and **SQL Server**.

---

## 1. Technology Stack

- **Framework**: .NET 8.0 (`net8.0`)
- **Web Layer**: ASP.NET Core MVC with Razor Views
- **ORM**: Entity Framework Core 8.0.20 (`Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`)
- **Database**: Microsoft SQL Server 2019 / 2022 / Express / LocalDB
- **Authentication**: Cookie Authentication (`CookieAuthenticationDefaults.AuthenticationScheme`) with Claims Identity & Role-Based Access Control (RBAC)
- **Security**: PBKDF2 Password Hashing with HMAC-SHA256 (`Microsoft.AspNetCore.Identity.PasswordHasher<User>`), Global CSRF/Anti-Forgery token validation (`AutoValidateAntiforgeryTokenAttribute`)
- **Frontend**: HTML5, CSS3, Bootstrap 5, Bootstrap Icons, JavaScript, jQuery, jQuery Validation & Unobtrusive Validation, AJAX

---

## 2. Default Login Credentials

The application is pre-seeded with Admin and Manager credentials:

| Role | Username | Password | Linked Employee | Description |
| :--- | :--- | :--- | :--- | :--- |
| **Admin** | `admin` | `Admin@123` | EMP003 - Anlit James | Full administrative access to manage Employees, Leave Types, Approvals, and Balances |
| **Manager** | `manager.bose` | `Manager@123` | EMP002 - Bose Thomas | Approves/rejects leave applications for reporting subordinates (EMP001, EMP005, EMP006) |
| **Manager** | `Rahul` | `Manager@123` | EMP004 - Rahul Kumar | Approves/rejects leave applications for reporting subordinates |

> Passwords are cryptographically hashed using PBKDF2 with unique salts. No plaintext passwords are stored.

---

## 3. Database Configuration & Setup

### Database Connection
Configured in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=EmployeeLeaveManagement;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Automatic Setup at Startup
When you run the application via `dotnet run`, it automatically:
1. Validates the SQL Server connection.
2. Seeds default `Users` (`admin`, `manager.bose`, `manager.rahul`) if the table is empty.
3. Deploys or updates the **concurrency-safe stored procedure** `dbo.sp_ApproveLeaveApplication`.

### Manual Database Setup (Optional)
If you wish to re-create the database from scratch:
1. Open SQL Server Management Studio (SSMS) or Azure Data Studio.
2. Connect to `Server=.` (or `localhost`).
3. Open and execute the script:
   ```
   EmployeeLeaveApplication/Scripts/Database_Setup.sql
   ```

---

## 4. Implemented Features & Architecture

### A. Employee Management (`/Employee`)
- **Server-Side Pagination & Filtering**: Real-time search across Employee Name, Code, and Email. Filter by Department and Active/Inactive status.
- **CRUD Operations**: Add, Edit, View Details, and Activate/Deactivate employees.
- **Unique Validation**: Uniqueness of `EmployeeCode` and `Email` is enforced both on the client, in the Service layer, and via unique database indexes.
- **Reporting Hierarchy**: Self-referencing relationship ensuring an employee cannot report to themselves, with circular reporting prevention.

### B. Leave Type Management (`/LeaveType`)
- **Policy Configuration**: Create, Edit, View, and Toggle Status for Leave Types.
- **Carry-Forward Business Rule**: Strictly enforces that if `CarryForwardAllowed = false`, `MaxCarryForwardDays` **must be 0**. Enforced in the UI (dynamic disablement), the ViewModel (`IValidatableObject`), the Service layer, and SQL Server `CK_LeaveTypes_CarryForward` check constraint.

### C. Leave Application (`/LeaveApplication`)
- **Interactive UI**: Dropdown selection of active employees and active leave types.
- **Dynamic AJAX Balance**: Selecting an employee and leave type immediately fetches and displays their live allocated, used, and available balance without a page refresh.
- **Dynamic Working Days**: Selecting `FromDate` and `ToDate` calculates the inclusive number of days in real-time.
- **Server-Side Integrity (Zero Trust)**:
  - Client-submitted `NumberOfDays` is never trusted; the server recalculates the duration.
  - Overlap validation checks against all non-rejected applications (`Status != 'Rejected'`).
  - Available balance sufficiency check prevents overdrafting leave.
  - `FromDate` is validated to prevent back-dated applications.
  - Initial status is enforced to `Pending`.

### D. Concurrency-Safe Leave Approval (`/LeaveApproval`)
- **Role-Based Workflow**:
  - **Admin**: Can approve or reject applications for any employee in the company.
  - **Reporting Manager**: Can only view and decide on applications submitted by their direct subordinates.
- **Concurrency Protection (`sp_ApproveLeaveApplication`)**:
  - Solves the classic race condition where two simultaneous approvals could over-allocate leaves and produce a negative balance.
  - Utilizes SQL Server application locks:
    ```sql
    EXEC sp_getapplock
        @Resource = 'LeaveApprove_Emp_<EmpId>_Type_<TypeId>_Year_<Year>',
        @LockMode = 'Exclusive',
        @LockOwner = 'Transaction';
    ```
  - Serializes approval transactions for the same employee, leave type, and calendar year.
  - Rejection requires mandatory **Rejection Remarks**.
  - Automatically records `ApprovedDate` and `ApprovedBy`.

### E. Leave Balance Screen (`/LeaveBalance`)
- Displays: Employee Name, Department, Leave Type, Annual Allocation, Used (Approved) Days, and Available Balance.
- Filtering by Employee, Leave Type, and Year.
- Balances are computed dynamically from approved leaves (`Available = Allocated - Used`). Pending and Rejected applications do not deplete balance.

### F. Dashboard (`/Home/Index`)
- Metric KPI Cards: Total Employees, Active Employees, Pending Approvals, Approved Leaves, and Employees Currently on Leave Today.
- "Employees On Leave Today" real-time table.
- "Recent Applications" timeline.

---

## 5. Security Implementations

1. **Password Hashing**: Utilizes ASP.NET Core Identity's PBKDF2 implementation with HMAC-SHA256 and unique 128-bit per-user salt.
2. **Anti-Forgery (CSRF)**: `AutoValidateAntiforgeryTokenAttribute` is globally registered on all MVC POST actions. AJAX calls pass the verification token in HTTP headers via `$.ajaxPrefilter`.
3. **Authorization**: Strict `[Authorize(Roles = "Admin")]` and `[Authorize(Roles = "Admin,Manager")]` on all management endpoints.
4. **Overposting Prevention**: Uses isolated ViewModels with explicit mapping to entities rather than direct entity binding.
5. **Exception Shielding**: Internal SQL and runtime errors are logged via `ILogger` and translated to user-friendly notifications without leaking database internals or stack traces.

---

## 6. How to Run the Application

1. Open PowerShell or Command Prompt.
2. Navigate to the project folder:
   ```bash
   cd EmployeeLeaveApplication\EmployeeLeaveApplication
   ```
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run
   ```
5. Open your browser and navigate to:
   ```
   http://localhost:5080
   ```
6. Sign in with `admin` / `Admin@123` to test Admin features or `manager.bose` / `Manager@123` to test Manager features.
