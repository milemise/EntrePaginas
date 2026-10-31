using System.Security.Claims;
using EntrePaginas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.ViewComponents;

public sealed class NotificacionesViewComponent : ViewComponent
{
    private readonly EntrePaginasContext _context;

    public NotificacionesViewComponent(EntrePaginasContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (UserClaimsPrincipal.Identity?.IsAuthenticated != true)
            return Content(string.Empty);

        var claim = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var idUsuario) || idUsuario <= 0)
            return Content(string.Empty);

        var cantidad = await _context.Notificaciones.AsNoTracking()
            .CountAsync(notificacion => notificacion.IdUsuario == idUsuario && !notificacion.Leida);

        return View(cantidad);
    }
}
