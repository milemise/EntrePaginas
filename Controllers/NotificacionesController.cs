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
    private const int TamanoPagina = 10;
    private readonly EntrePaginasContext _context;

    public NotificacionesController(EntrePaginasContext context)
    {
        _context = context;
    }

    private int? ObtenerIdUsuario()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var idUsuario) && idUsuario > 0 ? idUsuario : null;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? buscar, string? filtro, int pagina = 1, CancellationToken cancellationToken = default)
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario is null)
            return Challenge();

        var propias = _context.Notificaciones.AsNoTracking()
            .Where(n => n.IdUsuario == idUsuario.Value);

        ViewBag.CantidadNoLeidas = await propias.CountAsync(n => !n.Leida, cancellationToken);
        ViewBag.TotalNotificacionesUsuario = await propias.CountAsync(cancellationToken);

        filtro = filtro is "no-leidas" or "favoritas" ? filtro : "todas";
        var consulta = propias.AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            consulta = consulta.Where(n => n.Titulo.Contains(buscar)
                || n.Mensaje.Contains(buscar)
                || n.Tipo.Contains(buscar));
        }

        if (filtro == "no-leidas")
            consulta = consulta.Where(n => !n.Leida);
        else if (filtro == "favoritas")
            consulta = consulta.Where(n => n.Favorita);

        var totalFiltrado = await consulta.CountAsync(cancellationToken);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalFiltrado / (double)TamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        ViewBag.Buscar = buscar;
        ViewBag.Filtro = filtro;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalFiltrado = totalFiltrado;

        var notificaciones = await consulta
            .OrderByDescending(n => n.FechaCreacion)
            .ThenByDescending(n => n.Id)
            .Skip((pagina - 1) * TamanoPagina)
            .Take(TamanoPagina)
            .ToListAsync(cancellationToken);

        return View(notificaciones);
    }

    [HttpPost]
    public async Task<IActionResult> Abrir(int id)
    {
        var notificacion = await ObtenerNotificacionPropia(id);
        if (notificacion == null) return NotFound();

        notificacion.Leida = true;
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(notificacion.UrlDestino) && Url.IsLocalUrl(notificacion.UrlDestino))
            return Redirect(notificacion.UrlDestino);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarcarComoLeida(int id)
    {
        var notificacion = await ObtenerNotificacionPropia(id);
        if (notificacion == null) return NotFound();

        notificacion.Leida = true;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarcarComoNoLeida(int id)
    {
        var notificacion = await ObtenerNotificacionPropia(id);
        if (notificacion == null) return NotFound();

        notificacion.Leida = false;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarcarTodasComoLeidas()
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario == null) return Challenge();

        var pendientes = await _context.Notificaciones
            .Where(n => n.IdUsuario == idUsuario.Value && !n.Leida)
            .ToListAsync();

        foreach (var notificacion in pendientes)
            notificacion.Leida = true;

        await _context.SaveChangesAsync();
        TempData["MensajeNotificacion"] = "Todas tus notificaciones quedaron marcadas como leídas.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AlternarFavorita(int id)
    {
        var notificacion = await ObtenerNotificacionPropia(id);
        if (notificacion == null) return NotFound();

        notificacion.Favorita = !notificacion.Favorita;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var notificacion = await ObtenerNotificacionPropia(id);
        if (notificacion == null) return NotFound();

        _context.Notificaciones.Remove(notificacion);
        await _context.SaveChangesAsync();
        TempData["MensajeNotificacion"] = "La notificación fue eliminada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Notificacion?> ObtenerNotificacionPropia(int id)
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario == null) return null;

        return await _context.Notificaciones.FirstOrDefaultAsync(n => n.Id == id && n.IdUsuario == idUsuario.Value);
    }
}
