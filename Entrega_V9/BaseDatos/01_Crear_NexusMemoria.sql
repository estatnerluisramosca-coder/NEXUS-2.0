/* Ejecutar una vez en SQL Server Management Studio o Azure Data Studio. */
IF DB_ID(N'NexusMemoria') IS NULL EXEC(N'CREATE DATABASE NexusMemoria');
GO
USE NexusMemoria;
GO

IF OBJECT_ID(N'dbo.Perfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfiles (
        PerfilId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Perfiles PRIMARY KEY,
        Nombre NVARCHAR(60) NOT NULL CONSTRAINT UQ_Perfiles_Nombre UNIQUE,
        FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Perfiles_Fecha DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID(N'dbo.Cadetes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cadetes (
        CadeteId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Cadetes PRIMARY KEY,
        PerfilId INT NOT NULL CONSTRAINT UQ_Cadetes_Perfil UNIQUE,
        Nombre NVARCHAR(60) NOT NULL,
        Salud INT NOT NULL CONSTRAINT DF_Cadetes_Salud DEFAULT 100,
        Energia INT NOT NULL CONSTRAINT DF_Cadetes_Energia DEFAULT 100,
        Ataque INT NOT NULL CONSTRAINT DF_Cadetes_Ataque DEFAULT 20,
        Experiencia INT NOT NULL CONSTRAINT DF_Cadetes_Experiencia DEFAULT 0,
        Escudos INT NOT NULL CONSTRAINT DF_Cadetes_Escudos DEFAULT 0,
        Materiales INT NOT NULL CONSTRAINT DF_Cadetes_Materiales DEFAULT 0,
        Suministros INT NOT NULL CONSTRAINT DF_Cadetes_Suministros DEFAULT 0,
        FechaActualizacion DATETIME2(0) NOT NULL CONSTRAINT DF_Cadetes_Fecha DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Cadetes_Perfiles FOREIGN KEY (PerfilId) REFERENCES dbo.Perfiles(PerfilId) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.Partidas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Partidas (
        PartidaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Partidas PRIMARY KEY,
        PerfilId INT NOT NULL,
        Muralla VARCHAR(10) NOT NULL,
        Salud INT NOT NULL CONSTRAINT DF_Partidas_Salud DEFAULT 100,
        Energia INT NOT NULL CONSTRAINT DF_Partidas_Energia DEFAULT 100,
        Ataque INT NOT NULL CONSTRAINT DF_Partidas_Ataque DEFAULT 20,
        Experiencia INT NOT NULL CONSTRAINT DF_Partidas_Experiencia DEFAULT 0,
        Escudos INT NOT NULL CONSTRAINT DF_Partidas_Escudos DEFAULT 0,
        Materiales INT NOT NULL CONSTRAINT DF_Partidas_Materiales DEFAULT 0,
        Suministros INT NOT NULL CONSTRAINT DF_Partidas_Suministros DEFAULT 0,
        Nivel INT NOT NULL CONSTRAINT DF_Partidas_Nivel DEFAULT 1,
        Eliminaciones INT NOT NULL CONSTRAINT DF_Partidas_Eliminaciones DEFAULT 0,
        ExperienciaOleada INT NOT NULL CONSTRAINT DF_Partidas_ExpOleada DEFAULT 0,
        TiempoOleada INT NOT NULL CONSTRAINT DF_Partidas_Tiempo DEFAULT 0,
        Fila INT NOT NULL CONSTRAINT DF_Partidas_Fila DEFAULT 1,
        Columna INT NOT NULL CONSTRAINT DF_Partidas_Columna DEFAULT 1,
        JefeDerrotado BIT NOT NULL CONSTRAINT DF_Partidas_Jefe DEFAULT 0,
        Terminada BIT NOT NULL CONSTRAINT DF_Partidas_Terminada DEFAULT 0,
        CasillasProcesadas NVARCHAR(500) NOT NULL CONSTRAINT DF_Partidas_Casillas DEFAULT N'',
        FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Partidas_Creada DEFAULT SYSUTCDATETIME(),
        FechaActualizacion DATETIME2(0) NOT NULL CONSTRAINT DF_Partidas_Actualizada DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Partidas_Perfiles FOREIGN KEY (PerfilId) REFERENCES dbo.Perfiles(PerfilId) ON DELETE CASCADE,
        CONSTRAINT CK_Partidas_Muralla CHECK (Muralla IN ('MARIA','ROSE','SINA')),
        CONSTRAINT CK_Partidas_Salud CHECK (Salud BETWEEN 0 AND 100)
    );
END;
GO

IF OBJECT_ID(N'dbo.Inventario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inventario (
        InventarioId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventario PRIMARY KEY,
        PartidaId INT NOT NULL,
        Nombre NVARCHAR(120) NOT NULL,
        FechaObtencion DATETIME2(0) NOT NULL CONSTRAINT DF_Inventario_Fecha DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Inventario_Partidas FOREIGN KEY (PartidaId) REFERENCES dbo.Partidas(PartidaId) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.EventosPartida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventosPartida (
        EventoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventosPartida PRIMARY KEY,
        PartidaId INT NOT NULL,
        Evento NVARCHAR(240) NOT NULL,
        Fecha DATETIME2(0) NOT NULL CONSTRAINT DF_EventosPartida_Fecha DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_EventosPartida_Partidas FOREIGN KEY (PartidaId) REFERENCES dbo.Partidas(PartidaId) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_Partidas_Perfil_Fecha' AND object_id=OBJECT_ID(N'dbo.Partidas'))
    CREATE INDEX IX_Partidas_Perfil_Fecha ON dbo.Partidas(PerfilId, FechaActualizacion DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_EventosPartida_Partida_Fecha' AND object_id=OBJECT_ID(N'dbo.EventosPartida'))
    CREATE INDEX IX_EventosPartida_Partida_Fecha ON dbo.EventosPartida(PartidaId, Fecha DESC);
GO
