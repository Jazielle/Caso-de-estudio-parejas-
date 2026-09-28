using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Core.DTOs;
using CampusConnect.Core.Interfaces;
using CampusConnect.Core.Enums;
using Microsoft.EntityFrameworkCore;
using CampusConnect.Infrastructure.Data;

namespace CampusConnect.Web.Controllers
{
    [Authorize(Roles = "Administrativo")]
    public class AdminController : Controller
    {
        private readonly ISolicitudService _solicitudService;
        private readonly IRecursoService _recursoService;
        private readonly IReporteService _reporteService;
        private readonly ApplicationDbContext _context;

        public AdminController(ISolicitudService solicitudService,
                               IRecursoService recursoService,
                               IReporteService reporteService,
                               ApplicationDbContext context)
        {
            _solicitudService = solicitudService;
            _recursoService = recursoService;
            _reporteService = reporteService;
            _context = context;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        // ======= DASHBOARD =======
        public async Task<IActionResult> Index(string? estado, string? prioridad)
        {
            ViewBag.Solicitudes = await _solicitudService.GetAllAsync(estado, prioridad);
            ViewBag.EstadoFiltro = estado;
            ViewBag.PrioridadFiltro = prioridad;
            ViewBag.Estados = Enum.GetValues<EstadoSolicitud>();
            ViewBag.Prioridades = Enum.GetValues<PrioridadSolicitud>();
            return View();
        }

        // ======= DETALLE SOLICITUD =======
        public async Task<IActionResult> DetalleSolicitud(int id)
        {
            var solicitud = await _solicitudService.GetByIdAsync(id);
            if (solicitud == null) return NotFound();

            ViewBag.Tecnicos = await _context.Usuarios
                .Where(u => u.Rol == Core.Enums.RolUsuario.ResponsableTecnico)
                .Select(u => new { u.Id, u.Nombre })
                .ToListAsync();
            ViewBag.Estados = Enum.GetValues<EstadoSolicitud>();

            return View(solicitud);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarSolicitud(int id, ActualizarSolicitudDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor complete todos los campos requeridos.";
                return RedirectToAction(nameof(DetalleSolicitud), new { id });
            }

            await _solicitudService.ActualizarAsync(id, dto, GetCurrentUserId());
            TempData["Success"] = "Solicitud actualizada correctamente.";
            return RedirectToAction(nameof(DetalleSolicitud), new { id });
        }

        // ======= RECURSOS =======
        public async Task<IActionResult> Recursos()
        {
            var recursos = await _recursoService.GetAllAsync();
            ViewBag.Categorias = Enum.GetValues<CategoriaRecurso>();
            return View(recursos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearRecurso(CrearRecursoDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos.";
                return RedirectToAction(nameof(Recursos));
            }
            await _recursoService.CrearAsync(dto);
            TempData["Success"] = "Recurso creado exitosamente.";
            return RedirectToAction(nameof(Recursos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecurso(int id, CrearRecursoDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos.";
                return RedirectToAction(nameof(Recursos));
            }
            await _recursoService.ActualizarAsync(id, dto);
            TempData["Success"] = "Recurso actualizado.";
            return RedirectToAction(nameof(Recursos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarRecurso(int id)
        {
            await _recursoService.EliminarAsync(id);
            TempData["Success"] = "Recurso eliminado.";
            return RedirectToAction(nameof(Recursos));
        }

        // ======= REPORTES =======
        public async Task<IActionResult> Reportes()
        {
            var kpis = await _reporteService.GetKpisAsync();
            return View(kpis);
        }
    }
}
