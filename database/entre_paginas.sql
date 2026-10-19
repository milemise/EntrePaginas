-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 20-10-2026 a las 00:27:44
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.0.30

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `entre_paginas`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `alquileres`
--

CREATE TABLE `alquileres` (
  `id` int(11) NOT NULL,
  `id_libro` int(11) NOT NULL,
  `id_lector` int(11) NOT NULL,
  `fecha_inicio` datetime NOT NULL,
  `fecha_hasta` datetime NOT NULL,
  `importe` decimal(10,2) NOT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `alquileres`
--

INSERT INTO `alquileres` (`id`, `id_libro`, `id_lector`, `fecha_inicio`, `fecha_hasta`, `importe`, `activo`) VALUES
(1, 3, 6, '2026-10-15 18:11:41', '2026-10-22 18:11:41', 1100.00, 1),
(2, 1, 5, '2026-10-15 18:54:28', '2026-10-22 18:54:28', 700.00, 1),
(3, 2, 5, '2026-10-15 18:56:39', '2026-10-22 18:56:39', 900.00, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `categorias`
--

CREATE TABLE `categorias` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `categorias`
--

INSERT INTO `categorias` (`id`, `nombre`) VALUES
(5, 'Ciencia ficción'),
(1, 'Fantasía'),
(3, 'Historia'),
(2, 'Literatura infantil'),
(6, 'Misterio'),
(4, 'Romance'),
(7, 'Suspenso');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `compras`
--

CREATE TABLE `compras` (
  `id` int(11) NOT NULL,
  `id_libro` int(11) NOT NULL,
  `id_lector` int(11) NOT NULL,
  `fecha_compra` datetime NOT NULL DEFAULT current_timestamp(),
  `importe` decimal(10,2) NOT NULL,
  `devuelto` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `compras`
--

INSERT INTO `compras` (`id`, `id_libro`, `id_lector`, `fecha_compra`, `importe`, `devuelto`) VALUES
(1, 4, 6, '2026-10-15 18:08:36', 450000.00, 0),
(2, 1, 5, '2026-10-15 18:54:35', 3500.00, 1),
(3, 3, 5, '2026-10-15 19:04:25', 6500.00, 0),
(4, 4, 7, '2026-10-15 19:07:32', 450000.00, 0),
(5, 5, 6, '2026-10-19 19:26:06', 100.00, 0);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `libros`
--

CREATE TABLE `libros` (
  `id` int(11) NOT NULL,
  `titulo` varchar(200) NOT NULL,
  `autor` varchar(200) DEFAULT NULL,
  `open_library_key` varchar(100) DEFAULT NULL,
  `descripcion` text DEFAULT NULL,
  `portada_url` varchar(500) DEFAULT NULL,
  `archivo_url` varchar(500) DEFAULT NULL,
  `precio_compra` decimal(10,2) NOT NULL,
  `precio_alquiler` decimal(10,2) NOT NULL,
  `es_mas_18` tinyint(1) NOT NULL DEFAULT 0,
  `id_categoria` int(11) NOT NULL,
  `id_escritor` int(11) NOT NULL,
  `id_libro_anterior` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `libros`
--

INSERT INTO `libros` (`id`, `titulo`, `autor`, `open_library_key`, `descripcion`, `portada_url`, `archivo_url`, `precio_compra`, `precio_alquiler`, `es_mas_18`, `id_categoria`, `id_escritor`, `id_libro_anterior`) VALUES
(1, 'Las aventuras de Pinocho', 'Carlo Collodi', NULL, 'Un clásico de la literatura infantil que acompaña el viaje de Pinocho para convertirse en un niño de verdad. Este archivo es una muestra de lectura de prueba del sistema.', 'images/portada1.jpg', 'muestra-pinocho.pdf', 3500.00, 700.00, 0, 2, 2, NULL),
(2, 'Mamá nos dijo adiós', 'Lucía Paredes (obra de muestra)', NULL, 'Una historia sobre los vínculos familiares, la memoria y los viajes que marcan una vida. El archivo adjunto es una muestra de lectura de prueba.', 'images/portada2.jpg', 'muestra-mama.pdf', 5200.00, 900.00, 0, 3, 2, NULL),
(3, 'Harry Potter y la piedra filosofal', 'J. K. Rowling', NULL, 'La historia de un joven que descubre un mundo mágico y comienza una aventura inolvidable. El archivo adjunto es una muestra de lectura de prueba.', 'images/portada3.jpg', 'muestra-harry-potter.pdf', 6500.00, 1100.00, 0, 1, 2, NULL),
(4, 'La última página', 'Lourdes Villegas', NULL, 'Una joven descubre un libro mágico que puede transportarla al interior de sus propias historias.', 'uploads/portadas/0410ebc413124bf9a42b032f5aac4f4d.jpg', 'ea76c793b2bf465f871debee6f625c87.pdf', 450000.00, 80000.00, 0, 1, 5, NULL),
(5, 'Libro', 'Lourdes Villegas', NULL, 'Un libro de prueba', 'uploads/portadas/7d97eb3cdbea43e58c29274a26a86105.jpg', '64ca61f90c5345aab001a193ccda78d8.pdf', 100.00, 20.00, 1, 4, 5, NULL);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `notificaciones`
--

CREATE TABLE `notificaciones` (
  `id` int(11) NOT NULL,
  `id_usuario` int(11) NOT NULL,
  `tipo` varchar(50) NOT NULL,
  `titulo` varchar(150) NOT NULL,
  `mensaje` text NOT NULL,
  `leida` tinyint(1) NOT NULL DEFAULT 0,
  `favorita` tinyint(1) NOT NULL DEFAULT 0,
  `fecha_creacion` datetime NOT NULL DEFAULT current_timestamp(),
  `entidad_tipo` varchar(50) DEFAULT NULL,
  `entidad_id` int(11) DEFAULT NULL,
  `url_destino` varchar(500) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `palabras_favoritas`
--

CREATE TABLE `palabras_favoritas` (
  `id` int(11) NOT NULL,
  `id_usuario` int(11) NOT NULL,
  `palabra` varchar(150) NOT NULL,
  `fecha_agregado` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `palabras_favoritas`
--

INSERT INTO `palabras_favoritas` (`id`, `id_usuario`, `palabra`, `fecha_agregado`) VALUES
(2, 7, 'Amor', '2026-10-19 18:56:11'),
(3, 7, 'mate', '2026-10-19 18:56:58');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `resenas`
--

CREATE TABLE `resenas` (
  `id` int(11) NOT NULL,
  `id_libro` int(11) NOT NULL,
  `id_lector` int(11) NOT NULL,
  `puntuacion` int(11) NOT NULL,
  `comentario` text DEFAULT NULL,
  `fecha_comentario` datetime NOT NULL DEFAULT current_timestamp(),
  `anonimo` tinyint(1) NOT NULL DEFAULT 0,
  `reportado` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `resenas`
--

INSERT INTO `resenas` (`id`, `id_libro`, `id_lector`, `puntuacion`, `comentario`, `fecha_comentario`, `anonimo`, `reportado`) VALUES
(1, 4, 6, 5, 'Corta pero interesante', '2026-10-15 18:09:56', 0, 1),
(2, 3, 6, 1, 'Me mintieron', '2026-10-15 18:12:28', 1, 0),
(3, 4, 7, 5, 'Muy lindo libro, felicidades a la escritora', '2026-10-15 19:20:02', 0, 0),
(4, 2, 5, 4, 'No le doy 5 estrellas porque me hizo llorar', '2026-10-15 19:21:36', 0, 0),
(5, 5, 6, 5, 'Manso', '2026-10-19 19:26:15', 0, 0);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuarios`
--

CREATE TABLE `usuarios` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `apellido` varchar(100) NOT NULL,
  `nombre_usuario` varchar(50) NOT NULL,
  `email` varchar(150) NOT NULL,
  `password` varchar(255) NOT NULL,
  `dni` varchar(20) NOT NULL,
  `telefono` varchar(30) DEFAULT NULL,
  `fecha_nacimiento` date NOT NULL,
  `avatar_url` varchar(500) DEFAULT NULL,
  `rol` varchar(30) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `usuarios`
--

INSERT INTO `usuarios` (`id`, `nombre`, `apellido`, `nombre_usuario`, `email`, `password`, `dni`, `telefono`, `fecha_nacimiento`, `avatar_url`, `rol`) VALUES
(1, 'Administración', 'Entre Páginas', 'admin', 'admin@entrepaginas.local', '$2a$11$BnftF6FiGv5B.xzhv.B8xO44r55SufHkWdtEnU8LzZG38qRbGItzq', '90000001', NULL, '1990-01-01', NULL, 'Administrador'),
(2, 'Lucía', 'Paredes', 'escritor_demo', 'escritor@entrepaginas.local', '$2a$11$iocAzS4pkbLBvKB09dZON.6OavgSEdowWgjxoT3mf475fE/LyPCPe', '90000002', NULL, '1994-04-12', NULL, 'Escritor'),
(3, 'Mateo', 'Luna', 'lector_demo', 'lector@entrepaginas.local', '$2a$11$lfld5JZ4ICP3pnGdJ/tVjuKlfIonJPn3mGbB/edCLMb8xl0GVe1WC', '90000003', NULL, '2000-07-07', NULL, 'Lector'),
(5, 'Lourdes', 'Villegas', 'Luu', 'luu@gmail.com', '$2a$11$D6vzuVaojSWX7zmxEjWiZuF4.BProLk.pjX0krpSFTrjKK.uuaTBi', '46332709', '2665112822', '2005-02-17', 'uploads/avatares/404f07f878cf4f13bb893eab855d869d.jpg', 'Escritor'),
(6, 'Mile', 'Miselli', 'Mile', 'mile@gmail.com', '$2a$11$9GJ191xo1M5A9Eo/9RLZK.ZrfG3e8u6dVlef9UOrCnee2x0XNKQ/G', '45334534', '2664112211', '2000-01-01', NULL, 'Lector'),
(7, 'Admi', 'Aaa', 'Ad', 'adminpiola@gmail.com', '$2a$11$XjesdY8LkX/5ZgrxFWk9vOwutIZ3gMtO..e13a7M0/oHlszOiXJfm', '26776767', '2664112221', '2000-01-01', NULL, 'Administrador'),
(8, 'AdminJunior', 'Junior', 'Junior', 'junior@gmail.com', '$2a$11$0YqKgalg/6F1v9LHvqrmGeKWftAlIp7OhCyfZqBNDnB4gKCb2eCDC', '46332722', '2454556455', '2006-01-01', NULL, 'Administrador'),
(10, 'Messi', 'Messie', 'user', 'correo@gmail.com', '$2a$11$V3kzlTn9ZqK9F9HyzRmxZ.4eHBWbQOMx59q/rjssmpYxIa5s0mfS6', '12343234', '3261263216136', '2003-02-21', NULL, 'Lector');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `alquileres`
--
ALTER TABLE `alquileres`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_alquiler_libro` (`id_libro`),
  ADD KEY `fk_alquiler_lector` (`id_lector`);

--
-- Indices de la tabla `categorias`
--
ALTER TABLE `categorias`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre` (`nombre`);

--
-- Indices de la tabla `compras`
--
ALTER TABLE `compras`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_compra_libro` (`id_libro`),
  ADD KEY `fk_compra_lector` (`id_lector`);

--
-- Indices de la tabla `libros`
--
ALTER TABLE `libros`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_libro_categoria` (`id_categoria`),
  ADD KEY `fk_libro_escritor` (`id_escritor`),
  ADD KEY `fk_libro_anterior` (`id_libro_anterior`);

--
-- Indices de la tabla `notificaciones`
--
ALTER TABLE `notificaciones`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_notificaciones_usuario_fecha` (`id_usuario`,`fecha_creacion`),
  ADD KEY `idx_notificaciones_usuario_leida` (`id_usuario`,`leida`),
  ADD KEY `idx_notificaciones_usuario_favorita` (`id_usuario`,`favorita`);

--
-- Indices de la tabla `palabras_favoritas`
--
ALTER TABLE `palabras_favoritas`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uq_favorito_usuario_palabra` (`id_usuario`,`palabra`);

--
-- Indices de la tabla `resenas`
--
ALTER TABLE `resenas`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_resena_libro` (`id_libro`),
  ADD KEY `fk_resena_lector` (`id_lector`);

--
-- Indices de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre_usuario` (`nombre_usuario`),
  ADD UNIQUE KEY `email` (`email`),
  ADD UNIQUE KEY `dni` (`dni`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `alquileres`
--
ALTER TABLE `alquileres`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `categorias`
--
ALTER TABLE `categorias`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `compras`
--
ALTER TABLE `compras`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT de la tabla `libros`
--
ALTER TABLE `libros`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT de la tabla `notificaciones`
--
ALTER TABLE `notificaciones`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `palabras_favoritas`
--
ALTER TABLE `palabras_favoritas`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `resenas`
--
ALTER TABLE `resenas`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `alquileres`
--
ALTER TABLE `alquileres`
  ADD CONSTRAINT `fk_alquiler_lector` FOREIGN KEY (`id_lector`) REFERENCES `usuarios` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_alquiler_libro` FOREIGN KEY (`id_libro`) REFERENCES `libros` (`id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `compras`
--
ALTER TABLE `compras`
  ADD CONSTRAINT `fk_compra_lector` FOREIGN KEY (`id_lector`) REFERENCES `usuarios` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_compra_libro` FOREIGN KEY (`id_libro`) REFERENCES `libros` (`id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `libros`
--
ALTER TABLE `libros`
  ADD CONSTRAINT `fk_libro_anterior` FOREIGN KEY (`id_libro_anterior`) REFERENCES `libros` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_libro_categoria` FOREIGN KEY (`id_categoria`) REFERENCES `categorias` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_libro_escritor` FOREIGN KEY (`id_escritor`) REFERENCES `usuarios` (`id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `notificaciones`
--
ALTER TABLE `notificaciones`
  ADD CONSTRAINT `fk_notificaciones_usuario` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Filtros para la tabla `palabras_favoritas`
--
ALTER TABLE `palabras_favoritas`
  ADD CONSTRAINT `fk_favoritos_usuario` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Filtros para la tabla `resenas`
--
ALTER TABLE `resenas`
  ADD CONSTRAINT `fk_resena_lector` FOREIGN KEY (`id_lector`) REFERENCES `usuarios` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_resena_libro` FOREIGN KEY (`id_libro`) REFERENCES `libros` (`id`) ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
