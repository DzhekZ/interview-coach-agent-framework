using InterviewCoach.AppHost.Core;

var builder = DistributedApplication.CreateBuilder(args);

var config = builder.Configuration;

var mcpMarkItDown = builder.AddContainer(ResourceConstants.McpMarkItDown, "mcp/markitdown", "latest")
                           .WithArgs("--http", "--host", "0.0.0.0", "--port", "3001")
                           .WithHttpEndpoint(targetPort: 3001, name: "http");

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
                   //.WithEnvironment("AZURE_TENANT_ID", config["AZURE_TENANT_ID"] ?? string.Empty)
                   .WithEnvironment("MARKITDOWN_MCP_URL", mcpMarkItDown.GetEndpoint("http"))
                   .WithReference(mcpMarkItDown.GetEndpoint("http"))
                   .WithReference(mcpInterviewData)
                   .WaitFor(mcpMarkItDown)
                   .WaitFor(mcpInterviewData);

var webUI = builder.AddProject<Projects.InterviewCoach_WebUI>(ResourceConstants.WebUI)
                   .WithExternalHttpEndpoints()
                   .WithReference(agent)
                   .WaitFor(agent);

await builder.Build().RunAsync();
