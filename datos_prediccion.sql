DELIMITER //
CREATE PROCEDURE datos_entrenamiento (IN periodoInicial CHAR(6)) (IN periodoFinal CHAR(6)) (IN codigo CHAR(6))
BEGIN
  SELECT * FROM kardex A
    WHERE ((A.periodo >= periodoInicial AND A.periodo <= periodoFinal) AND
           (A.materia IS NOT NULL) AND
           (A.final IS NOT NULL) AND
           NOT (A.final IN('RV.','RV','REV','RE','Q','EQ.','EQ','E')) AND
           (EXISTS (SELECT * FROM Grupos G WHERE (G.grupo = A.grupo))) AND
           (EXISTS (SELECT * FROM Planes P WHERE (A.materia = P.clave))) AND
            (NOT (EXISTS (SELECT * FROM kardex B
            WHERE (A.matricula = B.matricula AND B.materia = codigo))) OR
            EXISTS (SELECT * FROM Kardex B WHERE (A.matricula = B.matricula AND B.materia = codigo AND
                                                  B.final IS NULL))))
ORDER BY matricula, materia, periodo;
END //
DELIMITER ;
