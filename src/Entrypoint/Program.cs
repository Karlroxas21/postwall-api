using Entrypoint.Middlewares;
using Infrastructure;
using Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// DbContext and Infra DI
builder.Services.AddInfrastructure(builder.Configuration);

// Service DI
builder.Services.AddService();

// Controller
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

await app.MigrateDbAsync();
app.Run();
