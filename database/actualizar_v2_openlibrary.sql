-- Ejecutar solamente si la base entre_paginas ya existía antes de incorporar Open Library.
-- Si la aplicación se inicia después del cambio, DatabaseInitializer agrega estas columnas automáticamente.
ALTER TABLE libros ADD COLUMN autor VARCHAR(200) NULL;
ALTER TABLE libros ADD COLUMN open_library_key VARCHAR(100) NULL;
