
using System.Security.Claims;
using EntrePaginas.Data;
using EntrePaginas.Models;
using EntrePaginas.Services;
using EntrePaginas.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Controllers;

[AutoValidateAntiforgeryToken]
public class DiccionarioController : Controller
{
    private readonly DiccionarioApiService _diccionario;
    private readonly EntrePaginasContext _context;

    public DiccionarioController(
        DiccionarioApiService diccionario,
        EntrePaginasContext context)
    {
        _diccionario = diccionario;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda)
    {
        var modelo = new DiccionarioViewModel
        {
            Busqueda = busqueda?.Trim() ?? string.Empty,
            EsUsuarioAutenticado = User.Identity?.IsAuthenticated == true
        };

        if (User.Identity?.IsAuthenticated == true)
        {
            var idUsuario = ObtenerIdUsuario();

            if (idUsuario.HasValue)
            {
                modelo.PalabrasFavoritas = await _context.PalabrasFavoritas
                    .Where(f => f.IdUsuario == idUsuario.Value)
                    .OrderByDescending(f => f.FechaAgregado)
                    .Select(f => f.Palabra)
                    .Take(10)
                    .ToListAsync();
            }
        }

        if (!string.IsNullOrWhiteSpace(modelo.Busqueda))
        {
            try
            {
                modelo.Resultado =
                    await _diccionario.BuscarAsync(modelo.Busqueda);

                if (modelo.Resultado == null)
                {
                    modelo.Mensaje =
                        "No encontramos una entrada para esa palabra. "
                        + "Probá con otra búsqueda.";
                }
            }
            catch (HttpRequestException)
            {
                modelo.Mensaje =
                    "No pudimos conectar con el diccionario. "
                    + "Intentá nuevamente en unos minutos.";
            }
            catch (TaskCanceledException)
            {
                modelo.Mensaje =
                    "La consulta tardó demasiado. Intentá nuevamente.";
            }
            catch (System.Text.Json.JsonException)
            {
                modelo.Mensaje =
                    "La respuesta del diccionario no se pudo interpretar.";
            }
        }

        return View(modelo);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AgregarFavorito(string? palabra)
    {
        var idUsuario = ObtenerIdUsuario();

        if (idUsuario == null)
            return Forbid();

        palabra = palabra?.Trim();

        if (string.IsNullOrWhiteSpace(palabra) || palabra.Length > 150)
        {
            TempData["MensajeDiccionario"] =
                "La palabra no es válida.";
            return RedirectToAction(nameof(Index));
        }

        var yaExiste = await _context.PalabrasFavoritas.AnyAsync(x =>
            x.IdUsuario == idUsuario.Value &&
            x.Palabra == palabra);

        if (!yaExiste)
        {
            _context.PalabrasFavoritas.Add(new PalabraFavorita
            {
                IdUsuario = idUsuario.Value,
                Palabra = palabra,
                FechaAgregado = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["MensajeDiccionario"] =
                "¡Palabra agregada a tus favoritos!";
        }
        else
        {
            TempData["MensajeDiccionario"] =
                "Esa palabra ya está entre tus favoritos.";
        }

        return RedirectToAction(nameof(Index), new { busqueda = palabra });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Favoritos()
    {
        var idUsuario = ObtenerIdUsuario();

        if (idUsuario == null)
            return Forbid();

        var favoritos = await _context.PalabrasFavoritas
            .AsNoTracking()
            .Where(x => x.IdUsuario == idUsuario.Value)
            .OrderBy(x => x.Palabra)
            .ToListAsync();

        return View(favoritos);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> QuitarFavorito(int id)
    {
        var idUsuario = ObtenerIdUsuario();

        if (idUsuario == null)
            return Forbid();

        var favorito = await _context.PalabrasFavoritas
            .FirstOrDefaultAsync(x =>
                x.Id == id && x.IdUsuario == idUsuario.Value);

        if (favorito != null)
        {
            _context.PalabrasFavoritas.Remove(favorito);
            await _context.SaveChangesAsync();

            TempData["MensajeDiccionario"] =
                "Palabra eliminada de tus favoritos.";
        }

        return RedirectToAction(nameof(Favoritos));
    }

    private int? ObtenerIdUsuario()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("UsuarioId")
                    ?? User.FindFirstValue("Id");

        return int.TryParse(valor, out var id) && id > 0
            ? id
            : null;
    }
}
