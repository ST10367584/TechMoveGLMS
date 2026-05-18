# TechMove GLMS — ASP.NET Core MVC
### ST10369372 | Phathutshedzo Ramakuela | EAPD7111 — Part 2

---

## Project Structure

```
TechMoveGLMS/
├── TechMoveGLMS.sln
├── TechMoveGLMS.Web/                  ← ASP.NET Core MVC (Monolith)
│   ├── Controllers/
│   │   ├── HomeController.cs          ← Dashboard
│   │   ├── ClientsController.cs       ← Full CRUD
│   │   ├── ContractsController.cs     ← CRUD + PDF upload + LINQ search
│   │   └── ServiceRequestsController.cs ← CRUD + workflow guard + currency
│   ├── Models/
│   │   ├── Client.cs
│   │   ├── Contract.cs                ← Enums: ContractStatus, ServiceLevel
│   │   ├── ServiceRequest.cs          ← USD + ZAR + ExchangeRateUsed columns
│   │   └── ViewModels/ViewModels.cs
│   ├── Data/
│   │   └── ApplicationDbContext.cs    ← EF Core DbContext + seed data
│   ├── Services/
│   │   ├── CurrencyService.cs         ← HttpClient → ExchangeRate-API → ZAR
│   │   └── FileService.cs             ← PDF-only validation + save/delete
│   ├── Migrations/
│   │   ├── 20240101000000_InitialCreate.cs
│   │   └── ApplicationDbContextModelSnapshot.cs
│   ├── Views/
│   │   ├── Home/Index.cshtml          ← Dashboard KPIs
│   │   ├── Clients/                   ← Index, Create, Edit, Details, Delete
│   │   ├── Contracts/                 ← Index (filter), Create, Edit, Details, Delete
│   │   └── ServiceRequests/           ← Create (live FX), Edit, Delete, Index
│   ├── wwwroot/
│   │   ├── css/site.css
│   │   └── uploads/contracts/         ← PDF files saved here (file server sim)
│   ├── appsettings.json               ← Connection string here
│   └── Program.cs                     ← DI, EF, HttpClient, auto-migrate
│
└── TechMoveGLMS.Tests/                ← xUnit test project
    ├── CurrencyCalculationTests.cs    ← 9 tests for USD→ZAR math
    ├── FileValidationTests.cs         ← 11 tests for file type enforcement
    └── WorkflowLogicTests.cs          ← 6 tests for contract status rules
```

---

## Step 1 — Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| Visual Studio 2022 | 17.x | https://visualstudio.microsoft.com |
| .NET 8 SDK | 8.0+ | https://dotnet.microsoft.com/download |
| SQL Server Express | 2019/2022 | https://www.microsoft.com/en-us/sql-server/sql-server-downloads |
| SQL Server Management Studio | 19+ | https://aka.ms/ssmsfullsetup |

---

## Step 2 — Configure Connection String

Open `TechMoveGLMS.Web/appsettings.json` and update the connection string to match
your SQL Server instance name:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TechMoveGLMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

**Common instance names:**
- `localhost\SQLEXPRESS` — default SQL Server Express
- `localhost\MSSQLSERVER` — default full SQL Server
- `.\SQLEXPRESS` — shorthand for local Express
- `(localdb)\MSSQLLocalDB` — Visual Studio LocalDB

> To find your instance name: open SSMS → the server name in the login dialog is it.

---

## Step 3 — Apply Migrations (Create the Database)

### Option A — Using Visual Studio Package Manager Console (recommended)

1. Open the solution in Visual Studio
2. Go to **Tools → NuGet Package Manager → Package Manager Console**
3. Make sure **Default Project** is set to `TechMoveGLMS.Web`
4. Run:

```powershell
Update-Database
```

This will:
- Create the `TechMoveGLMS` database in SQL Server
- Create tables: `Clients`, `Contracts`, `ServiceRequests`
- Insert 2 seed clients

### Option B — Using dotnet CLI

```bash
cd TechMoveGLMS.Web
dotnet ef database update
```

### Verify in SSMS
Open SSMS → connect → expand **Databases → TechMoveGLMS → Tables**  
You should see: `dbo.Clients`, `dbo.Contracts`, `dbo.ServiceRequests`

---

## Step 4 — Run the Application

### In Visual Studio:
Press **F5** or click the green **Run** button (IIS Express or https profile)

