using JWTAuth.Data;
using JWTAuth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Data;
using System.Text;

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

// Dependency injection for DataAccess class
builder.Services.AddTransient<IDataAccess, DataAccess>();

// Dependency injection for authorization service
builder.Services.AddScoped<IAuthService, AuthService>();

// Make an authentication schema to be used for refreshing the token.
// As such, we have to allow the access token to be outdated.
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
        (JwtBearerOptions options) => options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = appSettingsIssuer,
            ValidateAudience = true,
            ValidAudience = appSettingsAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettingsToken)),
            ValidateLifetime = false
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
