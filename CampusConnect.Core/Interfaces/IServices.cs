using CampusConnect.Core.DTOs;
using CampusConnect.Core.Models;

namespace CampusConnect.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        string GenerateJwtToken(Usuario usuario);
    }

    public interface ISolicitudService
    {
        Task<List<SolicitudResponseDto>> GetAllAsync(string? estadoFilter = null, string? prioridadFilter = null);
        Task<SolicitudResponseDto?> GetByIdAsync(int id);
        Task<List<SolicitudResponseDto>> GetByEstudianteIdAsync(int estudianteId);
        Task<SolicitudResponseDto> CrearAsync(CrearSolicitudDto dto, int estudianteId);
        Task<bool> ActualizarAsync(int id, ActualizarSolicitudDto dto, int usuarioId);
        Task<bool> AdjuntarEvidenciaAsync(int solicitudId, string rutaArchivo, string tipoContenido);
        Task<List<SeguimientoDto>> GetSeguimientosAsync(int solicitudId);
    }

    public interface IRecursoService
    {
        Task<List<RecursoResponseDto>> GetAllAsync();
        Task<RecursoResponseDto?> GetByIdAsync(int id);
        Task<RecursoResponseDto> CrearAsync(CrearRecursoDto dto);
        Task<bool> ActualizarAsync(int id, CrearRecursoDto dto);
        Task<bool> EliminarAsync(int id);
    }

    public interface IReporteService
    {
        Task<ReporteKpiDto> GetKpisAsync();
    }
}
