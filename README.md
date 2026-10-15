# Entre Páginas — Plataforma de libros digitales

**Entre Páginas** es una aplicación web desarrollada como proyecto integrador. Su objetivo es ofrecer una plataforma para la publicación, compra, alquiler y lectura de libros digitales, con distintos permisos según el tipo de usuario.

El sistema permite que los lectores accedan a un catálogo de libros, que los escritores publiquen y administren sus obras y que el administrador supervise los usuarios, las categorías y las reseñas.

El proyecto está desarrollado con ASP.NET Core MVC y C#, con persistencia mediante Entity Framework Core y MySQL/MariaDB. También incorpora Vue.js y AJAX para operaciones dinámicas, una API REST protegida con JWT y la integración con Open Library para consultar información bibliográfica.

## 1. Funcionalidades según el tipo de usuario

### Lector

- Registrarse e iniciar sesión en la plataforma.
- Consultar el catálogo, buscar libros y filtrar por categoría.
- Ver la información de cada obra, su autor, portada, precio y reseñas.
- Comprar libros y acceder a los ejemplares adquiridos.
- Alquilar libros por períodos determinados y acceder a ellos mientras el alquiler esté vigente.
- Solicitar la devolución de una compra dentro de los primeros siete días.
- Calificar y reseñar libros a los que haya tenido acceso.
- Elegir si sus reseñas se muestran con su nombre o de forma anónima.
- Recibir una advertencia y tener bloqueado el acceso al contenido de libros clasificados como +18 si no cumple la edad requerida.
- Administrar su perfil y su avatar.

### Escritor

- Publicar libros digitales y adjuntar sus archivos y portadas.
- Definir el precio de compra y el precio de alquiler.
- Seleccionar una categoría e indicar si la obra tiene contenido para mayores de 18 años.
- Relacionar libros de una misma saga mediante la referencia al libro anterior.
- Consultar, editar y eliminar sus propias publicaciones.
- Buscar libros para establecer relaciones entre entregas de una saga sin cargar todos los registros de la base de datos.
- Consultar estadísticas generales de publicaciones, compras, alquileres e ingresos registrados.
- Reportar o eliminar reseñas relacionadas con sus obras, según las operaciones habilitadas.

### Administrador

- Consultar y administrar los usuarios registrados.
- Modificar roles y gestionar cuentas de acuerdo con las restricciones del sistema.
- Crear, editar y eliminar categorías.
- Revisar las reseñas reportadas y realizar tareas de moderación.
- Consultar información general de la plataforma.

Las operaciones restringidas se validan en el backend mediante autenticación y autorización por roles, no solamente ocultando opciones en la interfaz.

# Usuarios:
Usuario escritor: Luu 
Contraseña: 12345678

Usuario lector: Mile
Contraseña: 12345678

Usuario administrador: Ad  
Contraseña: administrador

## 2. Modelo de datos y persistencia

La base de datos se llama `entre_paginas` y está diseñada para funcionar con MySQL o MariaDB.

Se utiliza una única entidad de usuarios para representar a lectores, escritores y administradores. El rol permite diferenciar sus permisos sin duplicar los datos personales de cada tipo de cuenta.

| Entidad | Descripción |
|---|---|
| **Usuarios** | Almacena los datos de las cuentas, las credenciales, el rol y la información del perfil. |
| **Categorias** | Contiene los géneros y tipos de libros disponibles. |
| **Libros** | Guarda los datos bibliográficos, precios, archivos, portada, categoría, escritor y relación con una saga. |
| **Compras** | Registra las compras realizadas por los lectores, junto con la fecha y el importe. |
| **Alquileres** | Registra los períodos de alquiler, sus fechas de inicio y finalización y el lector correspondiente. |
| **Resenas** | Almacena la puntuación, el comentario, la opción de anonimato y el estado de reporte de cada reseña. |

Entity Framework Core se utiliza para mapear las entidades, establecer las relaciones y realizar las operaciones de persistencia sobre la base de datos.

### Relaciones entre las entidades

