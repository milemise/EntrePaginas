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
public class LibrosController : Controller
{
    private const int TamanoPagina = 12;
    private readonly EntrePaginasContext _context;
    private readonly UploadStorageService _storage;
    private readonly OpenLibraryService _openLibrary;

    public LibrosController(EntrePaginasContext context, UploadStorageService storage, OpenLibraryService openLibrary)
    {
        _context = context;
        _storage = storage;
        _openLibrary = openLibrary;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(string? buscar, int? categoriaId, string? ordenar, int pagina = 1)
    {
        pagina = Math.Max(1, pagina);
        var consulta = _context.Libros.AsNoTracking().Include(x => x.Categoria).Include(x => x.Escritor).AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();
            consulta = consulta.Where(x => x.Titulo.Contains(termino) || (x.Descripcion != null && x.Descripcion.Contains(termino)) || x.Escritor.Nombre.Contains(termino) || x.Escritor.Apellido.Contains(termino));
        }
        if (categoriaId.HasValue) consulta = consulta.Where(x => x.IdCategoria == categoriaId.Value);
        consulta = ordenar switch
        {
            "precio-menor" => consulta.OrderBy(x => x.PrecioCompra).ThenBy(x => x.Titulo),
            "precio-mayor" => consulta.OrderByDescending(x => x.PrecioCompra).ThenBy(x => x.Titulo),
            "titulo" => consulta.OrderBy(x => x.Titulo),
            "recientes" => consulta.OrderByDescending(x => x.Id),
            "puntuacion" => consulta.OrderByDescending(x => x.Resenas.Average(r => (double?)r.Puntuacion) ?? 0),
            _ => consulta.OrderBy(x => x.Titulo)
        };
        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;
        var libros = await consulta.Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync();
        var idsLibros = libros.Select(x => x.Id).ToArray();
        var estadisticas = await _context.Resenas.AsNoTracking().Where(x => idsLibros.Contains(x.IdLibro))
            .GroupBy(x => x.IdLibro).Select(g => new { IdLibro = g.Key, Cantidad = g.Count(), Promedio = g.Average(r => (double)r.Puntuacion) }).ToListAsync();
        var resumen = estadisticas.ToDictionary(x => x.IdLibro, x => new LibroResenasResumen { Cantidad = x.Cantidad, Promedio = x.Promedio });
        var categorias = await _context.Categorias.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        return View(new LibroListadoViewModel
        {
            Libros = libros, Categorias = categorias, Pagina = pagina, TotalPaginas = totalPaginas,
            TotalRegistros = total, Buscar = buscar, CategoriaId = categoriaId, Ordenar = ordenar, ResumenResenas = resumen
        });
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Detalle(int id, int paginaResenas = 1)
    {
        var libro = await _context.Libros.AsNoTracking()
            .Include(x => x.Categoria).Include(x => x.Escritor).Include(x => x.LibroAnterior)
            .Include(x => x.LibrosSiguientes)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (libro is null) return NotFound();

        var totalResenas = await _context.Resenas.CountAsync(x => x.IdLibro == id);
        var promedioResenas = await _context.Resenas.Where(x => x.IdLibro == id).AverageAsync(x => (double?)x.Puntuacion) ?? 0;
        var totalPaginasResenas = Math.Max(1, (int)Math.Ceiling(totalResenas / 5.0));
        paginaResenas = Math.Clamp(paginaResenas, 1, totalPaginasResenas);
        libro.Resenas = await _context.Resenas.AsNoTracking().Include(x => x.Lector).Where(x => x.IdLibro == id)
            .OrderByDescending(x => x.FechaComentario).Skip((paginaResenas - 1) * 5).Take(5).ToListAsync();

        var bloqueado = false;
        var motivo = (string?)null;
        var compraVigente = false;
        var alquilerVigente = false;
        var yaCompro = false;
        var puedeResenar = false;
        int? miResenaId = null;
        if (User.Identity?.IsAuthenticated == true && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
        {
            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == usuarioId);
            if (usuario is null) return Challenge();
            if (libro.EsMas18 && CalcularEdad(usuario.FechaNacimiento) < 18)
            {
                bloqueado = true;
                motivo = "Este libro está marcado como +18. Tu cuenta no cumple con la edad mínima para acceder al contenido.";
            }
            compraVigente = await _context.Compras.AnyAsync(x => x.IdLibro == id && x.IdLector == usuarioId && !x.Devuelto);
            alquilerVigente = await _context.Alquileres.AnyAsync(x => x.IdLibro == id && x.IdLector == usuarioId && x.Activo && x.FechaInicio <= DateTime.Now && x.FechaHasta >= DateTime.Now);
            yaCompro = compraVigente;
            puedeResenar = User.IsInRole(RolesAplicacion.Lector) &&
                (await _context.Compras.AnyAsync(x => x.IdLibro == id && x.IdLector == usuarioId) ||
                 await _context.Alquileres.AnyAsync(x => x.IdLibro == id && x.IdLector == usuarioId));
            miResenaId = (await _context.Resenas.AsNoTracking().FirstOrDefaultAsync(x => x.IdLibro == id && x.IdLector == usuarioId))?.Id;
        }

        return View(new LibroDetalleViewModel
        {
            Libro = libro, ContenidoBloqueado = bloqueado, MotivoBloqueo = motivo,
            PuedeComprar = User.IsInRole(RolesAplicacion.Lector) && !bloqueado && !compraVigente,
            PuedeAlquilar = User.IsInRole(RolesAplicacion.Lector) && !bloqueado && !alquilerVigente && !compraVigente,
            PuedeResenar = puedeResenar, YaCompro = yaCompro, AlquilerVigente = alquilerVigente,
            CompraVigente = compraVigente, MiResenaId = miResenaId, PaginaResenas = paginaResenas,
            TotalPaginasResenas = totalPaginasResenas, TotalResenas = totalResenas, PromedioResenas = promedioResenas
        });
    }

    [Authorize(Roles = RolesAplicacion.Escritor)]
    [HttpGet]
    public async Task<IActionResult> MisLibros(int pagina = 1)
    {
        pagina = Math.Max(1, pagina);
        var idUsuario = ObtenerIdUsuario();
        var consulta = _context.Libros.AsNoTracking().Include(x => x.Categoria).Where(x => x.IdEscritor == idUsuario).OrderByDescending(x => x.Id);
        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;
        var libros = await consulta.Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync();
        return View(new LibroListadoViewModel { Libros = libros, Pagina = pagina, TotalPaginas = totalPaginas, TotalRegistros = total });
    }

    [Authorize(Roles = RolesAplicacion.Escritor)]
    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        ViewBag.Categorias = await _context.Categorias.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        return View("Formulario", new LibroFormViewModel());
    }

