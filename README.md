# Entre Páginas — Plataforma de libros digitales

Aplicación web educativa desarrollada con **ASP.NET Core MVC, C#, Entity Framework Core, MySQL/MariaDB, Bootstrap, Vue.js, AJAX y JWT**. Mantiene la identidad visual del proyecto original y suma persistencia real, autorización por roles y reglas de negocio en el servidor.

## 1. Requisitos

- .NET SDK 10.
- MySQL o MariaDB (el SQL incluido fue preparado para MariaDB 10.4.32).
- Visual Studio 2022 actualizado para .NET 10, Visual Studio Code o una terminal.
- Postman para probar la API JWT.

## 2. Preparar la base de datos

1. Iniciá MySQL/MariaDB.
2. Creá una base de datos llamada `entre_paginas` con codificación `utf8mb4`.
3. Desde phpMyAdmin, seleccioná esa base e importá `database/entre_paginas.sql`.
4. Revisá `ConnectionStrings:DefaultConnection` en `appsettings.json` y configurá servidor, puerto, usuario y contraseña según tu instalación.
5. El proyecto viene configurado para MariaDB 10.4.32. Para MySQL, cambiá `Database:ServerType` a `MySQL` y `Database:ServerVersion` a la versión instalada (por ejemplo, `8.0.36`).
6. La aplicación ejecuta un inicializador al arrancar: agrega `autor` y `open_library_key` si faltan, y carga cuentas, categorías y libros de muestra cuando corresponda.

> Si la base ya tiene las columnas `autor` y `open_library_key`, no necesitás ejecutar `database/actualizar_v2_openlibrary.sql`. Ese archivo es una alternativa manual para una base anterior; ejecutalo una sola vez y solo si esas columnas no existen.

## 3. Ejecutar

Desde la carpeta que contiene `EntrePaginas.csproj`:

```bash
dotnet restore
dotnet run
```

El perfil HTTP utiliza `http://localhost:5006` y el perfil HTTPS `https://localhost:7066`. La primera ejecución necesita que la base de datos esté disponible. Si el inicio registra un error de conexión o inicialización, revisá la cadena de conexión y la consola antes de probar las pantallas.

## 4. Usuarios de prueba

La inicialización crea estas cuentas si no existen:

| Rol | Usuario | Contraseña |
|---|---|---|
| Administrador | `admin` | `Admin123!` |
| Escritor | `escritor_demo` | `Escritor123!` |
| Lector | `lector_demo` | `Lector123!` |

Son credenciales exclusivamente de desarrollo. Cambialas y reemplazá la clave JWT antes de cualquier despliegue real. Las contraseñas se guardan con BCrypt; la aplicación no guarda las contraseñas en texto plano.

## 5. Funcionalidades implementadas

### Visitantes y lectores

- Registro, login, logout y perfil con avatar.
- Catálogo con búsqueda por título, autor o descripción, filtro de categoría, orden y paginación del lado del servidor.
- Detalle de libro, puntuación promedio, reseñas paginadas y enlaces entre entregas de una saga.
- Registro de compra y alquiler de 7, 15 o 30 días.
- Biblioteca separada para compras y alquileres, con paginado desde el backend.
- Descarga habilitada solo para una compra vigente. Devolución dentro de los primeros siete días; después del plazo, la devolución se rechaza.
- Lectura en línea de PDFs si existe una compra o alquiler vigente. Los alquileres no habilitan descarga; la lectura en línea embebida se ofrece para PDF.
- Reseñas de 1 a 5 puntos solo para lectores con compra o alquiler histórico, con opción de anonimato y una reseña por usuario y libro.
- Validación de edad +18 también en las acciones de compra, alquiler, descarga y lectura.

### Escritores

- ABM de libros propios (crear, listar, editar y eliminar).
- Carga de archivo PDF, DOC o DOCX y portada de imagen.
- Precio de compra y alquiler, selección de categoría y marca +18.
- Búsqueda de libros propios por AJAX para relacionar una entrega con el libro anterior de una saga.
- Estadísticas de publicaciones, ventas, alquileres vigentes e ingresos registrados.
- Reporte y eliminación de reseñas de sus obras.

### Administración

- Panel con conteos generales.
- Listado y búsqueda de usuarios con paginación, cambio de rol y eliminación protegida para cuentas sin relaciones históricas.
- Gestión de categorías como SPA en Vue 3: listar, crear, editar y eliminar vía AJAX/JSON sin recargar la página; se bloquea la eliminación si hay libros asociados.
- Revisión de reseñas reportadas.

