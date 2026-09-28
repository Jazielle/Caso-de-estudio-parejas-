CREATE TABLE "Usuarios" (
    "Id"           SERIAL       PRIMARY KEY,
    "Nombre"       VARCHAR(150) NOT NULL,
    "Correo"       VARCHAR(200) NOT NULL UNIQUE,
    "PasswordHash" TEXT         NOT NULL,
    "Rol"          INTEGER      NOT NULL
);

CREATE TABLE "Recursos" (
    "Id"        SERIAL        PRIMARY KEY,
    "Nombre"    VARCHAR(200)  NOT NULL,
    "Categoria" INTEGER       NOT NULL,
    "Estado"    VARCHAR(100)  NOT NULL DEFAULT 'Disponible',
    "Detalles"  VARCHAR(1000)
);

CREATE TABLE "Solicitudes" (
    "Id"                 SERIAL        PRIMARY KEY,
    "Titulo"             VARCHAR(200)  NOT NULL,
    "Descripcion"        VARCHAR(2000) NOT NULL,
    "Estado"             INTEGER       NOT NULL DEFAULT 1,
    "Prioridad"          INTEGER       NOT NULL DEFAULT 2,
    "FechaCreacion"      TIMESTAMP     NOT NULL DEFAULT NOW(),
    "FechaActualizacion" TIMESTAMP     NOT NULL DEFAULT NOW(),
    "EstudianteId"       INTEGER       NOT NULL REFERENCES "Usuarios"("Id") ON DELETE RESTRICT,
    "ResponsableId"      INTEGER       REFERENCES "Usuarios"("Id") ON DELETE SET NULL,
    "RecursoId"          INTEGER       REFERENCES "Recursos"("Id") ON DELETE SET NULL
);

CREATE TABLE "Evidencias" (
    "Id"            SERIAL       PRIMARY KEY,
    "SolicitudId"   INTEGER      NOT NULL REFERENCES "Solicitudes"("Id") ON DELETE CASCADE,
    "RutaArchivo"   VARCHAR(500) NOT NULL,
    "TipoContenido" VARCHAR(100) NOT NULL,
    "FechaSubida"   TIMESTAMP    NOT NULL DEFAULT NOW()
);

CREATE TABLE "Seguimientos" (
    "Id"             SERIAL        PRIMARY KEY,
    "SolicitudId"    INTEGER       NOT NULL REFERENCES "Solicitudes"("Id") ON DELETE CASCADE,
    "UsuarioId"      INTEGER       NOT NULL REFERENCES "Usuarios"("Id") ON DELETE RESTRICT,
    "Comentario"     VARCHAR(2000) NOT NULL,
    "EstadoAnterior" INTEGER,
    "EstadoNuevo"    INTEGER,
    "Fecha"          TIMESTAMP     NOT NULL DEFAULT NOW()
);

INSERT INTO "Usuarios" ("Nombre", "Correo", "PasswordHash", "Rol") VALUES
('Administrador Sistema', 'admin@campusconnect.edu', '$2a$11$K7bFqJz1XqVw2mN3oP4tReKnL8mYxCvBuThWqZdMnPjRsSeLfAiOu', 2),
('Tecnico General', 'tecnico@campusconnect.edu', '$2a$11$K7bFqJz1XqVw2mN3oP4tReKnL8mYxCvBuThWqZdMnPjRsSeLfAiOu', 3);

INSERT INTO "Recursos" ("Nombre", "Categoria", "Estado", "Detalles") VALUES
('Laboratorio de Computacion 101', 1, 'Disponible', 'Sala con 30 equipos Dell Optiplex'),
('Proyector Aula Magna', 2, 'Disponible', 'Epson 3500 lumens, HDMI y VGA'),
('Red WiFi Campus Norte', 3, 'Disponible', 'Cobertura edificio norte, 100 Mbps'),
('Aire Acondicionado Sala B', 2, 'En mantenimiento', 'LG 18000 BTU'),
('Impresora Biblioteca', 2, 'Disponible', 'HP LaserJet Pro, planta 1'),
('Cancha Deportiva Principal', 1, 'Disponible', 'Cancha multiusos'),
('Servicio de Fotocopiado', 3, 'Disponible', 'Lunes a viernes 7am - 6pm');

INSERT INTO "Solicitudes" ("Titulo", "Descripcion", "Estado", "Prioridad", "EstudianteId", "ResponsableId", "RecursoId") VALUES
('Computadora sin encender en Lab 101', 'El equipo 15 no enciende desde el lunes.', 1, 3, 1, NULL, 1),
('Fuga de agua en corredor B', 'Fuga visible en edificio B, segundo piso.', 2, 3, 1, 2, NULL),
('Silla rota Aula 205', 'La silla del docente tiene una pata rota.', 3, 1, 1, 2, NULL),
('Proyector sin imagen', 'El proyector enciende pero no proyecta imagen.', 4, 2, 1, 2, 2);

INSERT INTO "Seguimientos" ("SolicitudId", "UsuarioId", "Comentario", "EstadoAnterior", "EstadoNuevo") VALUES
(1, 1, 'Solicitud recibida y registrada.', NULL, 1),
(2, 2, 'Asignada al tecnico para revision.', 1, 2),
(3, 2, 'Tecnico en camino al aula 205.', 2, 3),
(4, 2, 'Problema resuelto, proyector reemplazado.', 3, 4);
