# TechMove — ASP.NET Core MVC
### ST0367584 | Gundo Mathantshani | EAPD7111 — Part 2

---

## Project Structure

```text
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
    ├── CurrencyCalculationTests.cs    ← Tests for USD→ZAR conversion logic
    ├── FileValidationTests.cs         ← Tests for file type enforcement
    └── WorkflowLogicTests.cs          ← Tests for contract workflow rules
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

Open `TechMoveGLMS.Web/appsettings.json` and update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS02;Database=TechMoveGLMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### Common SQL Server Instance Names

- `localhost\SQLEXPRESS`
- `localhost\MSSQLSERVER`
- `.\SQLEXPRESS`
- `(localdb)\MSSQLLocalDB`

> Open SSMS to confirm your SQL Server instance name.

---

## Step 3 — Apply Database Migrations

### Option A — Visual Studio

1. Open the solution
2. Go to:
   `Tools → NuGet Package Manager → Package Manager Console`
3. Set default project to:
   `TechMoveGLMS.Web`
4. Run:

```powershell
Update-Database
```

This creates:
- `Clients`
- `Contracts`
- `ServiceRequests`

tables in SQL Server.

---

### Option B — CLI

```bash
cd TechMoveGLMS.Web
dotnet ef database update
```

---

## Step 4 — Run the Application

### Visual Studio

Press:

```text
F5
```

or click:

```text
Start Debugging
```

---

### CLI

```bash
cd TechMoveGLMS.Web
dotnet run
```

Navigate to:

```text
https://localhost:5001
```

---

## Step 5 — Run Unit Tests

### Visual Studio

Go to:

```text
Test → Run All Tests
```

---

### CLI

```bash
cd TechMoveGLMS.Tests
dotnet test --verbosity normal
```

Expected result:

```text
Test summary:
  Passed: All
  Failed: 0
```

---

## Test Coverage

| Test Class | Coverage |
|------------|----------|
| `CurrencyCalculationTests` | Currency conversion and rounding |
| `FileValidationTests` | PDF-only validation |
| `WorkflowLogicTests` | Contract workflow restrictions |

---

## Features Implemented

### Database & EF Core

- SQL Server + Entity Framework Core
- Code-first migrations
- Foreign key relationships
- Enum support

---

### File Handling

- PDF upload support
- Files stored in:
  `wwwroot/uploads/contracts/`
- Download support
- File type validation

---

### Workflow Rules

- Cannot create service requests for:
  - `Expired`
  - `OnHold`
  contracts

- Validation exists in both GET and POST actions

---

### LINQ Search

Contract filtering supports:
- Keyword
- Client
- Status
- Date range

---

### Currency Exchange API

Endpoint:

```text
https://open.er-api.com/v6/latest/USD
```

Features:
- Live USD → ZAR conversion
- Cached for 1 hour
- Fallback rate = `18.50`
- Exchange rate stored per request

---

### Unit Testing

- xUnit
- Moq
- Business rule validation
- File validation testing

---

## Database Live Sync

Every action updates SQL Server immediately using:

```csharp
await _context.SaveChangesAsync();
```

Examples:
- Add Client → INSERT
- Edit Contract → UPDATE
- Delete Service Request → DELETE

---

## Screenshots Folder

Create:

```text
/screenshots
```

Suggested screenshots:
- Dashboard
- Clients
- Contracts
- Service Requests
- Test Explorer
- PDF Upload

---

## Submission Checklist

- ✅ GitHub repository uploaded
- ✅ EF Core migrations included
- ✅ Unit tests completed
- ✅ Screenshots added
- ✅ Video walkthrough completed

---

## Author

**Gundo Mathantshani**  
Student Number: ST0367584


---

## License

Academic submission for Enterprise Software Development (EAPD7111).

© 2026 The Independent Institute of Education (Pty) Ltd.

---

## Troubleshooting

| Problem | Solution |
|---------|---------|
| Cannot open database | Check SQL Server instance name |
| SQL connection error | Ensure SQL Server service is running |
| Currency API unavailable | Fallback rate R18.50 used |
| PDF upload fails | Ensure upload folder exists |
| Tests fail | Restore NuGet packages and rebuild |
