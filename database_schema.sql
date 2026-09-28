-- ============================================================
-- CAMPUS CONNECT — Script SQL PostgreSQL
-- Ejecutar en pgAdmin o psql sobre la BD "CampusConnectDB"
-- ============================================================

-- 1. TABLA USUARIOS
CREATE TABLE IF NOT EXISTS "Usuarios" (
    "Id"           SERIAL PRIMARY KEY,
    "Nombre"       VARCHAR(150) NOT NULL,
    "Correo"       VARCHAR(200) NOT NULL UNIQUE,
    "PasswordHash" TEXT         NOT NULL,
    "Rol"          INTEGER      NOT NULL  -- 1=Estudiante, 2=Administrativo, 3=ResponsableTecnico
);

-- 2. TABLA RECURSOS
CREATE TABLE IF NOT EXISTS "Recursos" (
    "Id"        SERIAL PRIMARY KEY,
    "Nombre"    VARCHAR(200) NOT NULL,
    "Categoria" INTEGER      NOT NULL,   -- 1=Infraestructura, 2=Equipamiento, 3=Servicio
    "Estado"    VARCHAR(100) NOT NULL DEFAULT 'Disponible',
    "Detalles"  VARCHAR(1000)
);

-- 3. TABLA SOLICITUDES
CREATE TABLE IF NOT EXISTS "Solicitudes" (
    "Id"                SERIAL PRIMARY KEY,
    "Titulo"            VARCHAR(200) NOT NULL,
    "Descripcion"       VARCHAR(2000) NOT NULL,
    "Estado"            INTEGER NOT NULL DEFAULT 1,  -- 1=Pendiente, 2=Asignada, 3=EnProceso, 4=Cerrada
    "Prioridad"         INTEGER NOT NULL DEFAULT 2,  -- 1=Baja, 2=Media, 3=Alta
    "FechaCreacion"     TIMESTAMP NOT NULL DEFAULT NOW(),
    "FechaActualizacion" TIMESTAMP NOT NULL DEFAULT NOW(),
    "EstudianteId"      INTEGER NOT NULL REFERENCES "Usuarios"("Id") ON DELETE RESTRICT,
    "ResponsableId"     INTEGER          REFERENCES "Usuarios"("Id") ON DELETE SET NULL,
    "RecursoId"         INTEGER          REFERENCES "Recursos"("Id") ON DELETE SET NULL
);

-- 4. TABLA EVIDENCIAS
CREATE TABLE IF NOT EXISTS "Evidencias" (
    "Id"            SERIAL PRIMARY KEY,
    "SolicitudId"   INTEGER      NOT NULL REFERENCES "Solicitudes"("Id") ON DELETE CASCADE,
    "RutaArchivo"   VARCHAR(500) NOT NULL,
    "TipoContenido" VARCHAR(100) NOT NULL,
    "FechaSubida"   TIMESTAMP    NOT NULL DEFAULT NOW()
);

-- 5. TABLA SEGUIMIENTO
CREATE TABLE IF NOT EXISTS "Seguimientos" (
    "Id"             SERIAL PRIMARY KEY,
    "SolicitudId"    INTEGER       NOT NULL REFERENCES "Solicitudes"("Id") ON DELETE CASCADE,
    "UsuarioId"      INTEGER       NOT NULL REFERENCES "Usuarios"("Id") ON DELETE RESTRICT,
    "Comentario"     VARCHAR(2000) NOT NULL,
    "EstadoAnterior" INTEGER,
    "EstadoNuevo"    INTEGER,
    "Fecha"          TIMESTAMP     NOT NULL DEFAULT NOW()
);

-- ============================================================
-- DATOS INICIALES (Seed Data)
-- Contraseñas hasheadas con BCrypt (Admin@123 / Tecnico@123)
-- ============================================================
INSERT INTO "Usuarios" ("Nombre", "Correo", "PasswordHash", "Rol")
VALUES
  ('Administrador Sistema',
   'admin@campusconnect.edu',
   '$2a$11$example_hash_admin_replace_with_real_bcrypt',
   2),
  ('Técnico General',
   'tecnico@campusconnect.edu',
   '$2a$11$example_hash_tecnico_replace_with_real_bcrypt',
   3)
ON CONFLICT ("Correo") DO NOTHING;

-- Recursos de ejemplo
INSERT INTO "Recursos" ("Nombre", "Categoria", "Estado", "Detalles") VALUES
  ('Laboratorio de Computación 101', 1, 'Disponible', 'Sala con 30 equipos Dell'),
  ('Proyector Sala A',              2, 'En mantenimiento', 'Proyector Epson 3500 lumens'),
  ('Red WiFi Campus Norte',         3, 'Disponible', 'Cobertura edificio norte'),
  ('Aula Magna',                    1, 'Disponible', 'Capacidad 200 personas'),
  ('Impresora Biblioteca',          2, 'Disponible', 'HP LaserJet, planta 1')
ON CONFLICT DO NOTHING;

-- ============================================================
-- NOTA IMPORTANTE:
-- Los hashes de contraseña del seed arriba son EJEMPLO.
-- El sistema usa EF Core Migrations + BCrypt automáticamente.
-- Usa preferiblemente: dotnet ef database update
-- ============================================================
