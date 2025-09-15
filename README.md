# BingoGame Project

## Overview

BingoGame is a full-stack bingo game application supporting real-time multiplayer gameplay, user authentication, room creation/joining, and bingo card management. Key features include:
- Real-time communication via SignalR for game updates, chat, and number draws.
- User authentication with JWT (register, login, guest access).
- Room management: create, join, start/pause/end games.
- Bilingual UI (English default, Spanish support via Angular i18n).
- Bingo types: Traditional 75-ball bingo.
- Testing: Unit tests for backend, E2E tests with Cypress for frontend.

**Tech Stack:**
- **Backend**: .NET 8 Web API, Entity Framework Core with SQL Server, SignalR for real-time, JWT for auth.
- **Frontend**: Angular 17, Angular Material, Tailwind CSS, Angular i18n for internationalization, SignalR client.

## Project Structure

- **Backend/**: Contains the BingoGameApi project with models, controllers, services, DTOs, and migrations.
  - BingoGameApi/: API source code, including Program.cs, appsettings.json, and EF DbContext.
- **Frontend/**: Angular application.
  - BingoGameFrontend/: Components (dashboard, room-create, room-join, game-room), services (auth, api, signalr), and i18n files.
- **Tests**: Backend unit tests in Backend/BingoGameApi.Tests, frontend E2E in Frontend/BingoGameFrontend/cypress.

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server (LocalDB for development) or Azure SQL for production
- Angular CLI (for frontend development)

## Running the Project

### Database Setup
1. Ensure SQL Server LocalDB is installed and running.
2. In `Backend/BingoGameApi/appsettings.json` or `appsettings.Development.json`, configure the connection string:
   ```
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BingoDb;Trusted_Connection=true;TrustServerCertificate=true;"
   }
   ```
3. Run migrations: Navigate to `Backend/BingoGameApi` and execute `dotnet ef database update`.

### Backend
1. Navigate to `BingoGame/Backend/BingoGameApi`.
2. Restore packages: `dotnet restore`.
3. Run: `dotnet run`.
   - API available at `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP). Check console for exact ports.
   - Swagger docs at `https://localhost:5001/swagger`.
   - SignalR hub at `/gamehub`.

### Frontend
1. Navigate to `BingoGame/Frontend/BingoGameFrontend`.
2. Install dependencies: `npm install`.
3. Run: `ng serve`.
   - App available at `http://localhost:4200`.
   - Ensure proxy to backend in `angular.json` or environment config for API calls.

### Environment Variables
- JWT settings in `appsettings.json`: `Jwt:Key` (secret key), `Jwt:Issuer`, `Jwt:Audience`.
- CORS origins configured in `Program.cs` (allow frontend origin).

## Testing

### Backend
- Run unit tests: Navigate to `Backend/BingoGameApi.Tests` and execute `dotnet test`.

### Frontend
- Run E2E tests: In `Frontend/BingoGameFrontend`, execute `npx cypress run` (headless) or `npx cypress open` (GUI).

## Bilingual Support
- Frontend uses Angular i18n for English (default) and Spanish.
- Translation files: `src/locale/messages.es.xlf`.
- Build for specific locale: `ng build --locales=es` or serve with i18n enabled.
- UI switches languages based on user preference or browser settings.

For detailed setup, API endpoints, and deployment, see the [docs](./docs/) folder.