### Via CLI:
```bash
cd TechMoveGLMS.Web
dotnet run
```

Navigate to: `https://localhost:5001` (or whichever port shown in console)

---

## Step 5 — Run Unit Tests

### In Visual Studio:
1. Go to **Test → Run All Tests** (or press `Ctrl+R, A`)
2. Open **Test Explorer** (Test → Test Explorer) to see results

### Via CLI:
```bash
cd TechMoveGLMS.Tests
dotnet test --verbosity normal
```

Expected output:
```
Test summary:
  Passed: 26
  Failed: 0
  Total:  26
```

### Test Coverage:

| Test Class | Tests | What is tested |
|-----------|-------|----------------|
| `CurrencyCalculationTests` | 9 | USD→ZAR math, rounding, negative/zero guards |
| `FileValidationTests` | 11 | .pdf allowed, .exe/.docx/.jpg rejected, size limit |
| `WorkflowLogicTests` | 6 | Expired/OnHold contracts block service requests |

---

## How Database Updates Work (Live Sync)

Every action on the website immediately writes to SQL Server:

| Action | Method | SQL Operation |
|--------|--------|---------------|
| Add Client | POST /Clients/Create | `INSERT INTO Clients` |
| Edit Client | POST /Clients/Edit/{id} | `UPDATE Clients SET ...` |
| Delete Client | POST /Clients/Delete/{id} | `DELETE FROM Clients` |
| Add Contract | POST /Contracts/Create | `INSERT INTO Contracts` |
| Upload PDF | POST /Contracts/Create | File saved to disk; path stored in `Contracts.SignedAgreementPath` |
| Create Service Request | POST /ServiceRequests/Create | `INSERT INTO ServiceRequests` with live ZAR |
| Edit Service Request | POST /ServiceRequests/Edit/{id} | `UPDATE ServiceRequests SET ...` |

`SaveChangesAsync()` is called immediately after every operation — **no buffering**.  
You can open SSMS, refresh the table, and see the change instantly.

---

## Features Implemented

### 1. Database & EF Core
- SQL Server via Entity Framework Core 8
- Code-first with migrations
- All 3 entities with proper FK relationships
- Enums stored as strings for readability in SSMS

### 2. File Handling (PDF Signed Agreements)
- Upload on Contract Create/Edit
- Saved to `wwwroot/uploads/contracts/` (simulated file server)
- Downloadable via the UI (`/Contracts/DownloadAgreement/{id}`)
- **Only .pdf accepted** — validated by extension AND MIME type

### 3. Workflow Logic
- `ServiceRequest` cannot be created if Contract is `Expired` or `OnHold`
- Guard exists in BOTH the GET and POST actions (never trust only the GET)
- Visual indicator on the Contract details page

### 4. LINQ Search (Contracts)
- Filter by: **keyword**, **status**, **client**, **start date range**
- All filters are composable (stack multiple at once)
- Implemented with LINQ `.Where()` chaining

### 5. Currency Exchange API
- Calls `https://open.er-api.com/v6/latest/USD` (free, no API key needed)
- `HttpClient` registered as a typed service
- Rate cached for 1 hour to avoid hammering the API
- Fallback rate (R18.50) if the API is unavailable
- Live AJAX preview on the Create page — type a USD amount, ZAR updates instantly
- `ExchangeRateUsed` saved alongside every service request for audit trail

### 6. Unit Tests (xUnit)
- Separate `TechMoveGLMS.Tests` project
- Uses `Moq` for mocking `IFormFile` and `IWebHostEnvironment`
- 26 tests total across 3 test classes

---

## Adding New Migrations (if you change models)

```powershell
# In Package Manager Console (default project = TechMoveGLMS.Web)
Add-Migration YourMigrationName
Update-Database
```

---

## Troubleshooting

| Problem | Solution |
|---------|---------|
| "Cannot open database" | Check connection string in appsettings.json matches your SSMS server name |
| "A network-related error..." | Ensure SQL Server service is running (services.msc → SQL Server) |
| Currency shows fallback rate | The free API may be down; R18.50 is used automatically |
| PDF upload fails | Ensure `wwwroot/uploads/contracts/` folder exists (created automatically on first run) |
| Tests fail to build | Ensure `Microsoft.AspNetCore.Http` NuGet package is installed in the Tests project |
