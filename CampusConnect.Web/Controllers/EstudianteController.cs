using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CampusConnect.Core.DTOs;
using CampusConnect.Core.Interfaces;
using CampusConnect.Core.Enums;

namespace CampusConnect.Web.Controllers
{
    [Authorize(Roles = "Estudiante")]
    public class EstudianteController : Controller
    {
        private readonly ISolicitudService _solicitudService;
        private readonly IRecursoService _recursoService;
        private readonly IWebHostEnvironment _env;

        public EstudianteController(ISolicitudService solicitudService,
                                    IRecursoService recursoService,
                                    IWebHostEnvironment env)
        {
            _solicitudService = solicitudService;
            _recursoService = recursoService;
            _env = env;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        public async Task<IActionResult> Index()
        {
            var solicitudes = await _solicitudService.GetByEstudianteIdAsync(GetCurrentUserId());
            return View(solicitudes);
        }

        [HttpGet]
        public async Task<IActionResult> CrearSolicitud()
        {
            ViewBag.Recursos = await _recursoService.GetAllAsync();
            ViewBag.Prioridades = Enum.GetValues<PrioridadSolicitud>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSolicitud(CrearSolicitudDto dto, IFormFile? archivo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Recursos = await _recursoService.GetAllAsync();
                ViewBag.Prioridades = Enum.GetValues<PrioridadSolicitud>();
                return View(dto);
            }

            var solicitud = await _solicitudService.CrearAsync(dto, GetCurrentUserId());

            // Adjuntar evidencia si se subió
            if (archivo != null && archivo.Length > 0)
            {
                var tiposPermitidos = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf" };
                if (tiposPermitidos.Contains(archivo.ContentType) && archivo.Length <= 10 * 1024 * 1024)
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "evidencias");
                    Directory.CreateDirectory(uploadsDir);
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(archivo.FileName)}";
                    using var stream = new FileStream(Path.Combine(uploadsDir, fileName), FileMode.Create);
                    await archivo.CopyToAsync(stream);
                    await _solicitudService.AdjuntarEvidenciaAsync(solicitud.Id, $"/uploads/evidencias/{fileName}", archivo.ContentType);
                }
            }

            TempData["Success"] = "¡Solicitud creada exitosamente!";
            return RedirectToAction(nameof(Seguimiento), new { id = solicitud.Id });
        }

        public async Task<IActionResult> Seguimiento(int id)
        {
            var solicitud = await _solicitudService.GetByIdAsync(id);
            if (solicitud == null || solicitud.EstudianteNombre != User.Identity?.Name)
            {
                // Verificar por userId también
                var allMias = await _solicitudService.GetByEstudianteIdAsync(GetCurrentUserId());
                if (!allMias.Any(s => s.Id == id))
                    return Forbid();
                solicitud = await _solicitudService.GetByIdAsync(id);
            }
            return View(solicitud);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirEvidencia(int solicitudId, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "No se seleccionó un archivo.";
                return RedirectToAction(nameof(Seguimiento), new { id = solicitudId });
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "evidencias");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(archivo.FileName)}";
            using var stream = new FileStream(Path.Combine(uploadsDir, fileName), FileMode.Create);
            await archivo.CopyToAsync(stream);

            await _solicitudService.AdjuntarEvidenciaAsync(solicitudId, $"/uploads/evidencias/{fileName}", archivo.ContentType);
            TempData["Success"] = "Evidencia adjuntada correctamente.";
            return RedirectToAction(nameof(Seguimiento), new { id = solicitudId });
        }
    }
}
