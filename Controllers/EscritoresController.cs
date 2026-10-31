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
    private const int TamanoPagina = 10;
    private readonly EntrePaginasContext _context;
    public EscritoresController(EntrePaginasContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Estadisticas(string? buscar, int pagina = 1, CancellationToken cancellationToken = default)
    {
        var id = ObtenerIdUsuario();
        pagina = Math.Max(1, pagina);
        var todosLosLibros = _context.Libros.AsNoTracking().Where(x => x.IdEscritor == id);

        // Los indicadores generales se calculan sobre todas las obras del escritor,
        // independientemente del filtro aplicado al detalle de la tabla.
        var ids = await todosLosLibros.Select(x => x.Id).ToListAsync(cancellationToken);
        var librosPublicados = ids.Count;
        var ventas = await _context.Compras.CountAsync(x => ids.Contains(x.IdLibro) && !x.Devuelto, cancellationToken);
        var alquileres = await _context.Alquileres.CountAsync(x => ids.Contains(x.IdLibro) && x.Activo && x.FechaHasta >= DateTime.Now, cancellationToken);
        var ingresosVentas = await _context.Compras.Where(x => ids.Contains(x.IdLibro) && !x.Devuelto).SumAsync(x => (decimal?)x.Importe, cancellationToken) ?? 0m;
        var ingresosAlquileres = await _context.Alquileres.Where(x => ids.Contains(x.IdLibro)).SumAsync(x => (decimal?)x.Importe, cancellationToken) ?? 0m;

        var consulta = todosLosLibros.Include(x => x.Categoria).AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            consulta = consulta.Where(x => x.Titulo.Contains(buscar)
                || (x.Autor != null && x.Autor.Contains(buscar))
                || x.Categoria.Nombre.Contains(buscar));
        }
        var total = await consulta.CountAsync(cancellationToken);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var libros = await consulta.OrderByDescending(x => x.Id)
            .Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina)
            .Select(x => new LibroEstadisticaItem
            {
                Id = x.Id,
                Titulo = x.Titulo,
                Ventas = x.Compras.Count(c => !c.Devuelto),
                Alquileres = x.Alquileres.Count(a => a.Activo && a.FechaHasta >= DateTime.Now),
                Ingresos = (x.Compras.Where(c => !c.Devuelto).Sum(c => (decimal?)c.Importe) ?? 0m)
                    + (x.Alquileres.Sum(a => (decimal?)a.Importe) ?? 0m)
            }).ToListAsync(cancellationToken);

        return View(new EstadisticasEscritorViewModel
        {
            LibrosPublicados = librosPublicados,
            Ventas = ventas,
            Alquileres = alquileres,
            IngresosVentas = ingresosVentas,
            IngresosAlquileres = ingresosAlquileres,
            Libros = libros,
            Buscar = buscar,
            Pagina = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = total
        });
    }

    private int ObtenerIdUsuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