- **Usuario – Libro:** un escritor puede publicar varios libros.
- **Categoría – Libro:** una categoría puede contener varios libros.
- **Usuario – Compra:** un lector puede realizar múltiples compras.
- **Libro – Compra:** un libro puede estar asociado a múltiples compras.
- **Usuario – Alquiler:** un lector puede realizar múltiples alquileres.
- **Libro – Alquiler:** un libro puede estar asociado a múltiples alquileres.
- **Usuario – Reseña:** un lector puede escribir varias reseñas.
- **Libro – Reseña:** un libro puede recibir varias reseñas.
- **Libro – Libro:** una obra puede relacionarse con otra obra anterior para representar una saga.

Estas relaciones permiten mantener la integridad de los datos y consultar las operaciones de cada usuario y de cada libro.

## 3. Reglas de negocio

### Compras y devoluciones

Al realizar una compra se registra la operación y su importe. La devolución se permite únicamente durante los siete días posteriores a la fecha de compra. El sistema calcula el plazo a partir de esa fecha.

El acceso al archivo de un libro comprado se valida en el servidor.

### Alquileres

Cada alquiler tiene una fecha de inicio y una fecha de finalización. El lector puede acceder al contenido mientras se encuentre vigente.

El alquiler habilita la lectura en línea, pero no la descarga del archivo como si se tratara de una compra. El acceso se vuelve a validar cada vez que se solicita el contenido.

### Reseñas

Para publicar una reseña, el lector debe haber tenido acceso al libro mediante una compra o un alquiler. La puntuación permitida va de 1 a 5.

Cada lector puede dejar una reseña por libro y elegir si desea que su identidad sea visible. Las reseñas pueden reportarse para su posterior revisión.

### Libros para mayores de 18 años

Cuando una obra está marcada como +18, se verifica la edad del usuario a partir de su fecha de nacimiento.

Si el usuario es menor de edad, el sistema bloquea el acceso al contenido. Esta restricción también se aplica a las acciones de compra, alquiler, lectura y descarga.

### Sagas

Los libros pueden relacionarse mediante una referencia al libro anterior. El primer libro de una saga no necesita tener una referencia previa.

El sistema debe impedir relaciones inválidas y evitar que un libro termine relacionado consigo mismo o que se formen ciclos en la secuencia de una saga.

## 4. Gestión de archivos

La plataforma permite que los escritores adjunten archivos digitales en formatos PDF, DOC y DOCX, además de una portada para cada publicación.

Los archivos de lectura se almacenan fuera de `wwwroot`, para que no estén disponibles mediante una URL pública directa. El acceso se realiza a través de las acciones del servidor, que verifican los permisos correspondientes.

Los avatares y las portadas se almacenan en el directorio destinado a las imágenes subidas por los usuarios.

Se realizan validaciones sobre los archivos recibidos, incluyendo extensiones y tamaño, para reducir el riesgo asociado a las cargas de archivos.

## 5. Catálogo e integración con Open Library

El catálogo permite consultar los libros publicados, buscar por título, autor o descripción, filtrar por categoría y acceder a la ficha de cada obra.

Para facilitar la publicación de libros, se incorpora la API pública de **Open Library**. Desde el formulario de publicación se pueden buscar referencias bibliográficas y utilizar los resultados como sugerencias para completar el título, el autor, el año de publicación, la referencia bibliográfica y la portada cuando esté disponible.

Los datos obtenidos pueden revisarse y editarse antes de guardar la publicación.

Open Library se utiliza como fuente de información bibliográfica; no reemplaza el archivo digital que el escritor debe adjuntar ni implica que la plataforma tenga autorización para distribuir el contenido completo de cualquier obra encontrada.

Documentación de la API: https://openlibrary.org/developers/api

## 6. Tecnologías utilizadas

- **ASP.NET Core MVC:** estructura principal de la aplicación web.
- **C#:** implementación de controladores, servicios y reglas de negocio.
- **Entity Framework Core:** acceso a datos y mapeo objeto-relacional.
- **MySQL/MariaDB:** almacenamiento persistente de la información.
- **HTML, CSS y Bootstrap:** estructura y presentación de las vistas.
- **JavaScript:** interacciones del lado del cliente.
- **Vue.js:** desarrollo del módulo de gestión de categorías.
- **AJAX y JSON:** comunicación asíncrona entre la interfaz y el servidor.
- **Web API REST:** exposición de operaciones mediante endpoints HTTP.
- **JWT:** autenticación mediante tokens para los endpoints protegidos.
- **BCrypt:** almacenamiento de contraseñas mediante hashes.
- **Open Library API:** consulta de metadatos bibliográficos.
- **Postman:** pruebas de los endpoints de la API REST.

