using System.Security.Claims;
using EntrePaginas.Data;
using EntrePaginas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Controllers.Api;

[ApiController]
[Route("api/libros")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ApiLibrosController : ControllerBase
{
    private readonly EntrePaginasContext _context;
    public ApiLibrosController(EntrePaginasContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? buscar, [FromQuery] int? categoriaId, [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10, CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 50);
        var consulta = _context.Libros.AsNoTracking().Include(x => x.Categoria).Include(x => x.Escritor).AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();
            consulta = consulta.Where(x => x.Titulo.Contains(termino) || (x.Autor != null && x.Autor.Contains(termino)) || (x.Descripcion != null && x.Descripcion.Contains(termino)));
        }
        if (categoriaId.HasValue) consulta = consulta.Where(x => x.IdCategoria == categoriaId.Value);
        consulta = consulta.OrderBy(x => x.Titulo);
        var total = await consulta.CountAsync(cancellationToken);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;
        var items = await consulta.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina)
            .Select(x => new
            {
                x.Id, x.Titulo, x.Autor, x.Descripcion, x.PortadaUrl, x.PrecioCompra, x.PrecioAlquiler, x.EsMas18,
                Categoria = x.Categoria.Nombre, Escritor = x.Escritor.Nombre + " " + x.Escritor.Apellido
            }).ToListAsync(cancellationToken);
        return Ok(new { pagina, tamanoPagina, total, totalPaginas, items });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(int id, CancellationToken cancellationToken)
    {
        var libro = await _context.Libros.AsNoTracking().Include(x => x.Categoria).Include(x => x.Escritor)
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id, x.Titulo, x.Autor, x.Descripcion, x.PortadaUrl, x.PrecioCompra, x.PrecioAlquiler,
                x.EsMas18, Categoria = x.Categoria.Nombre,
                Escritor = x.Escritor.Nombre + " " + x.Escritor.Apellido,
                Promedio = x.Resenas.Average(r => (double?)r.Puntuacion) ?? 0,
                TotalResenas = x.Resenas.Count
            }).FirstOrDefaultAsync(cancellationToken);
        return libro is null ? NotFound() : Ok(libro);
    }

    [HttpGet("mi-biblioteca/compras")]
    [Authorize(Roles = RolesAplicacion.Lector)]
    public async Task<IActionResult> MisCompras([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId)) return Unauthorized();
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 50);
        var consulta = _context.Compras.AsNoTracking().Include(x => x.Libro).Where(x => x.IdLector == usuarioId).OrderByDescending(x => x.FechaCompra);
        var total = await consulta.CountAsync(cancellationToken);
        var items = await consulta.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina)
            .Select(x => new { x.Id, x.FechaCompra, x.Importe, x.Devuelto, LibroId = x.Libro.Id, x.Libro.Titulo }).ToListAsync(cancellationToken);
        return Ok(new { pagina, tamanoPagina, total, items });
    }
}
