# Deployment Guide

This guide covers deploying the BingoGame application to production environments. For local setup, refer to [setup.md](./setup.md).

## Local Deployment

Local development is covered in the [setup guide](./setup.md). Use LocalDB for database, run backend with `dotnet run`, and frontend with `ng serve`. Swagger at `/swagger` for API testing.

## Production Deployment

### Backend (.NET API)

Deploy the backend to a cloud platform supporting .NET 8, such as Azure App Service.

1. **Prepare for Deployment**:
   - Run `dotnet publish -c Release -o ./publish` in `Backend/BingoGameApi` to build the release package.
   - Update `appsettings.Production.json` with production config (remove development secrets).

2. **Database**:
   - Migrate to Azure SQL Database: Create an Azure SQL server/instance.
   - Update connection string in appsettings: `"DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=BingoDb;Persist Security Info=False;User ID=youruser;Password=yourpass;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"`.
   - Run migrations on Azure: Use Azure Data Studio or `dotnet ef database update` with updated connection.

3. **Azure App Service**:
   - Create an App Service in Azure Portal (Windows/Linux, .NET 8 runtime).
   - Deploy via ZIP (upload publish folder) or Git/DevOps pipeline.
   - Configure Application Settings in App Service: Add `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` from Key Vault for secrets.
   - Enable HTTPS: Set to required.
   - Set WEBSITES_PORT to 80/443 if needed.
   - CORS: Update policy to allow production frontend URL (e.g., `https://yourapp.azurestaticapps.net`).

4. **Scalability**:
   - SignalR Backplane: For multiple instances, add Redis backplane. Install `Microsoft.AspNetCore.SignalR.Redis` package, configure in `Program.cs`:
     ```
     builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = "your-redis-connection"; });
     builder.Services.AddSignalR().AddStackExchangeRedis("your-redis-connection");
     ```
   - DB Connection Pooling: EF Core handles pooling by default; set `Max Pool Size` in connection string.
   - Auto-scaling: Configure App Service scale-out based on CPU/memory.

**Notes (Notas en Español)**: Para producción, use Azure Key Vault para secretos JWT. Asegúrese de que HTTPS esté habilitado para seguridad. / For production, use Azure Key Vault for JWT secrets. Ensure HTTPS is enabled for security.

### Frontend (Angular)

Deploy as static files to a hosting service like Azure Static Web Apps, Vercel, or Netlify.

1. **Build for Production**:
   - Navigate to `Frontend/BingoGameFrontend`.
   - Run `ng build --configuration production --base-href /` (adjust base-href for subpath).
   - Output in `dist/BingoGameFrontend/` (static files: index.html, JS, CSS).

2. **Azure Static Web App**:
   - Create Static Web App in Azure Portal, link to GitHub repo.
   - Configure build: App location `/Frontend/BingoGameFrontend`, Output location `dist/BingoGameFrontend`.
   - API integration: Link to backend App Service for serverless API routes if needed.
   - Custom domain/HTTPS: Enabled by default.
   - Environment vars: Set API base URL (e.g., `https://your-backend.azurewebsites.net/api`).

3. **Vercel Alternative**:
   - Install Vercel CLI: `npm i -g vercel`.
   - Run `vercel --prod` from frontend root.
   - Configure `vercel.json` for rewrites: Proxy API calls to backend (e.g., `{ "rewrites": [{ "source": "/api/(.*)", "destination": "https://your-backend.azurewebsites.net/api/$1" }] }`).
   - i18n: Build with locales: `ng build --configuration production --locales=en,es`.

4. **Bilingual Support**:
   - Serve locale-specific builds or use runtime switching with i18n polyfill.
   - **Notas en Español**: El frontend soporta UI bilingüe (inglés/español). Configure rutas para locales en producción. / The frontend supports bilingual UI (English/Spanish). Configure routes for locales in production.

**Proxy Configuration**: Update Angular environment.ts or proxy.conf.json to point to production backend URL. For static hosts, use service worker or server rewrites for API proxying.

### Full Stack Deployment

- **Integrated**: Use Azure Static Web Apps with API (backend as functions, but for full API, separate App Service).
- **Monitoring**: Add Application Insights in Azure for backend logs/metrics.
- **CI/CD**: GitHub Actions or Azure DevOps: Build/test/deploy on push.
- **Security**: Use Azure AD for auth enhancement, WAF for protection.
- **Costs**: App Service Basic tier (~$50/month), Static Web App free tier for small apps, Azure SQL Basic (~$5/month).

## Verification

- Backend: Deployed app responds at `/health` (add if needed), Swagger accessible.
- Frontend: Build artifacts served, API calls succeed (check network tab), real-time SignalR connects.
- End-to-End: Create room, join, start game, draw numbers, chat works.

For setup and API details, see [setup.md](./setup.md) and [api.md](./api.md).