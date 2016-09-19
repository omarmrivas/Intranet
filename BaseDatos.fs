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

let select_matriculas carrera periodo =
    query { for registro in ctx.Intranet.Inscripciones do
            where (registro.Plan = carrera && registro.Periodo = periodo)
            select (registro.Matricula)}
            |> Seq.toList

let rec actualiza_alumno (matricula : string) (nombre : string) (genero : string) (fecha_nacimiento : DateTime) =
    let result = query { for registro in ctx.Intranet.Alumnos do
                         where (registro.Matricula = matricula)
                         select (registro)}
                            |> Seq.toList
    match result with
        [registro] -> registro.Delete()
                      ctx.SubmitUpdates()
                      actualiza_alumno matricula nombre genero fecha_nacimiento
       | _ -> let registro = ctx.Intranet.Alumnos.Create()
              registro.Matricula <- matricula
              registro.Nombre <- nombre
              registro.Genero <- genero
              registro.FechaNacimiento <- fecha_nacimiento
              ctx.SubmitUpdates()


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