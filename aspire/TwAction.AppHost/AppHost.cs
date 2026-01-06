var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL (container) + a logical database
var postgres = builder.AddPostgres("postgres");
var appDb = postgres.AddDatabase("TWActionDatabase");

// Backend API (.NET project)
var api = builder.AddProject<Projects.TWAction_Api>("api")
    .WithReference(appDb); // passes connection info to the API

// React frontend (npm)
// Path is relative to AppHost project folder. Adjust as needed.
var web = builder.AddNpmApp("web", "../../src/frontend/TWActionFrontend", "dev")
    .WithHttpEndpoint(port: 3000, env: "PORT")       // Aspire will set PORT
    .WithExternalHttpEndpoints()         // makes it reachable from host
    .WithReference(api);                 // gives the frontend info about the API

builder.Build().Run();
