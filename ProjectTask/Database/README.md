# Travel Organization System - Database Setup

SQL Server 2022 Docker setup for both Mac (local development) and Windows (shared remote access via Tailscale).

## Quick Start (Mac)

```bash
cd ProjectTask/Database
docker-compose up -d
```

The database will be available at `localhost:1433` with:
- **Database:** `TravelOrganizationDB`
- **Username:** `sa`
- **Password:** `Marvel247@$&`

## Container Details

**Container Name:** `sql-server`
**Image:** `mcr.microsoft.com/mssql/server:2022-latest`
**Port:** `1433`

## Database Schema

The initialization script creates:

### Tables
- **Destination** - Travel destinations (6 seeded)
- **Guide** - Tour guides (5 seeded)
- **Trip** - Available trips (16 seeded)
- **User** - System users (3 seeded)
- **TripGuide** - Many-to-many relationship
- **TripRegistration** - User trip bookings
- **Log** - Application logging

### Test Users (Password: 123456)
- `admin@travel.com` - Admin user
- `user1@travel.com` - Regular user (John Doe)
- `user2@travel.com` - Regular user (Jane Smith)

## DBeaver Connection Setup

### Mac Local Connection

1. Open DBeaver
2. Create New Connection → SQL Server
3. Connection Settings:
   - **Host:** `localhost`
   - **Port:** `1433`
   - **Database:** `TravelOrganizationDB` (or `master` to see all databases)
   - **Authentication:** SQL Server Authentication
   - **Username:** `sa`
   - **Password:** `Marvel247@$&`
4. Test Connection → OK → Finish

### Windows Remote Connection (via Tailscale)

1. Ensure Tailscale is running on both Mac and Windows
2. Verify Windows PC hostname: `sokol.falcon-parore.ts.net`
3. DBeaver Connection Settings:
   - **Host:** `sokol.falcon-parore.ts.net`
   - **Port:** `1433`
   - **Database:** `TravelOrganizationDB`
   - **Username:** `sa`
   - **Password:** `Marvel247@$&`
4. Test Connection

## Docker Commands

### Start Container
```bash
docker-compose up -d
```

### Stop Container
```bash
docker-compose down
```

### View Logs
```bash
docker-compose logs -f sql-server
```

### Restart Container
```bash
docker-compose restart
```

### Check Container Status
```bash
docker ps | grep sql-server
```

### Access SQL Server CLI
```bash
docker exec -it sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Marvel247@$&'
```

## Troubleshooting

### Container won't start
Check logs:
```bash
docker-compose logs sql-server
```

Common issues:
- Password doesn't meet SQL Server complexity requirements
- Port 1433 already in use
- Insufficient memory allocated to Docker

### Can't connect from DBeaver
1. Verify container is running: `docker ps`
2. Check health: `docker inspect sql-server | grep Health`
3. Test connection from terminal:
   ```bash
   docker exec -it sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Marvel247@$&' -Q "SELECT @@VERSION"
   ```

### Tailscale connection issues
1. Verify Tailscale is running: `tailscale status`
2. Ping Windows PC: `ping sokol.falcon-parore.ts.net`
3. Test port connectivity: `nc -zv sokol.falcon-parore.ts.net 1433`
4. Ensure Windows firewall allows port 1433

### Database not initialized
If tables are missing, run initialization manually:
```bash
docker exec -it sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Marvel247@$&' -i /docker-entrypoint-initdb.d/01-travel-org.sql
```

## Windows PC Setup

### Prerequisites
- Docker Desktop for Windows installed
- Tailscale installed and logged in
- Port 1433 not in use

### Installation Steps

1. Extract `windows-deployment.zip` to a folder (e.g., `C:\TravelOrgDB\`)

2. Open PowerShell/CMD in that folder

3. Start the container:
   ```powershell
   docker-compose up -d
   ```

4. Verify it's running:
   ```powershell
   docker ps
   ```

5. Test connection from Windows:
   ```powershell
   docker exec -it sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Marvel247@$&" -Q "SELECT @@VERSION"
   ```

6. Configure Windows Firewall (if needed):
   ```powershell
   New-NetFirewallRule -DisplayName "SQL Server Docker" -Direction Inbound -LocalPort 1433 -Protocol TCP -Action Allow
   ```

### Verify Tailscale Access

From your Mac, test the connection:
```bash
# Ping test
ping sokol.falcon-parore.ts.net

# Port test
nc -zv sokol.falcon-parore.ts.net 1433

# SQL test (requires sqlcmd installed on Mac)
sqlcmd -S sokol.falcon-parore.ts.net,1433 -U sa -P 'Marvel247@$&' -Q "SELECT @@VERSION"
```

## Adding Additional Databases

To add more databases to the same container:

### Option 1: Via DBeaver
1. Connect to `master` database
2. Execute: `CREATE DATABASE YourNewDB;`

### Option 2: Via Init Script
1. Create `init-scripts/02-your-project.sql`:
   ```sql
   USE master;
   GO

   IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'YourNewDB')
   BEGIN
       CREATE DATABASE YourNewDB;
   END
   GO

   USE YourNewDB;
   GO

   -- Your tables here
   ```

2. Rebuild container:
   ```bash
   docker-compose down -v
   docker-compose up -d
   ```

## Data Persistence

Database files are stored in `./data/` directory (gitignored).

To backup your data:
```bash
# Backup
docker exec sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Marvel247@$&' -Q "BACKUP DATABASE TravelOrganizationDB TO DISK='/var/opt/mssql/data/TravelOrgBackup.bak'"

# Copy backup to host
docker cp sql-server:/var/opt/mssql/data/TravelOrgBackup.bak ./TravelOrgBackup.bak
```

To restore:
```bash
# Copy backup to container
docker cp ./TravelOrgBackup.bak sql-server:/var/opt/mssql/data/

# Restore
docker exec sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Marvel247@$&' -Q "RESTORE DATABASE TravelOrganizationDB FROM DISK='/var/opt/mssql/data/TravelOrgBackup.bak' WITH REPLACE"
```

## Security Notes

- `.env` file contains password and is gitignored
- Never commit the `.env` file to version control
- Same password used on both Mac and Windows for development convenience
- For production, use different credentials and secure key management
