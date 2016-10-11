module Grupos

open System
open FSharp.Data
open System.Net
open System.Collections.Generic

[<Literal>]
let TablaGrupos = """
<table>
<row>
<column>A16-001</column>
<column>Curso del Núcleo General II: Comunicación e Investigación</column>
<column>A45</column>
<column>11:00 - 12:00</column>
<column>11:00 - 12:00</column>
<column>11:00 - 12:00</column>
<column>11:00 - 12:00</column>
<column>11:00 - 12:00</column>
<column> - </column>
<column>881 / Gutiérrez Peralta / Alma Flor</column>
<column>35</column>
<column>35</column>
<column>33</column>
<column>Abierto</column>
<column>ITMA. ITEM. LMKT. LAG. ITI. ISTI. </column>
</row>
<row>
<column>A16-002</column>
<column>Curso del Núcleo General III: Filosofía y Valores</column>
<column>A28</column>
<column>10:00 - 11:00</column>
<column>10:00 - 11:00</column>
<column>10:00 - 11:00</column>
<column> - </column>
<column> - </column>
<column> - </column>
<column>881 / Gutiérrez Peralta / Alma Flor</column>
<column>40</column>
<column>40</column>
<column>39</column>
<column>Abierto</column>
<column>ITMA. ITEM. LMKT. LAG. ITI. ISTI. </column>
</row>
</table>"""

type Grupos = XmlProvider<TablaGrupos>

let extraerInfo (info : string) =
    match info.Split [|'/'|] with
        | [| id; pendiente |] -> (id.Trim(), (pendiente.Trim(), pendiente.Trim()))
        | [| id; apellidos; nombre |] -> (id.Trim(), (apellidos.Trim(), nombre.Trim()))
        | _ -> printfn "Información de profesor inválida: %s" info
               ("", ("", ""))

let obtener_grupos cookie periodo =
    let rec aux cookie =
     try
(*      let f () =     Http.RequestString ("http://intranet.upslp.edu.mx:9080/Users/periodo.do",
                                         query = [("6578706f7274","1"); ("aula", "0"); ("aulanom", "*"); ("cveMateria", "0");
                                                  ("d-1782-e","3"); ("method","grupos"); ("nommat",""); ("nomprof", "");
                                                  ("pdo",periodo); ("rep", "si"); ("ultimo","20013S")],
                                         cookieContainer = cookie)*)
      let f () =     IntranetAccess.request_string' ("http://intranet.upslp.edu.mx:9080/Users/periodo.do",
                                                     [("6578706f7274","1"); ("aula", "0"); ("aulanom", "*"); ("cveMateria", "0");
                                                      ("d-1782-e","3"); ("method","grupos"); ("nommat",""); ("nomprof", "");
                                                      ("pdo",periodo); ("rep", "si"); ("ultimo","20013S")],
                                                     cookie)
      let intranet = Library.recursive_timeout BaseDatos.db_timeout f ()
      let grupos = Grupos.Parse(intranet)
      (cookie, grupos)
     with | :? System.Xml.XmlException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux cookie
          | :? System.Net.WebException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux cookie
    let (cookie, grupos) = aux cookie
    printfn "Actualizando grupos en el periodo %s..." periodo
    Array.fold (fun m (grupo : Grupos.Row) -> 
        let valores = [| for campo in grupo.Columns do
                            match campo.String with
                                Some s -> yield s
                                | _ -> yield "" |]
        let grupo = valores.[0]
//        let materia = valores.[1]
        let materia = valores.[1]
        let aula = valores.[2]
        let lunes = valores.[3]
        let martes = valores.[4]
        let miercoles = valores.[5]
        let jueves = valores.[6]
        let viernes = valores.[7]
        let sabado = valores.[8]
        let (idProfesor, (apellidosProfesor, nombreProfesor)) = extraerInfo (valores.[9])
        let alumnos = valores.[12]
        let estado = valores.[13]
        let plan = valores.[14]
//        printfn "grupo:%s periodo:%s materia:%s aula:%s lunes:%s martes:%s miercoles:%s jueves:%s viernes:%s sabado:%s idProfesor:%s alumnos:%s estado:%s plan:%s" grupo periodo materia aula lunes martes miercoles jueves viernes sabado idProfesor alumnos estado plan
        BaseDatos.actualiza_grupos grupo periodo materia aula lunes martes miercoles jueves viernes sabado idProfesor alumnos estado plan
        (Map.add (idProfesor,periodo) (apellidosProfesor, nombreProfesor) m)) Map.empty grupos.Rows
        |> (fun x -> cookie)

