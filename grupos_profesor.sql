DELIMITER $$
CREATE DEFINER=`intranet`@`%` PROCEDURE `grupos_profesor`(IN `periodo` CHAR(6), IN `parcial` INT(11), IN `nombre` VARCHAR(200), IN `apellidos` VARCHAR(200))
    READS SQL DATA
    DETERMINISTIC
SELECT DISTINCT G.materia, G.grupo, K.matricula, A.nombre, Q.estatus, M.precision, M.numero_instancias, M.atributos, AC.descripcion from kardex K
INNER JOIN grupos G
ON (K.grupo = G.grupo AND
    K.periodo = periodo AND
    K.periodo = G.periodo)
INNER JOIN profesores P
ON (G.profesor = P.profesor AND
    P.periodo = G.periodo AND
    P.nombre = nombre AND
    P.apellidos = apellidos)
INNER JOIN modelos_nominales M
ON (K.materia = M.materia AND
    M.parcial = 0)
INNER JOIN prediccion_kardex Q
ON (M.mId = Q.mId AND
    Q.periodo = periodo AND
    Q.matricula = K.matricula)
INNER JOIN alumnos A
ON (Q.matricula = A.matricula)
INNER JOIN algoritmos_clasificadores AC
ON (M.clase = AC.clase)
ORDER BY G.grupo, A.nombre$$
DELIMITER ;
