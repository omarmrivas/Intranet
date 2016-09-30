module BaseDatos

open System
open MySql
open MySql.Data
open FSharp.Data
open FSharp.Data.Sql
open MySql.Data.MySqlClient
open System.Net
open System.Linq
open System.Collections.Generic

let db_timeout = 60000

[<Literal>]
let connectionString = @"Server=127.0.0.1; Port=3306; User ID=intranet; Password=intranet; Database=intranet"

[<Literal>]
let resolutionFolder = @"packages/MySql.Data.6.9.9/lib/net40"

[<Literal>]
let dbVendor = Common.DatabaseProviderTypes.MYSQL


type Sql = 
    SqlDataProvider< 
        ConnectionString = connectionString,
        DatabaseVendor = dbVendor,
        ResolutionPath = resolutionFolder,
        UseOptionTypes = true >

let ctx = Sql.GetDataContext()

let to_number number (str : string) =
    try
        number str
    with | :? System.FormatException -> number "0"

let to_sbyte = to_number sbyte
let to_double = to_number float32
let to_uint32 = to_number uint32

let select_matriculas carrera periodo =
    query { for registro in ctx.Intranet.Inscripciones do
            where (registro.Plan = carrera && registro.Periodo = periodo)
            select (registro.Matricula)}
            |> Seq.toList

let obtener_clave_profesor (grupo : string) =
    match query {for A in ctx.Intranet.Grupos do
                 where (A.Grupo = grupo)
                 select A.Profesor}
                |> Seq.toList with
        [profesor] -> Some profesor
       | _ -> None

// matricula nombre genero fecha_nacimiento ingreso telefono direccion colonia cp municipio procedencia
let rec actualiza_alumno (matricula : string) (nombre : string) (genero : string) (fecha_nacimiento : DateTime) ingreso telefono direccion colonia cp municipio procedencia =
    let result = query { for registro in ctx.Intranet.Alumnos do
                         where (registro.Matricula = matricula)
                         select (registro)}
                            |> Seq.toList
    match result with
        [registro] -> registro.Delete()
                      ctx.SubmitUpdates()
                      actualiza_alumno matricula nombre genero fecha_nacimiento ingreso telefono direccion colonia cp municipio procedencia
       | _ -> let registro = ctx.Intranet.Alumnos.Create()
              registro.Matricula <- matricula
              registro.Nombre <- nombre
              registro.Genero <- genero
              registro.FechaNacimiento <- fecha_nacimiento
              registro.Ingreso <- ingreso
              registro.Telefono <- telefono
              registro.Direccion <- direccion
              registro.Colonia <- colonia
              registro.Cp <- cp
              registro.Municipio <- municipio
              registro.Procedencia <- procedencia
              ctx.SubmitUpdates()

let obtener_clave_materia carrera (materia : string) =
    match query {for A in ctx.Intranet.Planes do
                 where (A.Materia = materia && A.Carrera = carrera)
                 select A.Clave}
                |> Seq.toList with
        [clave] -> Some (clave.Trim())
        | [] -> match query {for A in ctx.Intranet.Extracurriculares do
                             where (A.Materia = materia)
                             select A.Clave}
                        |> Seq.toList with
                    [clave] -> Some (clave.Trim())
                  | [] -> printfn "Materia '%s' (%i) no encontrada en la carrera '%s'." materia (String.length materia) carrera
                          None
                  | _ ->  printfn "Más de una materia con nombre %s en la carrera %s" materia carrera
                          None
        | _ -> printfn "Más de una materia con nombre %s en la carrera %s" materia carrera
               None

let rec actualiza_inscripciones (matricula : string) (periodo : string) (estado : string) (semestre : string) (plan : string) (fecha : DateTime) =
    let result = query { for registro in ctx.Intranet.Inscripciones do
                         where (registro.Matricula = matricula && registro.Periodo = periodo)
                         select (registro)}
                            |> Seq.toList
    match result with
        [registro] -> registro.Delete()
                      ctx.SubmitUpdates()
                      actualiza_inscripciones matricula periodo estado semestre plan fecha
       | _ -> let registro = ctx.Intranet.Inscripciones.Create()
              registro.Matricula <- matricula
              registro.Periodo <- periodo
              registro.Estado <- estado
              registro.Semestre <- to_sbyte semestre
              registro.Plan <- plan
              registro.Fecha <- fecha
              ctx.SubmitUpdates()

