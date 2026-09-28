using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Core.DTOs;
using CampusConnect.Core.Interfaces;

namespace CampusConnect.Web.Controllers
{
    [Authorize(Roles = "ResponsableTecnico")]
    public class TecnicoController : Controller
    {
        private readonly ISolicitudService _solicitudService;

        public TecnicoController(ISolicitudService solicitudService)
        {
            _solicitudService = solicitudService;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        public async Task<IActionResult> MisSolicitudes()
        {
            var todas = await _solicitudService.GetAllAsync();
            var userId = GetCurrentUserId();
            var mias = todas.Where(s => s.ResponsableNombre == User.Identity?.Name).ToList();
            return View(mias);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var solicitud = await _solicitudService.GetByIdAsync(id);
            if (solicitud == null) return NotFound();
            return View(solicitud);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int id, ActualizarSolicitudDto dto)
        {
            await _solicitudService.ActualizarAsync(id, dto, GetCurrentUserId());
            TempData["Success"] = "Estado actualizado correctamente.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
    }
}
