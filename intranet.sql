-- phpMyAdmin SQL Dump
-- version 4.0.10deb1
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Sep 19, 2016 at 09:29 AM
-- Server version: 5.5.49-0ubuntu0.14.04.1
-- PHP Version: 7.0.8-4+deb.sury.org~trusty+1

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `curricula_upslp`
--

DELIMITER $$
--
-- Procedures
--
CREATE DEFINER=`root`@`localhost` PROCEDURE `entrenamiento`(IN `codigo` CHAR(6))
    DETERMINISTIC
    SQL SECURITY INVOKER
BEGIN
  SELECT * FROM kardex A
    WHERE EXISTS (SELECT * FROM kardex B
    	  	  WHERE (B.materia = codigo AND A.matricula = B.matricula AND
		        (A.semestre < B.semestre OR A.periodo < B.periodo OR A.materia = codigo) AND
			(A.estatus = 'Aprobado' OR A.estatus = 'Reprobado') AND
			(B.estatus = 'Aprobado' OR B.estatus = 'Reprobado')))
ORDER BY matricula, materia, periodo;
END$$

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
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `alumnos`
--

CREATE TABLE IF NOT EXISTS `alumnos` (
  `matricula` char(6) NOT NULL COMMENT 'Matrícula del alumno.',
  `nombre` varchar(200) NOT NULL COMMENT 'Nombre del alumno.',
  `genero` char(1) NOT NULL COMMENT 'Genero del alumno (M = Masculino, F = Femenino).).',
  `fecha_nacimiento` date NOT NULL COMMENT 'Fecha de nacimiento del alumno.',
  PRIMARY KEY (`matricula`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='Almacena a todos los alumnos de la universidad sin importar la carrera.';

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
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

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
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='Contiene todas las incripciones de alumnos';

-- --------------------------------------------------------

--
-- Table structure for table `kardex`
--

CREATE TABLE IF NOT EXISTS `kardex` (
  `matricula` char(6) NOT NULL,
  `semestre` tinyint(4) NOT NULL,
  `materia` varchar(200) NOT NULL,
  `periodo` char(6) NOT NULL,
  `final` float NOT NULL,
  `extraordinario` float NOT NULL,
  `regularizacion` float NOT NULL,
  `inasistencias` tinyint(4) NOT NULL,
  `estatus` varchar(20) NOT NULL,
  PRIMARY KEY (`matricula`,`periodo`,`materia`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

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
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

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
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
