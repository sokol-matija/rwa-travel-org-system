# Travel Organization System - Refactoring Plan

## Goal: Achieve Full Grade (100/100 points)

This document covers everything needed to convert the project from Razor Pages to MVC
and implement all missing features for maximum points.

---

## Current Score Estimate: ~70/100

| Ishod | Min (10) | Desired (10) | Status |
|-------|----------|--------------|--------|
| I1 - Web API CRUD + Logs | 10/10 | 10/10 | DONE |
| I2 - Database + Static HTML | 10/10 | 0/10 | Missing static HTML logs pages |
| I3 - MVC Web App | 10/10 | ~5/10 | Missing admin bookings view, image upload |
| I4 - Validation + Multi-tier | 10/10 | 0/10 | Missing multi-tier + AutoMapper |
| I5 - AJAX Profile + Pagination | 10/10 | ~5/10 | Missing AJAX pagination |
| **TOTAL** | **50** | **~20** | **~70/100** |

---

## Architecture Overview

### Current Structure (2 projects)
```
TravelOrganizationSystem.sln
├── WebAPI/    (REST API with EF Core + JWT) ✅ OK
└── WebApp/    (Razor Pages - NEEDS CONVERSION TO MVC)
```

### Target Structure (4 projects - for Ishod 4 Desired)
```
TravelOrganizationSystem.sln
├── WebAPI/        (REST API - presentation tier)
├── WebApp/        (MVC Web App - presentation tier)  ← CONVERT FROM RAZOR PAGES
├── BLL/           (Business Logic Layer)              ← NEW PROJECT
└── DAL/           (Data Access Layer)                 ← NEW PROJECT
```

---

## Phase 1: Convert WebApp from Razor Pages to MVC

### Why This Is Critical
The spec says "MVC modul" three times (Ishods 3, 4, 5). Using Razor Pages instead of
MVC Controllers+Views could cost 30-50 points.

### 1.1 Update Program.cs

**File:** `WebApp/Program.cs`

Replace:
```csharp
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
// ...
builder.Services.AddRazorPages();
```

With:
```csharp
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
// ...
builder.Services.AddControllersWithViews();
```

Remove:
```csharp
builder.Services.AddServerSideBlazor();
app.MapRazorPages();
```

