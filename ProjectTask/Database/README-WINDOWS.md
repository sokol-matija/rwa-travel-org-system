# SQL Server Docker Setup for Windows PC

This package contains everything needed to run the Travel Organization System database on your Windows PC.

## Prerequisites

- Docker Desktop for Windows (installed and running)
- Tailscale installed and logged in
- Port 1433 available

## Quick Setup

### 1. Extract Files

Extract all files from the zip to a folder, e.g., `C:\TravelOrgDB\`

### 2. Start Docker Desktop

Make sure Docker Desktop is running (check system tray icon).

### 3. Open PowerShell

Navigate to the extracted folder:
```powershell
cd C:\TravelOrgDB
```

### 4. Start the Container

```powershell
docker-compose up -d
```

You should see:
```
Creating network "travelorgdb_default" with the default driver
Creating sql-server ... done
```

### 5. Wait for SQL Server to Start (30-60 seconds)

Check if it's ready:
```powershell
docker ps
```

Should show `sql-server` with status `Up` and `(healthy)`.

### 6. Initialize the Database

Run the initialization script:
```powershell
docker exec -i sql-server /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Marvel247@$&" -C -i /docker-entrypoint-initdb.d/01-travel-org.sql
```

You should see:
```
TravelOrganizationDB created and initialized successfully!
```

### 7. Configure Windows Firewall

Allow Docker to accept connections from Tailscale network:

```powershell
New-NetFirewallRule -DisplayName "SQL Server Docker" -Direction Inbound -LocalPort 1433 -Protocol TCP -Action Allow
```

## Verify Setup

### Test Local Connection (on Windows)

```powershell
docker exec -it sql-server /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Marvel247@$&" -C -Q "SELECT @@VERSION"
```

### Test Tailscale Connection (from Mac)

On your Mac, verify you can reach the Windows PC:

```bash
ping sokol.falcon-parore.ts.net
nc -zv sokol.falcon-parore.ts.net 1433
```

## Connection Details

- **Container Name:** `sql-server`
- **Host (from Windows):** `localhost`
- **Host (from Mac via Tailscale):** `sokol.falcon-parore.ts.net`
- **Port:** `1433`
- **Database:** `TravelOrganizationDB`
- **Username:** `sa`
- **Password:** `Marvel247@$&`

## DBeaver Setup (Windows)

1. Install DBeaver (if not already installed)
2. Create New Connection → SQL Server
3. Settings:
   - Host: `localhost`
   - Port: `1433`
   - Database: `TravelOrganizationDB`
   - Authentication: SQL Server Authentication
   - Username: `sa`
   - Password: `Marvel247@$&`
4. Test Connection → Finish

## Stopping the Container

```powershell
docker-compose down
```

## Restarting the Container

```powershell
docker-compose restart
```

## Troubleshooting

### Port 1433 Already in Use

Check if SQL Server is already running on Windows:
```powershell
Get-NetTCPConnection -LocalPort 1433
```

If another SQL Server is using port 1433, either:
- Stop the other SQL Server service
- Or change the port in `docker-compose.yml` (both host and container sides)

### Container Won't Start

Check logs:
```powershell
docker-compose logs sql-server
```

### Can't Connect from Mac

1. Verify Tailscale is running on Windows: `tailscale status`
2. Check Windows hostname matches: Should be `sokol.falcon-parore.ts.net`
3. Verify firewall rule was added successfully
4. Try disabling Windows Firewall temporarily to test

### Database Not Initialized

If you see errors about tables not existing, run the initialization script again:
```powershell
docker exec -i sql-server /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Marvel247@$&" -C -i /docker-entrypoint-initdb.d/01-travel-org.sql
```

## File Structure

```
.
├── docker-compose.yml       # Container configuration
├── .env                     # SA password (DO NOT share!)
├── init-scripts/
│   └── 01-travel-org.sql   # Database initialization
└── README-WINDOWS.md        # This file
```

## Data Persistence

Database files are stored in `./data/` folder. Even if you stop/restart the container, your data persists.

To completely reset the database:
```powershell
docker-compose down -v
docker-compose up -d
# Then run initialization script again
```

## Security Note

The `.env` file contains the database password. Keep it secure and do not commit it to version control.
