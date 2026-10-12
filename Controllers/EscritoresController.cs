using System.Security.Claims;
using EntrePaginas.Data;
using EntrePaginas.Models;
using EntrePaginas.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Controllers;

[Authorize(Roles = RolesAplicacion.Escritor)]
public class EscritoresController : Controller
{
    private readonly EntrePaginasContext _context;
    public EscritoresController(EntrePaginasContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Estadisticas()
    {
        var id = ObtenerIdUsuario();
        var libros = await _context.Libros.AsNoTracking().Where(x => x.IdEscritor == id)
            .Select(x => new LibroEstadisticaItem
            {
                Id = x.Id, Titulo = x.Titulo,
                Ventas = x.Compras.Count(c => !c.Devuelto),
                Alquileres = x.Alquileres.Count(a => a.Activo && a.FechaHasta >= DateTime.Now),
                Ingresos = (x.Compras.Where(c => !c.Devuelto).Sum(c => (decimal?)c.Importe) ?? 0m)
                          + (x.Alquileres.Sum(a => (decimal?)a.Importe) ?? 0m)
            }).OrderByDescending(x => x.Ingresos).ToListAsync();
        var ids = await _context.Libros.Where(x => x.IdEscritor == id).Select(x => x.Id).ToListAsync();
        var ventas = await _context.Compras.CountAsync(x => ids.Contains(x.IdLibro) && !x.Devuelto);
        var alquileres = await _context.Alquileres.CountAsync(x => ids.Contains(x.IdLibro) && x.Activo && x.FechaHasta >= DateTime.Now);
        var ingresosVentas = await _context.Compras.Where(x => ids.Contains(x.IdLibro) && !x.Devuelto).SumAsync(x => (decimal?)x.Importe) ?? 0m;
        var ingresosAlquileres = await _context.Alquileres.Where(x => ids.Contains(x.IdLibro)).SumAsync(x => (decimal?)x.Importe) ?? 0m;
        return View(new EstadisticasEscritorViewModel
        {
            LibrosPublicados = ids.Count, Ventas = ventas, Alquileres = alquileres,
            IngresosVentas = ingresosVentas, IngresosAlquileres = ingresosAlquileres, Libros = libros
        });
    }

    private int ObtenerIdUsuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
