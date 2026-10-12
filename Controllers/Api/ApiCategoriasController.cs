using EntrePaginas.Data;
using EntrePaginas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Controllers.Api;

[ApiController]
[Route("api/categorias")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ApiCategoriasController : ControllerBase
{
    private readonly EntrePaginasContext _context;
    public ApiCategoriasController(EntrePaginasContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var items = await _context.Categorias.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new { x.Id, x.Nombre }).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = RolesAplicacion.Administrador)]
    public async Task<IActionResult> Crear([FromBody] CategoriaRequest request, CancellationToken cancellationToken)
    {
        var nombre = request.Nombre?.Trim();
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100) return BadRequest(new { mensaje = "Nombre obligatorio (hasta 100 caracteres)." });
        if (await _context.Categorias.AnyAsync(x => x.Nombre.ToLower() == nombre.ToLower(), cancellationToken)) return Conflict(new { mensaje = "La categoría ya existe." });
        var categoria = new Categoria { Nombre = nombre };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync(cancellationToken);
        return Created($"/api/categorias/{categoria.Id}", new { categoria.Id, categoria.Nombre });
    }

    public sealed class CategoriaRequest { public string? Nombre { get; set; } }
}
