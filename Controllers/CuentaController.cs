using System.Security.Claims;
using EntrePaginas.Data;
using EntrePaginas.Models;
using EntrePaginas.Services;
using EntrePaginas.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Controllers;

[AutoValidateAntiforgeryToken]
public class CuentaController : Controller
{
    private readonly EntrePaginasContext _context;
    private readonly UploadStorageService _storage;

    public CuentaController(EntrePaginasContext context, UploadStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);
        var identificador = model.Usuario.Trim();
        var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.NombreUsuario == identificador || x.Email == identificador);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.Password))
        {
            ModelState.AddModelError(string.Empty, "El usuario/correo o la contraseña no son correctos.");
            return View(model);
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            CrearIdentidad(usuario), new AuthenticationProperties
            {
                IsPersistent = model.Recordarme,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.Recordarme ? 72 : 8)
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Registro() => View(new RegistroViewModel { FechaNacimiento = new DateTime(2000, 1, 1) });

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        if (model.FechaNacimiento.Date < new DateTime(1900, 1, 1) || model.FechaNacimiento.Date > DateTime.Today)
            ModelState.AddModelError(nameof(model.FechaNacimiento), "Ingresá una fecha de nacimiento válida.");
        var usuarioRepetido = await _context.Usuarios.AnyAsync(x => x.NombreUsuario == model.NombreUsuario || x.Email == model.Email || x.Dni == model.Dni);
        if (usuarioRepetido)
            ModelState.AddModelError(string.Empty, "Ya existe una cuenta con ese nombre de usuario, correo electrónico o DNI.");
        if (!ModelState.IsValid) return View(model);

        var nuevo = new Usuario
        {
            Nombre = model.Nombre.Trim(),
            Apellido = model.Apellido.Trim(),
            NombreUsuario = model.NombreUsuario.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            Dni = model.Dni.Trim(),
            Telefono = model.Telefono?.Trim(),
            FechaNacimiento = model.FechaNacimiento.Date,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Rol = model.TipoCuenta == RolesAplicacion.Escritor ? RolesAplicacion.Escritor : RolesAplicacion.Lector
        };
        _context.Usuarios.Add(nuevo);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "No se pudo crear la cuenta. Revisá que el usuario, correo y DNI no estén registrados.");
            return View(model);
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, CrearIdentidad(nuevo));
        TempData["Mensaje"] = "Tu cuenta ya está lista. ¡Bienvenido/a a Entre Páginas!";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Perfil()
    {
        var id = ObtenerIdUsuario();
        var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (usuario is null) return Challenge();
        return View(new PerfilViewModel
        {
            Id = usuario.Id, Nombre = usuario.Nombre, Apellido = usuario.Apellido,
            NombreUsuario = usuario.NombreUsuario, Email = usuario.Email,
            Rol = usuario.Rol, AvatarUrl = usuario.AvatarUrl, Telefono = usuario.Telefono
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Perfil(PerfilViewModel model, CancellationToken cancellationToken)
    {
        var id = ObtenerIdUsuario();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (usuario is null) return Challenge();
        model.Id = usuario.Id;
        model.Rol = usuario.Rol;
        model.AvatarUrl = usuario.AvatarUrl;

        if (await _context.Usuarios.AnyAsync(x => x.Id != id && (x.NombreUsuario == model.NombreUsuario || x.Email == model.Email), cancellationToken))
            ModelState.AddModelError(string.Empty, "Ese nombre de usuario o correo ya está registrado.");

        if (!ModelState.IsValid) return View(model);

        string? avatarNuevo = null;
        try
        {
            if (model.Avatar is { Length: > 0 })
                avatarNuevo = await _storage.GuardarImagenPublicaAsync(model.Avatar, "uploads/avatares", 4 * 1024 * 1024, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Avatar), ex.Message);
            return View(model);
        }

        var avatarAnterior = usuario.AvatarUrl;
        usuario.Nombre = model.Nombre.Trim();
        usuario.Apellido = model.Apellido.Trim();
        usuario.NombreUsuario = model.NombreUsuario.Trim();
        usuario.Email = model.Email.Trim().ToLowerInvariant();
        usuario.Telefono = model.Telefono?.Trim();
        if (avatarNuevo is not null) usuario.AvatarUrl = avatarNuevo;
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _storage.EliminarImagenPublica(avatarNuevo);
            ModelState.AddModelError(string.Empty, "No se pudieron guardar los cambios. Revisá que el usuario y correo no estén usados por otra cuenta.");
            return View(model);
        }
        if (avatarNuevo is not null) _storage.EliminarImagenPublica(avatarAnterior);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, CrearIdentidad(usuario));
        TempData["Mensaje"] = "Perfil actualizado correctamente.";
        return RedirectToAction(nameof(Perfil));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccesoDenegado() => View();

    private int ObtenerIdUsuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    private static ClaimsPrincipal CrearIdentidad(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol),
            new("nombre_completo", $"{usuario.Nombre} {usuario.Apellido}"),
            new("avatar_url", usuario.AvatarUrl ?? string.Empty)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
