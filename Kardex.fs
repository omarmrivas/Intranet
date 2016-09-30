module Kardex

open FSharp.Data
open System.Net
open System.Collections.Generic
open System.Xml

let counter = ref 0

let materias_equivalentes =
// Anormalidades detectadas
    ["jajaja", "jajaja"]
(*    [("Inglés-PET A", "Inglés-PET I");
     ("Inglés-FCE A", "Inglés-FCE I");
     ("Inglés-PET B", "Inglés-PET II");
     ("Introductorio de Inglés", "Inglés KET Intro");
     ("Inglés VI", "Inglés V");
     ("Inglés VII", "Inglés V");
     ("Inglés VIII", "Inglés V");
     ("Inglés VI-B", "Inglés V")
    ]*)

let equivalente codigo =
    match List.tryFind (fun (c, _) -> c = codigo) materias_equivalentes with
        Some (_, codigo) -> codigo
      | None             -> codigo

type kardex = 
    {matricula      : string
     materia        : string option
     grupo          : string
     c1             : string option
     i1             : uint32
     c2             : string option
     i2             : uint32
     c3             : string option
     i3             : uint32
     efinal         : string option
     final          : string option
     inasistencias  : uint32
     extraordinario : string option
     regularizacion : string option}

let kardex_revalidado matricula materia final = 
             {matricula      = matricula
              materia        = materia
              grupo          = Option.get materia
              c1             = None
              i1             = uint32 0
              c2             = None
              i2             = uint32 0
              c3             = None
              i3             = uint32 0
              efinal         = None
              final          = final
              inasistencias  = uint32 0
              extraordinario = None
              regularizacion = None}

[<Literal>]
let Kardex = """
    <table>
        <row>
            <column>080675</column>
            <column>Osuna Mart&#237;nez Fernando</column>
            <column>9</column>
            <column>Inteligencia Artificial II</column>
            <column>E15-643</column>
            <column>20151S</column>
            <column>9.6</column>
            <column> </column>
            <column> </column>
            <column>7</column>
            <column>Aprobado </column>
        </row>
        <row>
            <column>080675</column>
            <column>Osuna Mart&#237;nez Fernando</column>
            <column>9</column>
            <column>Residencia Profesional</column>
            <column>E15-942</column>
            <column>20151S</column>
            <column></column>
            <column></column>
            <column></column>
            <column></column>
            <column>*No-Evaluada</column>
        </row>
    </table>"""

[<Literal>]
let Parciales = """
    <table>
        <row>
            <column>100251</column>
            <column>Narváez Carrizalez Imelda</column>
            <column>ITI</column>
            <column>20151S</column>
            <column>Compiladores</column>
            <column>E15-837 - Reyes  / Alejandro </column>
            <column>7.5</column>
            <column>0</column>
            <column>5.0</column>
            <column>2</column>
            <column>8.0</column>
            <column>0</column>
            <column>8.0</column>
            <column>7.3</column>
            <column>2</column>
            <column> </column>
            <column> </column>
        </row>
        <row>
            <column>100251</column>
            <column>Narváez Carrizalez Imelda</column>
            <column>ITI</column>
            <column>20151S</column>
            <column>Inteligencia Artificial II</column>
            <column>E15-643 - Montaño Rivas / Omar</column>
            <column>6.5</column>
            <column>4</column>
            <column>4.0</column>
            <column>0</column>
            <column>10.0</column>
            <column>0</column>
            <column>10.0</column>
            <column>8.1</column>
            <column>4</column>
            <column> </column>
            <column> </column>
        </row>
    </table>"""

type TParciales = XmlProvider<Parciales>
type TKardex = XmlProvider<Kardex>

let obtiener_semestre codigo =
    let semestre = query {for A in BaseDatos.ctx.Intranet.Planes do
                          where (A.Clave = codigo)
                          select A.Semestre}
    if Seq.isEmpty semestre
    then sbyte 0
    else Seq.head semestre

