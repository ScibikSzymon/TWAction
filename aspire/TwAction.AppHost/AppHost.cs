var builder = DistributedApplication.CreateBuilder(args);

var appDb = builder.AddConnectionString("TWActionDatabase");

// Shared secret: single source of truth so both services always agree on the key.
var generatorApiKey = builder.AddParameter("generator-api-key", secret: true);

// ActionGenerator Api (.NET project)
var actionGeneratorApi = builder.AddProject<Projects.ActionGenerator_Api>("action-generator-Api")
    .WithHttpEndpoint(port: 5277)
    .WithEnvironment("Authentication__ApiKey", generatorApiKey);

// Backend Api (.NET project)
var Api = builder.AddProject<Projects.TWAction_Api>("Api")
    .WithReference(appDb)
    .WithReference(actionGeneratorApi)  // TWAction.Api calls Generator.Api
    .WithEnvironment("GeneratorApi__ApiKey", generatorApiKey);

// React frontend (npm)
// Path is relative to AppHost project folder. Adjust as needed.
builder.AddNpmApp("web", "../../src/frontend/TWActionFrontend", "dev")
    .WithHttpEndpoint(port: 3000, env: "PORT")       // Aspire will set PORT
    .WithExternalHttpEndpoints()         // makes it reachable from host
    .WithReference(Api);                 // gives the frontend info about the Api

builder.Build().Run();

