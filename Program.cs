using Microsoft.EntityFrameworkCore;
using NLog.Web;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Configure OpenTelemetry
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("YourServiceName")
    .AddAttributes(new Dictionary<string, object>
    {
      ["at-project-id"] = "00000000-0000-0000-0000-000000000000",
      ["at-project-key"] = "kKMdJZdMPikzn91K0qZsTzsc9DjBSYCe6bjp0b9fojtT9Y3C",
    });

// Reusable method to configure OTLP options
Action<OtlpExporterOptions> ConfigureOtlpOptions()
{
  return otlp =>
  {
    otlp.Protocol = OtlpExportProtocol.Grpc;
    otlp.Endpoint = new Uri("http://otelcol.apitoolkit.io:4317");
  };
};


builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(ConfigureOtlpOptions()));

// Configure OpenTelemetry for logging
builder.Logging.AddOpenTelemetry(options =>
{
  options.SetResourceBuilder(resourceBuilder);
  options.AddOtlpExporter(ConfigureOtlpOptions());
});

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
  config.DocumentName = "TodoAPI";
  config.Title = "TodoAPI v1";
  config.Version = "v1";
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
  app.UseOpenApi();
  app.UseSwaggerUi(config =>
  {
    config.DocumentTitle = "TodoAPI";
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
  });
}

app.Logger.LogInformation("Info logging ===");
app.Logger.LogError("Miden Error logging === ");

app.MapGet("/todoitems", async (ILogger<Program> logger, TodoDb db) =>
{
  logger.LogError("GET: Miden Demo test log ");
  await db.Todos.ToListAsync();
});

app.MapGet("/todoitems/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
  db.Todos.Add(todo);
  await db.SaveChangesAsync();

  return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
  var todo = await db.Todos.FindAsync(id);

  if (todo is null) return Results.NotFound();

  todo.Name = inputTodo.Name;
  todo.IsComplete = inputTodo.IsComplete;

  await db.SaveChangesAsync();

  return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
  if (await db.Todos.FindAsync(id) is Todo todo)
  {
    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.NoContent();
  }

  return Results.NotFound();
});

app.Run();
NLog.LogManager.Shutdown();
