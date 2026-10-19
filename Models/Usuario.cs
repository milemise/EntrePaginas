using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ]+(?:[ '\-][a-zA-ZáéíóúÁÉÍÓÚüÜñÑ]+)*$",
        ErrorMessage = "Ingresá un nombre válido, utilizando solo letras.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ]+(?:[ '\-][a-zA-ZáéíóúÁÉÍÓÚüÜñÑ]+)*$",
        ErrorMessage = "Ingresá un apellido válido, utilizando solo letras.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 4,
        ErrorMessage = "El nombre de usuario debe tener entre 4 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9._-]+$",
        ErrorMessage = "Usá letras, números, puntos, guiones o guiones bajos, sin espacios.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(255, MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener entre 8 y 255 caracteres.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
        
    [NotMapped]
    [Required(ErrorMessage = "Debés confirmar tu contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarPassword { get; set; } = string.Empty;


    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [RegularExpression(@"^[0-9]{7,8}$",
        ErrorMessage = "El DNI debe contener exactamente 7 u 8 números, sin puntos.")]
    public string Dni { get; set; } = string.Empty;

    [RegularExpression(@"^[0-9]{10,13}$",
        ErrorMessage = "El teléfono debe contener entre 10 y 13 números, sin espacios ni guiones.")]
    public string? Telefono { get; set; }

    [CustomValidation(typeof(Usuario), nameof(ValidarFechaNacimiento))]
    [DataType(DataType.Date)]
    public DateTime FechaNacimiento { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [StringLength(30)]
    public string Rol { get; set; } = RolesAplicacion.Lector;

    public static ValidationResult? ValidarFechaNacimiento(
        DateTime fecha, ValidationContext context)
    {
        if (fecha == default)
        {
            return new ValidationResult("La fecha de nacimiento es obligatoria.");
        }

        if (fecha.Date > DateTime.Today)
        {
            return new ValidationResult(
                "La fecha de nacimiento no puede ser futura.");
        }

        return ValidationResult.Success;
    }

    

    public ICollection<Libro> LibrosEscritos { get; set; } = new List<Libro>();
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
    public ICollection<Resena> Resenas { get; set; } = new List<Resena>();
}