## 7. Operaciones asíncronas y paginación

Los listados principales utilizan paginación del lado del servidor. De esta forma, cada solicitud recupera la página requerida en lugar de cargar todos los registros disponibles.

Las búsquedas de entidades relacionadas, como la selección del libro anterior de una saga, se realizan mediante solicitudes AJAX al servidor.

El módulo de categorías está desarrollado con Vue.js y permite listar, crear, editar y eliminar categorías mediante operaciones asíncronas con respuestas JSON, sin necesidad de recargar la página completa.

## 8. API REST y autenticación mediante JWT

La aplicación incorpora una API REST con endpoints protegidos mediante JSON Web Tokens. El token se obtiene a través del endpoint de autenticación y se envía posteriormente en la cabecera `Authorization` mediante el esquema `Bearer`.

Entre las operaciones disponibles se encuentran:

| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/auth/token` | Autenticación y obtención de un token JWT. |
| GET | `/api/libros` | Consulta paginada del catálogo. |
| GET | `/api/libros/{id}` | Consulta del detalle de un libro. |
| GET | `/api/categorias` | Consulta de categorías. |
| POST | `/api/categorias` | Creación de una categoría, restringida al administrador. |
| GET | `/api/libros/mi-biblioteca/compras` | Consulta de las compras del lector autenticado. |

Los listados de la API admiten parámetros de paginación. La autorización determina qué operaciones puede realizar cada usuario.

Se incluye una colección de Postman en `postman/EntrePaginas.postman_collection.json`, que permite probar la obtención del token y las solicitudes protegidas.

## 9. Ejecución del proyecto

Para ejecutar la aplicación se necesita el SDK de .NET 10 y un servidor MySQL o MariaDB disponible.

1. Iniciar el servicio de base de datos.
2. Crear la base `entre_paginas` con codificación `utf8mb4`.
3. Importar el archivo `database/entre_paginas.sql` desde phpMyAdmin o mediante otra herramienta de administración de bases de datos.
4. Configurar la cadena de conexión en `appsettings.json` según el servidor, puerto, usuario y contraseña de la instalación local.
5. Abrir una terminal en la carpeta que contiene `EntrePaginas.csproj` y ejecutar:

```bash
dotnet restore
dotnet run
```

La aplicación utiliza un inicializador de base de datos para comprobar y preparar determinados datos necesarios para su funcionamiento.

## 10. Estructura del proyecto

- `Controllers/`: controladores MVC y endpoints de la API REST.
- `Data/`: contexto de Entity Framework Core e inicialización de datos.
- `Models/`: entidades que representan el modelo de datos.
- `ViewModels/`: modelos utilizados para formularios y vistas.
- `Services/`: servicios para la gestión de archivos y consultas a Open Library.
- `Views/`: vistas de catálogo, usuarios, escritores y administración.
- `wwwroot/`: estilos, recursos estáticos, imágenes, avatares y portadas.
- `Storage/Libros/`: almacenamiento de los archivos digitales de lectura.
- `database/`: script SQL de la base de datos.
- `postman/`: colección para las pruebas de la API REST.

## 11. Integrantes

- **Lourdes Villegas** — [GitHub](https://github.com/Luly-bitcoin)
- **Milena Miselli** — [GitHub](https://github.com/milemise)

## 12. Objetivo del proyecto

El objetivo de Entre Páginas es integrar los conocimientos de desarrollo web, modelado de datos, persistencia, autenticación, autorización y comunicación entre componentes en un sistema con funcionalidades relacionadas entre sí.

La plataforma reúne la administración de libros digitales, las operaciones de compra y alquiler, la interacción mediante reseñas y la gestión de usuarios en una aplicación MVC que incorpora además una API REST y un módulo reactivo desarrollado con Vue.js.
