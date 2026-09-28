# Campus Connect

Sistema universitario centralizado para gestión de solicitudes estudiantiles (mantenimiento, soporte, infraestructura).

## Stack
- **Backend/API:** .NET 8, C#
- **Frontend:** Razor Views, Bootstrap 5
- **Base de datos:** PostgreSQL + Entity Framework Core (Code-First)
- **Pruebas:** xUnit + Moq (9/9 passing ✅)
- **CI/CD:** GitHub Actions

## Cómo correr

```bash
# 1. Crear la BD en PostgreSQL
createdb CampusConnectDB

# 2. Aplicar migraciones
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project CampusConnect.Infrastructure --startup-project CampusConnect.Web
dotnet ef database update --project CampusConnect.Infrastructure --startup-project CampusConnect.Web

# 3. Correr la aplicación
dotnet run --project CampusConnect.Web
```

## Credenciales demo
| Rol | Correo | Contraseña |
|---|---|---|
| Administrativo | admin@campusconnect.edu | Admin@123 |
| Técnico | tecnico@campusconnect.edu | Tecnico@123 |
| Estudiante | Regístrate en /Auth/Register | — |

## Estructura
```
CampusConnect.Core/          → Modelos, DTOs, Interfaces, Enums
CampusConnect.Infrastructure/ → DbContext, AuthService, SolicitudService, RecursoService
CampusConnect.Web/           → Controllers MVC + API REST, Vistas Razor
CampusConnect.Tests/         → 9 pruebas xUnit
.github/workflows/ci.yml     → Pipeline CI/CD
database_schema.sql          → Script SQL manual
```
