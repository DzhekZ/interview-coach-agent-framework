#:sdk Aspire.AppHost.Sdk@13.4.6
#:package Aspire.Hosting.Azure.PostgreSQL
#:project ./src/InterviewCoach.Agent/InterviewCoach.Agent.csproj
#:project ./src/InterviewCoach.AppHost.Core/InterviewCoach.AppHost.Core.csproj
#:project ./src/InterviewCoach.Mcp.InterviewData/InterviewCoach.Mcp.InterviewData.csproj
#:project ./src/InterviewCoach.WebUI/InterviewCoach.WebUI.csproj

using InterviewCoach.AppHost.Core;

using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var config = builder.Configuration
                    .AddJsonFile("apphost.settings.json", optional: true, reloadOnChange: true)
                    .AddUserSecrets(typeof(Program).Assembly, optional: true, reloadOnChange: true)
                    .Build();

var mcpMarkItDown = builder.AddContainer(ResourceConstants.McpMarkItDown, "mcp/markitdown", "latest")
                           .WithHttpEndpoint(targetPort: 3001)
                           .WithArgs("--http", "--host", "0.0.0.0", "--port", "3001");

// PostgreSQL. Runs as a local container (with pgAdmin) in run mode and provisions an Azure
// Database for PostgreSQL flexible server when published. Aspire creates the database as a
// resource; the MCP server creates the table schema on startup (EnsureCreatedAsync).
var postgres = builder.AddAzurePostgresFlexibleServer(ResourceConstants.Postgres)
                      .RunAsContainer(container => container.WithDataVolume()
                                                            .WithPgAdmin());

var postgresDb = postgres.AddDatabase(ResourceConstants.PostgresDatabase);

var mcpInterviewData = builder.AddProject<Projects.InterviewCoach_Mcp_InterviewData>(ResourceConstants.McpInterviewData)
                              .WithReference(postgresDb)
                              .WaitFor(postgresDb);

var agent = builder.AddProject<Projects.InterviewCoach_Agent>(ResourceConstants.Agent)
                   .WithExternalHttpEndpoints()
                   .WithLlmReference(config, args)
                   .WithReference(mcpMarkItDown.GetEndpoint("http"))
                   .WithReference(mcpInterviewData)
                   .WaitFor(mcpMarkItDown)
                   .WaitFor(mcpInterviewData);

var webUI = builder.AddProject<Projects.InterviewCoach_WebUI>(ResourceConstants.WebUI)
                   .WithExternalHttpEndpoints()
                   .WithReference(agent)
                   .WaitFor(agent);

await builder.Build().RunAsync();
