using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Core.DTOs;
using CampusConnect.Core.Interfaces;

namespace CampusConnect.Web.Controllers.Api
{
    [Authorize]
    public class RecursosApiController : ApiBaseController
    {
        private readonly IRecursoService _recursoService;

        public RecursosApiController(IRecursoService recursoService)
        {
            _recursoService = recursoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            ApiOk(await _recursoService.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _recursoService.GetByIdAsync(id);
            return r == null ? ApiError("Recurso no encontrado.", 404) : ApiOk(r);
        }

        [HttpPost]
        [Authorize(Roles = "Administrativo")]
        public async Task<IActionResult> Create([FromBody] CrearRecursoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var r = await _recursoService.CrearAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = r.Id }, new { success = true, data = r });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrativo")]
        public async Task<IActionResult> Update(int id, [FromBody] CrearRecursoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ok = await _recursoService.ActualizarAsync(id, dto);
            return ok ? ApiOk(null, "Recurso actualizado.") : ApiError("Recurso no encontrado.", 404);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrativo")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _recursoService.EliminarAsync(id);
            return ok ? ApiOk(null, "Recurso eliminado.") : ApiError("Recurso no encontrado.", 404);
        }
    }
}