    [Authorize(Roles = RolesAplicacion.Escritor)]
    [HttpPost]
    [RequestSizeLimit(35 * 1024 * 1024)]
    public async Task<IActionResult> Crear(LibroFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Archivo is null || model.Archivo.Length == 0)
            ModelState.AddModelError(nameof(model.Archivo), "Adjuntá el archivo del libro en PDF, DOC o DOCX.");
        if (model.IdCategoria.HasValue && !await _context.Categorias.AnyAsync(x => x.Id == model.IdCategoria.Value, cancellationToken))
            ModelState.AddModelError(nameof(model.IdCategoria), "La categoría seleccionada no existe.");
        if (model.IdLibroAnterior.HasValue && !await _context.Libros.AnyAsync(x => x.Id == model.IdLibroAnterior.Value && x.IdEscritor == ObtenerIdUsuario(), cancellationToken))
            ModelState.AddModelError(nameof(model.IdLibroAnterior), "Solo podés relacionar la saga con una obra propia existente.");
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _context.Categorias.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
            return View("Formulario", model);
        }

        string? archivoGuardado = null;
        string? portadaGuardada = null;
        try
        {
            archivoGuardado = await _storage.GuardarLibroPrivadoAsync(model.Archivo!, cancellationToken);
            if (model.Portada is { Length: > 0 }) portadaGuardada = await _storage.GuardarImagenPublicaAsync(model.Portada, "uploads/portadas", 5 * 1024 * 1024, cancellationToken);
            var libro = new Libro
            {
                Titulo = model.Titulo.Trim(), Autor = model.Autor?.Trim(), OpenLibraryKey = model.OpenLibraryKey?.Trim(), Descripcion = model.Descripcion?.Trim(),
                PortadaUrl = portadaGuardada ?? NormalizarPortadaExterna(model.PortadaUrl), ArchivoUrl = archivoGuardado,
                PrecioCompra = model.PrecioCompra, PrecioAlquiler = model.PrecioAlquiler,
                EsMas18 = model.EsMas18, IdCategoria = model.IdCategoria!.Value,
                IdEscritor = ObtenerIdUsuario(), IdLibroAnterior = model.IdLibroAnterior
            };
            _context.Libros.Add(libro);
            await _context.SaveChangesAsync(cancellationToken);
            TempData["Mensaje"] = "El libro se publicó correctamente.";
            return RedirectToAction(nameof(MisLibros));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "No se pudo guardar el libro. Verificá los datos e intentá nuevamente.");
        }
        finally
        {
            if (ModelState.ErrorCount > 0)
            {
                _storage.EliminarLibroPrivado(archivoGuardado);
                _storage.EliminarImagenPublica(portadaGuardada);
            }
        }
        ViewBag.Categorias = await _context.Categorias.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
        return View("Formulario", model);
    }

    [Authorize(Roles = RolesAplicacion.Escritor)]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var libro = await _context.Libros.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.IdEscritor == ObtenerIdUsuario());
        if (libro is null) return NotFound();
        ViewBag.Categorias = await _context.Categorias.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.TituloAnterior = libro.IdLibroAnterior.HasValue ? await _context.Libros.Where(x => x.Id == libro.IdLibroAnterior).Select(x => x.Titulo).FirstOrDefaultAsync() : null;
        return View("Formulario", new LibroFormViewModel
        {
            Id = libro.Id, Titulo = libro.Titulo, Autor = libro.Autor, OpenLibraryKey = libro.OpenLibraryKey, Descripcion = libro.Descripcion,
            PrecioCompra = libro.PrecioCompra, PrecioAlquiler = libro.PrecioAlquiler,
            EsMas18 = libro.EsMas18, IdCategoria = libro.IdCategoria,
            IdLibroAnterior = libro.IdLibroAnterior, PortadaUrl = libro.PortadaUrl
        });
    }

    [Authorize(Roles = RolesAplicacion.Escritor)]
    [HttpPost]
    [RequestSizeLimit(35 * 1024 * 1024)]
    public async Task<IActionResult> Editar(int id, LibroFormViewModel model, CancellationToken cancellationToken)
    {
        var libro = await _context.Libros.FirstOrDefaultAsync(x => x.Id == id && x.IdEscritor == ObtenerIdUsuario(), cancellationToken);
        if (libro is null) return NotFound();
        if (model.IdCategoria.HasValue && !await _context.Categorias.AnyAsync(x => x.Id == model.IdCategoria.Value, cancellationToken))
            ModelState.AddModelError(nameof(model.IdCategoria), "La categoría seleccionada no existe.");
        if (model.IdLibroAnterior == id)
            ModelState.AddModelError(nameof(model.IdLibroAnterior), "Un libro no puede relacionarse consigo mismo.");
        if (model.IdLibroAnterior.HasValue && !await _context.Libros.AnyAsync(x => x.Id == model.IdLibroAnterior.Value && x.Id != id && x.IdEscritor == ObtenerIdUsuario(), cancellationToken))
            ModelState.AddModelError(nameof(model.IdLibroAnterior), "Solo podés relacionar la saga con otra obra propia existente.");
        if (model.IdLibroAnterior.HasValue && await GeneraCicloDeSagaAsync(id, model.IdLibroAnterior.Value, cancellationToken))
            ModelState.AddModelError(nameof(model.IdLibroAnterior), "Esa relación generaría un ciclo en la saga. Elegí una entrega anterior diferente.");
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _context.Categorias.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
            ViewBag.TituloAnterior = model.LibroAnteriorTitulo;
            return View("Formulario", model);
        }

        var archivoAnterior = libro.ArchivoUrl;
        var portadaAnterior = libro.PortadaUrl;
        string? archivoNuevo = null;
        string? portadaNueva = null;
        try
        {
            if (model.Archivo is { Length: > 0 }) archivoNuevo = await _storage.GuardarLibroPrivadoAsync(model.Archivo, cancellationToken);
            if (model.Portada is { Length: > 0 }) portadaNueva = await _storage.GuardarImagenPublicaAsync(model.Portada, "uploads/portadas", 5 * 1024 * 1024, cancellationToken);
            libro.Titulo = model.Titulo.Trim();
            libro.Autor = model.Autor?.Trim();
            libro.OpenLibraryKey = model.OpenLibraryKey?.Trim();
            libro.Descripcion = model.Descripcion?.Trim();
            libro.PrecioCompra = model.PrecioCompra;
            libro.PrecioAlquiler = model.PrecioAlquiler;
            libro.EsMas18 = model.EsMas18;
            libro.IdCategoria = model.IdCategoria!.Value;
            libro.IdLibroAnterior = model.IdLibroAnterior;
            if (archivoNuevo is not null) libro.ArchivoUrl = archivoNuevo;
            if (portadaNueva is not null) libro.PortadaUrl = portadaNueva;
            else if (!string.IsNullOrWhiteSpace(model.PortadaUrl)) libro.PortadaUrl = NormalizarPortadaExterna(model.PortadaUrl);
            await _context.SaveChangesAsync(cancellationToken);
            if (archivoNuevo is not null) _storage.EliminarLibroPrivado(archivoAnterior);
            if (!string.Equals(portadaAnterior, libro.PortadaUrl, StringComparison.Ordinal) && portadaAnterior is not null) _storage.EliminarImagenPublica(portadaAnterior);
            TempData["Mensaje"] = "Los cambios se guardaron correctamente.";
            return RedirectToAction(nameof(MisLibros));
        }
        catch (InvalidOperationException ex)
        {
            if (archivoNuevo is not null) _storage.EliminarLibroPrivado(archivoNuevo);
            _storage.EliminarImagenPublica(portadaNueva);
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (DbUpdateException)
        {
            if (archivoNuevo is not null) _storage.EliminarLibroPrivado(archivoNuevo);
            _storage.EliminarImagenPublica(portadaNueva);
            ModelState.AddModelError(string.Empty, "No se pudieron guardar los cambios.");
        }
        ViewBag.Categorias = await _context.Categorias.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
        return View("Formulario", model);
    }

    [Authorize(Roles = RolesAplicacion.Escritor)]
    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var libro = await _context.Libros.Include(x => x.LibrosSiguientes).FirstOrDefaultAsync(x => x.Id == id && x.IdEscritor == ObtenerIdUsuario());
        if (libro is null) return NotFound();
        if (await _context.Compras.AnyAsync(x => x.IdLibro == id) || await _context.Alquileres.AnyAsync(x => x.IdLibro == id) || await _context.Resenas.AnyAsync(x => x.IdLibro == id))
        {
            TempData["Error"] = "No se puede eliminar una obra que ya tiene compras, alquileres o reseñas asociadas, para conservar su historial.";
            return RedirectToAction(nameof(MisLibros));
        }
        _context.Libros.Remove(libro);
        await _context.SaveChangesAsync();
        _storage.EliminarLibroPrivado(libro.ArchivoUrl);
        _storage.EliminarImagenPublica(libro.PortadaUrl);
        TempData["Mensaje"] = "El libro se eliminó correctamente.";
        return RedirectToAction(nameof(MisLibros));
    }

    [Authorize(Roles = RolesAplicacion.Escritor)]
    [HttpGet]
    public async Task<IActionResult> BuscarRelacionados(string? termino, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(termino) || termino.Trim().Length < 2) return Json(new { items = Array.Empty<object>() });
        var idUsuario = ObtenerIdUsuario();
        var items = await _context.Libros.AsNoTracking().Where(x => x.IdEscritor == idUsuario && x.Titulo.Contains(termino.Trim()))
            .OrderBy(x => x.Titulo).Take(10).Select(x => new { id = x.Id, titulo = x.Titulo }).ToListAsync(cancellationToken);
        return Json(new { items });
    }

    [Authorize(Roles = RolesAplicacion.Escritor)]
    [HttpGet]
    public async Task<IActionResult> BuscarOpenLibrary(string? termino, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(termino) || termino.Trim().Length < 2) return Json(new { items = Array.Empty<object>() });
        try
        {
            var items = await _openLibrary.BuscarAsync(termino, 8, cancellationToken);
            return Json(new { items });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { mensaje = "Open Library no está disponible en este momento. Podés completar los datos manualmente." });
        }
    }

    [Authorize(Roles = RolesAplicacion.Lector)]
    [HttpPost]
    public async Task<IActionResult> Comprar(int id)
    {
        var usuarioId = ObtenerIdUsuario();
        var libro = await _context.Libros.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (libro is null) return NotFound();
        if (await EdadInsuficienteAsync(usuarioId, libro))
        {
            TempData["Error"] = "Este libro es +18 y tu cuenta no cumple la edad mínima.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        if (await _context.Compras.AnyAsync(x => x.IdLibro == id && x.IdLector == usuarioId && !x.Devuelto))
        {
            TempData["Error"] = "Ya tenés este libro en tu biblioteca.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        _context.Compras.Add(new Compra { IdLibro = id, IdLector = usuarioId, FechaCompra = DateTime.Now, Importe = libro.PrecioCompra });
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "Compra registrada. El libro ya está disponible en tu biblioteca.";
        return RedirectToAction("Compras", "Biblioteca");
    }

    [Authorize(Roles = RolesAplicacion.Lector)]
    [HttpPost]
    public async Task<IActionResult> Alquilar(int id, int dias = 7)
    {
        var usuarioId = ObtenerIdUsuario();
        if (dias is not (7 or 15 or 30)) dias = 7;
        var libro = await _context.Libros.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (libro is null) return NotFound();
        if (await EdadInsuficienteAsync(usuarioId, libro))
        {
            TempData["Error"] = "Este libro es +18 y tu cuenta no cumple la edad mínima.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        if (await _context.Compras.AnyAsync(x => x.IdLibro == id && x.IdLector == usuarioId && !x.Devuelto))
        {
            TempData["Error"] = "Ya compraste este libro y está disponible en tu biblioteca.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        if (await _context.Alquileres.AnyAsync(x => x.IdLibro == id && x.IdLector == usuarioId && x.Activo && x.FechaHasta >= DateTime.Now))
        {
            TempData["Error"] = "Ya tenés un alquiler vigente de este libro.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        _context.Alquileres.Add(new Alquiler
        {
            IdLibro = id, IdLector = usuarioId, FechaInicio = DateTime.Now,
            FechaHasta = DateTime.Now.AddDays(dias), Importe = libro.PrecioAlquiler, Activo = true
        });
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = $"Alquiler registrado por {dias} días. Podés leerlo online mientras esté vigente.";
        return RedirectToAction("Alquileres", "Biblioteca");
    }

    [Authorize(Roles = RolesAplicacion.Lector)]
    [HttpPost]
    public async Task<IActionResult> DevolverCompra(int id)
    {
        var usuarioId = ObtenerIdUsuario();
        var compra = await _context.Compras.FirstOrDefaultAsync(x => x.Id == id && x.IdLector == usuarioId && !x.Devuelto);
        if (compra is null) return NotFound();
        if (DateTime.Now > compra.FechaCompra.AddDays(7))
        {
            TempData["Error"] = "El plazo de devolución de 7 días ya venció.";
            return RedirectToAction("Compras", "Biblioteca");
        }
        compra.Devuelto = true;
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "La devolución se registró correctamente. Ya no tenés acceso al archivo de esta compra.";
        return RedirectToAction("Compras", "Biblioteca");
    }

    [Authorize(Roles = RolesAplicacion.Lector)]
    [HttpGet]
    public async Task<IActionResult> Descargar(int id)
    {
        var usuarioId = ObtenerIdUsuario();
        var libro = await _context.Libros.FirstOrDefaultAsync(x => x.Id == id);
        if (libro is null) return NotFound();
        if (await EdadInsuficienteAsync(usuarioId, libro)) return Forbid();
        var comprado = await _context.Compras.AnyAsync(x => x.IdLibro == id && x.IdLector == usuarioId && !x.Devuelto);
        if (!comprado) return Forbid();
        if (string.IsNullOrWhiteSpace(libro.ArchivoUrl)) return NotFound("El archivo de este libro todavía no está cargado.");
        var ruta = _storage.ObtenerRutaLibro(libro.ArchivoUrl);
        if (!System.IO.File.Exists(ruta)) return NotFound("El archivo no está disponible en el servidor.");
        var mime = Path.GetExtension(ruta).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf", ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document", _ => "application/octet-stream"
        };
        return PhysicalFile(ruta, mime, $"{NombreArchivoSeguro(libro.Titulo)}{Path.GetExtension(ruta)}");
    }

    [Authorize(Roles = RolesAplicacion.Lector)]
    [HttpGet]
    public async Task<IActionResult> Leer(int id)
    {
        var usuarioId = ObtenerIdUsuario();
        var libro = await _context.Libros.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (libro is null) return NotFound();
        if (await EdadInsuficienteAsync(usuarioId, libro)) return Forbid();
        var comprado = await _context.Compras.AnyAsync(x => x.IdLibro == id && x.IdLector == usuarioId && !x.Devuelto);
        var alquilado = await _context.Alquileres.AnyAsync(x => x.IdLibro == id && x.IdLector == usuarioId && x.Activo && x.FechaInicio <= DateTime.Now && x.FechaHasta >= DateTime.Now);
        if (!comprado && !alquilado) return Forbid();
        if (string.IsNullOrWhiteSpace(libro.ArchivoUrl)) return NotFound("El archivo todavía no está cargado.");
        var extension = Path.GetExtension(libro.ArchivoUrl).ToLowerInvariant();
        if (extension != ".pdf")
        {
            TempData["Error"] = comprado ? "La lectura online del navegador está disponible para PDF. Como compraste el libro, podés obtener el archivo desde Descargar." : "La lectura online del navegador está disponible para PDF. El alquiler no habilita descargar documentos DOC/DOCX.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        var ruta = _storage.ObtenerRutaLibro(libro.ArchivoUrl);
        if (!System.IO.File.Exists(ruta)) return NotFound("El archivo no está disponible en el servidor.");
        return PhysicalFile(ruta, "application/pdf", enableRangeProcessing: true);
    }


    private async Task<bool> GeneraCicloDeSagaAsync(int idLibroActual, int idLibroAnterior, CancellationToken cancellationToken)
    {
        var visitados = new HashSet<int>();
        int? idActual = idLibroAnterior;
        while (idActual.HasValue)
        {
            if (idActual.Value == idLibroActual || !visitados.Add(idActual.Value)) return true;
            idActual = await _context.Libros.AsNoTracking()
                .Where(x => x.Id == idActual.Value)
                .Select(x => x.IdLibroAnterior)
                .FirstOrDefaultAsync(cancellationToken);
        }
        return false;
    }

    private int ObtenerIdUsuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    private async Task<bool> EdadInsuficienteAsync(int usuarioId, Libro libro)
    {
        if (!libro.EsMas18) return false;
        var nacimiento = await _context.Usuarios.AsNoTracking().Where(x => x.Id == usuarioId).Select(x => (DateTime?)x.FechaNacimiento).FirstOrDefaultAsync();
        return !nacimiento.HasValue || CalcularEdad(nacimiento.Value) < 18;
    }

    private static int CalcularEdad(DateTime nacimiento)
    {
        var edad = DateTime.Today.Year - nacimiento.Year;
        if (nacimiento.Date > DateTime.Today.AddYears(-edad)) edad--;
        return edad;
    }

    private static string? NormalizarPortadaExterna(string? portadaUrl)
    {
        if (string.IsNullOrWhiteSpace(portadaUrl)) return null;
        return Uri.TryCreate(portadaUrl, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps && uri.Host.Equals("covers.openlibrary.org", StringComparison.OrdinalIgnoreCase)
            ? uri.ToString() : !portadaUrl.Contains("..", StringComparison.Ordinal) && (portadaUrl.StartsWith("images/", StringComparison.Ordinal) || portadaUrl.StartsWith("uploads/portadas/", StringComparison.Ordinal)) ? portadaUrl : null;
    }

    private static string NombreArchivoSeguro(string titulo)
    {
        var caracteresInvalidos = Path.GetInvalidFileNameChars();
        var limpio = new string(titulo.Where(c => !caracteresInvalidos.Contains(c)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(limpio) ? "libro" : limpio;
    }
}
