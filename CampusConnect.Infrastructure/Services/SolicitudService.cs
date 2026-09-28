using Microsoft.EntityFrameworkCore;
using CampusConnect.Core.DTOs;
using CampusConnect.Core.Enums;
using CampusConnect.Core.Interfaces;
using CampusConnect.Core.Models;
using CampusConnect.Infrastructure.Data;

namespace CampusConnect.Infrastructure.Services
{
    public class SolicitudService : ISolicitudService
    {
        private readonly ApplicationDbContext _context;

        public SolicitudService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SolicitudResponseDto>> GetAllAsync(string? estadoFilter = null, string? prioridadFilter = null)
        {
            var query = _context.Solicitudes
                .Include(s => s.Estudiante)
                .Include(s => s.Responsable)
                .Include(s => s.Recurso)
                .Include(s => s.Evidencias)
                .Include(s => s.Seguimientos).ThenInclude(sg => sg.Usuario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estadoFilter) && Enum.TryParse<EstadoSolicitud>(estadoFilter, out var estado))
                query = query.Where(s => s.Estado == estado);

            if (!string.IsNullOrEmpty(prioridadFilter) && Enum.TryParse<PrioridadSolicitud>(prioridadFilter, out var prioridad))
                query = query.Where(s => s.Prioridad == prioridad);

            var solicitudes = await query.OrderByDescending(s => s.FechaCreacion).ToListAsync();
            return solicitudes.Select(MapToDto).ToList();
        }

        public async Task<SolicitudResponseDto?> GetByIdAsync(int id)
        {
            var s = await _context.Solicitudes
                .Include(x => x.Estudiante)
                .Include(x => x.Responsable)
                .Include(x => x.Recurso)
                .Include(x => x.Evidencias)
                .Include(x => x.Seguimientos).ThenInclude(sg => sg.Usuario)
                .FirstOrDefaultAsync(x => x.Id == id);

            return s == null ? null : MapToDto(s);
        }

        public async Task<List<SolicitudResponseDto>> GetByEstudianteIdAsync(int estudianteId)
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Estudiante)
                .Include(s => s.Responsable)
                .Include(s => s.Recurso)
                .Include(s => s.Evidencias)
                .Include(s => s.Seguimientos).ThenInclude(sg => sg.Usuario)
                .Where(s => s.EstudianteId == estudianteId)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            return solicitudes.Select(MapToDto).ToList();
        }

        public async Task<SolicitudResponseDto> CrearAsync(CrearSolicitudDto dto, int estudianteId)
        {
            var solicitud = new Solicitud
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Prioridad = dto.Prioridad,
                RecursoId = dto.RecursoId,
                EstudianteId = estudianteId,
                Estado = EstadoSolicitud.Pendiente,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            };

            _context.Solicitudes.Add(solicitud);
            await _context.SaveChangesAsync();

            // Registro automático de seguimiento inicial
            _context.Seguimientos.Add(new Seguimiento
            {
                SolicitudId = solicitud.Id,
                UsuarioId = estudianteId,
                Comentario = "Solicitud creada.",
                EstadoAnterior = null,
                EstadoNuevo = EstadoSolicitud.Pendiente,
                Fecha = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(solicitud.Id))!;
        }

        public async Task<bool> ActualizarAsync(int id, ActualizarSolicitudDto dto, int usuarioId)
        {
            var solicitud = await _context.Solicitudes.FindAsync(id);
            if (solicitud == null) return false;

            var estadoAnterior = solicitud.Estado;
            solicitud.Estado = dto.Estado;
            solicitud.FechaActualizacion = DateTime.UtcNow;

            if (dto.ResponsableId.HasValue)
                solicitud.ResponsableId = dto.ResponsableId;

            _context.Seguimientos.Add(new Seguimiento
            {
                SolicitudId = id,
                UsuarioId = usuarioId,
                Comentario = dto.Comentario,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = dto.Estado,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AdjuntarEvidenciaAsync(int solicitudId, string rutaArchivo, string tipoContenido)
        {
            if (!await _context.Solicitudes.AnyAsync(s => s.Id == solicitudId)) return false;

            _context.Evidencias.Add(new Evidencia
            {
                SolicitudId = solicitudId,
                RutaArchivo = rutaArchivo,
                TipoContenido = tipoContenido,
                FechaSubida = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SeguimientoDto>> GetSeguimientosAsync(int solicitudId)
        {
            var seguimientos = await _context.Seguimientos
                .Include(sg => sg.Usuario)
                .Where(sg => sg.SolicitudId == solicitudId)
                .OrderBy(sg => sg.Fecha)
                .ToListAsync();

            return seguimientos.Select(sg => new SeguimientoDto
            {
                Id = sg.Id,
                Comentario = sg.Comentario,
                EstadoAnterior = sg.EstadoAnterior?.ToString(),
                EstadoNuevo = sg.EstadoNuevo?.ToString(),
                AutorNombre = sg.Usuario?.Nombre ?? "Sistema",
                Fecha = sg.Fecha
            }).ToList();
        }

        private static SolicitudResponseDto MapToDto(Solicitud s) => new()
        {
            Id = s.Id,
            Titulo = s.Titulo,
            Descripcion = s.Descripcion,
            Estado = s.Estado.ToString(),
            Prioridad = s.Prioridad.ToString(),
            FechaCreacion = s.FechaCreacion,
            FechaActualizacion = s.FechaActualizacion,
            EstudianteNombre = s.Estudiante?.Nombre ?? "",
            ResponsableNombre = s.Responsable?.Nombre,
            RecursoNombre = s.Recurso?.Nombre,
            Evidencias = s.Evidencias.Select(e => new EvidenciaDto
            {
                Id = e.Id,
                RutaArchivo = e.RutaArchivo,
                TipoContenido = e.TipoContenido,
                FechaSubida = e.FechaSubida
            }).ToList(),
            Seguimientos = s.Seguimientos.Select(sg => new SeguimientoDto
            {
                Id = sg.Id,
                Comentario = sg.Comentario,
                EstadoAnterior = sg.EstadoAnterior?.ToString(),
                EstadoNuevo = sg.EstadoNuevo?.ToString(),
                AutorNombre = sg.Usuario?.Nombre ?? "Sistema",
                Fecha = sg.Fecha
            }).ToList()
        };
    }
}
