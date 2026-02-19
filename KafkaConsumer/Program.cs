using KafkaConsumer.Kafka;
using KafkaConsumer.Kafka.Values;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

// Kafka consumer
// Won't use dependency injection for the consumer.
// The consumer will be created inside the background service
//builder.Services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
// Run the Kafka consumer on the background
builder.Services.AddHostedService<ConsumerOnBackground>();

builder.Services.AddSingleton<ISerializer, Serializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
/*
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
*/
//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.Run();
