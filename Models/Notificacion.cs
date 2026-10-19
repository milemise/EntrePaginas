
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrePaginas.Models;

[Table("notificaciones")]
public class Notificacion
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Required]
    [StringLength(50)]
    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Column("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [Column("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [Column("leida")]
    public bool Leida { get; set; } = false;

    [Column("favorita")]
    public bool Favorita { get; set; } = false;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [StringLength(50)]
    [Column("entidad_tipo")]
    public string? EntidadTipo { get; set; }

    [Column("entidad_id")]
    public int? EntidadId { get; set; }

    [StringLength(500)]
    [Column("url_destino")]
    public string? UrlDestino { get; set; }
}