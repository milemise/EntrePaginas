namespace EntrePaginas.Models;

public class Alquiler
{
    public int Id { get; set; }
    public int IdLibro { get; set; }
    public int IdLector { get; set; }
    public DateTime FechaInicio { get; set; } = DateTime.Now;
    public DateTime FechaHasta { get; set; }
    public decimal Importe { get; set; }
    public bool Activo { get; set; } = true;
    public Libro Libro { get; set; } = null!;
    public Usuario Lector { get; set; } = null!;
}
