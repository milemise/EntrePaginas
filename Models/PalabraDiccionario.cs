
namespace EntrePaginas.Models;

public class PalabraDiccionario
{
    public string Palabra { get; set; } = string.Empty;

    public string Contenido { get; set; } = string.Empty;

    public List<string> Definiciones { get; set; } = new();

    public List<string> Ejemplos { get; set; } = new();

    public string UrlFuente { get; set; } = string.Empty;
}
