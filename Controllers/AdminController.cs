using System.Security.Claims;
using EntrePaginas.Data;
using EntrePaginas.Models;
using EntrePaginas.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Controllers;

[Authorize(Roles = RolesAplicacion.Administrador)]
[AutoValidateAntiforgeryToken]
public class AdminController : Controller
{
    private const int TamanoPagina = 15;
    private readonly EntrePaginasContext _context;
    public AdminController(EntrePaginasContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Usuarios = await _context.Usuarios.CountAsync();
        ViewBag.Libros = await _context.Libros.CountAsync();
        ViewBag.Compras = await _context.Compras.CountAsync();
        ViewBag.Alquileres = await _context.Alquileres.CountAsync();
        ViewBag.ResenasReportadas = await _context.Resenas.CountAsync(x => x.Reportado);
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Usuarios(string? buscar, int pagina = 1)
    {
        pagina = Math.Max(1, pagina);
        var consulta = _context.Usuarios.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();
            consulta = consulta.Where(x => x.Nombre.Contains(termino) || x.Apellido.Contains(termino) || x.NombreUsuario.Contains(termino) || x.Email.Contains(termino));
        }
        consulta = consulta.OrderBy(x => x.Apellido).ThenBy(x => x.Nombre);
        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;
        return View(new UsuariosAdminViewModel
        {
            Usuarios = await consulta.Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync(),
            Pagina = pagina, TotalPaginas = totalPaginas, TotalRegistros = total, Buscar = buscar
        });
    }

    [HttpPost]
    public async Task<IActionResult> CambiarRol(int id, string rol)
    {
        if (id == ObtenerIdUsuario())
        {
            TempData["Error"] = "No podés modificar tu propio rol desde esta pantalla.";
            return RedirectToAction(nameof(Usuarios));
        }
        if (rol is not (RolesAplicacion.Administrador or RolesAplicacion.Lector or RolesAplicacion.Escritor))
        {
            TempData["Error"] = "El rol indicado no es válido.";
            return RedirectToAction(nameof(Usuarios));
        }
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
        if (usuario is null) return NotFound();
        usuario.Rol = rol;
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "El rol del usuario fue actualizado.";
        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    public async Task<IActionResult> EliminarUsuario(int id)
    {
        if (id == ObtenerIdUsuario())
        {
            TempData["Error"] = "No podés eliminar la cuenta con la que estás trabajando.";
            return RedirectToAction(nameof(Usuarios));
        }
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
        if (usuario is null) return NotFound();
        var tieneRelaciones = await _context.Libros.AnyAsync(x => x.IdEscritor == id) || await _context.Compras.AnyAsync(x => x.IdLector == id) ||
                              await _context.Alquileres.AnyAsync(x => x.IdLector == id) || await _context.Resenas.AnyAsync(x => x.IdLector == id);
        if (tieneRelaciones)
        {
            TempData["Error"] = "No se puede eliminar una cuenta con libros, compras, alquileres o reseñas relacionados. Cambiá su rol si necesitás restringir su acceso.";
            return RedirectToAction(nameof(Usuarios));
        }
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "La cuenta fue eliminada.";
        return RedirectToAction(nameof(Usuarios));
    }

    [HttpGet]
    public IActionResult CrearAdministrador()
    {
        return View(new Usuario());
    }

    [HttpPost]
    public async Task<IActionResult> CrearAdministrador(Usuario model)
    {
        ModelState.Remove(nameof(Usuario.Rol));
        model.Nombre = model.Nombre.Trim();
        model.Apellido = model.Apellido.Trim();
        model.NombreUsuario = model.NombreUsuario.Trim();
        model.Email = model.Email.Trim();
        model.Dni = model.Dni.Trim();
        model.Telefono = model.Telefono?.Trim();

        if (await _context.Usuarios.AnyAsync(x =>
            x.NombreUsuario == model.NombreUsuario))
        {
            ModelState.AddModelError(
                nameof(model.NombreUsuario),
                "Ese nombre de usuario ya está registrado.");
        }

        if (await _context.Usuarios.AnyAsync(x =>
            x.Email == model.Email))
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "Ese correo electrónico ya está registrado.");
        }

        if (await _context.Usuarios.AnyAsync(x =>
            x.Dni == model.Dni))
        {
            ModelState.AddModelError(
                nameof(model.Dni),
                "Ya existe una cuenta registrada con ese DNI.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Id = 0;
        model.AvatarUrl = null;
        model.Rol = RolesAplicacion.Administrador;
        model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

        _context.Usuarios.Add(model);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "El administrador se creó correctamente.";

        return RedirectToAction(nameof(Usuarios));
    }

    [HttpGet]
    public async Task<IActionResult> ResenasReportadas(int pagina = 1)
    {
        pagina = Math.Max(1, pagina);
        var consulta = _context.Resenas.AsNoTracking().Include(x => x.Libro).Include(x => x.Lector).Where(x => x.Reportado).OrderByDescending(x => x.FechaComentario);
        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;
        return View(new ResenasAdminViewModel
        {
            Resenas = await consulta.Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync(),
            Pagina = pagina, TotalPaginas = totalPaginas, TotalRegistros = total
        });
    }

    [HttpPost]
    public async Task<IActionResult> DesestimarReporte(int id)
    {
        var resena = await _context.Resenas.FirstOrDefaultAsync(x => x.Id == id);
        if (resena is null) return NotFound();
        resena.Reportado = false;
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "El reporte fue desestimado.";
        return RedirectToAction(nameof(ResenasReportadas));
    }

    private int ObtenerIdUsuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
