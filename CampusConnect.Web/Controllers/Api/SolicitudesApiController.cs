using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Core.DTOs;
using CampusConnect.Core.Interfaces;

namespace CampusConnect.Web.Controllers.Api
{
    [Authorize]
    public class SolicitudesApiController : ApiBaseController
    {
        private readonly ISolicitudService _solicitudService;
        private readonly IWebHostEnvironment _env;

        public SolicitudesApiController(ISolicitudService solicitudService, IWebHostEnvironment env)
        {
            _solicitudService = solicitudService;
            _env = env;
        }

        /// <summary>Obtener todas las solicitudes (Admin / Técnico).</summary>
        [HttpGet]
        [Authorize(Roles = "Administrativo,ResponsableTecnico")]
        public async Task<IActionResult> GetAll([FromQuery] string? estado, [FromQuery] string? prioridad)
        {
            var solicitudes = await _solicitudService.GetAllAsync(estado, prioridad);
            return ApiOk(solicitudes);
        }

        /// <summary>Obtener solicitudes del estudiante autenticado.</summary>
        [HttpGet("mis-solicitudes")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> GetMias()
        {
            var userId = GetCurrentUserId();
            var solicitudes = await _solicitudService.GetByEstudianteIdAsync(userId);
            return ApiOk(solicitudes);
        }

        /// <summary>Obtener detalle de una solicitud por ID.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var solicitud = await _solicitudService.GetByIdAsync(id);
            if (solicitud == null) return ApiError("Solicitud no encontrada.", 404);
            return ApiOk(solicitud);
        }

        /// <summary>Crear nueva solicitud (Estudiante).</summary>
        [HttpPost]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> Create([FromBody] CrearSolicitudDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var resultado = await _solicitudService.CrearAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = resultado.Id }, new { success = true, data = resultado });
        }

        /// <summary>Actualizar estado, asignar responsable, comentar.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrativo,ResponsableTecnico")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarSolicitudDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var ok = await _solicitudService.ActualizarAsync(id, dto, userId);
            if (!ok) return ApiError("Solicitud no encontrada.", 404);
            return ApiOk(null, "Solicitud actualizada correctamente.");
        }

        /// <summary>Adjuntar evidencia multipart (imágenes / PDFs).</summary>
        [HttpPost("{id:int}/evidencia")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> SubirEvidencia(int id, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return ApiError("No se proporcionó un archivo válido.");

            var tiposPermitidos = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf" };
            if (!tiposPermitidos.Contains(archivo.ContentType))
                return ApiError("Tipo de archivo no permitido. Use JPG, PNG o PDF.");

            if (archivo.Length > 10 * 1024 * 1024)
                return ApiError("El archivo no puede superar los 10 MB.");

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "evidencias");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(archivo.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await archivo.CopyToAsync(stream);

            var rutaRelativa = $"/uploads/evidencias/{fileName}";
            var ok = await _solicitudService.AdjuntarEvidenciaAsync(id, rutaRelativa, archivo.ContentType);
            if (!ok) return ApiError("Solicitud no encontrada.", 404);

            return ApiOk(new { ruta = rutaRelativa }, "Evidencia subida correctamente.");
        }

        /// <summary>Obtener historial de seguimiento de una solicitud.</summary>
        [HttpGet("{id:int}/seguimiento")]
        public async Task<IActionResult> GetSeguimiento(int id)
        {
            var seguimientos = await _solicitudService.GetSeguimientosAsync(id);
            return ApiOk(seguimientos);
        }
    }
}
