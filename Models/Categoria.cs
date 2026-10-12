using System.ComponentModel.DataAnnotations;

namespace EntrePaginas.Models;

public class Categoria
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nombre { get; set; } = string.Empty;
    public ICollection<Libro> Libros { get; set; } = new List<Libro>();
}
