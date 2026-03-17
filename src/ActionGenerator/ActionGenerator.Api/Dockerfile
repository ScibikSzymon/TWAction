# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution and project files
COPY ["TWAction.sln", "./"]
COPY ["src/ActionGenerator/ActionGenerator.Api/ActionGenerator.Api.csproj", "src/ActionGenerator/ActionGenerator.Api/"]
COPY ["src/ActionGenerator/ActionGenerator.Application/ActionGenerator.Application.csproj", "src/ActionGenerator/ActionGenerator.Application/"]
COPY ["src/ActionGenerator/ActionGenerator.Domain/ActionGenerator.Domain.csproj", "src/ActionGenerator/ActionGenerator.Domain/"]
COPY ["src/ActionGenerator/ActionGenerator.Infrastructure/ActionGenerator.Infrastructure.csproj", "src/ActionGenerator/ActionGenerator.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/ActionGenerator/ActionGenerator.Api/ActionGenerator.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/ActionGenerator/ActionGenerator.Api"
RUN dotnet build "ActionGenerator.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ActionGenerator.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080

# Start the application
ENTRYPOINT ["dotnet", "ActionGenerator.Api.dll"]
