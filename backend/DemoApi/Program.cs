using System.Diagnostics.CodeAnalysis;
using DemoApi.Data;
using DemoApi.Logica;
using DemoApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// CORS abierto para el sample: permite que el front en dev (localhost:5173) o
// servido por nginx (localhost:3000) consuma la API publicada en localhost:8080.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// 🔴 EL PASO QUE LOS TESTS NO RECLAMAN (TP5 §3.0). Al sacar el `new` de adentro
// de ServicioDeTareas, la app dejó de saber qué instancia usar. Si esto falta, los
// tests pasan igual —le pasan el impostor a mano— y la primera llamada real revienta.
builder.Services.AddScoped<DemoApi.Servicios.INotificador, DemoApi.Servicios.NotificadorEmail>();
builder.Services.AddScoped<DemoApi.Servicios.ServicioDeTareas>();

var app = builder.Build();

app.UseCors();

// Crea el schema al arrancar si no existe (suficiente para el sample de la cátedra;
// en un proyecto real esto se resuelve con migraciones).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/health", () => Results.Ok(new {
    status = "ok",
    commit = Environment.GetEnvironmentVariable("APP_COMMIT") ?? "desconocido"
}));

app.MapGet("/api/tareas", async (AppDbContext db) =>
    await db.Tareas.OrderBy(t => t.Id).ToListAsync());

app.MapPost("/api/tareas", async (AppDbContext db, TareaNueva input) =>
{
    var tarea = new Tarea
    {
        Titulo = input.Titulo!,
        CreadaEl = DateTime.UtcNow
    };
    db.Tareas.Add(tarea);
    await db.SaveChangesAsync();
    return Results.Created($"/api/tareas/{tarea.Id}", tarea);
});

app.MapDelete("/api/tareas/{id:int}", async (AppDbContext db, int id) =>
{
    var tarea = await db.Tareas.FindAsync(id);
    if (tarea is null)
        return Results.NotFound();

    db.Tareas.Remove(tarea);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

public record TareaNueva(string? Titulo);

[ExcludeFromCodeCoverage]
public partial class Program { }
