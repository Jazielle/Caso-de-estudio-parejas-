using Microsoft.EntityFrameworkCore;
using CampusConnect.Core.DTOs;
using CampusConnect.Core.Enums;
using CampusConnect.Core.Interfaces;
using CampusConnect.Core.Models;
using CampusConnect.Infrastructure.Data;

namespace CampusConnect.Infrastructure.Services
{
    public class RecursoService : IRecursoService
    {
        private readonly ApplicationDbContext _context;

        public RecursoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RecursoResponseDto>> GetAllAsync()
        {
            return await _context.Recursos
                .Select(r => new RecursoResponseDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    Categoria = r.Categoria.ToString(),
                    Estado = r.Estado,
                    Detalles = r.Detalles,
                    TotalSolicitudes = r.Solicitudes.Count
                })
                .ToListAsync();
        }

        public async Task<RecursoResponseDto?> GetByIdAsync(int id)
        {
            var r = await _context.Recursos.Include(x => x.Solicitudes).FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return null;
            return new RecursoResponseDto
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Categoria = r.Categoria.ToString(),
                Estado = r.Estado,
                Detalles = r.Detalles,
                TotalSolicitudes = r.Solicitudes.Count
            };
        }

        public async Task<RecursoResponseDto> CrearAsync(CrearRecursoDto dto)
        {
            var recurso = new Recurso
            {
                Nombre = dto.Nombre,
                Categoria = dto.Categoria,
                Estado = dto.Estado,
                Detalles = dto.Detalles
            };
            _context.Recursos.Add(recurso);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(recurso.Id))!;
        }

        public async Task<bool> ActualizarAsync(int id, CrearRecursoDto dto)
        {
            var recurso = await _context.Recursos.FindAsync(id);
            if (recurso == null) return false;
            recurso.Nombre = dto.Nombre;
            recurso.Categoria = dto.Categoria;
            recurso.Estado = dto.Estado;
            recurso.Detalles = dto.Detalles;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var recurso = await _context.Recursos.FindAsync(id);
            if (recurso == null) return false;
            _context.Recursos.Remove(recurso);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class ReporteService : IReporteService
    {
        private readonly ApplicationDbContext _context;

        public ReporteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReporteKpiDto> GetKpisAsync()
        {
            var total = await _context.Solicitudes.CountAsync();
            var pendientes = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitud.Pendiente);
            var cerradas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitud.Cerrada);

            var cerradasConFechas = await _context.Solicitudes
                .Where(s => s.Estado == EstadoSolicitud.Cerrada)
                .Select(s => new { s.FechaCreacion, s.FechaActualizacion })
                .ToListAsync();

            double promedio = cerradasConFechas.Any()
                ? cerradasConFechas.Average(s => (s.FechaActualizacion - s.FechaCreacion).TotalHours)
                : 0;

            var responsables = await _context.Usuarios
                .Where(u => u.Rol == RolUsuario.ResponsableTecnico)
                .Select(u => new ResponsableKpiDto
                {
                    Nombre = u.Nombre,
                    SolicitudesAsignadas = u.SolicitudesComoResponsable.Count,
                    SolicitudesCerradas = u.SolicitudesComoResponsable.Count(s => s.Estado == EstadoSolicitud.Cerrada)
                })
                .ToListAsync();

            return new ReporteKpiDto
            {
                TotalRecibidas = total,
                TotalPendientes = pendientes,
                TotalCerradas = cerradas,
                TiempoPromedioAtencionHoras = Math.Round(promedio, 2),
                ResponsablesAsignados = responsables
            };
        }
    }
}