let rec calificacion cal =
    match cal with
        "" -> None
      | str -> if str.Length > 5
               then (try
                        let v = double str
                        if v > 10.0
                        then let str' = string (v / 10.0)
                             printfn "Cambiando calificación '%s' por '%s'" str str'
                             Option.get (calificacion str')
                        else printfn "Algun error en el valor numérico de '%s' (truncando)" str
                             str.Substring(0,4)
                     with | :? System.FormatException -> str.Substring(0,4)) |> Some
               else Some str

let extraerGrupo (info : string) =
    match info.Split [|'-'|] |> Array.toList with
        | code1 :: code2 :: _ -> let grupo = code1.Trim() + "-" + code2.Trim()
                                 grupo
        | _ -> printfn "No se pudo extraer la información del grupo! %s" info
               ""

// http://intranet.upslp.edu.mx:9080/Users/kardex.do?cveMateria=0&gpo=*&matricula=100251&method=parciales&nomalu=*&nommat=*&pdo=20151S&plan=ITI&rep=si

//let obtener_kardex planes mapa_extra_codigo ((carrera, ((mapa_ser : Dictionary<string, string>), (mapa_sem : Dictionary<int, string list>))), periodo) =
let obtener_kardex cookie (carrera, periodo) =
  let matriculas = BaseDatos.select_matriculas carrera periodo
  printfn "Actualizando el kardex de la carrera %s en el periodo %s..." carrera periodo
  List.fold (fun cookie matricula ->
    printfn "%s" matricula
    let rec aux1 cookie =
     try
(*      let f () = 
        Http.RequestString ("http://intranet.upslp.edu.mx:9080/Users/kardex.do",
                            query = [("6578706f7274","1");
                                     ("cveMateria","0");
                                     ("d-1782-e","3")
                                     ("gpo","*");
                                     ("matricula",matricula);
                                     ("method","parciales");
                                     ("nomalu","*");
                                     ("nommat","*");
                                     ("pdo",periodo);
                                     ("plan",carrera);
                                     ("rep","si")],
                            cookieContainer = cookie)*)
      let f () = 
        IntranetAccess.request_string' 
                                   ("http://intranet.upslp.edu.mx:9080/Users/kardex.do",
                                    [("6578706f7274","1");
                                     ("cveMateria","0");
                                     ("d-1782-e","3")
                                     ("gpo","*");
                                     ("matricula",matricula);
                                     ("method","parciales");
                                     ("nomalu","*");
                                     ("nommat","*");
                                     ("pdo",periodo);
                                     ("plan",carrera);
                                     ("rep","si")],
                                    cookie)
      let intranet = Library.recursive_timeout BaseDatos.db_timeout f ()
      let materias = TParciales.Parse(intranet)
      (cookie, materias)
     with | :? System.Xml.XmlException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux1 cookie
          | :? System.Net.WebException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux1 cookie
          | :? System.AggregateException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                            aux1 cookie
    let (cookie, materias) = aux1 cookie
//    printfn "Actualizando el kardex del alumno con matrícula %s de la carrera %s en el periodo %s..." matricula carrera periodo
    let calificaciones =
        Array.fold (fun m (materia : TParciales.Row) -> 
//    for materia in materias.Rows do
            let valores = [| for campo in materia.Columns do
                                match campo.String with
                                    Some s -> yield s
                                    | _ -> yield "" |]
            let matricula = matricula
            let materia = equivalente (valores.[4].Trim())
                            |> Library.normaliza_materia
                            |> BaseDatos.obtener_clave_materia carrera
            let grupo = extraerGrupo (valores.[5].Trim())
            let c1 = calificacion (valores.[6].Trim())
            let i1 = uint32 (valores.[7].Trim())
            let c2 = calificacion (valores.[8].Trim())
            let i2 = uint32 (valores.[9].Trim())
            let c3 = calificacion (valores.[10].Trim())
            let i3 = uint32 (valores.[11].Trim())
            let efinal = calificacion (valores.[12].Trim())
            let final = calificacion (valores.[13].Trim())
            let inasistencias = uint32 (valores.[14].Trim())
            let extraordinario = calificacion (valores.[15].Trim())
            let regularizacion = calificacion (valores.[16].Trim())
            let kardex = {matricula      = matricula
                          grupo          = grupo
                          materia        = materia
                          c1             = c1
                          i1             = i1
                          c2             = c2
                          i2             = i2
                          c3             = c3
                          i3             = i3
                          efinal         = efinal
                          final          = final
                          inasistencias  = inasistencias
                          extraordinario = extraordinario
                          regularizacion = regularizacion}
            Map.add grupo kardex m) Map.empty materias.Rows
    let rec aux2 cookie =
     try
(*      let f () = 
        Http.RequestString ("http://intranet.upslp.edu.mx:9080/Users/kardex.do",
                            query = [("6578706f7274","1");
                                     ("aprobo","*");
                                     ("cveMateria","0");
                                     ("d-1782-e","3");
                                     ("gpo","*");
                                     ("matricula",matricula);
                                     ("method","list");
                                     ("nomalu","*");
                                     ("nommat","*");
                                     ("pdo",periodo);
                                     ("plan",carrera);
                                     ("rep","si");
                                     ("ultimo","20013S")],
                            cookieContainer = cookie)*)
      let f () = 
        IntranetAccess.request_string' 
                                   ("http://intranet.upslp.edu.mx:9080/Users/kardex.do",
                                    [("6578706f7274","1");
                                     ("aprobo","*");
                                     ("cveMateria","0");
                                     ("d-1782-e","3");
                                     ("gpo","*");
                                     ("matricula",matricula);
                                     ("method","list");
                                     ("nomalu","*");
                                     ("nommat","*");
                                     ("pdo",periodo);
                                     ("plan",carrera);
                                     ("rep","si");
                                     ("ultimo","20013S")],
                                    cookie)
      let intranet = Library.recursive_timeout BaseDatos.db_timeout f ()
      let materias = TKardex.Parse(intranet)
      (cookie, materias)
     with | :? System.Xml.XmlException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux2 cookie
          | :? System.Net.WebException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                          aux2 cookie
          | :? System.AggregateException -> let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                            aux2 cookie
    let (cookie, materias) = aux2 cookie
//    printfn "Actualizando el kardex del alumno con matrícula %s de la carrera %s en el periodo %s..." matricula carrera periodo
    for materia in materias.Rows do
        let valores = [| for campo in materia.Columns do
                            match campo.String with
                                Some s -> yield s
                                | _ -> yield "" |]
        let grupo = valores.[4].Trim()
        let kardex = if grupo = "REV"
                     then let matricula = valores.[0]
                          let materia' = valores.[3].Trim()
                          let materia = equivalente materia'
                                         |> Library.normaliza_materia
                                         |> BaseDatos.obtener_clave_materia carrera
                          let final = calificacion (valores.[6].Trim())
                          match materia with
                            Some _ -> Some (kardex_revalidado matricula materia final)
                          | None -> printfn "Caso raro: %s - %s - %s - %s" matricula materia' carrera periodo
                                    None
                     else if Map.containsKey grupo calificaciones
                          then Some (Map.find grupo calificaciones)
                          else None
        let semestre = sbyte (valores.[2].Trim())
        let estatus = valores.[10].Trim()
        match kardex with
            Some kardex -> 
                BaseDatos.actualiza_kardex matricula kardex.grupo kardex.materia semestre periodo kardex.c1 kardex.i1 
                                           kardex.c2 kardex.i2 kardex.c3 kardex.i3 kardex.efinal kardex.final 
                                           kardex.inasistencias kardex.extraordinario kardex.regularizacion estatus
          | None -> printfn "Materia en kardex que no aparece en parciales: (%s) - %s" grupo (valores.[3].Trim())
    cookie) cookie matriculas

