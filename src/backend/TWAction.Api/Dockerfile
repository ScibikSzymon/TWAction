# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /src

# Copy solution and project files
COPY ["TWAction.sln", "./"]
COPY ["src/backend/TWAction.Api/TWAction.Api.csproj", "src/backend/TWAction.Api/"]
COPY ["src/backend/TWAction.Application/TWAction.Application.csproj", "src/backend/TWAction.Application/"]
COPY ["src/backend/TWAction.Domain/TWAction.Domain.csproj", "src/backend/TWAction.Domain/"]
COPY ["src/backend/TWAction.Infrastructure/TWAction.Infrastructure.csproj", "src/backend/TWAction.Infrastructure/"]
COPY ["src/backend/TWAction.Persistence/TWAction.Persistence.csproj", "src/backend/TWAction.Persistence/"]
COPY ["aspire/TwAction.ServiceDefaults/TwAction.ServiceDefaults.csproj", "aspire/TwAction.ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "src/backend/TWAction.Api/TWAction.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/backend/TWAction.Api"
RUN dotnet build "TWAction.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TWAction.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final

WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080

# Start the application
ENTRYPOINT ["dotnet", "TWAction.Api.dll"]
