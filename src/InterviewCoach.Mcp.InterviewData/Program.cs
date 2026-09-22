using System.Reflection;

using InterviewCoach.Mcp.InterviewData;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddAzureNpgsqlDbContext<InterviewDataDbContext>("interviewdb");
builder.Services.AddScoped<IInterviewSessionRepository, InterviewSessionRepository>();

builder.Services.AddMcpServer()
                .WithHttpTransport(o => o.Stateless = true)
                .WithToolsFromAssembly(Assembly.GetEntryAssembly());

var app = builder.Build();

app.MapDefaultEndpoints();

// Aspire provisions the PostgreSQL server and the database (AddDatabase), both for the local
// container and in Azure, where the app's managed identity is a Microsoft Entra administrator
// of the server. Only the table schema is left to create here.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InterviewDataDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapMcp("/mcp");

await app.RunAsync();
