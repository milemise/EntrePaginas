
using EntrePaginas.Services;

namespace EntrePaginas.ViewModels;

public class DiccionarioViewModel
{
    public string Busqueda { get; set; } = string.Empty;

    public DefinicionDiccionario? Resultado { get; set; }

    public string? Mensaje { get; set; }

    public bool EsUsuarioAutenticado { get; set; }
    public List<string> PalabrasFavoritas { get; set; } = new();

    public List<string> TerminosLiterarios { get; set; } = new()
    {
        "Metáfora",
        "Hipérbole",
        "Analepsis",
        "Protagonista",
        "Narrador omnisciente",
        "Personificación",
        "Símil",
        "Clímax",
        "Antagonista",
        "Prólogo",
        "Epílogo",
        "Ironía",
        "Alegoría",
        "Fábula",
        "Desenlace",
        "Narrativa",
        "Serendipia",
        "Nostalgia",
        "Efímero"
    };
}
