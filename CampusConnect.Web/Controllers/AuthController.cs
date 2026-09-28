using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CampusConnect.Core.DTOs;
using CampusConnect.Core.Interfaces;

namespace CampusConnect.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToRoleHome();

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(dto);

            var result = await _authService.LoginAsync(dto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message ?? "Credenciales inválidas.");
                return View(dto);
            }

            // Crear claims para cookie de sesión
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, result.UserName ?? ""),
                new(ClaimTypes.Email, dto.Correo),
                new(ClaimTypes.Role, result.Rol ?? "Estudiante"),
                new("JwtToken", result.Token ?? ""),
                new("UserId", GetUserIdFromToken(result.Token ?? "").ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

            TempData["Success"] = $"¡Bienvenido, {result.UserName}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToRoleHome();
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToRoleHome();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var result = await _authService.RegisterAsync(dto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message ?? "Error en el registro.");
                return View(dto);
            }

            TempData["Success"] = "Registro exitoso. Por favor, inicia sesión.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied() => View();

        private IActionResult RedirectToRoleHome()
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            return rol switch
            {
                "Administrativo" => RedirectToAction("Index", "Admin"),
                "ResponsableTecnico" => RedirectToAction("MisSolicitudes", "Tecnico"),
                _ => RedirectToAction("Index", "Estudiante")
            };
        }

        private static int GetUserIdFromToken(string token)
        {
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "UserId");
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            }
            catch { return 0; }
        }
    }
}
