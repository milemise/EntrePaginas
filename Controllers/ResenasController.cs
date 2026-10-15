using System.Security.Claims;
using EntrePaginas.Data;
using EntrePaginas.Models;
using EntrePaginas.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Controllers;

[AutoValidateAntiforgeryToken]
[Authorize]
public class ResenasController : Controller
{
    private readonly EntrePaginasContext _context;
    public ResenasController(EntrePaginasContext context) => _context = context;

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Crear(ReviewFormViewModel model)
    {
        var lectorId = ObtenerIdUsuario();
        var libro = await _context.Libros.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.IdLibro);
        if (libro is null) return NotFound();
        var tieneAccesoHistorico = await _context.Compras.AnyAsync(x => x.IdLibro == model.IdLibro && x.IdLector == lectorId) ||
                                   await _context.Alquileres.AnyAsync(x => x.IdLibro == model.IdLibro && x.IdLector == lectorId);
        if (!tieneAccesoHistorico)
        {
            TempData["Error"] = "Solo podés reseñar un libro al que hayas accedido mediante compra o alquiler.";
            return RedirectToAction("Detalle", "Libros", new { id = model.IdLibro });
        }
        if (await _context.Resenas.AnyAsync(x => x.IdLibro == model.IdLibro && x.IdLector == lectorId))
        {
            TempData["Error"] = "Ya publicaste una reseña para este libro.";
            return RedirectToAction("Detalle", "Libros", new { id = model.IdLibro });
        }
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "La puntuación debe estar entre 1 y 5 y el comentario no puede superar los 3000 caracteres.";
            return RedirectToAction("Detalle", "Libros", new { id = model.IdLibro });
        }
        _context.Resenas.Add(new Resena
        {
            IdLibro = model.IdLibro, IdLector = lectorId, Puntuacion = model.Puntuacion,
            Comentario = model.Comentario?.Trim(), Anonimo = model.Anonimo, FechaComentario = DateTime.Now
        });
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "Tu reseña se publicó correctamente.";
        return RedirectToAction("Detalle", "Libros", new { id = model.IdLibro });
    }

    [Authorize(Roles = RolesAplicacion.Escritor)]
    [HttpPost]
    public async Task<IActionResult> Reportar(int id)
    {
        var usuarioId = ObtenerIdUsuario();
        var resena = await _context.Resenas.Include(x => x.Libro).FirstOrDefaultAsync(x => x.Id == id);
        if (resena is null) return NotFound();
        if (resena.Libro.IdEscritor != usuarioId) return Forbid();
        resena.Reportado = true;
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "La reseña fue enviada a revisión del administrador.";
        return RedirectToAction("Detalle", "Libros", new { id = resena.IdLibro });
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resena = await _context.Resenas.Include(x => x.Libro).FirstOrDefaultAsync(x => x.Id == id);
        if (resena is null) return NotFound();
        var esAdmin = User.IsInRole(RolesAplicacion.Administrador);
        var esEscritorPropietario = User.IsInRole(RolesAplicacion.Escritor) && resena.Libro.IdEscritor == ObtenerIdUsuario();
        if (!esAdmin && !esEscritorPropietario) return Forbid();
        var libroId = resena.IdLibro;
        _context.Resenas.Remove(resena);
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "La reseña fue eliminada.";
        return esAdmin ? RedirectToAction("ResenasReportadas", "Admin") : RedirectToAction("Detalle", "Libros", new { id = libroId });
    }

    private int ObtenerIdUsuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
