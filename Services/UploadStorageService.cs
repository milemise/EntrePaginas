using Microsoft.AspNetCore.Http;

namespace EntrePaginas.Services;

public class UploadStorageService
{
    private readonly IWebHostEnvironment _environment;

    public UploadStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> GuardarImagenPublicaAsync(IFormFile archivo, string carpeta, long tamanoMaximoBytes, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (archivo.Length <= 0 || archivo.Length > tamanoMaximoBytes || !extensionesPermitidas.Contains(extension))
            throw new InvalidOperationException("La imagen no es válida o supera el tamaño permitido.");
        if (!await CoincideContenidoAsync(archivo, extension, cancellationToken))
            throw new InvalidOperationException("El contenido del archivo no coincide con una imagen permitida.");

        var nombre = $"{Guid.NewGuid():N}{extension}";
        var carpetaFisica = Path.Combine(_environment.WebRootPath, carpeta);
        Directory.CreateDirectory(carpetaFisica);
        var ruta = Path.Combine(carpetaFisica, nombre);
        await using (var destino = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            await archivo.CopyToAsync(destino, cancellationToken);
        return $"{carpeta.Replace('\\', '/').Trim('/')}/{nombre}";
    }

    public async Task<string> GuardarLibroPrivadoAsync(IFormFile archivo, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var extensionesPermitidas = new[] { ".pdf", ".doc", ".docx" };
        if (archivo.Length <= 0 || archivo.Length > 30 * 1024 * 1024 || !extensionesPermitidas.Contains(extension))
            throw new InvalidOperationException("El archivo debe ser PDF, DOC o DOCX y no superar los 30 MB.");
        if (!await CoincideContenidoAsync(archivo, extension, cancellationToken))
            throw new InvalidOperationException("El contenido del archivo no coincide con su extensión.");

        var nombre = $"{Guid.NewGuid():N}{extension}";
        var carpeta = Path.Combine(_environment.ContentRootPath, "Storage", "Libros");
        Directory.CreateDirectory(carpeta);
        var ruta = Path.Combine(carpeta, nombre);
        await using (var destino = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            await archivo.CopyToAsync(destino, cancellationToken);
        return nombre;
    }

    public string ObtenerRutaLibro(string nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo) || Path.GetFileName(nombreArchivo) != nombreArchivo)
            throw new InvalidOperationException("El archivo solicitado no es válido.");
        return Path.Combine(_environment.ContentRootPath, "Storage", "Libros", nombreArchivo);
    }

    public void EliminarLibroPrivado(string? nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo) || Path.GetFileName(nombreArchivo) != nombreArchivo)
            return;
        var ruta = ObtenerRutaLibro(nombreArchivo);
        if (File.Exists(ruta))
            File.Delete(ruta);
    }

    public void EliminarImagenPublica(string? rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa) || Path.IsPathRooted(rutaRelativa))
            return;
        var rutaNormalizada = rutaRelativa.Replace('\\', '/');
        if (!rutaNormalizada.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase) || rutaNormalizada.Contains("..", StringComparison.Ordinal))
            return;
        var ruta = Path.GetFullPath(Path.Combine(_environment.WebRootPath, rutaNormalizada.Replace('/', Path.DirectorySeparatorChar)));
        var raiz = Path.GetFullPath(_environment.WebRootPath + Path.DirectorySeparatorChar);
        if (ruta.StartsWith(raiz, StringComparison.OrdinalIgnoreCase) && File.Exists(ruta))
            File.Delete(ruta);
    }

    private static async Task<bool> CoincideContenidoAsync(IFormFile archivo, string extension, CancellationToken cancellationToken)
    {
        var encabezado = new byte[12];
        await using var entrada = archivo.OpenReadStream();
        var leidos = await entrada.ReadAsync(encabezado.AsMemory(0, encabezado.Length), cancellationToken);
        if (extension is ".jpg" or ".jpeg")
            return leidos >= 3 && encabezado[0] == 0xFF && encabezado[1] == 0xD8 && encabezado[2] == 0xFF;
        if (extension == ".png")
            return leidos >= 8 && encabezado.Take(8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        if (extension == ".webp")
            return leidos >= 12 && encabezado[0] == 0x52 && encabezado[1] == 0x49 && encabezado[2] == 0x46 && encabezado[3] == 0x46 && encabezado[8] == 0x57 && encabezado[9] == 0x45 && encabezado[10] == 0x42 && encabezado[11] == 0x50;
        if (extension == ".pdf")
            return leidos >= 5 && encabezado[0] == 0x25 && encabezado[1] == 0x50 && encabezado[2] == 0x44 && encabezado[3] == 0x46 && encabezado[4] == 0x2D;
        if (extension == ".doc")
            return leidos >= 8 && encabezado.Take(8).SequenceEqual(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 });
        if (extension == ".docx")
            return leidos >= 4 && encabezado[0] == 0x50 && encabezado[1] == 0x4B && encabezado[2] == 0x03 && encabezado[3] == 0x04;
        return false;
    }
}
