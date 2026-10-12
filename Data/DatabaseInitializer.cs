using EntrePaginas.Models;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Data;

public static class DatabaseInitializer
{
    public static async Task InicializarAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EntrePaginasContext>();
        try
        {
            await context.Database.EnsureCreatedAsync();
            await ActualizarColumnasDeMetadatosAsync(context);
            if (!await context.Usuarios.AnyAsync(x => x.NombreUsuario == "admin"))
            {
                context.Usuarios.Add(new Usuario
                {
                    Nombre = "Administración",
                    Apellido = "Entre Páginas",
                    NombreUsuario = "admin",
                    Email = "admin@entrepaginas.local",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Dni = "90000001",
                    FechaNacimiento = new DateTime(1990, 1, 1),
                    Rol = RolesAplicacion.Administrador
                });
            }

            if (!await context.Usuarios.AnyAsync(x => x.NombreUsuario == "escritor_demo"))
            {
                context.Usuarios.Add(new Usuario
                {
                    Nombre = "Lucía",
                    Apellido = "Paredes",
                    NombreUsuario = "escritor_demo",
                    Email = "escritor@entrepaginas.local",
                    Password = BCrypt.Net.BCrypt.HashPassword("Escritor123!"),
                    Dni = "90000002",
                    FechaNacimiento = new DateTime(1994, 4, 12),
                    Rol = RolesAplicacion.Escritor
                });
            }

            if (!await context.Usuarios.AnyAsync(x => x.NombreUsuario == "lector_demo"))
            {
                context.Usuarios.Add(new Usuario
                {
                    Nombre = "Mateo",
                    Apellido = "Luna",
                    NombreUsuario = "lector_demo",
                    Email = "lector@entrepaginas.local",
                    Password = BCrypt.Net.BCrypt.HashPassword("Lector123!"),
                    Dni = "90000003",
                    FechaNacimiento = new DateTime(2000, 7, 7),
                    Rol = RolesAplicacion.Lector
                });
            }

            await context.SaveChangesAsync();

            var nombresCategorias = new[] { "Fantasía", "Literatura infantil", "Historia", "Romance", "Ciencia ficción", "Misterio" };
            foreach (var nombreCategoria in nombresCategorias)
            {
                if (!await context.Categorias.AnyAsync(x => x.Nombre == nombreCategoria))
                    context.Categorias.Add(new Categoria { Nombre = nombreCategoria });
            }
            await context.SaveChangesAsync();

            if (!await context.Libros.AnyAsync())
            {
                var escritor = await context.Usuarios.SingleAsync(x => x.NombreUsuario == "escritor_demo");
                var categorias = await context.Categorias.ToDictionaryAsync(x => x.Nombre, x => x.Id);
                context.Libros.AddRange(
                    new Libro
                    {
                        Titulo = "Las aventuras de Pinocho",
                        Autor = "Carlo Collodi",
                        Descripcion = "Un clásico de la literatura infantil que acompaña el viaje de Pinocho para convertirse en un niño de verdad. Este archivo es una muestra de lectura de prueba del sistema.",
                        PortadaUrl = "images/portada1.jpg",
                        ArchivoUrl = "muestra-pinocho.pdf",
                        PrecioCompra = 3500,
                        PrecioAlquiler = 700,
                        IdCategoria = categorias["Literatura infantil"],
                        IdEscritor = escritor.Id
                    },
                    new Libro
                    {
                        Titulo = "Mamá nos dijo adiós",
                        Autor = "Lucía Paredes (obra de muestra)",
                        Descripcion = "Una historia sobre los vínculos familiares, la memoria y los viajes que marcan una vida. El archivo adjunto es una muestra de lectura de prueba.",
                        PortadaUrl = "images/portada2.jpg",
                        ArchivoUrl = "muestra-mama.pdf",
                        PrecioCompra = 5200,
                        PrecioAlquiler = 900,
                        IdCategoria = categorias["Historia"],
                        IdEscritor = escritor.Id
                    },
                    new Libro
                    {
                        Titulo = "Harry Potter y la piedra filosofal",
                        Autor = "J. K. Rowling",
                        Descripcion = "La historia de un joven que descubre un mundo mágico y comienza una aventura inolvidable. El archivo adjunto es una muestra de lectura de prueba.",
                        PortadaUrl = "images/portada3.jpg",
                        ArchivoUrl = "muestra-harry-potter.pdf",
                        PrecioCompra = 6500,
                        PrecioAlquiler = 1100,
                        IdCategoria = categorias["Fantasía"],
                        IdEscritor = escritor.Id
                    });
                await context.SaveChangesAsync();
            }

            logger.LogInformation("Base de datos de Entre Páginas inicializada.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "No se pudo inicializar la base de datos. Verificá la conexión y que la base entre_paginas exista.");
            throw;
        }
    }
    private static async Task ActualizarColumnasDeMetadatosAsync(EntrePaginasContext context)
    {
        var conexion = context.Database.GetDbConnection();
        var debeCerrar = conexion.State != System.Data.ConnectionState.Open;
        if (debeCerrar) await conexion.OpenAsync();
        try
        {
            foreach (var columna in new[] { (Nombre: "autor", Definicion: "VARCHAR(200) NULL"), (Nombre: "open_library_key", Definicion: "VARCHAR(100) NULL") })
            {
                await using var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'libros' AND COLUMN_NAME = @columna";
                var parametro = comando.CreateParameter();
                parametro.ParameterName = "@columna";
                parametro.Value = columna.Nombre;
                comando.Parameters.Add(parametro);
                var existe = Convert.ToInt32(await comando.ExecuteScalarAsync()) > 0;
                if (!existe)
                {
                    await context.Database.ExecuteSqlRawAsync($"ALTER TABLE libros ADD COLUMN {columna.Nombre} {columna.Definicion}");
                }
            }
        }
        finally
        {
            if (debeCerrar) await conexion.CloseAsync();
        }
    }

}
