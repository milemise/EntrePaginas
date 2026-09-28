-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 28-09-2026 a las 22:59:24
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
) ;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `categorias`
--

CREATE TABLE `categorias` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

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
) ;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `libros`
--

CREATE TABLE `libros` (
  `id` int(11) NOT NULL,
  `titulo` varchar(200) NOT NULL,
  `descripcion` text DEFAULT NULL,
  `portada_url` varchar(500) DEFAULT NULL,
  `archivo_url` varchar(500) DEFAULT NULL,
  `precio_compra` decimal(10,2) NOT NULL,
  `precio_alquiler` decimal(10,2) NOT NULL,
  `es_mas_18` tinyint(1) NOT NULL DEFAULT 0,
  `id_categoria` int(11) NOT NULL,
  `id_escritor` int(11) NOT NULL,
  `id_libro_anterior` int(11) DEFAULT NULL
) ;

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
) ;

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
) ;

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
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `compras`
--
ALTER TABLE `compras`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `libros`
--
ALTER TABLE `libros`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `resenas`
--
ALTER TABLE `resenas`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

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
-- Filtros para la tabla `resenas`
--
ALTER TABLE `resenas`
  ADD CONSTRAINT `fk_resena_lector` FOREIGN KEY (`id_lector`) REFERENCES `usuarios` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_resena_libro` FOREIGN KEY (`id_libro`) REFERENCES `libros` (`id`) ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
