using System.Net;
using System.Net.Http.Json;
using DemoApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class TareasApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    public TareasApiTests(WebApplicationFactory<Program> app) => _http = app.CreateClient();

    [Fact]                                  // ← ÉSTE es el que ningún doble puede contestar por vos
    public async Task CrearTarea_LaBaseDeVerdadLaGuarda_YSeLeeIgual()
    {
        var titulo = $"integracion {Guid.NewGuid()}";          // único: no choca con corridas anteriores
        var antes = DateTime.UtcNow.AddSeconds(-1);

        var r = await _http.PostAsJsonAsync("/api/tareas", new { titulo });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);    // si la base dice que no, acá viene un 500
        var creada = await r.Content.ReadFromJsonAsync<Tarea>();

        // lo que cuenta no es lo que contestó el POST: es lo que quedó en la base
        var todas = await _http.GetFromJsonAsync<List<Tarea>>("/api/tareas");
        var leida = todas!.Single(x => x.Id == creada!.Id);
        Assert.Equal(titulo, leida.Titulo);
        Assert.InRange(leida.CreadaEl, antes, DateTime.UtcNow.AddSeconds(1));   // volvió, y es de ahora

        await _http.DeleteAsync($"/api/tareas/{creada!.Id}");  // limpia lo que creó
    }

    [Fact]                                  // otro efecto en la base: el título se guarda ya recortado
    public async Task TituloConEspacios_SeGuardaRecortado()
    {
        var titulo = $"integracion {Guid.NewGuid()}";
        var r = await _http.PostAsJsonAsync("/api/tareas", new { titulo = $"   {titulo}   " });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);    // primero el 201: sin él, no hay cuerpo que leer
        var creada = await r.Content.ReadFromJsonAsync<Tarea>();

        var todas = await _http.GetFromJsonAsync<List<Tarea>>("/api/tareas");
        Assert.Equal(titulo, todas!.Single(x => x.Id == creada!.Id).Titulo);   // en la base, sin los espacios

        await _http.DeleteAsync($"/api/tareas/{creada!.Id}");  // limpia lo que creó
    }

    [Fact]                                  // al revés que los otros dos: algo que NO llega a la base
    public async Task TituloVacio_NoLlegaALaBase()
    {
        var antes = await _http.GetFromJsonAsync<List<Tarea>>("/api/tareas");
        var r = await _http.PostAsJsonAsync("/api/tareas", new { titulo = "" });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        var despues = await _http.GetFromJsonAsync<List<Tarea>>("/api/tareas");
        Assert.Equal(antes!.Count, despues!.Count);          // no se guardó NADA
    }
}
