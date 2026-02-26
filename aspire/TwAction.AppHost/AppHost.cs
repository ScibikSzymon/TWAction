var builder = DistributedApplication.CreateBuilder(args);

// Po³¹czenie z istniej¹c¹ baz¹ z Docker Compose (connection string z appsettings)
var appDb = builder.AddConnectionString("TWActionDatabase");

// Backend Api (.NET project)
var Api = builder.AddProject<Projects.TWAction_Api>("Api")
    .WithReference(appDb);

// ActionGenerator Api (.NET project)
var actionGeneratorApi = builder.AddProject<Projects.ActionGenerator_Api>("action-generator-Api")
    .WithReference(Api);

// React frontend (npm)
// Path is relative to AppHost project folder. Adjust as needed.
builder.AddNpmApp("web", "../../src/frontend/TWActionFrontend", "dev")
    .WithHttpEndpoint(port: 3000, env: "PORT")       // Aspire will set PORT
    .WithExternalHttpEndpoints()         // makes it reachable from host
    .WithReference(Api);                 // gives the frontend info about the Api

builder.Build().Run();

