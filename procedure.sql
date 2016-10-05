DELIMITER //
CREATE PROCEDURE datos_entrenamiento (IN periodoInicial CHAR(6)) (IN periodoFinal CHAR(6)) (IN codigo CHAR(6))
BEGIN
  SELECT * FROM kardex A
    WHERE EXISTS (SELECT * FROM kardex B
    	  	  WHERE (B.materia = codigo AND A.matricula = B.matricula AND
		            (A.semestre < B.semestre OR A.materia = codigo) AND
                    (A.periodo <= B.periodo) AND
                    (A.periodo >= periodoInicial AND A.periodo <= periodoFinal) AND
                    (B.periodo >= periodoInicial AND B.periodo <= periodoFinal) AND
                    (A.materia IS NOT NULL) AND
                    (B.materia IS NOT NULL) AND
                    NOT (A.final IN('RV.','RV','REV','RE','Q','EQ.','EQ','E')) AND
                    (EXISTS (SELECT * FROM Planes P 
                        WHERE (A.materia = P.clave))) AND
                    (EXISTS (SELECT * FROM Planes P 
                        WHERE (B.materia = P.clave)))))
ORDER BY matricula, materia, periodo;
END //
DELIMITER ;
