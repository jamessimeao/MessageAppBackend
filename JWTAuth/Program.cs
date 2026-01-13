using JWTAuth.Services;
using Microsoft.Data.SqlClient;
using Scalar.AspNetCore;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Dependency injection for database connection, to be used by DataAccess class
string? connectionString = builder.Configuration.GetConnectionString("Default");
if(connectionString == null)
{
    Console.WriteLine("Failed to get connection string.");
    return;
}

builder.Services.AddTransient<IDbConnection>
(
    (IServiceProvider serviceProvider) => new SqlConnection(connectionString)
);

builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