### API REST con JWT

Los endpoints de `/api/libros` y `/api/categorias` requieren un token Bearer, salvo el endpoint de autenticación. Las listas API reciben `pagina` y `tamanoPagina` y devuelven metadatos de paginación.

- `POST /api/auth/token` — recibe `{ "usuario": "admin", "password": "Admin123!" }` y devuelve `access_token`.
- `GET /api/libros?pagina=1&tamanoPagina=10` — listado paginado.
- `GET /api/libros/{id}` — detalle y resumen de reseñas.
- `GET /api/categorias` — categorías (JWT obligatorio).
- `POST /api/categorias` — crear categoría (solo administrador).
- `GET /api/libros/mi-biblioteca/compras` — compras del lector autenticado con JWT (rol Lector).

Importá `postman/EntrePaginas.postman_collection.json` en Postman. Primero ejecutá **Autenticación / Token administrador** para probar consultas y creación de categorías. Para **Mis compras del lector**, ejecutá antes **Autenticación / Token lector**; ambas solicitudes guardan el token en la variable de colección `token`.

## 6. Open Library

En el formulario de publicación de libros, el asistente consulta la API pública de búsqueda de Open Library a demanda. Permite seleccionar metadatos sugeridos (título, autor, año, referencia y portada cuando existe), que el escritor puede revisar y editar. El resultado no publica ni descarga el texto íntegro de una obra: el escritor debe adjuntar el archivo que tiene permiso para distribuir.

La aplicación realiza una búsqueda limitada a 12 resultados y mantiene los resultados en caché durante diez minutos para reducir solicitudes. Antes de usar Open Library fuera de una prueba local, configurá un correo de contacto propio mediante la variable de entorno `OpenLibrary__ContactEmail`.

Documentación: <https://openlibrary.org/developers/api>.

## 7. Archivos y seguridad

- Los libros se guardan en `Storage/Libros`, fuera de `wwwroot`, para que no se puedan descargar accediendo a una URL pública directa.
- Los avatares y las portadas subidas se guardan bajo `wwwroot/uploads`.
- Se restringen las extensiones, tamaño y firma básica del contenido de los archivos cargados.
- Las pantallas MVC y el módulo AJAX usan protección antifalsificación; los controladores restringen las operaciones por rol.
- Los documentos de muestra de `Storage/Libros` son PDFs breves preparados para pruebas, no son libros digitales completos ni sustituyen las obras reales.

## 8. Consideraciones para la defensa

- La compra y el alquiler representan operaciones registradas en la base de datos para demostrar el flujo; no se integra una pasarela de pago real.
- La lectura dentro del navegador se ofrece para PDF. Un DOC/DOCX adquirido puede descargarse; un DOC/DOCX alquilado no se entrega como descarga.
- El catálogo público usa un catálogo paginado desde EF Core; los géneros disponibles son una tabla maestra pequeña. La búsqueda de sagas consulta el servidor al escribir, sin descargar todos los libros al navegador.
- El secreto JWT que aparece en `appsettings.json` es solo de desarrollo. Para producción hay que usar un secreto fuerte en configuración protegida y credenciales seguras de base de datos.
- Esta entrega no fue compilada ni ejecutada en el entorno de generación porque allí no estaba instalado el SDK de .NET. Ejecutá `dotnet restore` y `dotnet run` localmente y revisá cualquier diferencia específica de tu instalación de MariaDB antes de presentarla.

## 9. Integrantes

- Lourdes Villegas — villegasmarialuly@gmail.com — <https://github.com/Luly-bitcoin>
- Milena Miselli — milivicmp@gmail.com — <https://github.com/milemise>

## 10. Estructura

- `Controllers/` — controladores MVC, reglas de negocio y API REST.
- `Data/` — `DbContext` y carga/inicialización de datos.
- `Models/` — entidades de las seis tablas principales.
- `ViewModels/` — modelos específicos para formularios y vistas.
- `Services/` — carga protegida de archivos y cliente Open Library.
- `Views/` — vistas de catálogo, perfil, biblioteca, administración y escritor.
- `wwwroot/` — estilos, imágenes, Bootstrap incluido en la base y runtime local Vue 3.
- `database/` — script de la base y actualización manual opcional.
- `postman/` — colección de pruebas de la API REST con JWT.
