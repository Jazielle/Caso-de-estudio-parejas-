using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Core.DTOs;
using CampusConnect.Core.Interfaces;

namespace CampusConnect.Web.Controllers.Api
{
    public class AuthApiController : ApiBaseController
    {
        private readonly IAuthService _authService;

        public AuthApiController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Login y obtención de token JWT.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(dto);
            if (!result.Success)
                return ApiError(result.Message ?? "Error de autenticación.", 401);

            return ApiOk(result, result.Message ?? "Login exitoso.");
        }

        /// <summary>Registro de nuevo estudiante.</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(dto);
            if (!result.Success)
                return ApiError(result.Message ?? "Error en el registro.", 400);

            return ApiOk(result, result.Message ?? "Registro exitoso.");
        }
    }
}
