using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EntrePaginas.ViewModels;

public class LoginViewModel
{
    [Required, Display(Name = "Usuario o correo electrónico")] public string Usuario { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Display(Name = "Recordarme")] public bool Recordarme { get; set; }
}

public class RegistroViewModel
{
    [Required, StringLength(100)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Apellido { get; set; } = string.Empty;
    [Required, StringLength(50), RegularExpression("^[a-zA-Z0-9_.-]+$", ErrorMessage = "Usá letras, números, punto, guion o guion bajo.")] public string NombreUsuario { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(20)] public string Dni { get; set; } = string.Empty;
    [Phone, StringLength(30)] public string? Telefono { get; set; }
    [Required, DataType(DataType.Date), Display(Name = "Fecha de nacimiento")] public DateTime FechaNacimiento { get; set; }
    [Required, MinLength(8), DataType(DataType.Password), Display(Name = "Contraseña")] public string Password { get; set; } = string.Empty;
    [Required, Compare(nameof(Password)), DataType(DataType.Password), Display(Name = "Repetir contraseña")] public string ConfirmarPassword { get; set; } = string.Empty;
    [Required, RegularExpression("^(Lector|Escritor)$", ErrorMessage = "Elegí Lector o Escritor.")] public string TipoCuenta { get; set; } = "Lector";
}

public class PerfilViewModel
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Apellido { get; set; } = string.Empty;
    [Required, StringLength(50)] public string NombreUsuario { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public IFormFile? Avatar { get; set; }
    [Phone, StringLength(30)] public string? Telefono { get; set; }
}
