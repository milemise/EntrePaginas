# Entre Páginas — Plataforma de libros digitales

**Entre Páginas** es una aplicación web educativa desarrollada para integrar los conocimientos de programación web, modelado de datos, persistencia, autenticación, autorización y consumo de servicios externos. La plataforma está orientada a la publicación, compra, alquiler, lectura y valoración de libros digitales.

El sistema contempla tres roles principales: lectores, escritores y administradores. Cada rol dispone de acciones y permisos propios, que se controlan en el servidor.

## 1. Funcionalidades por rol

### Lector

- Crear una cuenta, iniciar y cerrar sesión y personalizar el perfil con un avatar.
- Explorar el catálogo mediante búsqueda por título, autor o descripción, filtros por categoría, orden y paginación.
- Consultar el detalle de una obra, sus reseñas y los vínculos con otros libros de una saga.
- Registrar compras y acceder a los libros adquiridos mientras la compra no haya sido devuelta.
- Alquilar libros por períodos de 7, 15 o 30 días y leerlos en línea mientras el alquiler esté vigente.
- Solicitar la devolución de una compra durante los siete días siguientes a su fecha.
- Publicar una reseña con una puntuación de 1 a 5 después de comprar o alquilar una obra, con opción de anonimato.
- Recibir advertencias y restricciones de acceso al contenido marcado como +18 cuando no cumple la edad requerida.
- Consultar y filtrar sus notificaciones internas, marcarlas como leídas, guardarlas como favoritas o eliminarlas.
- Buscar términos en el diccionario literario y guardar palabras favoritas en su cuenta.

### Escritor

- Publicar libros y adjuntar archivos PDF, DOC o DOCX junto con una portada.
- Definir precios de compra y alquiler, seleccionar categoría y marcar obras +18.
- Buscar referencias bibliográficas mediante Open Library para completar los datos de una obra.
- Relacionar una nueva publicación con el libro anterior de una saga.
- Buscar, consultar, editar o eliminar sus propias obras, respetando las restricciones asociadas a las operaciones registradas.
- Consultar estadísticas de publicaciones, compras, alquileres e ingresos registrados, con búsqueda y paginación.
- Recibir notificaciones internas sobre nuevas reseñas y novedades relacionadas con sus obras.
- Reportar reseñas para que sean revisadas por la administración.

### Administrador

- Consultar usuarios y buscar cuentas por nombre, usuario, correo o DNI.
- Modificar roles y crear nuevas cuentas administrativas.
- Gestionar categorías desde un módulo SPA con Vue.js y AJAX.
- Consultar, buscar y paginar reseñas reportadas, desestimar reportes y moderar contenido.
- Recibir notificaciones cuando se reportan reseñas.

## 2. Modelo de datos

La base de datos se denomina `entre_paginas` y está preparada para MySQL/MariaDB. Entity Framework Core se utiliza como ORM para relacionar las entidades con las tablas y realizar las operaciones de persistencia.

| Entidad | Responsabilidad |
|---|---|
| **Usuarios** | Datos personales, credenciales, rol, fecha de nacimiento y avatar. |
| **Categorías** | Géneros y tipos de libros disponibles en el catálogo. |
| **Libros** | Información bibliográfica, descripción, precios, portada, archivo, escritor, categoría y relación con una entrega anterior. |
| **Compras** | Libro adquirido, lector, fecha, importe y estado de devolución. |
| **Alquileres** | Libro alquilado, lector, fechas, importe y estado del alquiler. |
| **Reseñas** | Puntuación, comentario, anonimato y estado de reporte. |
| **Palabras favoritas** | Términos del diccionario que cada usuario guarda en su colección personal. |
| **Notificaciones** | Mensajes internos asociados a una cuenta, con fecha, estado de lectura, favorito y destino opcional. |

### Relaciones principales

- Un escritor puede publicar muchos libros; cada libro pertenece a una categoría y a un escritor.
- Un lector puede registrar muchas compras, alquileres y reseñas.
- Un libro puede aparecer en muchas compras, alquileres y reseñas.
- Un libro puede apuntar a otro libro como entrega anterior de una saga.
- Un usuario puede guardar muchas palabras favoritas y recibir muchas notificaciones.