let rec actualiza_extra (clave : string) (programa : string) (materia : string) (teoria : string) (practica : string) (evaluacion : string) =
    let result = query { for registro in ctx.Intranet.Extracurriculares do
                         where (registro.Clave = clave)
                         select (registro)}
                            |> Seq.toList
    match result with
        [registro] -> registro.Delete()
                      ctx.SubmitUpdates()
                      actualiza_extra clave programa materia teoria practica evaluacion
       | _ -> let registro = ctx.Intranet.Extracurriculares.Create()
              registro.Clave <- clave
              registro.Programa <- programa
              registro.Materia <- materia
              registro.Teoria <- to_sbyte teoria
              registro.Practica <- to_sbyte practica
              registro.Evaluacion <- evaluacion
              ctx.SubmitUpdates()


let rec actualiza_planes carrera clave semestre materia seriacion creditos horas teoria practica evaluacion =
    let result = query { for registro in ctx.Intranet.Planes do
                         where (registro.Carrera = carrera && registro.Clave = clave && registro.Materia = materia)
                         select registro}
                            |> Seq.toList
    match result with
        [registro] -> registro.Delete()
                      ctx.SubmitUpdates()
                      actualiza_planes carrera clave semestre materia seriacion creditos horas teoria practica evaluacion
       | _ -> let registro = ctx.Intranet.Planes.Create()
              registro.Carrera <- carrera
              registro.Clave <- clave
              registro.Semestre <- to_sbyte semestre
              registro.Materia <- materia
              registro.Seriacion <- seriacion
              registro.Creditos <- to_sbyte creditos
              registro.Horas <- to_sbyte horas
              registro.Teoria <- to_sbyte teoria
              registro.Practica <- to_sbyte practica
              registro.Evaluacion <- evaluacion
              ctx.SubmitUpdates()

let rec actualiza_grupos grupo periodo materia aula lunes martes miercoles jueves viernes sabado profesor alumnos estado plan =
    let result = query { for registro in ctx.Intranet.Grupos do
                         where (registro.Grupo = grupo)
                         select registro}
                            |> Seq.toList
    match result with
        [registro] -> registro.Delete()
                      ctx.SubmitUpdates()
                      actualiza_grupos grupo periodo materia aula lunes martes miercoles jueves viernes sabado profesor alumnos estado plan
       | _ -> let registro = ctx.Intranet.Grupos.Create()
              registro.Grupo <- grupo
              registro.Periodo <- periodo
              registro.Materia <- materia
              registro.Aula <- aula
              registro.Lunes <- lunes
              registro.Martes <- martes
              registro.Miercoles <- miercoles
              registro.Jueves <- jueves
              registro.Viernes <- viernes
              registro.Sabado <- sabado
              registro.Profesor <- to_uint32 profesor
              registro.Alumnos <- to_uint32 alumnos
              registro.Estado <- estado
              registro.Plan <- plan
              ctx.SubmitUpdates()

let rec actualiza_profesores profesor periodo nombre apellidos tipo =
    let result = query { for registro in ctx.Intranet.Profesores do
                         where (registro.Profesor = to_uint32 profesor && registro.Periodo = periodo)
                         select registro}
                            |> Seq.toList
    match result with
        [registro] -> registro.Delete()
                      ctx.SubmitUpdates()
                      actualiza_profesores profesor periodo nombre apellidos tipo
       | _ -> let registro = ctx.Intranet.Profesores.Create()
              registro.Profesor <- to_uint32 profesor
              registro.Periodo <- periodo
              registro.Nombre <- nombre
              registro.Apellidos <- apellidos
              registro.Tipo <- tipo
              ctx.SubmitUpdates()

