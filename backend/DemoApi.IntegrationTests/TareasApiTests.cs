using System.Net;
using System.Net.Http.Json;
using DemoApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class TareasApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    public TareasApiTests(WebApplicationFactory<Program> app) => _http = app.CreateClient();

    [Fact]
    public async Task CrearTarea_LaGuardaYLaDevuelve()
    {
        var titulo = $"integracion {Guid.NewGuid()}";          // único: no choca con corridas anteriores
        var r = await _http.PostAsJsonAsync("/api/tareas", new { titulo });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var creada = await r.Content.ReadFromJsonAsync<Tarea>();

        var todas = await _http.GetFromJsonAsync<List<Tarea>>("/api/tareas");
        Assert.Contains(todas!, x => x.Titulo == titulo);

        await _http.DeleteAsync($"/api/tareas/{creada!.Id}");  // limpia lo que creó
    }

    [Fact]                                  // ← ÉSTE es el que ningún unitario puede hacer
    public async Task TituloVacio_NoLlegaALaBase()
    {
        var antes = await _http.GetFromJsonAsync<List<Tarea>>("/api/tareas");

        var r = await _http.PostAsJsonAsync("/api/tareas", new { titulo = "" });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);

        var despues = await _http.GetFromJsonAsync<List<Tarea>>("/api/tareas");
        Assert.Equal(antes!.Count, despues!.Count);          // no se guardó NADA
    }

    [Fact]                                  // otra costura: el endpoint guarda el título ya recortado
    public async Task TituloConEspacios_SeGuardaRecortado()
    {
        var titulo = $"integracion {Guid.NewGuid()}";
        var r = await _http.PostAsJsonAsync("/api/tareas", new { titulo = $"   {titulo}   " });
        var creada = await r.Content.ReadFromJsonAsync<Tarea>();

        var todas = await _http.GetFromJsonAsync<List<Tarea>>("/api/tareas");
        Assert.Equal(titulo, todas!.Single(x => x.Id == creada!.Id).Titulo);   // en la base, sin los espacios

        await _http.DeleteAsync($"/api/tareas/{creada!.Id}");  // limpia lo que creó
    }
}
