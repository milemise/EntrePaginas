using System.Security.Claims;
using EntrePaginas.Data;
using EntrePaginas.Models;
using EntrePaginas.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Controllers;

[Authorize(Roles = RolesAplicacion.Lector)]
public class BibliotecaController : Controller
{
    private const int TamanoPagina = 10;
    private readonly EntrePaginasContext _context;
    public BibliotecaController(EntrePaginasContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Compras(string? buscar, int pagina = 1, CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        var id = ObtenerIdUsuario();
        var consulta = _context.Compras.AsNoTracking().Include(x => x.Libro).Where(x => x.IdLector == id);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();
            consulta = consulta.Where(x => x.Libro.Titulo.Contains(termino)
                || (x.Libro.Autor != null && x.Libro.Autor.Contains(termino)));
        }
        consulta = consulta.OrderByDescending(x => x.FechaCompra);
        var total = await consulta.CountAsync(cancellationToken);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalRegistros = total;
        ViewBag.Buscar = buscar;
        return View(await consulta.Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Alquileres(string? buscar, int pagina = 1, CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        var id = ObtenerIdUsuario();
        var consulta = _context.Alquileres.AsNoTracking().Include(x => x.Libro).Where(x => x.IdLector == id);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();
            consulta = consulta.Where(x => x.Libro.Titulo.Contains(termino)
                || (x.Libro.Autor != null && x.Libro.Autor.Contains(termino)));
        }
        consulta = consulta.OrderByDescending(x => x.FechaInicio);
        var total = await consulta.CountAsync(cancellationToken);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalRegistros = total;
        ViewBag.Buscar = buscar;
        return View(await consulta.Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync(cancellationToken));
    }

    private int ObtenerIdUsuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
