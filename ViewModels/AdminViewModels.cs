using EntrePaginas.Models;

namespace EntrePaginas.ViewModels;

public class UsuariosAdminViewModel
{
    public IReadOnlyList<Usuario> Usuarios { get; set; } = Array.Empty<Usuario>();
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public string? Buscar { get; set; }
}

public class ResenasAdminViewModel
{
    public IReadOnlyList<Resena> Resenas { get; set; } = Array.Empty<Resena>();
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
}

public class EstadisticasEscritorViewModel
{
    public int LibrosPublicados { get; set; }
    public int Ventas { get; set; }
    public int Alquileres { get; set; }
    public decimal IngresosVentas { get; set; }
    public decimal IngresosAlquileres { get; set; }
    public IReadOnlyList<LibroEstadisticaItem> Libros { get; set; } = Array.Empty<LibroEstadisticaItem>();
}

public class LibroEstadisticaItem
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int Ventas { get; set; }
    public int Alquileres { get; set; }
    public decimal Ingresos { get; set; }
}
