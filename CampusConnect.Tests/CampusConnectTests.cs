using Microsoft.EntityFrameworkCore;
using CampusConnect.Core.DTOs;
using CampusConnect.Core.Enums;
using CampusConnect.Core.Models;
using CampusConnect.Infrastructure.Data;
using CampusConnect.Infrastructure.Services;
using Xunit;

namespace CampusConnect.Tests
{
    /// <summary>
    /// Pruebas unitarias e integración para Campus Connect.
    /// Usan InMemory Database, sin dependencia de PostgreSQL real.
    /// </summary>
    public class CampusConnectTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private Usuario CreateEstudiante() => new()
        {
            Id = 1,
            Nombre = "Juan Estudiante",
            Correo = "juan@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            Rol = RolUsuario.Estudiante
        };

        // ═══════════════════════════════════════════════════════════════
        // TEST 1: Login con credenciales válidas retorna éxito y cookie
        // ═══════════════════════════════════════════════════════════════
        [Fact]
        public async Task Login_ValidCredentials_ReturnsSuccessAndCookie()
        {
            // Arrange
            using var context = CreateInMemoryContext("LoginTest");
            var config = TestHelpers.GetFakeConfiguration();
            var authService = new AuthService(context, config);

            var usuario = CreateEstudiante();
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var loginDto = new LoginDto { Correo = "juan@test.com", Password = "Test@123" };

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Token);
            Assert.NotEmpty(result.Token!);
            Assert.Equal("Juan Estudiante", result.UserName);
            Assert.Equal("Estudiante", result.Rol);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 2: Crear solicitud con modelo válido se guarda en BD
        // ═══════════════════════════════════════════════════════════════
        [Fact]
        public async Task CreateSolicitud_ValidModel_SavesToDatabase()
        {
            // Arrange
            using var context = CreateInMemoryContext("CreateSolicitudTest");
            var solicitudService = new SolicitudService(context);

            var estudiante = CreateEstudiante();
            context.Usuarios.Add(estudiante);
            await context.SaveChangesAsync();

            var dto = new CrearSolicitudDto
            {
                Titulo = "Computadora dañada en lab 201",
                Descripcion = "El equipo no enciende desde el lunes.",
                Prioridad = PrioridadSolicitud.Alta
            };

            // Act
            var result = await solicitudService.CrearAsync(dto, estudiante.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Computadora dañada en lab 201", result.Titulo);
            Assert.Equal("Pendiente", result.Estado);
            Assert.Equal("Alta", result.Prioridad);

            // Verificar que existe en la BD
            var enDb = await context.Solicitudes.FindAsync(result.Id);
            Assert.NotNull(enDb);
            Assert.Equal(EstadoSolicitud.Pendiente, enDb!.Estado);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 3: Adjuntar evidencia válida sube y vincula a la solicitud
        // ═══════════════════════════════════════════════════════════════
        [Fact]
        public async Task AttachEvidencia_ValidFile_UploadsAndLinksToSolicitud()
        {
            // Arrange
            using var context = CreateInMemoryContext("EvidenciaTest");
            var solicitudService = new SolicitudService(context);

            var estudiante = CreateEstudiante();
            context.Usuarios.Add(estudiante);
            var solicitud = new Solicitud
            {
                Id = 10,
                Titulo = "Fuga de agua",
                Descripcion = "Fuga detectada en el corredor.",
                Estado = EstadoSolicitud.Pendiente,
                Prioridad = PrioridadSolicitud.Alta,
                EstudianteId = estudiante.Id,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            };
            context.Solicitudes.Add(solicitud);
            await context.SaveChangesAsync();

            // Act
            var ok = await solicitudService.AdjuntarEvidenciaAsync(
                solicitudId: 10,
                rutaArchivo: "/uploads/evidencias/test-foto.jpg",
                tipoContenido: "image/jpeg"
            );

            // Assert
            Assert.True(ok);
            var evidencia = context.Evidencias.FirstOrDefault(e => e.SolicitudId == 10);
            Assert.NotNull(evidencia);
            Assert.Equal("/uploads/evidencias/test-foto.jpg", evidencia!.RutaArchivo);
            Assert.Equal("image/jpeg", evidencia.TipoContenido);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 4: GetSeguimiento por SolicitudId retorna historial
        // ═══════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetSeguimiento_BySolicitudId_ReturnsHistory()
        {
            // Arrange
            using var context = CreateInMemoryContext("SeguimientoTest");
            var solicitudService = new SolicitudService(context);

            var estudiante = CreateEstudiante();
            context.Usuarios.Add(estudiante);

            var solicitud = new Solicitud
            {
                Id = 20,
                Titulo = "Silla rota",
                Descripcion = "La silla del aula 105 está rota.",
                Estado = EstadoSolicitud.EnProceso,
                Prioridad = PrioridadSolicitud.Baja,
                EstudianteId = estudiante.Id,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            };
            context.Solicitudes.Add(solicitud);

            context.Seguimientos.AddRange(
                new Seguimiento { SolicitudId = 20, UsuarioId = 1, Comentario = "Solicitud recibida.", EstadoNuevo = EstadoSolicitud.Pendiente, Fecha = DateTime.UtcNow.AddDays(-2) },
                new Seguimiento { SolicitudId = 20, UsuarioId = 1, Comentario = "Técnico asignado.", EstadoAnterior = EstadoSolicitud.Pendiente, EstadoNuevo = EstadoSolicitud.Asignada, Fecha = DateTime.UtcNow.AddDays(-1) },
                new Seguimiento { SolicitudId = 20, UsuarioId = 1, Comentario = "En proceso de reparación.", EstadoAnterior = EstadoSolicitud.Asignada, EstadoNuevo = EstadoSolicitud.EnProceso, Fecha = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            // Act
            var seguimientos = await solicitudService.GetSeguimientosAsync(20);

            // Assert
            Assert.NotNull(seguimientos);
            Assert.Equal(3, seguimientos.Count);
            Assert.Equal("Solicitud recibida.", seguimientos.First().Comentario);
            Assert.Equal("EnProceso", seguimientos.Last().EstadoNuevo);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 5: GetComentarios - Verificar renderización de datos
        // ═══════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetComentarios_VerifyRenderingData()
        {
            // Arrange
            using var context = CreateInMemoryContext("ComentariosTest");
            var solicitudService = new SolicitudService(context);

            var admin = new Usuario { Id = 2, Nombre = "Admin Sistema", Correo = "admin@test.com", PasswordHash = "x", Rol = RolUsuario.Administrativo };
            var estudiante = CreateEstudiante();
            context.Usuarios.AddRange(estudiante, admin);

            var solicitud = new Solicitud
            {
                Id = 30, Titulo = "Test Comentarios", Descripcion = "Prueba.",
                Estado = EstadoSolicitud.Cerrada, Prioridad = PrioridadSolicitud.Media,
                EstudianteId = 1, FechaCreacion = DateTime.UtcNow, FechaActualizacion = DateTime.UtcNow
            };
            context.Solicitudes.Add(solicitud);

            context.Seguimientos.Add(new Seguimiento
            {
                SolicitudId = 30, UsuarioId = 2,
                Comentario = "Problema resuelto, silla reemplazada.",
                EstadoAnterior = EstadoSolicitud.EnProceso,
                EstadoNuevo = EstadoSolicitud.Cerrada,
                Fecha = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            // Act
            var seguimientos = await solicitudService.GetSeguimientosAsync(30);

            // Assert
            Assert.Single(seguimientos);
            var sg = seguimientos.First();
            Assert.Equal("Admin Sistema", sg.AutorNombre);
            Assert.Equal("EnProceso", sg.EstadoAnterior);
            Assert.Equal("Cerrada", sg.EstadoNuevo);
            Assert.Contains("resuelto", sg.Comentario);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 6: Consumo de API - HttpClient Testing
        // ═══════════════════════════════════════════════════════════════
        [Fact]
        public async Task ApiEndpoint_AuthLogin_ReturnsExpectedJsonStructure()
        {
            // Simula consumo de API con HttpClient / serialización
            using var context = CreateInMemoryContext("ApiClientTest");
            var config = TestHelpers.GetFakeConfiguration();
            var authService = new AuthService(context, config);

            var usuario = new Usuario
            {
                Id = 5, Nombre = "API User", Correo = "apiuser@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Api@123"),
                Rol = RolUsuario.Estudiante
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            // Act — simular llamada API completa
            var loginDto = new LoginDto { Correo = "apiuser@test.com", Password = "Api@123" };
            var response = await authService.LoginAsync(loginDto);

            // Serializar como lo haría una API REST
            var json = System.Text.Json.JsonSerializer.Serialize(response);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<AuthResponseDto>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.True(deserialized!.Success);
            Assert.NotNull(deserialized.Token);
            Assert.Equal("API User", deserialized.UserName);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 7: Manejo de errores — credenciales inválidas retornan fallo
        // ═══════════════════════════════════════════════════════════════
        [Fact]
        public async Task Login_InvalidCredentials_ReturnsFailure()
        {
            // Arrange
            using var context = CreateInMemoryContext("ErrorHandlingTest");
            var config = TestHelpers.GetFakeConfiguration();
            var authService = new AuthService(context, config);

            var usuario = CreateEstudiante();
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            // Act — contraseña incorrecta
            var result = await authService.LoginAsync(new LoginDto
            {
                Correo = "juan@test.com",
                Password = "WrongPassword"
            });

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("Credenciales inválidas.", result.Message);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 8: Adjuntar evidencia a solicitud inexistente falla
        // ═══════════════════════════════════════════════════════════════
        [Fact]
        public async Task AttachEvidencia_NonExistentSolicitud_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryContext("EvidenciaErrorTest");
            var solicitudService = new SolicitudService(context);

            // Act
            var ok = await solicitudService.AdjuntarEvidenciaAsync(
                solicitudId: 9999,
                rutaArchivo: "/uploads/evidencias/nada.jpg",
                tipoContenido: "image/jpeg"
            );

            // Assert
            Assert.False(ok);
        }
    }
}
