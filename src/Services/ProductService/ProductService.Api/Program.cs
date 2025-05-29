using ProductService.Application; // Add this using for ProductAppService
using ProductService.Domain;      // Add this using for IProductRepository
using ProductService.Infrastructure; // Add this using for InMemoryProductRepository
using Prometheus; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// *** Add our DI registrations START ***
// Register the repository. Using Singleton as our in-memory repo uses a static collection.
// For a real database context (like EF Core), Scoped is usually preferred.
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();

// Register the application service. Scoped is a good default for app services.
builder.Services.AddScoped<ProductAppService>();
// *** Add our DI registrations END ***

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer(); // Needed for OpenAPI exploration
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Use and send metrics to Prometheus
app.UseHttpMetrics();
app.MapMetrics();

app.Run();