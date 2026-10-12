using System.ComponentModel.DataAnnotations;

namespace EntrePaginas.Models;

public class Libro
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string Titulo { get; set; } = string.Empty;
    [StringLength(200)] public string? Autor { get; set; }
    [StringLength(100)] public string? OpenLibraryKey { get; set; }
    public string? Descripcion { get; set; }
    [StringLength(500)] public string? PortadaUrl { get; set; }
    [StringLength(500)] public string? ArchivoUrl { get; set; }
    [Range(0, 99999999)] public decimal PrecioCompra { get; set; }
    [Range(0, 99999999)] public decimal PrecioAlquiler { get; set; }
    public bool EsMas18 { get; set; }
    public int IdCategoria { get; set; }
    public int IdEscritor { get; set; }
    public int? IdLibroAnterior { get; set; }
    public Categoria Categoria { get; set; } = null!;
    public Usuario Escritor { get; set; } = null!;
    public Libro? LibroAnterior { get; set; }
    public ICollection<Libro> LibrosSiguientes { get; set; } = new List<Libro>();
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
    public ICollection<Resena> Resenas { get; set; } = new List<Resena>();
}
