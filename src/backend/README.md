# TWAction Backend API

.NET 10 Web API built with Clean Architecture principles, using Minimal APIs, Wolverine for CQRS, Entity Framework Core, and PostgreSQL.

## Architecture

The backend follows Clean Architecture with clear separation of concerns:

- **TWAction.Domain** - Core business entities and domain logic
- **TWAction.Application** - Application business rules, DTOs, commands, queries, and handlers
- **TWAction.Infrastructure** - External services, message bus configuration (Wolverine)
- **TWAction.Persistence** - Database context, migrations, and repositories (EF Core + PostgreSQL)
- **TWAction.Api** - HTTP endpoints, configuration, and presentation layer

### Key Technologies

- **.NET 10** - Latest .NET framework
- **Minimal APIs** - Lightweight endpoint routing
- **Wolverine** - CQRS/MediatR-style message bus for commands and queries
- **Entity Framework Core** - ORM with PostgreSQL provider
- **Swagger/OpenAPI** - API documentation
- **Google OAuth 2.0** - Third-party authentication

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL (Docker recommended)
- Google OAuth credentials (for authentication)

### Database Setup

Start PostgreSQL using Docker:

```bash
# From repository root
docker compose -f docker-compose.postgres.yml up -d
```

This creates a PostgreSQL container with:
- Host: `localhost`
- Port: `5432`
- Database: `twaction`
- Username: `postgres`
- Password: `postgres`

### Configuration

1. **Database Connection**

Update [appsettings.Development.json](TWAction.Api/appsettings.Development.json):

```json
{
  "ConnectionStrings": {
    "TWActionDatabase": "Host=localhost;Port=5432;Database=twaction;Username=postgres;Password=postgres"
  }
}
```

2. **Google OAuth**

Configure Google OAuth credentials:

```json
{
  "Google": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret",
    "RedirectUri": "http://localhost:8000/auth/google/callback"
  }
}
```

To obtain Google OAuth credentials:
- Go to [Google Cloud Console](https://console.cloud.google.com/)
- Create a new project or select existing
- Enable Google+ API
- Create OAuth 2.0 credentials
- Add authorized redirect URI: `http://localhost:8000/auth/google/callback`

3. **Authentication Settings**

Configure cookie-based session management:

```json
{
  "Auth": {
    "CookieName": "TWAction.Session",
    "SessionExpiryHours": 8,
    "CookieSecure": false,
    "CookieSameSite": "Lax",
    "CookieDomain": ""
  }
}
```

**Note**: Set `CookieSecure: true` in production with HTTPS.

4. **CORS Configuration**

Configure allowed origins for cross-origin requests:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000"
    ]
  }
}
```

### Running the API

From the TWAction.Api directory:

```bash
cd src/backend/TWAction.Api
dotnet run
```

The API will start on `http://localhost:8000` (or port specified in launchSettings.json).

### Migrations

Entity Framework Core migrations are automatically applied on startup in non-production environments.

To create a new migration:

```bash
cd src/backend/TWAction.Persistence
dotnet ef migrations add MigrationName --startup-project ../TWAction.Api
```

To apply migrations manually:

```bash
dotnet ef database update --startup-project ../TWAction.Api
```

## API Endpoints

### Authentication

#### **GET /auth/google**
Initiates Google OAuth flow. Redirects user to Google login.

**Response**: `302 Redirect`

---

#### **GET /auth/google/callback**
OAuth callback endpoint. Handles Google's response after user authorization.

**Query Parameters**:
- `code` - Authorization code from Google
- `state` - State parameter for CSRF protection

**Response**: `302 Redirect` to frontend with session cookie

---

#### **GET /auth/me**
Get currently authenticated user information.

**Headers**:
- `Cookie: TWAction.Session=<session-id>`

**Response**: `200 OK`
```json
{
  "id": "uuid",
  "email": "user@example.com",
  "displayName": "John Doe",
  "provider": "google",
  "createdAt": "2026-01-12T10:30:00Z"
}
```

**Error Responses**:
- `401 Unauthorized` - No valid session

---

#### **POST /auth/logout**
Logs out the current user and invalidates session.

**Headers**:
- `Cookie: TWAction.Session=<session-id>`

**Response**: `204 No Content`

**Error Responses**:
- `401 Unauthorized` - No valid session

---

### Users

#### **GET /users**
Get all users (development/admin endpoint).

