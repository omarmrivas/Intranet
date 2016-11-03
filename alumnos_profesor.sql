DELIMITER $$
CREATE DEFINER=`intranet`@`%` PROCEDURE `alumnos_profesor`(IN `matricula` VARCHAR(6))
    READS SQL DATA
    DETERMINISTIC
SELECT DISTINCT G.materia, G.grupo, K.periodo, K.matricula, A.nombre as nombreAlumno, C.carrera, P.nombre as nombreProfesor, P.apellidos, K.c1, K.i1, K.c2, K.i2, K.c3, K.i3, Q.estatus as estatusPredicho, K.estatus, M.precision, M.numero_instancias, M.atributos, AC.descripcion, AC.descripcion_seleccion, M.periodo_inicial, M.periodo_final, M.parcial from kardex K
INNER JOIN grupos G
ON (K.grupo = G.grupo AND
    K.matricula = matricula AND
    K.periodo = G.periodo)
INNER JOIN profesores P
ON (G.profesor = P.profesor AND
    P.periodo = G.periodo)
INNER JOIN modelos_nominales M
ON (K.materia = M.materia)
INNER JOIN prediccion_kardex Q
ON (M.mId = Q.mId AND
    Q.matricula = K.matricula AND
    K.periodo = Q.periodo)
INNER JOIN planes C
ON (C.clave = K.materia)
INNER JOIN alumnos A
ON (Q.matricula = A.matricula)
INNER JOIN algoritmos_clasificadores AC
ON (M.aId = AC.aId)
ORDER BY G.materia, M.parcial, M.mId$$
DELIMITER ;

