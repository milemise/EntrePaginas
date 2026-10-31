using EntrePaginas.Data;
using EntrePaginas.Models;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Services;

/// <summary>
/// Centraliza la creación de avisos internos. No envía correos ni notificaciones push.
/// Los cambios se guardan junto con la operación de negocio desde el controlador.
/// </summary>
public sealed class NotificacionService
{
    private readonly EntrePaginasContext _context;

    public NotificacionService(EntrePaginasContext context)
    {
        _context = context;
    }

    public void CrearParaUsuario(
        int idUsuario,
        string tipo,
        string titulo,
        string mensaje,
        string? urlDestino = null,
        string? entidadTipo = null,
        int? entidadId = null)
    {
        if (idUsuario <= 0 || string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(mensaje))
            return;

        _context.Notificaciones.Add(new Notificacion
        {
            IdUsuario = idUsuario,
            Tipo = tipo.Trim()[..Math.Min(tipo.Trim().Length, 50)],
            Titulo = titulo.Trim()[..Math.Min(titulo.Trim().Length, 150)],
            Mensaje = mensaje.Trim(),
            Leida = false,
            Favorita = false,
            FechaCreacion = DateTime.Now,
            EntidadTipo = string.IsNullOrWhiteSpace(entidadTipo)
                ? null
                : entidadTipo.Trim()[..Math.Min(entidadTipo.Trim().Length, 50)],
            EntidadId = entidadId,
            UrlDestino = EsRutaLocal(urlDestino) ? urlDestino : null
        });
    }

    public async Task NotificarAdministradoresAsync(
        string tipo,
        string titulo,
        string mensaje,
        string? urlDestino = null,
        string? entidadTipo = null,
        int? entidadId = null,
        CancellationToken cancellationToken = default)
    {
        var administradores = await _context.Usuarios.AsNoTracking()
            .Where(usuario => usuario.Rol == RolesAplicacion.Administrador)
            .Select(usuario => usuario.Id)
            .ToListAsync(cancellationToken);

        foreach (var idAdministrador in administradores)
        {
            CrearParaUsuario(
                idAdministrador,
                tipo,
                titulo,
                mensaje,
                urlDestino,
                entidadTipo,
                entidadId);
        }
    }

private static bool EsRutaLocal(string? url)
{
    return !string.IsNullOrWhiteSpace(url)
        && url.StartsWith("/", StringComparison.Ordinal)
        && !url.StartsWith("//", StringComparison.Ordinal)
        && !url.Contains('\\');
}
}