namespace EntrePaginas.Models;

public class Compra
{
    public int Id { get; set; }
    public int IdLibro { get; set; }
    public int IdLector { get; set; }
    public DateTime FechaCompra { get; set; } = DateTime.Now;
    public decimal Importe { get; set; }
    public bool Devuelto { get; set; }
    public Libro Libro { get; set; } = null!;
    public Usuario Lector { get; set; } = null!;
}
