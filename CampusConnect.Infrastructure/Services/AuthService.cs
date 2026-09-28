using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusConnect.Core.DTOs;
using CampusConnect.Core.Enums;
using CampusConnect.Core.Interfaces;
using CampusConnect.Core.Models;
using CampusConnect.Infrastructure.Data;

namespace CampusConnect.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == dto.Correo);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            {
                return new AuthResponseDto { Success = false, Message = "Credenciales inválidas." };
            }

            var token = GenerateJwtToken(usuario);
            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                UserName = usuario.Nombre,
                Rol = usuario.Rol.ToString(),
                Message = "Login exitoso."
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Correo == dto.Correo))
            {
                return new AuthResponseDto { Success = false, Message = "El correo ya está registrado." };
            }

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = RolUsuario.Estudiante
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(usuario);
            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                UserName = usuario.Nombre,
                Rol = usuario.Rol.ToString(),
                Message = "Registro exitoso."
            };
        }

        public string GenerateJwtToken(Usuario usuario)
        {
            var jwtKey = _config["Jwt:Key"] ?? "CampusConnectSecretKey_MustBe32Chars!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                new Claim("UserId", usuario.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "CampusConnect",
                audience: _config["Jwt:Audience"] ?? "CampusConnectUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
