
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrePaginas.Models;

[Table("palabras_favoritas")]
public class PalabraFavorita
{
    public int Id { get; set; }

    [Required]
    public int IdUsuario { get; set; }

    [Required]
    [StringLength(150)]
    public string Palabra { get; set; } = string.Empty;

    public DateTime FechaAgregado { get; set; } = DateTime.Now;

    public Usuario? Usuario { get; set; }
}
