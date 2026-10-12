using EntrePaginas.Data;
using EntrePaginas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Controllers;

[ApiController]
[Route("ajax/categorias")]
[Authorize(Roles = RolesAplicacion.Administrador)]
[AutoValidateAntiforgeryToken]
public class CategoriasAjaxController : ControllerBase
{
    private readonly EntrePaginasContext _context;
    public CategoriasAjaxController(EntrePaginasContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var items = await _context.Categorias.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new { id = x.Id, nombre = x.Nombre, cantidadLibros = x.Libros.Count })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CategoriaEntrada entrada, CancellationToken cancellationToken)
    {
        var nombre = entrada.Nombre?.Trim();
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100)
            return BadRequest(new { mensaje = "El nombre es obligatorio y debe tener hasta 100 caracteres." });
        if (await _context.Categorias.AnyAsync(x => x.Nombre.ToLower() == nombre.ToLower(), cancellationToken))
            return Conflict(new { mensaje = "Ya existe una categoría con ese nombre." });
        var categoria = new Categoria { Nombre = nombre };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync(cancellationToken);
        return Created($"/ajax/categorias/{categoria.Id}", new { id = categoria.Id, nombre = categoria.Nombre, cantidadLibros = 0 });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, [FromBody] CategoriaEntrada entrada, CancellationToken cancellationToken)
    {
        var categoria = await _context.Categorias.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (categoria is null) return NotFound(new { mensaje = "La categoría no existe." });
        var nombre = entrada.Nombre?.Trim();
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100)
            return BadRequest(new { mensaje = "El nombre es obligatorio y debe tener hasta 100 caracteres." });
        if (await _context.Categorias.AnyAsync(x => x.Id != id && x.Nombre.ToLower() == nombre.ToLower(), cancellationToken))
            return Conflict(new { mensaje = "Ya existe una categoría con ese nombre." });
        categoria.Nombre = nombre;
        await _context.SaveChangesAsync(cancellationToken);
        var cantidad = await _context.Libros.CountAsync(x => x.IdCategoria == id, cancellationToken);
        return Ok(new { id = categoria.Id, nombre = categoria.Nombre, cantidadLibros = cantidad });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var categoria = await _context.Categorias.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (categoria is null) return NotFound(new { mensaje = "La categoría no existe." });
        if (await _context.Libros.AnyAsync(x => x.IdCategoria == id, cancellationToken))
            return Conflict(new { mensaje = "No se puede eliminar una categoría que tiene libros asociados." });
        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    public sealed class CategoriaEntrada
    {
        public string? Nombre { get; set; }
    }
}
