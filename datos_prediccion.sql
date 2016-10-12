DELIMITER //
CREATE PROCEDURE datos_prediccion (IN periodoInicial CHAR(6)) (IN periodoFinal CHAR(6)) (IN periodoPrediccion CHAR(6)) (IN codigo CHAR(6))
BEGIN
  SELECT * FROM kardex A
    WHERE (((A.periodo >= periodoInicial AND A.periodo <= periodoFinal) AND
           (A.materia IS NOT NULL) AND
           (A.final IS NOT NULL) AND
           NOT (A.final IN('RV.','RV','REV','RE','Q','EQ.','EQ','E')) AND
           (EXISTS (SELECT * FROM grupos G WHERE (G.grupo = A.grupo))) AND
           (EXISTS (SELECT * FROM planes P WHERE (A.materia = P.clave))) AND
           (EXISTS (SELECT * FROM Kardex B WHERE (A.matricula = B.matricula AND B.materia = codigo AND
                                                  B.periodo = periodoPrediccion)))) OR
           (A.materia = codigo AND A.periodo = periodoPrediccion AND
            A.final IS NULL))
ORDER BY matricula, materia, periodo;
END //
DELIMITER ;