Las relaciones entre entidades están configuradas en el contexto de Entity Framework Core y respaldadas por claves foráneas en la base de datos.

## 3. Reglas de negocio

### Compras y devoluciones

Las compras se registran con su fecha e importe. La devolución solo se permite dentro de los siete días posteriores a la compra. El servidor comprueba el plazo y el estado de la operación antes de autorizar el cambio. Una compra devuelta deja de habilitar la descarga y la lectura.

### Alquileres

Cada alquiler guarda su fecha de inicio y vencimiento. La lectura se habilita únicamente mientras el alquiler está vigente. El alquiler no habilita la descarga del archivo; la lectura embebida en el navegador está disponible para documentos PDF.

### Reseñas

Solo un usuario con rol de lector que haya tenido acceso al libro por compra o alquiler puede reseñarlo. La puntuación permitida va de 1 a 5 y se limita a una reseña por lector y libro. El lector puede elegir el anonimato. Los escritores pueden reportar reseñas de sus propias obras y la administración puede revisarlas.

### Contenido +18

Para las obras marcadas como +18 se calcula la edad a partir de la fecha de nacimiento guardada en la cuenta. Si no se cumple el requisito, el servidor bloquea las acciones de compra, alquiler, lectura y descarga del contenido.

### Sagas

Una obra puede estar relacionada con un libro anterior. La primera entrega puede dejar la relación vacía. La selección del libro anterior se realiza mediante una búsqueda en el servidor, y se validan las relaciones para impedir ciclos y referencias inválidas.

### Notificaciones

Las notificaciones se almacenan en la base de datos y se muestran dentro de la aplicación. - Recibir notificaciones internas sobre nuevas compras, alquileres y reseñas relacionadas con sus obras.

## 4. Catálogo y Open Library

El catálogo permite buscar libros, filtrar por categoría, ordenar resultados y navegar mediante paginación del lado del servidor.

En el formulario de publicación, Open Library se utiliza como asistente para consultar metadatos bibliográficos (por ejemplo, título, autor, año, identificador y portada cuando está disponible). El escritor puede revisar y corregir las sugerencias. Esta integración no descarga ni concede permisos sobre el texto íntegro de una obra: el escritor debe adjuntar el archivo que tiene autorización para distribuir.

Documentación de la API: https://openlibrary.org/developers/api

## 5. Diccionario literario interactivo

El diccionario se presenta como un libro virtual con páginas que pueden pasarse mediante controles, teclado o gestos táctiles. Incluye un índice de términos literarios con definiciones y ejemplos preparados para el proyecto. Para otras consultas utiliza Wikcionario en español y, como fuente de respaldo, un servicio externo de consulta del Diccionario de la lengua española cuando está disponible.

Las búsquedas más recientes se conservan en caché para reducir solicitudes externas. Si las fuentes externas no responden, el índice literario local sigue disponible. Los usuarios autenticados pueden guardar palabras favoritas, consultarlas desde su colección y buscar dentro de ella.

## 6. Tecnologías

- ASP.NET Core MVC y C# para la aplicación.
- Entity Framework Core para el acceso a datos.
- MySQL/MariaDB para la persistencia.
- HTML, CSS y Bootstrap para las vistas y el diseño adaptable.
- JavaScript y Vue.js para interacciones del lado del cliente.
- AJAX y JSON para operaciones asíncronas.
- BCrypt para almacenar hashes de contraseñas.
- JWT para autenticar los endpoints de la API REST.
- Open Library API, Wikcionario y un servicio externo de consulta del Diccionario de la lengua española para las búsquedas bibliográficas y léxicas.
- Postman para probar los endpoints protegidos con JWT.

## 7. Operaciones asíncronas, búsqueda y paginación

Los listados principales se paginan desde el backend, de modo que cada solicitud recupera solo la página necesaria. Las búsquedas de entidades relacionadas —como la elección del libro anterior de una saga— se consultan de forma asíncrona y no requieren descargar todos los registros al navegador.

El módulo de categorías está implementado como SPA con Vue.js: permite listar, crear, editar y eliminar categorías mediante AJAX/JSON sin recargar la página. Incorpora búsqueda local sobre el conjunto reducido de categorías, que funciona como tabla maestra.

