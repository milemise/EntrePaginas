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
[Authorize]
public class ResenasController : Controller
{
    private readonly EntrePaginasContext _context;
    private readonly NotificacionService _notificaciones;
    public ResenasController(EntrePaginasContext context, NotificacionService notificaciones)
    {
        _context = context;
        _notificaciones = notificaciones;
    }

    [Authorize(Roles = RolesAplicacion.Lector)]
    [HttpPost]
    public async Task<IActionResult> Crear(ReviewFormViewModel model)
    {
        var lectorId = ObtenerIdUsuario();
        var libro = await _context.Libros.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.IdLibro);
        if (libro is null) return NotFound();
        if (libro.EsMas18)
        {
            var nacimiento = await _context.Usuarios.AsNoTracking()
                .Where(x => x.Id == lectorId)
                .Select(x => (DateTime?)x.FechaNacimiento)
                .FirstOrDefaultAsync();
            var edad = 0;
            if (nacimiento.HasValue)
            {
                var hoy = DateTime.Today;
                edad = hoy.Year - nacimiento.Value.Year;
                if (nacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
            }
            if (edad < 18)
            {
                TempData["Error"] = "No podés reseñar el contenido de una obra clasificada como +18 si no cumplís la edad mínima.";
                return RedirectToAction("Detalle", "Libros", new { id = model.IdLibro });
            }
        }
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
        _notificaciones.CrearParaUsuario(libro.IdEscritor, "Resena", "Nueva reseña en tu libro", $"Un lector publicó una reseña en «{libro.Titulo}».", $"/Libros/Detalle/{libro.Id}", "Libro", libro.Id);
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
        var nuevoReporte = !resena.Reportado;
        resena.Reportado = true;
        if (nuevoReporte)
        {
            await _notificaciones.NotificarAdministradoresAsync(
                "Reporte",
                "Reseña reportada",
                $"Se reportó una reseña del libro «{resena.Libro.Titulo}».",
                "/Admin/ResenasReportadas",
                "Resena",
                resena.Id);
        }
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
        if (esAdmin && resena.Libro.IdEscritor > 0)
        {
            _notificaciones.CrearParaUsuario(resena.Libro.IdEscritor, "Moderacion", "Reseña retirada por administración", $"Una reseña reportada de «{resena.Libro.Titulo}» fue retirada por el administrador.", $"/Libros/Detalle/{libroId}", "Libro", libroId);
        }
        _context.Resenas.Remove(resena);
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "La reseña fue eliminada.";
        return esAdmin ? RedirectToAction("ResenasReportadas", "Admin") : RedirectToAction("Detalle", "Libros", new { id = libroId });
    }

    private int ObtenerIdUsuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
