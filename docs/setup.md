# Setup Guide

## Install Prerequisites

1. **.NET 8 SDK**: Download and install from [dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0). Verify with `dotnet --version`.

2. **Node.js 18+**: Download from [nodejs.org](https://nodejs.org). Verify with `node --version` and `npm --version`.

3. **SQL Server LocalDB**: Included with SQL Server Express or Visual Studio. Verify with `sqllocaldb info`. If not installed, download SQL Server Express from [microsoft.com/sql-server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads).

4. **Angular CLI** (optional for development): Run `npm install -g @angular/cli`.

## Restore Packages

### Backend
- Navigate to `BingoGame/Backend/BingoGameApi`.
- Run `dotnet restore`.

### Frontend
- Navigate to `BingoGame/Frontend/BingoGameFrontend`.
- Run `npm install`.

## Database Setup

1. Configure connection string in `Backend/BingoGameApi/appsettings.json` or `appsettings.Development.json`:
   ```
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BingoDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;"
   }
   ```

2. Add EF tools if not present: `dotnet tool install --global dotnet-ef`.

3. Run migrations:
   - Navigate to `Backend/BingoGameApi`.
   - Run `dotnet ef migrations add InitialCreate` (if needed).
   - Run `dotnet ef database update`.

## Run the Application

### Backend
- Navigate to `BingoGame/Backend/BingoGameApi`.
- Run `dotnet run`.
- Backend runs on `http://localhost:5000` (HTTP) or `https://localhost:5001` (HTTPS). Check console for exact ports.
- Swagger UI: `https://localhost:5001/swagger`.
- SignalR Hub: `/gamehub`.

### Frontend
- Navigate to `BingoGame/Frontend/BingoGameFrontend`.
- Run `ng serve`.
- Frontend runs on `http://localhost:4200`.
- Ensure the frontend proxies API calls to backend (configure in `proxy.conf.json` if needed).

## Environment Variables

- **JWT Configuration** (in `appsettings.json`):
  ```
  "Jwt": {
    "Key": "YourSuperSecretKeyWithAtLeast32CharactersLongForSecurity",
    "Issuer": "BingoGameApi",
    "Audience": "BingoGameClient"
  }
  ```
  Replace `Key` with a secure random string (at least 32 chars).

- **CORS**: Configured in `Program.cs` to allow any origin for development. For production, restrict to frontend URL (e.g., `http://localhost:4200`).

- **Database**: Use Azure SQL for production; update connection string accordingly.

## Troubleshooting

- **Port conflicts**: Edit `Properties/launchSettings.json` in backend.
- **DB connection issues**: Ensure LocalDB is running (`sqllocaldb start mssqllocaldb`).
- **CORS errors**: Verify origins in backend CORS policy.
- **SignalR**: Ensure WebSockets are enabled in browser/network.

For API details, see [api.md](./api.md). For deployment, see [deployment.md](./deployment.md).