DELIMITER $$
CREATE DEFINER=`intranet`@`%` PROCEDURE `materia_profesor_correctas`(IN `periodoInicial` CHAR(6), IN `periodoFinal` CHAR(6), IN `periodoPrediccion` CHAR(6), IN `parcial` INT(11), IN `materia` CHAR(6))
    READS SQL DATA
    DETERMINISTIC
SELECT COUNT(1) from kardex K
INNER JOIN grupos G
ON (K.materia = materia AND
    K.grupo = G.grupo AND
    K.periodo = G.periodo)
INNER JOIN profesores P
ON (G.profesor = P.profesor AND
    P.periodo = G.periodo)
INNER JOIN modelos_nominales M
ON (K.materia = M.materia AND
    M.periodo_inicial = periodo_inicial AND
    M.periodo_final = periodo_final AND
    M.parcial = parcial)
INNER JOIN prediccion_kardex Q
ON (M.mId = Q.mId AND
    Q.matricula = K.matricula AND
    K.periodo = Q.periodo AND
    Q.estatus = K.estatus)
INNER JOIN planes C
ON (C.clave = K.materia)
INNER JOIN alumnos A
ON (Q.matricula = A.matricula)
INNER JOIN algoritmos_clasificadores AC
ON (M.aId = AC.aId)$$
DELIMITER ;