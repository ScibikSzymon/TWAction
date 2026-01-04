# TWAction Frontend

This is a Vite + React + TypeScript frontend using Tailwind CSS and Axios to call the TWAction .NET backend.

Quick start:

1. Start the backend (from repo root):

```powershell
dotnet run --project src/backend/TWAction.Api/TWAction.Api.csproj
```

By default the backend listens on `http://localhost:5111` and `https://localhost:7169`.

2. In another terminal, install and run the frontend:

```bash
cd src/frontend
npm install
npm run dev
```

3. Open `http://localhost:5173` in your browser. The app will call `http://localhost:5111/` and display the response.

Notes:

- This setup uses CORS on the backend; the API allows requests from the frontend dev server.
- If you want to use HTTPS for the backend dev server, you may need to trust the .NET dev certificate.
