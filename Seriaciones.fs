module Seriaciones

open FSharp.Data
open System.Net
open System.Collections.Generic

type TSeriaciones = HtmlProvider<"seriaciones.html">

let plan_ plan =
    match plan with
        "ISTI" -> "1"
      | "ITI" -> "2"
      | "LAG" -> "3"
      | "LMKT" -> "4"
      | "ITEM" -> "5"
      | "ITMA" -> "6"
      | _ -> "0"

let obtiene_clave (materia : string) materia' carrera =
    let materia = if materia.Trim() = ""
                  then materia'
                  else materia
(*    match query {for A in DB.db.Planes do*)
    match query {for A in BaseDatos.ctx.Intranet.Planes do
                 where (A.Materia = materia && A.Carrera = carrera)
                 select A.Clave}
                |> Seq.toList with
        clave :: _ -> clave.Trim()
        | [] -> match query {for A in BaseDatos.ctx.Intranet.Extracurriculares do
                             where (A.Materia = materia)
                             select A.Clave}
                        |> Seq.toList with
                    clave :: _ -> clave.Trim()
                  | [] -> printfn "Materia %s no encontrada." materia
                          failwith (sprintf "Materia %s no encontrada." materia)

let actualiza_seriacion codigo seriacion =
    match query {for A in BaseDatos.ctx.Intranet.Planes do
                 where (A.Clave = codigo)
                 select A}
                 |> Seq.toList with
        [A] -> if A.Seriacion.Split [|','|]
                        |> Array.exists (fun c -> c = seriacion)
               then ()
               else let seriaciones = if A.Seriacion <> ""
                                      then A.Seriacion + "," + seriacion
                                      else seriacion
                    ignore (BaseDatos.actualiza_planes A.Carrera A.Clave (string A.Semestre) A.Materia seriaciones (string A.Creditos) (string A.Horas) (string A.Teoria) (string A.Practica) A.Evaluacion)
        | [] -> //failwith (sprintf "No existe la materia con código %s en los planes de estudio." codigo)
                printfn "No existe la materia con código %s en los planes de estudio." codigo
                printfn "Probablemente es una materia extracurricular. Ignorando dependencia."
(*       | [] -> match query {for A in DB.db.Extracurriculares do
                            where (A.Clave = codigo)
                            select A}
                 |> Seq.toList with
                [A] -> if A.Seriacion.Split [|','|]
                            |> Array.exists (fun c -> c = seriacion)
                       then ()
                       else let seriaciones = if A.Seriacion <> ""
                                              then A.Seriacion + "," + seriacion
                                              else seriacion
                            ignore (DBin.actualiza_extra A.Clave A.Programa A.Materia A.Teoria A.Practica A.Evaluacion)
               | [] -> failwith (sprintf "No existe la materia con código %s en los planes de estudio." codigo)
               | _ -> failwith (sprintf "Existen varias materias con este código %s en los planes de estudio." codigo)*)
       | _ -> failwith (sprintf "Existen varias materias con este código %s en los planes de estudio." codigo)

let obtener_seriacion_plan cookie carrera =
    let plan' = plan_ carrera
    let rec aux cookie =
     try
// http://intranet.upslp.edu.mx:9080/Users/planest.do?method=ser&plan=2&rep=si&ultimo=
      let f () =   Http.RequestString ("http://intranet.upslp.edu.mx:9080/Users/planest.do",
                                       query = [("method", "ser");
                                                ("plan", plan');
                                                ("rep", "si");
                                                ("ultimo","")],
                                       cookieContainer = cookie)
      let intranet = Library.recursive_timeout BaseDatos.db_timeout f ()
//      printfn "%s" intranet
      let planes = TSeriaciones.Parse(intranet)
      (cookie, planes)
     with | :? System.Xml.XmlException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux cookie
          | :? System.Net.WebException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux cookie
    let (cookie, planes) = aux cookie
    printfn "Actualizando Seriaciones de la carrera %s ..." carrera
    
    let materias_nombre_largos = 
        List.item 1 (planes.Tables.``Seriaciones Planes de estudio``.Html.Elements())
          |> fun x -> x.Elements()
          |> List.map (fun (e : HtmlNode) -> 
                    let materia = List.item 1 (e.Elements())
                    let materia_str = materia.ToString()
                    let indx = materia_str.IndexOf("title=\"")
                    if indx >= 0
                    then let materia_str = materia_str.Substring( indx + 7 )
                         Some (materia_str.Remove( materia_str.IndexOf( "\"" ) ))
                    else None)
    let seriadas_nombre_largos = 
        List.item 1 (planes.Tables.``Seriaciones Planes de estudio``.Html.Elements())
          |> fun x -> x.Elements()
          |> List.map (fun (e : HtmlNode) -> 
                    let materia = List.item 3 (e.Elements())
                    let materia_str = materia.ToString()
                    let indx = materia_str.IndexOf("title=\"")
                    if indx >= 0
                    then let materia_str = materia_str.Substring( indx + 7 )
                         Some (materia_str.Remove( materia_str.IndexOf( "\"" ) ))
                    else None)
    let nombre_largos = List.map2 (fun x y -> (x, y)) materias_nombre_largos seriadas_nombre_largos
    [for plan in planes.Tables.``Seriaciones Planes de estudio``.Rows do
        yield plan]
       |> List.map2 (fun x y -> (y,x)) nombre_largos
       |> List.fold (fun (materia', seriada') (plan, (smateria, sseriada)) -> 
            let semestre1 = plan.Semestre
            let semestre2 = plan.Semestre2
            let materia = match smateria with
                            Some materia -> Library.normaliza_materia materia
                          | None         -> Library.normaliza_materia plan.Materia
            let seriada = match sseriada with
                            Some materia -> Library.normaliza_materia materia
                          | None         -> Library.normaliza_materia plan.``Seriada con``
            let codigo_materia = obtiene_clave materia materia' carrera
            printfn "--materia (%s), materia' (%s), carrera (%s), codigo_materia (%s)" materia materia' carrera codigo_materia
            let codigo_seriada = obtiene_clave seriada seriada' carrera
            printfn "--seriada (%s), seriada' (%s), carrera (%s), codigo_seriada (%s)" seriada seriada' carrera codigo_seriada
//            printfn "Semestre: %i - Materia: '%s' - Semestre: %i - Seriada: '%s'" semestre1 materia semestre2 seriada
                                                
//            printfn "Semestre: %i - Materia: '%s' (%s) - Semestre: %i - Seriada: '%s' (%s)" semestre1 materia codigo_materia semestre2 seriada codigo_seriada
            actualiza_seriacion codigo_seriada codigo_materia
//            ignore (DBin.actualiza_planes carrera clave semestre materia seriacion creditos horas teoria practica evaluacion)
            (materia, seriada)) ("", "")
       |> ignore
//    let algo = List.length (planes.Tables.``Seriaciones Planes de estudio 2``.Html.Elements())
    try
    let optativas_nombre_largos = 
        List.item 1 (planes.Tables.``Seriaciones Planes de estudio 2``.Html.Elements())
          |> fun x -> x.Elements()
          |> List.map (fun (e : HtmlNode) -> 
                    let materia = List.item 1 (e.Elements())
                    let materia_str = materia.ToString()
                    let indx = materia_str.IndexOf("title=\"")
                    if indx >= 0
                    then let materia_str = materia_str.Substring( indx + 7 )
                         Some (materia_str.Remove( materia_str.IndexOf( "\"" ) ))
                    else None)
    let optativas_seriada_nombre_largos = 
        List.item 1 (planes.Tables.``Seriaciones Planes de estudio 2``.Html.Elements())
          |> fun x -> x.Elements()
          |> List.map (fun (e : HtmlNode) -> 
                    let materia = List.item 2 (e.Elements())
                    let materia_str = materia.ToString()
                    let indx = materia_str.IndexOf("title=\"")
                    if indx >= 0
                    then let materia_str = materia_str.Substring( indx + 7 )
                         Some (materia_str.Remove( materia_str.IndexOf( "\"" ) ))
                    else None)
    let opt_nombre_largos = List.map2 (fun x y -> (x, y)) optativas_nombre_largos optativas_seriada_nombre_largos
    [for plan in planes.Tables.``Seriaciones Planes de estudio 2``.Rows do
        yield plan]
       |> List.map2 (fun x y -> (y,x)) opt_nombre_largos
       |> List.fold (fun (materia', seriada') (plan, (smateria, sseriada)) -> 
            let materia = match smateria with
                            Some materia -> Library.normaliza_materia materia
                            | None       -> Library.normaliza_materia plan.Materia
            let seriada = match smateria with
                            Some materia -> Library.normaliza_materia materia
                            | None       -> if plan.``Seriada con`` = ""
                                            then Library.normaliza_materia seriada'
                                            else Library.normaliza_materia plan.``Seriada con``
            let codigo_materia = obtiene_clave materia materia' carrera
            printfn "--materia (%s), materia' (%s), carrera (%s), codigo_materia (%s)" materia materia' carrera codigo_materia
            let codigo_seriada = obtiene_clave seriada seriada' carrera
            printfn "--seriada (%s), seriada' (%s), carrera (%s), codigo_seriada (%s)" seriada seriada' carrera codigo_seriada
//            printfn "Materia: '%s' - Seriada: '%s'" materia seriada
            actualiza_seriacion codigo_seriada codigo_materia
//            printfn "Materia: '%s' (%s) - Seriada: '%s' (%s)" materia codigo_materia seriada codigo_seriada
            //ignore (DBin.actualiza_planes carrera clave semestre materia seriacion creditos horas teoria practica evaluacion)
            (materia, seriada)) ("", "")
       |> ignore
    with | :? System.Collections.Generic.KeyNotFoundException -> ()
    ()
