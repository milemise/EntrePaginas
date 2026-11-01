-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 01-11-2026 a las 21:01:24
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
(4, 'Romance');

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
(1, 4, 5, '2026-11-01 16:42:18', 1999.00, 0);

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
(4, 'La última página', 'Maria Lourdes Villegas', NULL, 'Un libro de prueba', 'uploads/portadas/6836b2422cf24c32ba557f282ce64aea.jpg', '6c51540019c64e75a9250f9f46a4334e.pdf', 1999.00, 1666.00, 0, 3, 4, NULL);

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

--
-- Volcado de datos para la tabla `notificaciones`
--

INSERT INTO `notificaciones` (`id`, `id_usuario`, `tipo`, `titulo`, `mensaje`, `leida`, `favorita`, `fecha_creacion`, `entidad_tipo`, `entidad_id`, `url_destino`) VALUES
(1, 5, 'Compra', 'Compra registrada', 'La compra de «La última página» quedó registrada. Ya podés acceder al libro desde tu biblioteca.', 0, 0, '2026-11-01 16:42:18', 'Libro', 4, '/Biblioteca/Compras'),
(2, 4, 'Venta', 'Nueva compra de una obra', 'Un lector compró «La última página».', 0, 0, '2026-11-01 16:42:18', 'Libro', 4, '/Escritores/Estadisticas'),
(3, 4, 'Resena', 'Nueva reseña en tu libro', 'Un lector publicó una reseña en «La última página».', 1, 0, '2026-11-01 16:44:05', 'Libro', 4, '/Libros/Detalle/4'),
(4, 1, 'Reporte', 'Reseña reportada', 'Se reportó una reseña del libro «La última página».', 0, 0, '2026-11-01 16:49:39', 'Resena', 1, '/Admin/ResenasReportadas'),
(5, 7, 'Reporte', 'Reseña reportada', 'Se reportó una reseña del libro «La última página».', 1, 0, '2026-11-01 16:49:39', 'Resena', 1, '/Admin/ResenasReportadas'),
(6, 4, 'Moderacion', 'Reporte revisado', 'La administración revisó el reporte de una reseña de «La última página» y decidió mantenerla publicada.', 0, 0, '2026-11-01 17:00:22', 'Libro', 4, '/Libros/Detalle/4');

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
(1, 4, 5, 5, 'DIVINO', '2026-11-01 16:44:05', 0, 0);

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
(1, 'Administración', 'Entre Páginas', 'admin', 'admin@entrepaginas.local', '$2a$11$zo9pjBW8iAHhhGndtnkt3e.ifL5YF5/nk6CMIG5gTnoj2K6zwxI8u', '90000001', NULL, '1990-01-01', NULL, 'Administrador'),
(2, 'Lucía', 'Paredes', 'escritor_demo', 'escritor@entrepaginas.local', '$2a$11$LqybBSCsMTofS83x.HDcQuw3ZHf7aMpqnY1AEcyneyertqMMCTEzO', '90000002', NULL, '1994-04-12', NULL, 'Escritor'),
(3, 'Mateo', 'Luna', 'lector_demo', 'lector@entrepaginas.local', '$2a$11$mWURr73dlwexSWXWMVP2bOoKrUnT/3.swVK8b4nqcpZqL90.uqvqi', '90000003', NULL, '2000-07-07', NULL, 'Lector'),
(4, 'Lourdes', 'Villegas', 'lulu', 'lulu@gmail.com', '$2a$11$NXy7aloNiazhuBAM0GKmL.zbHji4by6y8IDXfP1KACrElr7sM8k7i', '46332709', '2665112822', '2005-02-17', 'uploads/avatares/a072cc32f16740da9690a71075410de8.jpg', 'Escritor'),
(5, 'Mile', 'Miselli', 'milen', 'mile@gmail.com', '$2a$11$6CHNUjJ0D2I4dkoQk2lPV.7ta0ShCtIu5VvuXbHuyweI/y7cD6vUm', '45362615', '3666262662626', '2000-01-01', NULL, 'Lector'),
(7, 'admin', 'admin', 'administrador', 'admin@gmail.com', '$2a$11$6CHNUjJ0D2I4dkoQk2lPV.7ta0ShCtIu5VvuXbHuyweI/y7cD6vUm', '45445445', '4444444444', '2000-11-01', NULL, 'Administrador');

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
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `categorias`
--
ALTER TABLE `categorias`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT de la tabla `compras`
--
ALTER TABLE `compras`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT de la tabla `libros`
--
ALTER TABLE `libros`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT de la tabla `notificaciones`
--
ALTER TABLE `notificaciones`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT de la tabla `palabras_favoritas`
--
ALTER TABLE `palabras_favoritas`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `resenas`
--
ALTER TABLE `resenas`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

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
