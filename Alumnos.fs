﻿module Alumnos

open System
open FSharp.Data
open System.Net
open System.Collections.Generic

[<Literal>]
let TablaAlumnos = """
    <table>
        <row>
            <column>20013S</column>
            <column>Inscripci&#243;n</column>
            <column>1</column>
            <column>ITI</column>
            <column>010393</column>
            <column>Alba Torres Victor Hugo</column>
            <column>M</column>
            <column>  21</column>
            <column>20013S</column>
            <column></column>
            <column>8-11-73-31</column>
            <column>BASALTO #500</column>
            <column>San Alberto </column>
            <column>San Luis Potos&#237;</column>
            <column>S.E.E.R. - Profra. Celia Fernandez Capetillo</column>
            <column></column>
            <column></column>
            <column>08-01-1980</column>
            <column></column>
            <column></column>
        </row>
        <row>
            <column>20013S</column>
            <column>Inscripci&#243;n</column>
            <column>1</column>
            <column>ITI</column>
            <column>010284</column>
            <column>&#193;lvarez Castillo Gabriel Iznardo</column>
            <column>M</column>
            <column>  17</column>
            <column>20013S</column>
            <column></column>
            <column>8-31-10-95</column>
            <column>Lanzagorta 606 &quot;A&quot;</column>
            <column>Soledad de Graciano Sanch&#233;z</column>
            <column>San Luis Potos&#237;</column>
            <column>S.E.E.R. - Profr.Librado Rivera</column>
            <column></column>
            <column></column>
            <column>07-05-1984</column>
            <column></column>
            <column></column>
        </row>
    </table>"""

//type BD = XmlProvider<"salida.xml">
type Alumnos = XmlProvider<TablaAlumnos>

let obtener_alumnos cookie (carrera, periodo) = 
    let rec aux cookie =
     try
      let f () =     Http.RequestString ("http://intranet.upslp.edu.mx:9080/Users/periodo.do",
                                         query = [("6578706f7274","1"); ("d-1782-e", "3"); ("matricula", "*"); ("method", "inscritos");
                                                  ("nomalu","*");
                                                  //("pdo","20013S");
                                                  ("pdo",periodo);
                                                  ("planest", carrera); ("reg1","0"); ("reg2", "99"); ("rep", "si");
                                                  ("sem1","0"); ("sem2","0"); ("sexo","*"); ("ultimo","20013S")],
                                         cookieContainer = cookie)
      let intranet = Library.recursive_timeout BaseDatos.db_timeout f ()
      let alumnos = Alumnos.Parse(intranet)
      (cookie, alumnos)
     with | :? System.Xml.XmlException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux cookie
          | :? System.Net.WebException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux cookie
    let (cookie, alumnos) = aux cookie
    printfn "Actualizando Alumnos de la carrera %s en el periodo %s..." carrera periodo
    let matriculas = set []
    Array.fold (fun set (alumno : Alumnos.Row) ->
//    for alumno in alumnos.Rows do
        let valores = [| for campo in alumno.Columns do
                            match campo.String with
                                Some s -> yield s
                                | _ -> yield "" |]
        let periodo = valores.[0]
        let estado = valores.[1]
        let semestre = valores.[2]
        let plan = valores.[3]
        let matricula = valores.[4]
//        ignore (matriculas.Add matricula)
        let nombre = valores.[5].Replace("'", "")
        let genero = valores.[6]
//        printfn "%s" valores.[9]
        let fecha = [| '/' |] |> valores.[9].Split
                                         |> (fun arr -> match arr with
                                                            [| dia; mes; ano |] -> DateTime(2000 + int ano, int mes, int dia)
                                                            | _ -> DateTime.Now)
//        printfn "%A" matricula
        let fecha_nacimiento = [| '-' |] |> valores.[17].Split
                                         |> (fun arr -> match arr with
                                                            [| dia; mes; ano |] -> DateTime(int ano, int mes, int dia)
                                                            | _ -> DateTime.Now)
        BaseDatos.actualiza_alumno matricula nombre genero fecha_nacimiento
        BaseDatos.actualiza_inscripciones matricula periodo estado semestre plan fecha
        Set.add matricula set) matriculas alumnos.Rows
        |> ignore
    cookie
//        |> (fun matriculas -> (cookie, matriculas))