let rec actualiza_kardex matricula grupo materia semestre periodo c1 i1 c2 i2 c3 i3 efinal final inasistencias extraordinario regularizacion estatus =
    let result = query { for registro in ctx.Intranet.Kardex do
                         where (registro.Matricula = matricula && registro.Grupo = grupo)
                         select (registro)}
                            |> Seq.toList
    match result with
        [registro] -> registro.Delete()
                      ctx.SubmitUpdates()
                      actualiza_kardex matricula grupo materia semestre periodo c1 i1 c2 i2 c3 i3 efinal final inasistencias extraordinario regularizacion estatus
       | _ -> let registro = ctx.Intranet.Kardex.Create()
              registro.Matricula <- matricula
              registro.Grupo <- grupo
              registro.Materia <- materia
              registro.Semestre <- semestre
              registro.Periodo <- periodo
              registro.C1 <- c1
              registro.I1 <- i1
              registro.C2 <- c2
              registro.I2 <- i2
              registro.C3 <- c3
              registro.I3 <- i3
              registro.Efinal <- efinal
              registro.Final <- final
              registro.Inasistencias <- inasistencias
              registro.Extraordinario <- extraordinario
              registro.Regularizacion <- regularizacion
              registro.Estatus <- estatus
              ctx.SubmitUpdates()


(*

let rec actualiza_kardex matricula semestre materia periodo final extraordinario regularizacion inasistencias estatus =
    let result = query { for registro in ctx.``[curricula_upslp].[kardex]`` do
                         where (registro.matricula = matricula && registro.periodo = periodo && registro.materia = materia)
                         select (registro)}
                            |> Seq.toList
    match result with
        [registro] -> registro.Delete()
                      ctx.SubmitUpdates()
                      actualiza_kardex matricula semestre materia periodo final extraordinario regularizacion inasistencias estatus
(*                      registro.semestre <- to_sbyte semestre
                      registro.final <- to_double final
                      registro.extraordinario <- to_double extraordinario
                      registro.regularizacion <- to_double regularizacion
                      registro.inasistencias <- to_sbyte inasistencias
                      registro.estatus <- estatus
                      ctx.SubmitUpdates()*)
       | _ -> let registro = ctx.``[curricula_upslp].[kardex]``.Create()
              registro.matricula <- matricula
              registro.periodo <- periodo
              registro.materia <- materia
              registro.semestre <- to_sbyte semestre
              registro.final <- to_double final
              registro.extraordinario <- to_double extraordinario
              registro.regularizacion <- to_double regularizacion
              registro.inasistencias <- to_sbyte inasistencias
              registro.estatus <- estatus
              ctx.SubmitUpdates()
      
let rec actualiza_modelo materia periodo clase continuo_discreto atributos
                         aprobados_correctos reprobados_incorrectos
                         aprobados_incorrectos reprobados_correctos =
    let result = query { for registro in ctx.``[curricula_upslp].[modelos]`` do
                         where (registro.materia = materia &&
                                registro.periodo = periodo)
                         select registro}
                            |> Seq.toList
    match result with
        [registro] -> registro.Delete()
                      ctx.SubmitUpdates()
                      actualiza_modelo materia periodo clase continuo_discreto atributos
                                       aprobados_correctos reprobados_incorrectos
                                       aprobados_incorrectos reprobados_correctos
(*                      registro.nombre <- nombre
                      registro.genero <- genero
                      registro.fecha_nacimiento <- fecha_nacimiento
                      ctx.SubmitUpdates()*)
       | _ -> let registro = ctx.``[curricula_upslp].[modelos]``.Create()
              registro.materia <- materia
              registro.periodo <- periodo
              registro.clase <- clase
              registro.continuo_discreto <- continuo_discreto
              registro.atributos <- atributos
              registro.aprobados_correctos <- aprobados_correctos
              registro.reprobados_incorrectos <- reprobados_incorrectos
              registro.aprobados_incorrectos <- aprobados_incorrectos
              registro.reprobados_correctos <- reprobados_correctos
              ctx.SubmitUpdates()
                        *)