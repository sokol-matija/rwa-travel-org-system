# Travel Organization System

A .NET 8.0 web application for managing travel destinations, trips, and bookings.

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or Full SQL Server)
- Visual Studio 2022 or VS Code with C# extension

## Initial Setup

### 1. Configure Application Settings

Copy the example configuration file and update with your settings:

```bash
cd TravelOrganizationSystem/WebAPI
cp appsettings.example.json appsettings.json
```

**IMPORTANT**: Update the following in `appsettings.json`:
- `JwtSettings.Secret`: Replace with a secure random string (at least 32 characters)
- `ConnectionStrings.DefaultConnection`: Update server name if not using LocalDB

### 2. Database Setup

Run the database creation script:

```bash
# Using SQL Server Management Studio (SSMS)
# Open and execute: Database/Database.sql

# Or using sqlcmd:
sqlcmd -S . -i Database/Database.sql
```

### 3. Build and Run

```bash
# Build the solution
dotnet build TravelOrganizationSystem/TravelOrganizationSystem.sln

# Run the WebAPI (default port: 5000/5001)
cd TravelOrganizationSystem/WebAPI
dotnet run

# Run the WebApp (default port: 5002/5003)
cd TravelOrganizationSystem/WebApp
dotnet run
```

## Project Structure

```
TravelOrganizationSystem/
├── WebAPI/              # REST API backend
│   ├── Controllers/     # API endpoints
│   ├── Models/          # Data models
│   ├── Services/        # Business logic
│   ├── DTOs/           # Data transfer objects
│   └── Data/           # Database context
├── WebApp/             # ASP.NET Core Razor Pages frontend
│   ├── Pages/          # Razor pages
│   ├── Services/       # API client services
│   └── wwwroot/        # Static assets
└── Database/           # SQL database schema
```

## Security Notes

- **Never commit** `appsettings.json` or `appsettings.Development.json` to version control
- Always use strong, randomly generated JWT secrets in production
- Keep database credentials secure and use environment variables in production

## API Documentation

Once running, access the Swagger UI at:
- https://localhost:5001/swagger

## Default Credentials

Check the database seeding script for initial user credentials.
