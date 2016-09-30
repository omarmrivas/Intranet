-- phpMyAdmin SQL Dump
-- version 4.6.0
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Sep 22, 2016 at 04:09 PM
-- Server version: 5.6.10
-- PHP Version: 5.5.36

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `intranet`
--

DELIMITER $$
--
-- Procedures
--
$$

DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `algoritmos_clasificadores`
--

CREATE TABLE IF NOT EXISTS `algoritmos_clasificadores` (
  `clase` varchar(100) NOT NULL,
  `comando_seleccion` varchar(1000) NOT NULL,
  `comando_construccion` varchar(1000) NOT NULL,
  `comando_predecir` varchar(1000) NOT NULL,
  `descripcion` varchar(2000) NOT NULL,
  PRIMARY KEY (`clase`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

-- --------------------------------------------------------

--
-- Table structure for table `usuarios`
--

CREATE TABLE IF NOT EXISTS `usuarios` (
  `matricula` char(6) NOT NULL COMMENT 'Matrícula del alumno.',
  `nombre` varchar(200) NOT NULL COMMENT 'Nombre del alumno.',
  `genero` char(1) NOT NULL COMMENT 'Genero del alumno (M = Masculino, F = Femenino).).',
  `fecha_nacimiento` date NOT NULL COMMENT 'Fecha de nacimiento del alumno.',
  `ingreso` char(6) NOT NULL COMMENT 'Ingreso a la UPSLP.',
  `telefono` varchar(100) NOT NULL COMMENT 'Teléfono del alumno.',
  `direccion` varchar(300) NOT NULL COMMENT 'Calle y número.',
  `colonia` varchar(300) NOT NULL COMMENT 'Colonia.',
  `cp` varchar(10) NOT NULL COMMENT 'Código postal.',
  `municipio` varchar(200) NOT NULL COMMENT 'Lugar de nacimiento.',
  `procedencia` varchar(500) NOT NULL COMMENT 'Escuela de procedencia.',
  PRIMARY KEY (`matricula`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin COMMENT='Almacena a todos los alumnos de la universidad sin importar la carrera.';

-- --------------------------------------------------------

--
-- Table structure for table `alumnos`
--

CREATE TABLE IF NOT EXISTS `alumnos` (
  `matricula` char(6) NOT NULL COMMENT 'Matrícula del alumno.',
  `nombre` varchar(200) NOT NULL COMMENT 'Nombre del alumno.',
  `genero` char(1) NOT NULL COMMENT 'Genero del alumno (M = Masculino, F = Femenino).).',
  `fecha_nacimiento` date NOT NULL COMMENT 'Fecha de nacimiento del alumno.',
  `ingreso` char(6) NOT NULL COMMENT 'Ingreso a la UPSLP.',
  `telefono` varchar(100) NOT NULL COMMENT 'Teléfono del alumno.',
  `direccion` varchar(300) NOT NULL COMMENT 'Calle y número.',
  `colonia` varchar(300) NOT NULL COMMENT 'Colonia.',
  `cp` varchar(10) NOT NULL COMMENT 'Código postal.',
  `municipio` varchar(200) NOT NULL COMMENT 'Lugar de nacimiento.',
  `procedencia` varchar(500) NOT NULL COMMENT 'Escuela de procedencia.',
  PRIMARY KEY (`matricula`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin COMMENT='Almacena a todos los alumnos de la universidad sin importar la carrera.';

-- --------------------------------------------------------

--
-- Table structure for table `carreras`
--

CREATE TABLE IF NOT EXISTS `carreras` (
  `nombre` char(6) NOT NULL,
  `id` int(11) NOT NULL,
  `nombre_largo` varchar(100) NOT NULL,
  PRIMARY KEY (`nombre`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `extracurriculares`
--

CREATE TABLE IF NOT EXISTS `extracurriculares` (
  `clave` char(6) NOT NULL,
  `programa` varchar(100) NOT NULL,
  `materia` varchar(200) NOT NULL,
  `teoria` tinyint(4) NOT NULL,
  `practica` tinyint(4) NOT NULL,
  `evaluacion` varchar(200) NOT NULL,
  PRIMARY KEY (`clave`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

-- --------------------------------------------------------

--
-- Table structure for table `grupos`
--

CREATE TABLE IF NOT EXISTS `grupos` (
  `grupo` varchar(7) NOT NULL,
  `periodo` char(6) NOT NULL,
  `materia` varchar(200) NOT NULL,
  `aula` varchar(10) NOT NULL,
  `lunes` varchar(15) NOT NULL,
  `martes` varchar(15) NOT NULL,
  `miercoles` varchar(15) NOT NULL,
  `jueves` varchar(15) NOT NULL,
  `viernes` varchar(15) NOT NULL,
  `sabado` varchar(15) NOT NULL,
  `profesor` int(11) DEFAULT NULL,
  `alumnos` int(11) NOT NULL,
  `estado` varchar(15) NOT NULL,
  `plan` varchar(50) NOT NULL,
  PRIMARY KEY (`grupo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin COMMENT='Contiene todos los grupos';

-- --------------------------------------------------------

--
-- Table structure for table `profesores`
--

CREATE TABLE IF NOT EXISTS `profesores` (
  `profesor` int(11) NOT NULL,
  `periodo` char(6) NOT NULL,
  `nombre` varchar(200) NOT NULL,
  `apellidos` varchar(200) NOT NULL,
  `tipo` varchar(100) NOT NULL,
  PRIMARY KEY (`profesor`,`periodo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin COMMENT='Contiene todos los grupos';

-- --------------------------------------------------------

--
-- Table structure for table `inscripciones`
--

CREATE TABLE IF NOT EXISTS `inscripciones` (
  `matricula` char(6) NOT NULL,
  `periodo` char(6) NOT NULL,
  `estado` varchar(40) NOT NULL,
  `semestre` tinyint(4) NOT NULL,
  `plan` char(4) NOT NULL,
  `fecha` date NOT NULL,
  PRIMARY KEY (`matricula`,`periodo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin COMMENT='Contiene todas las incripciones de alumnos';

-- --------------------------------------------------------

--
-- Table structure for table `kardex`
--

CREATE TABLE IF NOT EXISTS `kardex` (
  `matricula` char(6) NOT NULL,
  `grupo` char(7) NOT NULL,
  `materia` char(6) DEFAULT NULL,
  `semestre` tinyint(4) NOT NULL,
  `periodo` char(6) NOT NULL,
  `c1` varchar(5) DEFAULT NULL,
  `i1` int(11) NOT NULL,
  `c2` varchar(5) DEFAULT NULL,
  `i2` int(11) NOT NULL,
  `c3` varchar(5) DEFAULT NULL,
  `i3` int(11) NOT NULL,
  `efinal` varchar(5) DEFAULT NULL,
  `final` varchar(5) DEFAULT NULL,
  `inasistencias` int(11) NOT NULL,
  `extraordinario` varchar(5) DEFAULT NULL,
  `regularizacion` varchar(5) DEFAULT NULL,
  `estatus` varchar(20) NOT NULL,
  PRIMARY KEY (`matricula`,`grupo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

-- --------------------------------------------------------

--
-- Table structure for table `modelos`
--

CREATE TABLE IF NOT EXISTS `modelos` (
  `materia` char(6) NOT NULL,
  `periodo` char(6) NOT NULL,
  `clase` varchar(100) NOT NULL,
  `continuo_discreto` tinyint(1) NOT NULL,
  `atributos` varchar(500) NOT NULL,
  `aprobados_correctos` int(11) NOT NULL,
  `reprobados_incorrectos` int(11) NOT NULL,
  `aprobados_incorrectos` int(11) NOT NULL,
  `reprobados_correctos` int(11) NOT NULL,
  `modelo` mediumblob NOT NULL,
  PRIMARY KEY (`materia`,`periodo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

-- --------------------------------------------------------

--
-- Table structure for table `planes`
--

CREATE TABLE IF NOT EXISTS `planes` (
  `carrera` char(6) NOT NULL,
  `clave` char(6) NOT NULL,
  `semestre` tinyint(4) NOT NULL,
  `materia` varchar(200) NOT NULL,
  `seriacion` varchar(60) NOT NULL,
  `creditos` tinyint(4) NOT NULL,
  `horas` tinyint(4) NOT NULL,
  `teoria` tinyint(4) NOT NULL,
  `practica` tinyint(4) NOT NULL,
  `evaluacion` varchar(200) NOT NULL,
  PRIMARY KEY (`carrera`,`clave`,`materia`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