**Response**: `200 OK`
```json
[
  {
    "id": "uuid",
    "email": "user@example.com",
    "displayName": "John Doe",
    "provider": "google",
    "createdAt": "2026-01-12T10:30:00Z"
  }
]
```

---

## Swagger Documentation

When running in Development mode, Swagger UI is available at:

```
http://localhost:8000/swagger
```

## Project Structure

```
src/backend/
├── TWAction.Api/              # HTTP API layer
│   ├── Endpoints/            # Minimal API endpoint definitions
│   ├── Options/              # Configuration option classes
│   ├── Program.cs            # Application entry point
│   └── appsettings.json      # Configuration
│
├── TWAction.Application/      # Application logic layer
│   ├── Commands/             # Write operations (CQRS commands)
│   ├── Queries/              # Read operations (CQRS queries)
│   ├── Handlers/             # Command/Query handlers (Wolverine)
│   ├── DTOs/                 # Data transfer objects
│   ├── Mappers/              # Entity-to-DTO mapping
│   └── Interfaces/           # Application contracts
│
├── TWAction.Domain/           # Core domain layer
│   └── Entities/             # Domain entities
│
├── TWAction.Infrastructure/   # Infrastructure services
│   └── DependencyInjection.cs
│
└── TWAction.Persistence/      # Data access layer
    ├── Configurations/       # EF Core entity configurations
    ├── Migrations/           # EF Core migrations
    ├── Repositories/         # Repository implementations
    └── TWActionDbContext.cs  # EF Core DbContext
```

## Development

### Adding a New Endpoint

1. **Create Command/Query** in `TWAction.Application/Commands` or `Queries/`
2. **Create Handler** in `TWAction.Application/Handlers/`
3. **Map Endpoint** in `TWAction.Api/Endpoints/`
4. **Register** in `Program.cs`

Example:

```csharp
// 1. Create Query
public record GetUserByIdQuery(Guid Id);

// 2. Create Handler
public class GetUserByIdHandler
{
    private readonly IRepository<User> _repository;
    
    public async Task<UserDto?> Handle(GetUserByIdQuery query)
    {
        var user = await _repository.GetByIdAsync(query.Id);
        return user?.ToDto();
    }
}

// 3. Map Endpoint
app.MapGet("/users/{id}", async (Guid id, IMessageBus bus) =>
{
    var user = await bus.InvokeAsync<UserDto?>(new GetUserByIdQuery(id));
    return user is not null ? Results.Ok(user) : Results.NotFound();
});
```

### Testing

Run unit tests:

```bash
cd tests/TWAction.UnitTests
dotnet test
```

Run integration tests:

```bash
cd tests/TWAction.IntegrationTests
dotnet test
```

## Security Considerations

### Production Checklist

- [ ] Set `CookieSecure: true` in Auth configuration
- [ ] Use `CookieSameSite: "Strict"` or `"Lax"`
- [ ] Configure proper `CookieDomain` for your domain
- [ ] Update CORS `AllowedOrigins` to production frontend URL
- [ ] Store Google OAuth credentials in User Secrets or Key Vault
- [ ] Use environment variables for sensitive configuration
- [ ] Enable HTTPS redirection
- [ ] Review and secure `/users` endpoint (add authorization)

### User Secrets

For local development, use .NET User Secrets instead of committing credentials:

```bash
cd src/backend/TWAction.Api
dotnet user-secrets init
dotnet user-secrets set "Google:ClientId" "your-client-id"
dotnet user-secrets set "Google:ClientSecret" "your-client-secret"
```

## Common Issues

### Database Connection Fails

- Verify PostgreSQL is running: `docker ps`
- Check connection string in appsettings.Development.json
- Ensure database exists: `psql -h localhost -U postgres -d twaction`

### Google OAuth Errors

- Verify redirect URI matches Google Cloud Console configuration
- Check ClientId and ClientSecret are correct
- Ensure Google+ API is enabled in Google Cloud Console

### CORS Errors

- Verify frontend URL is in `Cors:AllowedOrigins`
- Check that frontend uses credentials: `withCredentials: true`
- Ensure CORS middleware is before endpoint mapping in Program.cs

## Contributing

Follow Clean Architecture principles:
- Keep domain entities independent of external frameworks
- Use dependency injection for all cross-layer dependencies
- Commands/Queries should be immutable records
- Handlers should have single responsibility

## License

See the LICENSE file in the repository root.
