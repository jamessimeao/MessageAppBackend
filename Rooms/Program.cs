using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Rooms.Data;
using Scalar.AspNetCore;
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

// Dependency injection for DataAccess class
builder.Services.AddTransient<IDataAccess, DataAccess>();

string? appSettingsTokenNullable = builder.Configuration.GetValue<string>("AppSettings:Token");
string? appSettingsIssuerNullable = builder.Configuration.GetValue<string>("AppSettings:Issuer");
string? appSettingsAudienceNullable = builder.Configuration.GetValue<string>("AppSettings:Audience");
if (appSettingsTokenNullable == null || appSettingsIssuerNullable == null || appSettingsAudienceNullable == null)
{
    throw new Exception("Failed to get AppSettings token, issuer or audience.");
}
string appSettingsToken = appSettingsTokenNullable;
string appSettingsIssuer = appSettingsIssuerNullable;
string appSettingsAudience = appSettingsAudienceNullable;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        (JwtBearerOptions options) =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = appSettingsIssuer,
                ValidateAudience = true,
                ValidAudience = appSettingsAudience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettingsToken)),
                ValidateLifetime = true
            };
        }
    );

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
