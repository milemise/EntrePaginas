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
    public async Task<IActionResult> Compras(int pagina = 1)
    {
        pagina = Math.Max(1, pagina);
        var id = ObtenerIdUsuario();
        var consulta = _context.Compras.AsNoTracking().Include(x => x.Libro).Where(x => x.IdLector == id).OrderByDescending(x => x.FechaCompra);
        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalRegistros = total;
        return View(await consulta.Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Alquileres(int pagina = 1)
    {
        pagina = Math.Max(1, pagina);
        var id = ObtenerIdUsuario();
        var consulta = _context.Alquileres.AsNoTracking().Include(x => x.Libro).Where(x => x.IdLector == id).OrderByDescending(x => x.FechaInicio);
        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalRegistros = total;
        return View(await consulta.Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync());
    }

    private int ObtenerIdUsuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