## 8. API REST con JWT

La API REST incluye endpoints autenticados mediante tokens Bearer. El token se obtiene en `/api/auth/token` y se envía en la cabecera `Authorization: Bearer <token>` para acceder a los endpoints protegidos.

| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/auth/token` | Valida credenciales y entrega un JWT. |
| GET | `/api/libros?pagina=1&tamanoPagina=10` | Lista libros con paginación y filtros opcionales. |
| GET | `/api/libros/{id}` | Obtiene el detalle de un libro. |
| GET | `/api/libros/mi-biblioteca/compras` | Consulta paginada de las compras del lector autenticado. |
| GET | `/api/categorias` | Consulta las categorías. |
| POST | `/api/categorias` | Crea una categoría; requiere rol Administrador. |

La colección de pruebas se encuentra en `postman/EntrePaginas.postman_collection.json`.

## 9. Preparación y ejecución local

Requisitos: SDK de .NET 10 y MySQL/MariaDB. Para una instalación habitual con XAMPP se puede utilizar MariaDB administrado desde phpMyAdmin.

1. Iniciar el servicio de base de datos.
2. Crear la base `entre_paginas` con codificación `utf8mb4`.
3. Importar `database/entre_paginas.sql` desde phpMyAdmin. El archivo contiene la estructura sin datos personales; los datos de demostración se crean desde el inicializador de la aplicación.
4. Revisar `ConnectionStrings:DefaultConnection` en `appsettings.json` y configurarla para el usuario, contraseña y puerto de la instalación local.
5. Desde la carpeta que contiene `EntrePaginas.csproj`, ejecutar:

```bash
dotnet restore
dotnet build
dotnet run
```

El proyecto está preparado por defecto para MariaDB 10.4.32 en `localhost:3306`, con el usuario `root` y contraseña vacía, configuración habitual pero no universal de XAMPP. Si la instalación utiliza una contraseña, hay que actualizar la cadena de conexión.

### Cuentas de prueba

Cuando se inicializa una base que no contiene esas cuentas, el sistema crea:

| Rol | Usuario | Contraseña de desarrollo |
|---|---|---|
| Administrador | `admin` | `Admin123!` |
| Escritor | `escritor_demo` | `Escritor123!` |
| Lector | `lector_demo` | `Lector123!` |


## 10. Archivos y seguridad

Los libros digitales se guardan en `Storage/Libros`, fuera de `wwwroot`, y se sirven a través de acciones que verifican los permisos del usuario. Las portadas y avatares se guardan dentro de `wwwroot/uploads`. Las cargas validan extensión, tamaño y firma básica del archivo. Los formularios que modifican datos incluyen protección antifalsificación y las operaciones sensibles se restringen por rol.

Los PDF que acompañan al proyecto son muestras breves preparadas para las pruebas de funcionamiento; no sustituyen el contenido de obras completas ni una pasarela de pago real. Las compras y alquileres representan operaciones simuladas para el entorno educativo.

## 11. Estructura del proyecto

- `Controllers/`: controladores MVC y endpoints de la API.
- `Data/`: contexto de Entity Framework Core e inicialización de datos.
- `Models/`: entidades del sistema.
- `ViewModels/`: modelos para formularios y vistas.
- `Services/`: almacenamiento de archivos, diccionario, notificaciones y Open Library.
- `ViewComponents/`: componentes de interfaz, incluido el contador de notificaciones.
- `Views/`: catálogo, cuentas, biblioteca, diccionario, escritores y administración.
- `wwwroot/`: estilos, scripts, imágenes y recursos estáticos.
- `Storage/Libros/`: archivos de lectura privados usados por la aplicación.
- `database/`: estructura SQL y scripts de actualización.
- `postman/`: colección para pruebas de la API.

## 12. Integrantes

- **Lourdes Villegas** — [GitHub](https://github.com/Luly-bitcoin)
- **Milena Miselli** — [GitHub](https://github.com/milemise)

## 13. Objetivo del proyecto

Entre Páginas busca integrar persistencia relacional, reglas de negocio, gestión de permisos, carga de archivos, búsqueda y paginación, operaciones asíncronas, consumo de APIs externas y autenticación con JWT en una plataforma web coherente, accesible y mantenible.
