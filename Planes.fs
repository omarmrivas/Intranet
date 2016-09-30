module Planes

open FSharp.Data
open System.Net
open System.Collections.Generic

type TPlanes = HtmlProvider<"planes.html">

type TPlanExtra = HtmlProvider<"extras.html">

let plan_ plan =
    match plan with
        "ISTI" -> "1"
      | "ITI" -> "2"
      | "LAG" -> "3"
      | "LMKT" -> "4"
      | "ITEM" -> "5"
      | "ITMA" -> "6"
      | _ -> "0"

let extrae_materias (materias : string) =
    [|'-'|] |> materias.Split
            |> Array.map (fun materia ->
                            ((materia.Replace("&lt;br&gt;", "")).Replace("-", "")).Replace("<br>","").Trim())
            |> Array.filter (fun materia -> String.length materia > 0)
            |> Array.map Library.normaliza_materia
            |> Array.toList

let clave_no_optativa (carrera : string) (materia : string) (clave : string) =
//    let clave = clave.TrimEnd [|'0'; '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; ' '|]
    match query {for A in BaseDatos.ctx.Intranet.Planes do
                 where (A.Materia = materia.Trim() && A.Carrera = carrera)
                 select A}
           |> Seq.toList with
        [A] -> A.Clave
       | _ -> match query {for A in BaseDatos.ctx.Intranet.Planes do
                           select A}
                |> Seq.toList
                |> List.choose (fun A -> let clave' = A.Clave.Trim()
                                         if clave'.StartsWith( clave )
                                         then let clave' = clave'.Replace(clave, "")
                                              if clave' = ""
                                              then Some 0
                                              else Some (int clave')
                                         else None) with
                head :: tail -> (head :: tail)
                                    |> List.max
                                    |> fun i -> i + 1
                                    |> fun n -> clave + string n
                | [] -> clave

let clave_optativa (carrera : string) (clave : string) =
    let clave = clave.TrimEnd [|'0'; '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; ' '|]
    match query {for A in BaseDatos.ctx.Intranet.Planes do
                 select A.Clave}
           |> Seq.toList
           |> List.choose (fun A -> let clave' = A.Trim()
                                    if clave'.StartsWith( clave )
                                    then let clave' = clave'.Replace(clave, "")
                                         if clave' = ""
                                         then None
                                         else Some (int clave')
                                    else None) with
        head :: tail -> (head :: tail)
                            |> List.max
                            |> fun i -> i + 1
                            |> fun n -> clave + string n
      | [] -> clave + "0"

let obtener_extracurriculares cookie =
    printfn "Actualizando las materias extracurriculares ..."
    let rec aux cookie =
     try
(*      let f () =   Http.RequestString ("http://intranet.upslp.edu.mx:9080/Users/planest.do",
                                       query = [("6578706f7274","1"); ("d-1782-e", "3"); ("method", "list");
                                                ("plan", "0"); ("rep", "si"); ("ultimo","")],
                                       cookieContainer = cookie)*)
      let f () =   IntranetAccess.request_string' ("http://intranet.upslp.edu.mx:9080/Users/planest.do",
                                                   [("6578706f7274","1"); ("d-1782-e", "3"); ("method", "list");
                                                    ("plan", "0"); ("rep", "si"); ("ultimo","")],
                                                   cookie)
      let intranet = Library.recursive_timeout BaseDatos.db_timeout f ()
      let materias = TPlanExtra.Parse(intranet)
//      ignore (materias.Tables.``Planes de estudio``.Html.Elements())
      (cookie, materias)
     with | :? System.Xml.XmlException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux cookie
          | :? System.Net.WebException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux cookie
    let (cookie, materias) = aux cookie
    let materias_nombres_largos = 
        List.item 1 (materias.Tables.``Planes de estudio``.Html.Elements())
          |> fun x -> x.Elements()
          |> List.map (fun (e : HtmlNode) -> 
                    let materia = List.item 1 (e.Elements())
                    let materia_str = materia.ToString()
                    let indx = materia_str.IndexOf("title=\"")
                    if indx >= 0
                    then let materia_str = materia_str.Substring( indx + 7 )
                         Some (materia_str.Remove( materia_str.IndexOf( "\"" ) ))
                    else None)
    [for materia in materias.Tables.``Planes de estudio``.Rows do
        yield materia]
        |> List.map2 (fun x y -> (y,x)) materias_nombres_largos
        |> List.fold (fun (programa', contador) (materia,smateria) -> 
            let programa = materia.Programa
            let programa = if programa <> ""
                           then programa
                           else programa'
            let mmateria = match smateria with
                            Some materia -> materia
                            | None       -> materia.Materia
            let clave = "EXT" + string contador
//            mapa_nombre_codigo.[ mmateria ] <- clave
            ignore (BaseDatos.actualiza_extra clave programa (Library.normaliza_materia mmateria) (string materia.Teoría) (string materia.Práctica) (materia.``Tipo Eval.``))
            (programa, contador + 1))
            ("", 0)
        |> ignore
    cookie
//    mapa_nombre_codigo

//let obtener_plan (mapa_nombre_codigo : Dictionary<string, string>) carrera =
let obtener_plan cookie carrera =
    let plan' = plan_ carrera
    let rec aux cookie =
     try
(*      let f () =   Http.RequestString ("http://intranet.upslp.edu.mx:9080/Users/planest.do",
                                       query = [("method", "list");
                                                ("plan", plan'); 
                                                ("rep", "si"); 
                                                ("ultimo","")],
                                       cookieContainer = cookie)*)
      let f () =   IntranetAccess.request_string' ("http://intranet.upslp.edu.mx:9080/Users/planest.do",
                                                   [("method", "list");
                                                    ("plan", plan'); 
                                                    ("rep", "si"); 
                                                    ("ultimo","")],
                                                   cookie)
      let intranet = Library.recursive_timeout BaseDatos.db_timeout f ()
      let planes = TPlanes.Parse(intranet)
      (cookie, planes)
     with | :? System.Xml.XmlException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux cookie
          | :? System.Net.WebException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux cookie
    let (cookie, planes) = aux cookie
    printfn "Actualizando Planes de la carrera %s ..." carrera

    let materias_nombre_largos = 
        List.item 1 (planes.Tables.``Planes de estudio``.Html.Elements())
          |> fun x -> x.Elements()
          |> List.map (fun (e : HtmlNode) -> 
                    let materia = List.item 1 (e.Elements())
                    let materia_str = materia.ToString()
                    let indx = materia_str.IndexOf("title=\"")
                    if indx >= 0
                    then let materia_str = materia_str.Substring( indx + 7 )
                         Some (materia_str.Remove( materia_str.IndexOf( "\"" ) ))
                    else None)
    let optativas_nombre_largos = 
        List.item 1 (planes.Tables.``Planes de estudio 2``.Html.Elements())
          |> fun x -> x.Elements()
          |> List.map (fun (e : HtmlNode) -> 
                    let materia = List.item 2 (e.Elements())
                    let materia_str = materia.ToString()
                    let indx = materia_str.IndexOf("title=\"")
                    if indx >= 0
                    then let materia_str = materia_str.Substring( indx + 7 )
                         Some (materia_str.Remove( materia_str.IndexOf( "\"" ) ))
                    else None)
    [for plan in planes.Tables.``Planes de estudio``.Rows do
        yield plan]
       |> List.map2 (fun x y -> (y,x)) materias_nombre_largos
       |> List.fold (fun semestre' (plan, smateria) -> 
            let semestre = plan.``Sem.``
            let semestre = if semestre.HasValue
                           then semestre.Value
                           else semestre'
            let materia = match smateria with
                            Some materia -> Library.normaliza_materia materia
                          | None         -> Library.normaliza_materia plan.Materia
            printfn "Semestre: %i - Materia: \"%s\"" semestre materia
            let seriacion = let dependencias = extrae_materias plan.Seriación
                            dependencias
                               |> List.choose (BaseDatos.obtener_clave_materia carrera)
                               |> String.concat ","
            let creditos = plan.Créditos
//            let clave = plan.Clave
            let clave = clave_no_optativa carrera materia (plan.Clave.Trim())
            if clave <> plan.Clave.Trim()
            then printfn "Materia '%s' con misma clave en la carrera %s (%s)" materia carrera (plan.Clave.Trim())
            else ()
(*            mapa_semestre.[ semestre ] <- clave :: mapa_semestre.[ int semestre ]
            mapa_seriaciones.[materia] <- clave*)
            let horas = if plan.Horas.HasValue
                        then plan.Horas.Value
                        else 0
            let teoria = plan.Teoría
            let practica = plan.Práctica
            let evaluacion = plan.``Tipo Eval.``
            BaseDatos.actualiza_planes carrera clave (string semestre) materia seriacion (string creditos) (string horas) (string teoria) (string practica) evaluacion
            semestre) 1
       |> ignore
    [for plan in planes.Tables.``Planes de estudio 2``.Rows do
        yield plan]
       |> List.map2 (fun x y -> (y,x)) optativas_nombre_largos
       |> List.fold (fun semestre' (plan, smateria) -> 
            let semestre = plan.``Sem.``
            let semestre = if semestre.HasValue
                           then semestre.Value
                           else semestre'
            let materia = match smateria with
                            Some materia -> Library.normaliza_materia materia
                            | None       -> Library.normaliza_materia plan.Optativa
//            printfn "Materia: \"%s\"" materia
            printfn "Semestre: %i - Materia: \"%s\"" semestre materia
            let seriacion = let dependencias = extrae_materias plan.Seriación
                            dependencias
                                  |> List.choose (BaseDatos.obtener_clave_materia carrera)
                                  |> String.concat ","
            let creditos = plan.Créditos
//            let clave = clave_optativa carrera (plan.Clave.Trim())
            let clave = clave_no_optativa carrera materia (plan.Clave.Trim())
//            printfn "Clave: '%s'" clave
(*            mapa_semestre.[ semestre ] <- clave :: mapa_semestre.[ int semestre ]
            mapa_seriaciones.[materia] <- clave*)
            let horas = plan.Horas
            let teoria = plan.Teoría
            let practica = plan.Práctica
            let evaluacion = plan.``Tipo Eval.``
            BaseDatos.actualiza_planes carrera clave (string semestre) materia seriacion (string creditos) (string horas) (string teoria) (string practica) evaluacion
            semestre ) 1
       |> ignore
    cookie
//    (mapa_seriaciones, mapa_semestre)
