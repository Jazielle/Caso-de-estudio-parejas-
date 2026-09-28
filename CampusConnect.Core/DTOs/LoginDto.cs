using System.ComponentModel.DataAnnotations;

namespace CampusConnect.Core.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        public string? UserName { get; set; }
        public string? Rol { get; set; }
    }
}
