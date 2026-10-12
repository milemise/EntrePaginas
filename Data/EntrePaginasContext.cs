using EntrePaginas.Models;
using Microsoft.EntityFrameworkCore;

namespace EntrePaginas.Data;

public class EntrePaginasContext : DbContext
{
    public EntrePaginasContext(DbContextOptions<EntrePaginasContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Libro> Libros => Set<Libro>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<Alquiler> Alquileres => Set<Alquiler>();
    public DbSet<Resena> Resenas => Set<Resena>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Apellido).HasColumnName("apellido").HasMaxLength(100).IsRequired();
            entity.Property(x => x.NombreUsuario).HasColumnName("nombre_usuario").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Password).HasColumnName("password").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Dni).HasColumnName("dni").HasMaxLength(20).IsRequired();
            entity.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(30);
            entity.Property(x => x.FechaNacimiento).HasColumnName("fecha_nacimiento").HasColumnType("date");
            entity.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500);
            entity.Property(x => x.Rol).HasColumnName("rol").HasMaxLength(30).IsRequired();
            entity.HasIndex(x => x.NombreUsuario).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.Dni).IsUnique();
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("categorias");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Libro>(entity =>
        {
            entity.ToTable("libros");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(200).IsRequired();
            entity.Property(x => x.Autor).HasColumnName("autor").HasMaxLength(200);
            entity.Property(x => x.OpenLibraryKey).HasColumnName("open_library_key").HasMaxLength(100);
            entity.Property(x => x.Descripcion).HasColumnName("descripcion").HasColumnType("text");
            entity.Property(x => x.PortadaUrl).HasColumnName("portada_url").HasMaxLength(500);
            entity.Property(x => x.ArchivoUrl).HasColumnName("archivo_url").HasMaxLength(500);
            entity.Property(x => x.PrecioCompra).HasColumnName("precio_compra").HasPrecision(10, 2);
            entity.Property(x => x.PrecioAlquiler).HasColumnName("precio_alquiler").HasPrecision(10, 2);
            entity.Property(x => x.EsMas18).HasColumnName("es_mas_18");
            entity.Property(x => x.IdCategoria).HasColumnName("id_categoria");
            entity.Property(x => x.IdEscritor).HasColumnName("id_escritor");
            entity.Property(x => x.IdLibroAnterior).HasColumnName("id_libro_anterior");
            entity.HasOne(x => x.Categoria).WithMany(x => x.Libros).HasForeignKey(x => x.IdCategoria).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Escritor).WithMany(x => x.LibrosEscritos).HasForeignKey(x => x.IdEscritor).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LibroAnterior).WithMany(x => x.LibrosSiguientes).HasForeignKey(x => x.IdLibroAnterior).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.ToTable("compras");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.IdLibro).HasColumnName("id_libro");
            entity.Property(x => x.IdLector).HasColumnName("id_lector");
            entity.Property(x => x.FechaCompra).HasColumnName("fecha_compra").HasColumnType("datetime");
            entity.Property(x => x.Importe).HasColumnName("importe").HasPrecision(10, 2);
            entity.Property(x => x.Devuelto).HasColumnName("devuelto");
            entity.HasOne(x => x.Libro).WithMany(x => x.Compras).HasForeignKey(x => x.IdLibro).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Lector).WithMany(x => x.Compras).HasForeignKey(x => x.IdLector).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Alquiler>(entity =>
        {
            entity.ToTable("alquileres");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.IdLibro).HasColumnName("id_libro");
            entity.Property(x => x.IdLector).HasColumnName("id_lector");
            entity.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("datetime");
            entity.Property(x => x.FechaHasta).HasColumnName("fecha_hasta").HasColumnType("datetime");
            entity.Property(x => x.Importe).HasColumnName("importe").HasPrecision(10, 2);
            entity.Property(x => x.Activo).HasColumnName("activo");
            entity.HasOne(x => x.Libro).WithMany(x => x.Alquileres).HasForeignKey(x => x.IdLibro).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Lector).WithMany(x => x.Alquileres).HasForeignKey(x => x.IdLector).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Resena>(entity =>
        {
            entity.ToTable("resenas");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.IdLibro).HasColumnName("id_libro");
            entity.Property(x => x.IdLector).HasColumnName("id_lector");
            entity.Property(x => x.Puntuacion).HasColumnName("puntuacion");
            entity.Property(x => x.Comentario).HasColumnName("comentario").HasColumnType("text");
            entity.Property(x => x.FechaComentario).HasColumnName("fecha_comentario").HasColumnType("datetime");
            entity.Property(x => x.Anonimo).HasColumnName("anonimo");
            entity.Property(x => x.Reportado).HasColumnName("reportado");
            entity.HasOne(x => x.Libro).WithMany(x => x.Resenas).HasForeignKey(x => x.IdLibro).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Lector).WithMany(x => x.Resenas).HasForeignKey(x => x.IdLector).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
