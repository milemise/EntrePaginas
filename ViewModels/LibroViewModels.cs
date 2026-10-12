using System.ComponentModel.DataAnnotations;
using EntrePaginas.Models;
using Microsoft.AspNetCore.Http;

namespace EntrePaginas.ViewModels;

public class LibroListadoViewModel
{
    public IReadOnlyList<Libro> Libros { get; set; } = Array.Empty<Libro>();
    public IReadOnlyList<Categoria> Categorias { get; set; } = Array.Empty<Categoria>();
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public string? Buscar { get; set; }
    public int? CategoriaId { get; set; }
    public string? Ordenar { get; set; }
    public Dictionary<int, LibroResenasResumen> ResumenResenas { get; set; } = new();
}

public class LibroFormViewModel
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string Titulo { get; set; } = string.Empty;
    [StringLength(200)] public string? Autor { get; set; }
    [StringLength(100)] public string? OpenLibraryKey { get; set; }
    [StringLength(10000)] public string? Descripcion { get; set; }
    [Range(0, 99999999)] public decimal PrecioCompra { get; set; }
    [Range(0, 99999999)] public decimal PrecioAlquiler { get; set; }
    public bool EsMas18 { get; set; }
    [Required] public int? IdCategoria { get; set; }
    public int? IdLibroAnterior { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? LibroAnteriorTitulo { get; set; }
    public string? PortadaUrl { get; set; }
    public IFormFile? Portada { get; set; }
    public IFormFile? Archivo { get; set; }
}

public class LibroResenasResumen
{
    public int Cantidad { get; set; }
    public double Promedio { get; set; }
}

public class LibroDetalleViewModel
{
    public Libro Libro { get; set; } = null!;
    public bool ContenidoBloqueado { get; set; }
    public string? MotivoBloqueo { get; set; }
    public bool PuedeComprar { get; set; }
    public bool PuedeAlquilar { get; set; }
    public bool PuedeResenar { get; set; }
    public bool YaCompro { get; set; }
    public bool AlquilerVigente { get; set; }
    public bool CompraVigente { get; set; }
    public int? MiResenaId { get; set; }
    public int PaginaResenas { get; set; } = 1;
    public int TotalPaginasResenas { get; set; } = 1;
    public int TotalResenas { get; set; }
    public double PromedioResenas { get; set; }
}

public class PaginacionViewModel<T>
{
    public IReadOnlyList<T> Elementos { get; set; } = Array.Empty<T>();
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
}

public class ReviewFormViewModel
{
    public int IdLibro { get; set; }
    [Range(1, 5)] public int Puntuacion { get; set; } = 5;
    [StringLength(3000)] public string? Comentario { get; set; }
    public bool Anonimo { get; set; }
}

public class CompraListadoItem
{
    public Compra Compra { get; set; } = null!;
}

public class AlquilerListadoItem
{
    public Alquiler Alquiler { get; set; } = null!;
}
