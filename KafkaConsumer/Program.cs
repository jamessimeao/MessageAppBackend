using KafkaConsumer.Data;
using KafkaConsumer.Kafka;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

if (builder.Environment.IsProduction())
{
    // For K8s deployment.
    // Some secrets will be created in Kubernetes
    builder.Configuration.SetBasePath(builder.Environment.ContentRootPath).AddJsonFile("secrets/db-settings.json");
}

// Dependency injection for database connection, to be used by DataAccess class
string? connectionString = builder.Configuration.GetConnectionString("Default");
if (connectionString == null)
{
    throw new Exception("Failed to get connection string.");
}

builder.Services.AddSingleton<IDbConnection>
(
    (IServiceProvider serviceProvider) => new SqlConnection(connectionString)
);

// Dependency injection for DataAccess class
builder.Services.AddSingleton<IDataAccess, DataAccess>();

// Kafka consumer
// Won't use dependency injection for the consumer.
// The consumer will be created inside the background service
//builder.Services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
// Run the Kafka consumer on the background
builder.Services.AddHostedService<ConsumerOnBackground>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.Run();
