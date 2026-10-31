
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
                    await _diccionario.BuscarAsync(modelo.Busqueda, HttpContext.RequestAborted);

                if (modelo.Resultado == null)
                {
                    modelo.Mensaje =
                        "No encontramos una definición para esa palabra. "
                        + "Revisá la escritura o probá con un término relacionado.";
                }
            }
            catch (DiccionarioTemporalmenteNoDisponibleException)
            {
                modelo.Mensaje =
                    "No pudimos mostrar esta definición ahora. "
                    + "Probá nuevamente o elegí una palabra del índice.";
            }
            catch (HttpRequestException)
            {
                modelo.Mensaje =
                    "No pudimos mostrar esta definición ahora. "
                    + "Probá nuevamente o elegí una palabra del índice.";
            }
            catch (TaskCanceledException) when (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                modelo.Mensaje =
                    "La búsqueda está tardando más de lo esperado. Probá nuevamente.";
            }
            catch (System.Text.Json.JsonException)
            {
                modelo.Mensaje =
                    "No pudimos mostrar esta definición ahora. Probá con otra palabra.";
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
    public async Task<IActionResult> Favoritos(string? buscar, int pagina = 1, CancellationToken cancellationToken = default)
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario == null) return Forbid();

        const int tamanoPagina = 15;
        var consulta = _context.PalabrasFavoritas.AsNoTracking()
            .Where(x => x.IdUsuario == idUsuario.Value);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            consulta = consulta.Where(x => x.Palabra.Contains(buscar));
        }

        var total = await consulta.CountAsync(cancellationToken);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);
        ViewBag.Buscar = buscar;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalRegistros = total;
        var favoritos = await consulta.OrderBy(x => x.Palabra)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(cancellationToken);
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
