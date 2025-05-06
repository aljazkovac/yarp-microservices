using OrderService.Application; // Add this using
using OrderService.Domain;      // Add this using
using OrderService.Infrastructure; // Add this using

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// *** Add our DI registrations START ***
// Register the repository as Singleton (uses static collection)
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

// Register the application service as Scoped
builder.Services.AddScoped<OrderAppService>();
// *** Add our DI registrations END ***

// Configure OpenAPI/Swagger using Swashbuckle
builder.Services.AddEndpointsApiExplorer(); // Needed for discovery
builder.Services.AddSwaggerGen();          // Configures Swagger generation

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();         // Serves the generated OpenAPI spec file(s)
    app.UseSwaggerUI();         // Serves the interactive Swagger UI
}

app.UseHttpsRedirection();

app.UseAuthorization(); // We can keep this, though we aren't using auth yet

app.MapControllers(); // Maps controller routes

app.Run();