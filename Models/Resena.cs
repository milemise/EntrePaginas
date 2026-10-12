using System.ComponentModel.DataAnnotations;

namespace EntrePaginas.Models;

public class Resena
{
    public int Id { get; set; }
    public int IdLibro { get; set; }
    public int IdLector { get; set; }
    [Range(1, 5)] public int Puntuacion { get; set; }
    [StringLength(3000)] public string? Comentario { get; set; }
    public DateTime FechaComentario { get; set; } = DateTime.Now;
    public bool Anonimo { get; set; }
    public bool Reportado { get; set; }
    public Libro Libro { get; set; } = null!;
    public Usuario Lector { get; set; } = null!;
}
