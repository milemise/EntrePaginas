using System.Security.Claims;
using EntrePaginas.Data;
using EntrePaginas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class NotificacionesController : Controller
{
    private readonly EntrePaginasContext _context;

    public NotificacionesController(EntrePaginasContext context)
    {
        _context = context;
    }

    private int? ObtenerIdUsuario()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(id, out var idUsuario))
            return idUsuario;

        return null;
    }

    public async Task<IActionResult> Index()
    {
        var idUsuario = ObtenerIdUsuario();

        if (idUsuario == null)
            return Challenge();

        var notificaciones = await _context.Notificaciones
            .Where(n => n.IdUsuario == idUsuario.Value)
            .OrderByDescending(n => n.FechaCreacion)
            .ToListAsync();

        return View(notificaciones);
    }

    [HttpPost]
    public async Task<IActionResult> Abrir(int id)
    {
        var notificacion = await ObtenerNotificacionPropia(id);

        if (notificacion == null)
            return NotFound();

        notificacion.Leida = true;
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(notificacion.UrlDestino) &&
            Url.IsLocalUrl(notificacion.UrlDestino))
        {
            return Redirect(notificacion.UrlDestino);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarcarComoLeida(int id)
    {
        var notificacion = await ObtenerNotificacionPropia(id);

        if (notificacion == null)
            return NotFound();

        notificacion.Leida = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarcarComoNoLeida(int id)
    {
        var notificacion = await ObtenerNotificacionPropia(id);

        if (notificacion == null)
            return NotFound();

        notificacion.Leida = false;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarcarTodasComoLeidas()
    {
        var idUsuario = ObtenerIdUsuario();

        if (idUsuario == null)
            return Challenge();

        var pendientes = await _context.Notificaciones
            .Where(n => n.IdUsuario == idUsuario.Value && !n.Leida)
            .ToListAsync();

        foreach (var notificacion in pendientes)
            notificacion.Leida = true;

        await _context.SaveChangesAsync();

        TempData["MensajeNotificacion"] = "¡Todas tus notificaciones están leídas!";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AlternarFavorita(int id)
    {
        var notificacion = await ObtenerNotificacionPropia(id);

        if (notificacion == null)
            return NotFound();

        notificacion.Favorita = !notificacion.Favorita;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var notificacion = await ObtenerNotificacionPropia(id);

        if (notificacion == null)
            return NotFound();

        _context.Notificaciones.Remove(notificacion);
        await _context.SaveChangesAsync();

        TempData["MensajeNotificacion"] = "La notificación fue eliminada.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<Notificacion?> ObtenerNotificacionPropia(int id)
    {
        var idUsuario = ObtenerIdUsuario();

        if (idUsuario == null)
            return null;

        return await _context.Notificaciones
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.IdUsuario == idUsuario.Value);
    }
}