Add/Keep:
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers(); // for API controllers like ProfileController
```

Update auth paths:
```csharp
options.LoginPath = "/Account/Login";      // change to "/Account/Login"
options.LogoutPath = "/Account/Logout";    // change to "/Account/Logout"
options.AccessDeniedPath = "/Account/AccessDenied";
```

### 1.2 Create MVC Folder Structure

Create these directories inside `WebApp/`:
```
Views/
├── Home/
├── Account/
├── Trips/
├── Destinations/
├── Admin/
│   ├── Trips/
│   ├── Destinations/
│   ├── Guides/
│   ├── GuideAssignments/
│   ├── Logs/
│   └── Bookings/          ← NEW (for Ishod 3 Desired #7)
└── Shared/
ViewModels/                 ← expand existing
```

### 1.3 Move Shared Layout Files

Move from `Pages/Shared/` to `Views/Shared/`:
- `_Layout.cshtml` → `Views/Shared/_Layout.cshtml`
- `_LoginPartial.cshtml` → `Views/Shared/_LoginPartial.cshtml`
- `_ValidationScriptsPartial.cshtml` → `Views/Shared/_ValidationScriptsPartial.cshtml`
- `_Layout.cshtml.css` → `Views/Shared/_Layout.cshtml.css`

Create new:
- `Views/_ViewImports.cshtml`:
  ```razor
  @using WebApp
  @using WebApp.Models
  @using WebApp.ViewModels
  @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
  ```
- `Views/_ViewStart.cshtml`:
  ```razor
  @{
      Layout = "_Layout";
  }
  ```

### 1.4 Convert Each Page Group to Controllers

Below is the mapping from Razor Pages to MVC Controllers. For each conversion:
1. Extract logic from `PageModel.OnGetAsync()` / `OnPostAsync()` into Controller actions
2. Move `.cshtml` to `Views/{Controller}/` folder
3. Remove `@page` directive from `.cshtml`
4. Change `@model PageModel` to `@model ViewModel`
5. Update all `asp-page` tag helpers to `asp-controller`/`asp-action`

#### HomeController (Landing Page)
| Source | Target |
|--------|--------|
| `Pages/Index.cshtml` + `.cs` | `Controllers/HomeController.cs` → `Views/Home/Index.cshtml` |
| `Pages/Privacy.cshtml` + `.cs` | `HomeController.Privacy()` → `Views/Home/Privacy.cshtml` |
| `Pages/Error.cshtml` + `.cs` | `HomeController.Error()` → `Views/Shared/Error.cshtml` |

```csharp
// Controllers/HomeController.cs
public class HomeController : Controller
{
    private readonly IDestinationService _destinationService;

    public HomeController(IDestinationService destinationService)
    {
        _destinationService = destinationService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var destinations = await _destinationService.GetAllDestinationsAsync();
        var vm = new HomeViewModel { FeaturedDestinations = destinations.Take(3).ToList() };
        return View(vm);
    }
}
```

#### AccountController (Login, Register, Profile, Logout, ChangePassword)
| Source | Target |
|--------|--------|
| `Pages/Account/Login.cshtml` + `.cs` | `Controllers/AccountController.cs` → `Views/Account/Login.cshtml` |
| `Pages/Account/Register.cshtml` + `.cs` | `AccountController.Register()` → `Views/Account/Register.cshtml` |
| `Pages/Account/Profile.cshtml` + `.cs` | `AccountController.Profile()` → `Views/Account/Profile.cshtml` |
| `Pages/Account/Logout.cshtml` + `.cs` | `AccountController.Logout()` |
| `Pages/Account/ChangePassword.cshtml` + `.cs` | `AccountController.ChangePassword()` → `Views/Account/ChangePassword.cshtml` |

Key pattern:
```csharp
// Controllers/AccountController.cs
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);
        // ... authentication logic from Login.cshtml.cs OnPostAsync
    }
}
```

#### TripsController (User-facing trip pages)
| Source | Target |
|--------|--------|
| `Pages/Trips/Index.cshtml` + `.cs` | `Controllers/TripsController.cs` → `Views/Trips/Index.cshtml` |
| `Pages/Trips/Details.cshtml` + `.cs` | `TripsController.Details(id)` → `Views/Trips/Details.cshtml` |
| `Pages/Trips/Book.cshtml` + `.cs` | `TripsController.Book(id)` → `Views/Trips/Book.cshtml` |
| `Pages/Trips/MyBookings.cshtml` + `.cs` | `TripsController.MyBookings()` → `Views/Trips/MyBookings.cshtml` |

#### DestinationsController (Destination CRUD pages)
| Source | Target |
|--------|--------|
| `Pages/Destinations/Index.cshtml` + `.cs` | `Controllers/DestinationsController.cs` → `Views/Destinations/Index.cshtml` |
| `Pages/Destinations/Create.cshtml` + `.cs` | `DestinationsController.Create()` → `Views/Destinations/Create.cshtml` |
| `Pages/Destinations/Edit.cshtml` + `.cs` | `DestinationsController.Edit(id)` → `Views/Destinations/Edit.cshtml` |
| `Pages/Destinations/Details.cshtml` + `.cs` | `DestinationsController.Details(id)` → `Views/Destinations/Details.cshtml` |

#### Admin Area Controllers
Convert admin pages to area or namespace-based controllers:

| Source | Target Controller |
|--------|-------------------|
| `Pages/Admin/Guides/*` | `Controllers/Admin/GuidesController.cs` → `Views/Admin/Guides/*` |
| `Pages/Admin/GuideAssignments/*` | `Controllers/Admin/GuideAssignmentsController.cs` → `Views/Admin/GuideAssignments/*` |
| `Pages/Admin/Logs/*` | `Controllers/Admin/LogsController.cs` → `Views/Admin/Logs/*` |
| NEW | `Controllers/Admin/BookingsController.cs` → `Views/Admin/Bookings/*` |

All admin controllers should have:
```csharp
[Authorize(Roles = "Admin")]
public class GuidesController : Controller { ... }
```

### 1.5 Update All Navigation Links

In `_Layout.cshtml` and all views, update links:

```html
<!-- BEFORE (Razor Pages) -->
<a asp-page="/Trips/Index">Trips</a>
<a asp-page="/Destinations/Index">Destinations</a>
<a asp-page="/Account/Login">Login</a>
<a asp-page="/Account/Profile">Profile</a>

<!-- AFTER (MVC) -->
<a asp-controller="Trips" asp-action="Index">Trips</a>
<a asp-controller="Destinations" asp-action="Index">Destinations</a>
<a asp-controller="Account" asp-action="Login">Login</a>
<a asp-controller="Account" asp-action="Profile">Profile</a>
```

Also update form tag helpers:
```html
<!-- BEFORE -->
<form method="post" asp-page="/Account/Login">

<!-- AFTER -->
<form method="post" asp-controller="Account" asp-action="Login">
```

And redirect calls in controllers:
```csharp
// BEFORE (PageModel)
return RedirectToPage("/Trips/Index");

// AFTER (Controller)
return RedirectToAction("Index", "Trips");
```

### 1.6 Keep Existing API Controllers

These controllers in `WebApp/Controllers/` stay as-is since they are already MVC-style:
- `ProfileController.cs` (AJAX API for profile updates)
- `TripsController.cs` (if it's an API controller, rename to avoid conflict)
- `UnsplashController.cs`

**Important:** If `Controllers/TripsController.cs` already exists as an API controller,
rename it to `TripsApiController.cs` with `[Route("api/[controller]")]` to avoid
conflict with the new MVC TripsController.

### 1.7 Delete Old Pages Directory

After all conversions are verified working, delete:
```
Pages/          ← entire directory (except keep Error.cshtml if moved to Shared)
```

---

## Phase 2: Static HTML Pages for Log Viewer (Ishod 2 Desired - 10 points)

### 2.1 Create Static HTML Files in WebAPI

**Location:** `WebAPI/wwwroot/`

Create two files:

#### `wwwroot/login.html`
- Username + Password input fields
- Login button calls `POST /api/auth/login`
- On success: store JWT token in `localStorage`, redirect to `logs.html`
- On error: show error message

#### `wwwroot/logs.html`
- "Log list" header + Logout button
- List of log entries (unordered list or table)
- Dropdown to select count: 10, 25, 50
- "Show Logs" button
- Fetches from `GET /api/logs/get/{N}` with `Authorization: Bearer {token}` header
- Logout clears `localStorage` and redirects to `login.html`

### 2.2 Enable Static File Serving in WebAPI

Already done in Program.cs: `app.UseStaticFiles();` is present.

Verify that navigating to `https://localhost:{port}/login.html` serves the file.

---

## Phase 3: Missing Ishod 3 Desired Features

### 3.1 Admin Bookings Page (Ishod 3 Desired #7 - user actions list)

Create `Controllers/Admin/BookingsController.cs`:
```csharp
[Authorize(Roles = "Admin")]
public class BookingsController : Controller
{
    public async Task<IActionResult> Index()
    {
        // Fetch all trip registrations grouped by user
        // Show: Username, Trip Name, Registration Date, Participants, Price, Status
        return View(viewModel);
    }
}
```

Create `Views/Admin/Bookings/Index.cshtml`:
- Table showing all user bookings
- Columns: User, Trip, Date, Participants, Total Price, Status
- Navigation consistent with other admin pages

### 3.2 Image Upload for Trips (Ishod 3 Desired - image entity)

Add to Trip Create/Edit forms:
```html
<input type="file" name="ImageFile" accept="image/*" />
```

In the controller:
```csharp
[HttpPost]
public async Task<IActionResult> Create(TripCreateViewModel model, IFormFile ImageFile)
{
    if (ImageFile != null && ImageFile.Length > 0)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
        var path = Path.Combine("wwwroot/images/trips", fileName);
        using var stream = new FileStream(path, FileMode.Create);
        await ImageFile.CopyToAsync(stream);
        model.ImageUrl = "/images/trips/" + fileName;
    }
    // ... save trip
}
```

Create directory: `WebApp/wwwroot/images/trips/`

---

## Phase 4: Multi-Tier Architecture + AutoMapper (Ishod 4 Desired - 10 points)

### 4.1 Create New Projects

```bash
# From the TravelOrganizationSystem directory:
dotnet new classlib -n BLL -f net8.0
dotnet new classlib -n DAL -f net8.0
dotnet sln add BLL/BLL.csproj
dotnet sln add DAL/DAL.csproj
```

### 4.2 Set Up Project References

```
DAL     → references nothing (only EF Core + SQL Server packages)
BLL     → references DAL
WebAPI  → references BLL (and DAL for DI registration)
WebApp  → references BLL (for ViewModels/DTOs)
```

```bash
# Add references
dotnet add BLL/BLL.csproj reference DAL/DAL.csproj
dotnet add WebAPI/WebAPI.csproj reference BLL/BLL.csproj
dotnet add WebAPI/WebAPI.csproj reference DAL/DAL.csproj
dotnet add WebApp/WebApp.csproj reference BLL/BLL.csproj
```

### 4.3 Move Code to Layers

#### DAL (Data Access Layer)
Move from `WebAPI/`:
- `Data/ApplicationDbContext.cs` → `DAL/Data/ApplicationDbContext.cs`
- `Models/Trip.cs` → `DAL/Models/Trip.cs`
- `Models/Destination.cs` → `DAL/Models/Destination.cs`
- `Models/Guide.cs` → `DAL/Models/Guide.cs`
- `Models/TripGuide.cs` → `DAL/Models/TripGuide.cs`
- `Models/TripRegistration.cs` → `DAL/Models/TripRegistration.cs`
- `Models/User.cs` → `DAL/Models/User.cs`
- `Models/Log.cs` → `DAL/Models/Log.cs`

Add repositories (optional but cleaner):
- `DAL/Repositories/ITripRepository.cs` + `TripRepository.cs`
- etc.

Install packages in DAL:
```bash
dotnet add DAL/DAL.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.3
```

#### BLL (Business Logic Layer)
Create business models (separate from DB models):
- `BLL/Models/TripBM.cs` (Business Model)
- `BLL/Models/DestinationBM.cs`
- `BLL/Models/GuideBM.cs`
- etc.

Move/recreate services from `WebAPI/Services/`:
- `BLL/Services/ITripService.cs` + `TripService.cs`
- `BLL/Services/IDestinationService.cs` + `DestinationService.cs`
- `BLL/Services/IGuideService.cs` + `GuideService.cs`
- `BLL/Services/ILogService.cs` + `LogService.cs`
- `BLL/Services/IUserService.cs` + `UserService.cs`
- `BLL/Services/ITripRegistrationService.cs` + `TripRegistrationService.cs`

Install AutoMapper:
```bash
dotnet add BLL/BLL.csproj package AutoMapper
```

#### WebAPI (Presentation Layer)
Keep only:
- Controllers (use BLL services)
- DTOs (API-specific request/response models)
- JWT/Swagger configuration
- Program.cs

#### WebApp (Presentation Layer)
Keep only:
- MVC Controllers (use BLL services or call WebAPI)
- Views
- ViewModels (view-specific models - NO navigation properties!)
- wwwroot

### 4.4 AutoMapper Configuration

Create mapping profiles:

```csharp
// BLL/Mapping/MappingProfile.cs
using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // DAL → BLL
        CreateMap<DAL.Models.Trip, BLL.Models.TripBM>();
        CreateMap<DAL.Models.Destination, BLL.Models.DestinationBM>();
        CreateMap<DAL.Models.Guide, BLL.Models.GuideBM>();
        // etc.

        // BLL → DAL (for creates/updates)
        CreateMap<BLL.Models.TripBM, DAL.Models.Trip>();
        // etc.
    }
}
```

Register in both WebAPI and WebApp Program.cs:
```csharp
builder.Services.AddAutoMapper(typeof(BLL.Mapping.MappingProfile));
```

### 4.5 Model Separation Rules (Ishod 4 Desired requirement)

The spec explicitly states:
> "model baze podataka ne smije se koristiti u prikazu"
> (database model must not be used in the view)

> "Ne biste trebali imati svojstva navigacije u modelima prikaza"
> (You should not have navigation properties in view models)

Ensure:
- **DAL Models**: Have navigation properties (EF relationships)
- **BLL Models**: Business objects without EF navigation props
- **WebAPI DTOs**: Flat objects for API responses
- **WebApp ViewModels**: Flat objects for views, NO navigation properties, NO Id exposed in UI

---

## Phase 5: AJAX Pagination (Ishod 5 Desired)

### 5.1 Add AJAX Endpoint for Trips

In `WebApp/Controllers/TripsController.cs` (or an API controller):
```csharp
[HttpGet]
public async Task<IActionResult> GetTripsPage(string search, int? destinationId, int page = 1, int pageSize = 10)
{
    // Filter and paginate
    // Return partial view or JSON
    return Json(new {
        trips = pagedTrips,
        currentPage = page,
        totalPages = totalPages,
        totalItems = totalCount
    });
}
```

### 5.2 JavaScript AJAX Pagination

In `wwwroot/js/tripList.js` (or inline in the view):
```javascript
async function loadTripsPage(page) {
    const search = document.getElementById('searchText').value;
    const destinationId = document.getElementById('destinationFilter').value;

    const response = await fetch(
        `/Trips/GetTripsPage?search=${search}&destinationId=${destinationId}&page=${page}&pageSize=10`
    );
    const data = await response.json();

    renderTrips(data.trips);
    renderPagination(data.currentPage, data.totalPages);
}

function renderPagination(currentPage, totalPages) {
    // Show page numbers: e.g., 5, 6, [7], 8, 9
    // Plus Previous and Next buttons
}
```

### 5.3 Pagination UI

Show numbered page links (not just Previous/Next):
```
< Previous  5  6  [7]  8  9  Next >
```

The spec says: "Najbolji rezultat bio bi prikazati nekoliko stranica prije i poslije
trenutne stranice (brojevi poput 5, 6, 7, 8) i gumbe Previous i Next."

---

## Phase 6: Final Cleanup & Verification

### 6.1 Delete Old Razor Pages Structure
- Delete entire `WebApp/Pages/` directory
- Remove `builder.Services.AddRazorPages()` from Program.cs
- Remove `app.MapRazorPages()` from Program.cs

### 6.2 Verify Project Submission Structure
```
ProjectTask/
├── Database/
│   └── Database.sql              ← Single SQL file, no CREATE DATABASE/USE
├── TravelOrganizationSystem/
│   ├── TravelOrganizationSystem.sln
│   ├── WebAPI/
│   │   └── WebAPI.csproj
│   ├── WebApp/
│   │   └── WebApp.csproj
│   ├── BLL/                      ← New
│   │   └── BLL.csproj
│   └── DAL/                      ← New
│       └── DAL.csproj
```

### 6.3 Checklist Before Submission

- [ ] No `bin/`, `obj/`, `packages/` folders in ZIP
- [ ] Connection string loaded from `appsettings.json` (not hardcoded)
- [ ] No `CREATE DATABASE`, `ALTER DATABASE`, or `USE` in Database.sql
- [ ] Database.sql has INSERT statements for sample data (destinations, guides, trips, users)
- [ ] All table names are singular (Trip not Trips)
- [ ] .NET 8.0 target framework
- [ ] ZIP format only: `Sokol-Matija-TravelOrganizationSystem.zip`
- [ ] Swagger works with JWT auth support
- [ ] No entity IDs visible in the UI
- [ ] All models have validation annotations
- [ ] No duplicate entity names allowed
- [ ] Display labels use `[Display(Name = "...")]` annotations
- [ ] All views have consistent navigation (nav bar with links to all entity lists + logout)
- [ ] Admin pages are protected with `[Authorize(Roles = "Admin")]`
- [ ] User pages are protected with `[Authorize]`
- [ ] Landing page is `[AllowAnonymous]`

### 6.4 Test Scenarios for Defense

1. **Admin Login** → Redirect to Trip list → CRUD all entities
2. **User Registration** → Login → Browse trips → Book a trip → View My Bookings
3. **Search + Filter** → Text search + destination dropdown → Pagination works
4. **AJAX Profile** → Update email, name, phone → Saves without page reload
5. **AJAX Pagination** → Click page numbers → Trips load without page reload
6. **Static HTML Logs** → Login via static page → View logs with dropdown → Logout
7. **Swagger** → Demo all API endpoints → JWT auth flow
8. **Database** → Run Database.sql → Creates all tables with data

---

## Execution Order (Recommended)

### Step 1: MVC Conversion (HIGHEST PRIORITY)
Convert WebApp from Razor Pages to MVC. This is the foundation - everything else builds on it.
Do this first because it touches every file.

### Step 2: Multi-Tier Architecture
Create BLL and DAL projects, move code. This restructures the solution but doesn't change
visible behavior.

### Step 3: AutoMapper
Add AutoMapper profiles for model mapping between tiers. Quick win after multi-tier is set up.

### Step 4: Static HTML Logs Pages
Create login.html and logs.html in WebAPI/wwwroot. Independent of other changes.

### Step 5: Admin Bookings Page
Add the admin view for user bookings/registrations. Simple CRUD page.

### Step 6: Image Upload
Add IFormFile upload to Trip Create/Edit forms. Small change.

### Step 7: AJAX Pagination
Replace server-side pagination with AJAX on trips list. JavaScript work.

### Step 8: Final Testing & Cleanup
Run through all test scenarios. Clean up. Build ZIP.

---

## File Mapping Reference (Quick Lookup)

### Razor Page → MVC Controller + View

| Razor Page | Controller | Action | View |
|------------|-----------|--------|------|
| `Pages/Index` | `HomeController` | `Index` | `Views/Home/Index.cshtml` |
| `Pages/Privacy` | `HomeController` | `Privacy` | `Views/Home/Privacy.cshtml` |
| `Pages/Error` | `HomeController` | `Error` | `Views/Shared/Error.cshtml` |
| `Pages/Account/Login` | `AccountController` | `Login` (GET+POST) | `Views/Account/Login.cshtml` |
| `Pages/Account/Register` | `AccountController` | `Register` (GET+POST) | `Views/Account/Register.cshtml` |
| `Pages/Account/Profile` | `AccountController` | `Profile` (GET) | `Views/Account/Profile.cshtml` |
| `Pages/Account/Logout` | `AccountController` | `Logout` (POST) | (redirect) |
| `Pages/Account/ChangePassword` | `AccountController` | `ChangePassword` (GET+POST) | `Views/Account/ChangePassword.cshtml` |
| `Pages/Trips/Index` | `TripsController` | `Index` | `Views/Trips/Index.cshtml` |
| `Pages/Trips/Details` | `TripsController` | `Details(id)` | `Views/Trips/Details.cshtml` |
| `Pages/Trips/Create` | `TripsController` | `Create` (GET+POST) | `Views/Trips/Create.cshtml` |
| `Pages/Trips/Edit` | `TripsController` | `Edit(id)` (GET+POST) | `Views/Trips/Edit.cshtml` |
| `Pages/Trips/Book` | `TripsController` | `Book(id)` (GET+POST) | `Views/Trips/Book.cshtml` |
| `Pages/Trips/MyBookings` | `TripsController` | `MyBookings` | `Views/Trips/MyBookings.cshtml` |
| `Pages/Destinations/Index` | `DestinationsController` | `Index` | `Views/Destinations/Index.cshtml` |
| `Pages/Destinations/Create` | `DestinationsController` | `Create` (GET+POST) | `Views/Destinations/Create.cshtml` |
| `Pages/Destinations/Edit` | `DestinationsController` | `Edit(id)` (GET+POST) | `Views/Destinations/Edit.cshtml` |
| `Pages/Destinations/Details` | `DestinationsController` | `Details(id)` | `Views/Destinations/Details.cshtml` |
| `Pages/Admin/Guides/Index` | `Admin/GuidesController` | `Index` | `Views/Admin/Guides/Index.cshtml` |
| `Pages/Admin/Guides/Create` | `Admin/GuidesController` | `Create` (GET+POST) | `Views/Admin/Guides/Create.cshtml` |
| `Pages/Admin/Guides/Edit` | `Admin/GuidesController` | `Edit(id)` (GET+POST) | `Views/Admin/Guides/Edit.cshtml` |
| `Pages/Admin/Guides/Details` | `Admin/GuidesController` | `Details(id)` | `Views/Admin/Guides/Details.cshtml` |
| `Pages/Admin/GuideAssignments/Index` | `Admin/GuideAssignmentsController` | `Index` | `Views/Admin/GuideAssignments/Index.cshtml` |
| `Pages/Admin/Logs/Index` | `Admin/LogsController` | `Index` | `Views/Admin/Logs/Index.cshtml` |
| NEW | `Admin/BookingsController` | `Index` | `Views/Admin/Bookings/Index.cshtml` |

### Existing API Controllers (Keep As-Is, may need rename)

| File | Route | Purpose |
|------|-------|---------|
| `Controllers/ProfileController.cs` | `api/profile` | AJAX profile updates |
| `Controllers/TripsController.cs` | check route | May conflict - rename to `TripsApiController` |
| `Controllers/UnsplashController.cs` | check route | Unsplash image proxy |

---

## Notes for Claude (Fresh Context)

When starting a new context window with this plan:
1. Read this file first: `docs/REFACTORING-PLAN.md`
2. Read the project spec: `ProjectTask/RWA-Projekt-2025-hr.pdf`
3. The project is at: `ProjectTask/TravelOrganizationSystem/`
4. Current state: Razor Pages WebApp, needs conversion to MVC
5. WebAPI project is mostly complete and correct
6. Database.sql is complete with sample data
7. Services layer already exists in both projects (WebApp calls WebAPI via HTTP)
8. Authentication: WebApp uses Cookies, WebAPI uses JWT
9. Theme: Travel Organization System (Trips, Destinations, Guides)
10. Primary entity: Trip; 1-to-N: Destination; M-to-N: Guide (via TripGuide bridge)
11. User action: TripRegistration (booking a trip)
