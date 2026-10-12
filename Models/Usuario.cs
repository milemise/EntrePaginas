using System.ComponentModel.DataAnnotations;

namespace EntrePaginas.Models;

public static class RolesAplicacion
{
    public const string Administrador = "Administrador";
    public const string Lector = "Lector";
    public const string Escritor = "Escritor";
}

public class Usuario
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Apellido { get; set; } = string.Empty;
    [Required, StringLength(50)] public string NombreUsuario { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(255)] public string Password { get; set; } = string.Empty;
    [Required, StringLength(20)] public string Dni { get; set; } = string.Empty;
    [StringLength(30)] public string? Telefono { get; set; }
    [DataType(DataType.Date)] public DateTime FechaNacimiento { get; set; }
    [StringLength(500)] public string? AvatarUrl { get; set; }
    [Required, StringLength(30)] public string Rol { get; set; } = RolesAplicacion.Lector;
    public ICollection<Libro> LibrosEscritos { get; set; } = new List<Libro>();
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
    public ICollection<Resena> Resenas { get; set; } = new List<Resena>();
}
