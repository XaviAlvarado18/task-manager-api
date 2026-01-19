# Task Manager API (.NET + SQL Server)

<!-- Tech Badges -->
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![ASP.NET%20Core](https://img.shields.io/badge/ASP.NET_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C%23](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Entity%20Framework%20Core](https://img.shields.io/badge/Entity_Framework_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL%20Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)
![Git](https://img.shields.io/badge/Git-F05032?style=for-the-badge&logo=git&logoColor=white)


API REST sencilla para gestionar tareas internas (TaskManager), con endpoints para listar tareas (con orden y filtro) y marcar una tarea como completada.

---

## Requisitos

- .NET SDK 8 (o .NET 6+)
- SQL Server (por ejemplo SQL Express)
- Visual Studio 2022 (o VS Code)
- (Opcional) SQL Server Management Studio (SSMS)

---

## Cómo ejecutar el proyecto

### 1) Crear la base de datos y la tabla

Ejecuta este script en SQL Server:

```sql
IF DB_ID('TaskManager') IS NULL
    CREATE DATABASE TaskManager;
GO

USE TaskManager;
GO

IF OBJECT_ID('dbo.Tasks', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tasks
    (
        TaskId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tasks PRIMARY KEY,
        Title       NVARCHAR(150)      NOT NULL,
        IsCompleted BIT                NOT NULL CONSTRAINT DF_Tasks_IsCompleted DEFAULT (0),
        CreatedAt   DATETIME           NOT NULL CONSTRAINT DF_Tasks_CreatedAt   DEFAULT (GETDATE())
    );
END
GO

INSERT INTO dbo.Tasks (Title, IsCompleted, CreatedAt) VALUES
(N'Revisar requerimientos del sprint', 0, DATEADD(DAY, -5, GETDATE())),
(N'Configurar proyecto Web API',       1, DATEADD(DAY, -4, GETDATE())),
(N'Crear modelo y DbContext',          0, DATEADD(DAY, -3, GETDATE())),
(N'Implementar GET /api/tasks',        1, DATEADD(DAY, -2, GETDATE())),
(N'Implementar PATCH completar tarea', 0, DATEADD(DAY, -1, GETDATE())),
(N'Escribir README.md',                0, DATEADD(HOUR, -12, GETDATE()));
GO
```

## 2) Configurar la conexión a base de datos

En `appsettings.json` agrega/ajusta (ejemplo con SQL Express y autenticación integrada):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR\\TU_INSTANCIA;Database=TaskManager;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> **Importante:** Reemplaza `TU_SERVIDOR\\TU_INSTANCIA` por tu instancia real (por ejemplo `MI_PC\\SQLEXPRESS`).  

---

## 3) Ejecutar la API

- Desde Visual Studio: **F5** / **Run**
- Se abrirá Swagger en: `https://localhost:<puerto>/swagger`

---

## Endpoints disponibles

### 1) Listar tareas (con orden y filtro)

- **GET** `/api/tasks`  
  Devuelve todas las tareas ordenadas por `CreatedAt` descendente.

- **GET** `/api/tasks?isCompleted=false`  
  Devuelve tareas no completadas ordenadas por `CreatedAt` descendente.

- **GET** `/api/tasks?isCompleted=true`  
  Devuelve tareas completadas ordenadas por `CreatedAt` descendente.

### 2) Marcar tarea como completada

- **PATCH** `/api/tasks/{id}/complete`

**Respuesta:**
- `200 OK` con mensaje simple si se actualiza (o si ya estaba completada).
- `404 NotFound` con mensaje simple si no existe la tarea.

---

## Supuestos realizados

- Si el parámetro `isCompleted` no se envía, el endpoint `GET /api/tasks` devuelve todas las tareas.
- El endpoint para completar tareas es **idempotente**:
  - Si la tarea ya está completada, retorna `200 OK` con un mensaje indicando que ya estaba completada.
- Se usa SQL Server con autenticación integrada (Windows) en entorno local.
- Se usa el modelo `TaskItem` para evitar conflicto con `Task` de .NET.

---

## Caso de soporte

**Problema reportado:**  
“Algunas tareas no aparecen cuando se listan como no completadas.”

### Posible causa

Una causa común es inconsistencia en los datos, por ejemplo:

- La columna `IsCompleted` permite `NULL` (si fue creada sin `NOT NULL`), y el filtro `IsCompleted == false` no traerá registros con `NULL`.
- Otra posibilidad: el filtro se implementó con una condición incorrecta (`!= true` o lógica invertida) o el query param se interpreta mal.

### Cómo se resolvería

**A nivel de base de datos:**
- Asegurar que `IsCompleted` sea `NOT NULL` con `DEFAULT 0`.
- Normalizar datos existentes: actualizar `NULL` a `0`.

**A nivel de API:**
- Asegurar que el filtro sea explícito:
  - `IsCompleted == false` para no completadas.
- Agregar logging y pruebas para validar resultados esperados.

---

## Posibles mejoras futuras

- Paginación y parámetros adicionales (búsqueda por título, rangos de fechas).
- Uso de DTOs (evitar exponer entidades directamente).
- Validaciones más robustas (longitud de título, contenido vacío, etc.).
- Manejo global de errores (middleware) con respuestas consistentes.
- Pruebas unitarias e integración (por ejemplo con una DB de prueba).
- Autenticación/autorización si el servicio se expone a más usuarios.
- Observabilidad: logging estructurado, métricas y tracing.

---

## Parte 4 - Teoría

### ¿Qué fue fácil y qué fue difícil dentro del ejercicio?

- **Fácil:** estructurar endpoints REST simples (GET con filtro/orden y PATCH para actualizar estado) y probarlos con Swagger.
- **Difícil:** resolver la configuración de dependencias/paquetes (EF Core y NuGet) y alinear versiones con el framework, además de asegurar una conexión local estable a SQL Server.

### ¿Cómo validas cambios en producción?

- Validación previa en entornos: desarrollo → QA/staging → producción.
- Pipeline con build, análisis estático (si aplica), pruebas automatizadas (unitarias/integración).
- Deploy controlado con versionado, checklist y smoke tests post-deploy.
- Monitoreo (logs/métricas) y plan de rollback si hay impacto.

### ¿Qué harías si un usuario reporta un error urgente?

- Confirmar impacto/alcance (severidad, usuarios afectados, si es bloqueante).
- Reproducir el problema con pasos claros y revisar logs/telemetría.
- Si hay workaround, comunicarlo inmediatamente.
- Preparar hotfix con mínimo riesgo, validar en staging y desplegar.
- Comunicar avance y cierre (post-mortem si aplica).

### ¿Qué esperas aprender en este rol?

- Mejorar en diagnóstico de incidentes reales (soporte técnico) y su resolución con buenas prácticas.
- Profundizar en .NET + SQL Server en un entorno productivo (observabilidad, performance, mantenibilidad).
- Fortalecer procesos de despliegue y calidad (pruebas, validación en producción, documentación).

