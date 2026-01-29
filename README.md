# TWAction

Quick local setup to integrate the Aspire host with the React frontend and .NET backend (PostgreSQL):

Prerequisites:
- Docker (for PostgreSQL) or a local Postgres server
- .NET 10 SDK
- Node 18+ / pnpm/npm

Start PostgreSQL with Docker:

```bash
docker compose -f docker-compose.postgres.yml up -d
```

Run the backend (from repo root):

```bash
cd src/backend/TWAction.Api
dotnet run
```

Run the frontend (from repo root):

```bash
cd src/frontend
npm install
npm run dev
```

Notes:
- The backend expects a connection string named `TWActionDatabase` (configured in `src/backend/TWAction.Api/appsettings.Development.json`).
- Frontend API base URL is set in `src/frontend/src/api.ts` (defaults to `http://localhost:5111`).
