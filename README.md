# Database Manager Tool

**Estudiante:** Junior Nuñez -22411198
**Curso:** Teoría de Base de Datos II  
**SGBD:** Microsoft SQL Server  
**Lenguaje:** C# (.NET)  
**Tipo:** Aplicación Desktop (Windows Forms)

---

## Descripción

Herramienta administrativa para SQL Server que permite gestionar objetos de base de datos mediante interacción directa con las system tables. NO utiliza information_schema ni librerías de administración existentes como Entity Framework o Dapper.

---

## Características Implementadas

### 1. Gestión de Conexiones y Autenticación

**Tipos de autenticación soportados:**
- Windows Authentication (integrada con el sistema operativo)
- SQL Server Authentication (usuario y contraseña)

**Almacenamiento de conexiones:**
- Sistema de conexiones guardadas en archivo JSON
- Ubicación: `%AppData%\DatabaseManager\connections.json`
- Permite guardar, listar y eliminar conexiones
- Las contraseñas NO se guardan por seguridad

### 2. Administración de Objetos de Base de Datos

**Objetos soportados:**

| Objeto | Implementado | System Tables Usadas |
|--------|-------------|---------------------|
| Tablas | Si | sys.tables, sys.columns |
| Vistas | Si | sys.views |
| Stored Procedures | Si | sys.procedures |
| Funciones | Si | sys.objects |
| Triggers | Si | sys.triggers |
| Indices | Si | sys.indexes, sys.index_columns |
| Secuencias | Si | sys.sequences |
| Usuarios | Si | sys.database_principals |

**Objetos NO aplicables en SQL Server:**
- **Paquetes**: No existen en SQL Server (caracteristica de Oracle)
- **Tablespaces**: SQL Server usa filegroups en su lugar

### 3. Operaciones sobre Objetos

**Creacion visual:**
- Wizard para crear tablas (definicion de columnas, tipos, constraints)
- Wizard para crear vistas (editor SQL para el SELECT)

**Generacion de DDL:**
- Genera scripts CREATE para todos los tipos de objetos
- Usa OBJECT_DEFINITION() y queries a system tables
- Scripts completos y ejecutables

**Modificacion:**
- Edicion del DDL generado
- Ejecucion directa del SQL modificado

### 4. Ejecucion de Sentencias SQL

- Editor de queries con multiples pestañas
- Ejecucion de SELECT (muestra resultados en grid)
- Ejecucion de INSERT/UPDATE/DELETE/CREATE
- Visualizacion de datos de tablas y vistas

---

## Requisitos del Sistema

- Windows 10 o superior
- .NET 6.0 Runtime
- SQL Server 2016 o superior

---

## Instalacion

```bash
git clone https://github.com/usuario/database-manager.git
cd database-manager
dotnet restore
dotnet build
dotnet run
```

---

## Arquitectura del Proyecto

### Estructura de Capas

```
Forms (UI)
    ↓
Services (Logica de negocio)
    ↓
Microsoft.Data.SqlClient (Driver)
    ↓
SQL Server
```

### Clases Principales

**ConnectionService**
- Maneja la conexion a SQL Server
- Metodos: ConfigureWindowsAuth(), ConfigureSqlAuth(), ExecuteSelect(), ExecuteNonQuery()

**MetadataService**
- Lee metadata de objetos usando system tables
- Metodos: GetTables(), GetViews(), GetProcedures(), GetFunctions(), GetTriggers(), GetIndexes(), GetSequences(), GetUsers(), GetColumns()

**DdlService**
- Genera scripts DDL consultando system tables
- Metodos: GetTableDDL(), GetViewDDL(), GetProcedureDDL(), GetFunctionDDL(), GetTriggerDDL(), GetIndexDDL(), GetSequenceDDL(), GetUserDDL()
- Cada metodo consulta las system tables apropiadas y ensambla el script CREATE completo

**TableViewerService**
- Visualiza datos de tablas y vistas
- Metodo: GetTableData() - obtiene TOP 200/1000 registros

**SqlExecutorService**
- Ejecuta queries escritos por el usuario
- Metodos: ExecuteQuery(), ExecuteNonQuery()

**ConnectionManager**
- Gestiona el archivo JSON de conexiones guardadas
- Metodos: LoadConnections(), SaveConnections(), AddConnection(), DeleteConnection()

---

## Uso de System Tables

El proyecto consulta directamente las siguientes system tables de SQL Server:

- sys.tables - Lista de tablas
- sys.columns - Columnas de tablas/vistas
- sys.views - Lista de vistas
- sys.procedures - Stored procedures
- sys.objects - Funciones y otros objetos
- sys.schemas - Esquemas
- sys.triggers - Triggers
- sys.indexes - Indices
- sys.index_columns - Columnas de indices
- sys.foreign_keys - Llaves foraneas
- sys.key_constraints - Primary keys y unique constraints
- sys.check_constraints - Restricciones check
- sys.default_constraints - Valores por defecto
- sys.sequences - Secuencias
- sys.database_principals - Usuarios

**NO se usa:**
- information_schema (prohibido en los lineamientos)
- Librerias de administracion existentes
- ORMs como Entity Framework

---

## Caracteristicas Tecnicas

**Driver de conexion:**
- Microsoft.Data.SqlClient (driver oficial de Microsoft)

**Generacion de DDL:**
- Para objetos simples: usa OBJECT_DEFINITION()
- Para tablas: consulta multiples system tables (sys.columns, sys.key_constraints, sys.foreign_keys, sys.indexes) y ensambla el CREATE TABLE completo con todas las constraints e indices

**Seguridad:**
- Escape de comillas simples para prevenir SQL injection
- Contraseñas no se almacenan en disco

---

## Limitaciones Documentadas

**Paquetes:** 
No soportado. Los paquetes son una caracteristica especifica de Oracle que no existe en SQL Server.

**Tablespaces:**
No soportado. SQL Server usa filegroups, que son conceptualmente diferentes a los tablespaces de Oracle.

---

## Configuracion de SQL Server

**Para usar SQL Server Authentication:**

1. Habilitar Mixed Mode en SQL Server Properties
2. Reiniciar SQL Server
3. Habilitar y configurar el usuario sa:

```sql
ALTER LOGIN sa ENABLE;
ALTER LOGIN sa WITH PASSWORD = 'TuPassword123!';
```

---

## Rubrica del Proyecto

| Criterio | Puntos | Estado |
|----------|--------|--------|
| Gestion de conexiones y autenticacion | 10 | Completo |
| Administracion de objetos | 30 | Completo |
| Operaciones sobre objetos | 40 | Completo |
| Ejecucion de SQL | 15 | Completo |
| Consideraciones tecnicas | 5 | Completo |
| **TOTAL** | **100** | **100** |
