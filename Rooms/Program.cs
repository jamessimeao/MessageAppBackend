using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

if (builder.Environment.IsProduction())
{
    // For K8s deployment.
    // Some secrets will be created in Kubernetes
    // which will be copied the json files secrets/jwt-settings.json and secrets/db-settings.json.
    builder.Configuration.SetBasePath(builder.Environment.ContentRootPath).AddJsonFile("secrets/jwt-settings.json");
    builder.Configuration.SetBasePath(builder.Environment.ContentRootPath).AddJsonFile("secrets/db-settings.json");
}

// Dependency injection for database connection, to be used by DataAccess class
string? connectionString = builder.Configuration.GetConnectionString("Default");
if (connectionString == null)
{
    throw new Exception("Failed to get connection string.");
}

builder.Services.AddTransient<IDbConnection>
(
    (IServiceProvider serviceProvider) => new SqlConnection(connectionString)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
