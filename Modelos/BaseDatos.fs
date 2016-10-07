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

type Kardex = 
    {Matricula : string
     Grupo  : string
     Materia   : string
     Semestre  : sbyte
     Periodo   : string
     C1        : string
     I1        : int32
     C2        : string
     I2        : int32
     C3        : string
     I3        : int32
     Efinal    : string
     Final     : string
     Inasistencias  : int
     Extraordinario : string
     Regularizacion : string
     Estatus        : string}

type NominalModel =
    {materia                  : string
     periodoInicial           : string
     periodoFinal             : string
     clase                    : string
     rutaMaterias             : string list
     atributos                : string list
     matrizConfusion          : double [][]
     numInstancias            : double
     correctas                : double
     modelo                   : byte []
     instancias               : byte []}

type PersonalInfo =
    {Genero      : string
     Ingreso     : string
     Colonia     : string
     Cp          : string
     Municipio   : string
     Procedencia : string}

let db_timeout = 60000

[<Literal>]
let connectionString = @"Server=127.0.0.1; Port=3306; User ID=intranet; Password=intranet; Database=intranet"

[<Literal>]
let resolutionFolder = @"packages/MySql.Data.6.9.9/lib/net45"

[<Literal>]
let dbVendor = Common.DatabaseProviderTypes.MYSQL

type Sql = 
    SqlDataProvider< 
        ConnectionString = connectionString,
        DatabaseVendor = dbVendor,
        ResolutionPath = resolutionFolder,
        UseOptionTypes = true >

let ctx = Sql.GetDataContext()

let obtener_datos periodoInicial periodoFinal codigo =
    ctx.Procedures.DatosEntrenamiento.Invoke(periodoInicial, periodoFinal, codigo).ResultSet
        |> Seq.map (fun r -> r.MapTo<Kardex>())
        |> Seq.distinctBy (fun k -> (k.Matricula, k.Materia))
        |> Seq.toList

let obtener_clave_profesor (grupo : string) =
    match query {for A in ctx.Intranet.Grupos do
                 where (A.Grupo = grupo)
                 select A.Profesor}
                |> Seq.toList with
        [profesor] -> profesor
       | _ -> None

(* FIXME: Asume que la información personal de los alumnos que usarán los modelos
          coincide con la de LocalDataStoreSlot alumnos de entrenamiento. 
          Esto no es muy realista pero al menos funciona *)
let cabeceraPersonal data (atts :java.util.ArrayList, mapa) materia i =
    let matriculas = List.map fst data
    let personalInfo =
        matriculas |> List.map (fun matricula ->
            query {for A in ctx.Intranet.Alumnos do
                   where (A.Matricula = matricula)
                   select A})
                   |> Seq.concat
                   |> Seq.toList
                   |> List.map (fun A ->  {Genero = A.Genero
                                           Ingreso = A.Ingreso
                                           Colonia = A.Colonia
                                           Cp = A.Cp
                                           Municipio = A.Municipio
                                           Procedencia = A.Procedencia})
    let generos = personalInfo |> List.groupBy (fun A -> A.Genero)
                               |> List.map fst
    let ingresos = personalInfo |> List.groupBy (fun A -> A.Ingreso)
                                |> List.map fst
    let cps = personalInfo |> List.groupBy (fun A -> A.Cp)
                           |> List.map fst
    let colonias = personalInfo |> List.groupBy (fun A -> A.Colonia)
                                |> List.map fst
    let municipios = personalInfo |> List.groupBy (fun A -> A.Municipio)
                                  |> List.map fst
    let procedencias = personalInfo |> List.groupBy (fun A -> A.Municipio)
                                    |> List.map fst

    // Agregar los valores posibles a los atributos
    let generosVals = java.util.ArrayList()
    generos |> List.iter (ignore << generosVals.add)

    let ingresosVals = java.util.ArrayList()
    ingresos |> List.iter (ignore << ingresosVals.add)

    let cpsVals = java.util.ArrayList()
    cps |> List.iter (ignore << cpsVals.add)

    let coloniasVals = java.util.ArrayList()
    colonias |> List.iter (ignore << coloniasVals.add)

    let municipiosVals = java.util.ArrayList()
    municipios |> List.iter (ignore << municipiosVals.add)

    let procedenciasVals = java.util.ArrayList()
    procedencias |> List.iter (ignore << procedenciasVals.add)

    atts.add( weka.core.Attribute("genero", generosVals) )|> ignore
    atts.add( weka.core.Attribute("ingresos", ingresosVals) )|> ignore
    atts.add( weka.core.Attribute("colonias", coloniasVals) )|> ignore
    atts.add( weka.core.Attribute("municipios", municipiosVals) )|> ignore
    atts.add( weka.core.Attribute("genero", generosVals) )|> ignore
    atts.add( weka.core.Attribute("genero", generosVals) )|> ignore

    personalInfo



let cabeceraNumerica data (atts :java.util.ArrayList, mapa) materia i =
    // Clave Profesor
    let profesoresVals = java.util.ArrayList()
    data |> List.choose (fun (_, ks) -> obtener_clave_profesor ((List.item i ks).Grupo))
         |> set
         |> Set.toList
         |> List.map string
         |> List.iter (fun clave -> profesoresVals.add(clave) |> ignore)
    atts.add( weka.core.Attribute(materia + "_profesor", profesoresVals) ) |> ignore
    // C1
    atts.add( weka.core.Attribute(materia + "_c1") )|> ignore
    // I1
    atts.add( weka.core.Attribute(materia + "_i1") )|> ignore
    // C2
    atts.add( weka.core.Attribute(materia + "_c2") )|> ignore
    // I2
    atts.add( weka.core.Attribute(materia + "_i2") )|> ignore
    // C3
    atts.add( weka.core.Attribute(materia + "_c3") )|> ignore
    // I3
    atts.add( weka.core.Attribute(materia + "_i3") )|> ignore
    // Efinal
    atts.add( weka.core.Attribute(materia + "_efinal") )|> ignore
    // Final
    atts.add( weka.core.Attribute(materia + "_final") )|> ignore
    // Inasistencias
    atts.add( weka.core.Attribute(materia + "_inasistencias") )|> ignore
    // Estatus
    let estatusVals =java.util.ArrayList()
    estatusVals.add("Aprobado") |> ignore
    estatusVals.add("Reprobado") |> ignore
    atts.add( weka.core.Attribute(materia + "_estatus", estatusVals) ) |> ignore
    // Mapa
    let mapa = Map.add (materia + "_profesor") profesoresVals mapa
    (atts, Map.add (materia + "_estatus") estatusVals mapa)

let instanciaNumerica data (atts :java.util.ArrayList, mapa : Map<string,java.util.ArrayList>) i materia =
    let toDouble v =
        match v with
          | "SD" -> -1.0
          | "NP" -> -1.0
          | "NA" -> 0.0
          | "AC." -> 10.0
          | "AC" -> 10.0
          | null -> 0.0
          | "-" -> -1.0
          | "." -> -1.0
          | _ -> try double v
                 with | :? System.FormatException -> printfn "Caso no previsto: %s" v
                                                     0.0

    let toEstatus calFinal v =
        match v with
            | "Aprobado" -> "Aprobado"
            | "Aprobado(*REV)" -> "Aprobado"
            | _ -> if calFinal >= 7.0 
                   then "Aprobado"
                   else "Reprobado"
    data |> List.map (fun (_, instancia) -> List.item i instancia)
         |> List.map (fun k -> let profesoresVals = Map.find (materia + "_profesor") mapa
                               let estatusVals = Map.find (materia + "_estatus") mapa
                               let calFinal = toDouble k.Final
                               [| (float)(profesoresVals.indexOf( (string << Option.get) (obtener_clave_profesor k.Grupo)))
                                  toDouble k.C1
                                  double k.I1
                                  toDouble k.C2
                                  double k.I2
                                  toDouble k.C3
                                  double k.I3
                                  toDouble k.Efinal
                                  calFinal
                                  double k.Inasistencias
                                  (float)(estatusVals.indexOf( toEstatus calFinal k.Estatus )) |])

let agrega_indice ruta =
    ruta |> List.fold (fun (m, ruta) materia -> 
                             match Map.tryFind materia m with
                              | None   -> (Map.add materia 2 m, (materia + "_1") :: ruta)
                              | Some i -> (Map.add materia (i + 1) m, (materia + "_" + string i) :: ruta)) (Map.empty, [])
         |> (List.rev << snd)

let verifica_ruta ruta mapa matricula =
    let m = Map.find matricula mapa
    if ruta |> List.forall (fun materia -> Map.containsKey materia m)
    then Some (matricula, List.map (fun materia -> Map.find materia m) ruta)
    else None

let rec getAttributes L (en : java.util.Enumeration) =
    if en.hasMoreElements()
    then getAttributes (en.nextElement() :: L) en
    else L |> List.rev
           |> List.map (fun obj -> downcast obj : weka.core.Attribute)

let quitar_p3 prefijo =
    [prefijo + "_efinal"
     prefijo + "_final"
     prefijo + "_inasistencias"]

let quitar_p2 prefijo =
    [prefijo + "_c3"
     prefijo + "_i3"
     prefijo + "_efinal"
     prefijo + "_final"
     prefijo + "_inasistencias"]

let quitar_p1 prefijo =
    [prefijo + "_c2"
     prefijo + "_i2"
     prefijo + "_c3"
     prefijo + "_i3"
     prefijo + "_efinal"
     prefijo + "_final"
     prefijo + "_inasistencias"]

let quitar_todo prefijo =
    [prefijo + "_c1"
     prefijo + "_i1"
     prefijo + "_c2"
     prefijo + "_i2"
     prefijo + "_c3"
     prefijo + "_i3"
     prefijo + "_efinal"
     prefijo + "_final"
     prefijo + "_inasistencias"]


let nueva_ruta comando_filtro codigo (ruta, data) =
    let (attrs, m) = List.fold2 (cabeceraNumerica data) (java.util.ArrayList(), Map.empty) ruta [0 .. List.length ruta - 1]
    let instancias = List.mapi (instanciaNumerica data (attrs, m)) ruta
    let valores = [0 .. (List.length << List.head) instancias - 1]
                    |> List.map (fun i -> instancias |> List.map (List.item i)
                                                     |> Array.concat)
    let weka_data = weka.core.Instances(codigo + "_relation", attrs, 0)
    let target = getAttributes [] (weka_data.enumerateAttributes())
                    |> List.rev
                    |> List.tryFind (fun att -> let name = att.name()
                                                name.StartsWith codigo && name.EndsWith "_estatus")
    match target with
        Some target -> 
            weka_data.setClass( target )
            valores |> List.iter (fun values -> weka_data.add( weka.core.DenseInstance(1.0, values) ) |> ignore)

            // Quitar atributos de calificaciones de parciales y final
            let target_name = target.name()
            let prefijo = target_name.Remove (target_name.IndexOf "_estatus")
            let quitar = quitar_todo prefijo
            let filter_options =
                getAttributes [] (weka_data.enumerateAttributes())
                    |> List.mapi (fun i attr -> let attr_name = attr.name()
                                                if List.exists (fun name -> name = attr_name) quitar
                                                then Some (i+1)
                                                else None)
                    |> List.choose (fun x -> x)
                    |> List.map (fun i -> string i)
                    |> String.concat ","
                    |> (fun str -> "-R " + str)
            let remove = weka.filters.unsupervised.attribute.Remove()
            remove.setOptions( weka.core.Utils.splitOptions( filter_options ) )
            remove.setInputFormat( weka_data ) |> ignore
            let weka_data = weka.filters.Filter.useFilter( weka_data, remove )

            // Filtering
            let filter = new weka.filters.supervised.attribute.AttributeSelection()
            filter.setOptions( weka.core.Utils.splitOptions( comando_filtro ) )
            filter.setInputFormat( weka_data ) |> ignore
            let weka_data = weka.filters.supervised.attribute.AttributeSelection.useFilter( weka_data, filter )
            let attributes = getAttributes [] (weka_data.enumerateAttributes())
                                |> (fun attributes -> List.append attributes [target])
                                |> List.map (fun att -> att.name())
            attributes |> List.map (fun attribute -> match attribute.Split [|'_'|] |> Array.toList with
                                                        | materia :: i :: _ -> materia + "_" + i
                                                        | _ -> failwith ("Error on: " + attribute))
                       |> List.fold (fun (s, ruta) materia -> if Set.contains materia s
                                                              then (s, ruta)
                                                              else (Set.add materia s, materia :: ruta)) (Set.empty, [])
                       |> (List.rev << snd)
                       |> (fun ruta' -> //printfn "%A ==> %A" ruta ruta'
                                        (ruta', attributes))
                       |> Some
       | None -> None

let writeFile filename (str : string) =
    use streamWriter = new System.IO.StreamWriter(filename, false)
    streamWriter.Write str

let serializar obj =
    // serialize 
    let bos = new java.io.ByteArrayOutputStream()
    let out = new java.io.ObjectOutputStream(bos)
    out.writeObject( obj )
    out.flush()
    let bytes = bos.toByteArray()
    bos.close()
    bytes

let deserializar<'T> bytes =
    // deserialize
    let bis = new java.io.ByteArrayInputStream( bytes )
    let inn = new java.io.ObjectInputStream( bis )
    let obj = downcast inn.readObject() : 'T
    inn.close()
    obj

let to_weka clase comando_filtro comando_construccion codigo periodoInicial periodoFinal (ruta, atributos, data) =
//    printfn "Procesando ruta: %A" ruta
//    printfn "Alumnos: %A" (List.length data)
    let (attrs, m) = List.fold2 (cabeceraNumerica data) (java.util.ArrayList(), Map.empty) ruta [0 .. List.length ruta - 1]
    let instancias = List.mapi (instanciaNumerica data (attrs, m)) ruta
    let valores = [0 .. (List.length << List.head) instancias - 1]
                    |> List.map (fun i -> instancias |> List.map (List.item i)
                                                     |> Array.concat)
    let weka_data = weka.core.Instances(codigo + "_relation", attrs, 0)
    let target = getAttributes [] (weka_data.enumerateAttributes())
                    |> List.rev
                    |> List.tryFind (fun att -> let name = att.name()
                                                name.StartsWith codigo && name.EndsWith "_estatus")
    match target with
        | Some target -> 
            weka_data.setClass( target )
            valores |> List.iter (fun values -> weka_data.add( weka.core.DenseInstance(1.0, values) ) |> ignore)

            // Filtering
            let filter_options =
                getAttributes [] (weka_data.enumerateAttributes())
                    |> List.mapi (fun i attr -> let attr_name = attr.name()
                                                if List.exists (fun name -> name = attr_name) atributos
                                                then None
                                                else Some (i+1))
                    |> List.choose (fun x -> x)
                    |> List.map (fun i -> string i)
                    |> String.concat ","
                    |> (fun str -> "-R " + str)
            let remove = weka.filters.unsupervised.attribute.Remove()
            remove.setOptions( weka.core.Utils.splitOptions( filter_options ) )
            remove.setInputFormat( weka_data ) |> ignore
            let weka_data = weka.filters.Filter.useFilter( weka_data, remove )

(*            if List.length ruta = 2
            then writeFile (codigo + ".arff") (weka_data.toString())
            else ()*)

            // Build Model
            let clase' = java.lang.Class.forName( clase )
            let ctor = clase'.getConstructor()
            let classifier = downcast ctor.newInstance( [||] ) : weka.classifiers.AbstractClassifier

            classifier.setOptions(weka.core.Utils.splitOptions( comando_construccion ))
            classifier.buildClassifier( weka_data )

            // Cross validation (10 folds)
            let evaluation = new weka.classifiers.Evaluation( weka_data )
            evaluation.crossValidateModel(classifier, weka_data, 10, new java.util.Random(int64 1))

            // serialize model
            let bytes = serializar classifier

            // serialize data
            let data_bytes = serializar weka_data

            {numInstancias            = evaluation.numInstances()
             correctas                = evaluation.correct()
             matrizConfusion          = evaluation.confusionMatrix()
             modelo                   = bytes
             instancias               = data_bytes
             rutaMaterias             = ruta
             atributos                = atributos
             materia                  = codigo
             periodoInicial           = periodoInicial
             periodoFinal             = periodoFinal
             clase                    = clase} |> Some
        | None -> None

let modelo percent clase comando_filtro comando_construccion periodoInicial periodoFinal codigo =
    printfn "Construyendo modelo usando: %s" clase
    let datos = codigo |> obtener_datos periodoInicial periodoFinal
                       |> Seq.toList
    if List.isEmpty datos
    then None
    else 
         let por_alumnos = 
             datos |> List.groupBy (fun registro -> registro.Matricula)
                   |> List.map (fun (matricula, registros) -> (matricula, registros |> List.sortBy (fun r -> (r.Materia, r.Periodo))))
//         printfn "Alumnos: %i" (List.length por_alumnos)
         let matriculas = List.map fst por_alumnos
         let minAlumnos = int (double (List.length matriculas) * percent)
         let por_matricula_mapa = 
                List.fold (fun m (matricula, registros) -> registros |> List.map (fun k -> k.Materia)
                                                                     |> agrega_indice
                                                                     |> List.map2 (fun registro key -> (key, registro)) registros
                                                                     |> List.fold (fun m (key, registro) -> Map.add key registro m) Map.empty
                                                                     |> (fun m' -> Map.add matricula m' m)) Map.empty por_alumnos
         let rec actualiza_rutas mapa_datos mapa_rutas rutas =
             let (mapa_datos, rutas_datos) =
                rutas |> List.fold (fun (mapa, rutas) ruta -> 
                            match Map.tryFind ruta mapa with
                                | Some (Some (ruta, s)) -> (mapa, Some (ruta, s) :: rutas)
                                | Some None -> (mapa, rutas)
                                | None -> matriculas |> List.choose (verifica_ruta ruta por_matricula_mapa)
                                                     |> (fun s -> if List.length s >= minAlumnos
                                                                  then (Map.add ruta (Some (ruta, s)) mapa, Some (ruta, s) :: rutas)
                                                                  else (Map.add ruta None mapa, rutas))) (mapa_datos, [])
                      |> (fun (mapa, rutas) -> (mapa, rutas |> List.choose (fun x -> x)
                                                            |> List.sortBy (fun (ruta, _) -> ruta)))
//             printfn "Datos calculados"
             let (mapa_rutas, rutas') =
                 rutas_datos |> List.fold (fun (mapa, rutas) (ruta, data) -> 
                                    match Map.tryFind ruta mapa with
                                        | Some (Some ruta) -> (mapa, Some ruta :: rutas)
                                        | Some None -> (mapa, rutas)
                                        | None -> nueva_ruta comando_filtro codigo (ruta, data)
                                                    |> (fun nruta -> (Map.add ruta nruta mapa, nruta :: rutas))) (mapa_rutas, [])
                             |> (fun (mapa, rutas) -> (mapa, rutas |> List.choose (fun x -> x)
                                                                   |> (Set.toList << set)
                                                                   |> List.sort))
             let rutas_ = List.map fst rutas'
             if rutas = rutas_
             then rutas'
             else actualiza_rutas mapa_datos mapa_rutas rutas_

//         printfn "Mapa calculado"
         let rutas = 
             por_alumnos
                |> List.groupBy (fun (matricula, registros) -> List.map (fun r -> r.Materia) registros)
                |> List.map fst
                |> List.map agrega_indice
                |> actualiza_rutas Map.empty Map.empty

         let modelos = 
             rutas |> List.map (fun (ruta, atributos) -> matriculas |> List.choose (verifica_ruta ruta por_matricula_mapa)
                                                                    |> (fun s -> (ruta, atributos, s)))
                   |> List.choose (fun (ruta, atributos, s) -> 
                            if List.length s > 20
                            then Some (ruta, atributos, s)
                            else None)
                   |> List.choose (fun (ruta, atributos, data) -> 
                            to_weka clase comando_filtro comando_construccion codigo periodoInicial periodoFinal (ruta, atributos, data))
         if List.isEmpty modelos
         then None
         else modelos |> List.maxBy (fun modelo -> modelo.correctas / modelo.numInstancias)
                      |> Some

let rec actualiza_modelo_nominal materia periodoInicial periodoFinal parcial clase continuo rutaMaterias atributos matrizConfusion numeroInstancias correctas modelo instancias =
    let result = query { for registro in ctx.Intranet.ModelosNominales do
                         where (registro.Materia = materia && registro.PeriodoInicial = periodoInicial &&
                                registro.PeriodoFinal = periodoFinal && registro.Parcial = parcial)
                         select registro}
                            |> Seq.toList
    match result with
        [registro] -> registro.Delete()
                      ctx.SubmitUpdates()
                      actualiza_modelo_nominal materia periodoInicial periodoFinal parcial clase continuo rutaMaterias atributos matrizConfusion numeroInstancias correctas modelo instancias
       | _ -> printfn "Guardando modelo construido por: %s" clase
              let registro = ctx.Intranet.ModelosNominales.Create()
              registro.Materia <- materia
              registro.PeriodoInicial <- periodoInicial
              registro.PeriodoFinal <- periodoFinal
              registro.Parcial <- parcial
              registro.Clase <- clase
              registro.ContinuoDiscreto <- continuo
              registro.RutaMaterias <- rutaMaterias
              registro.Atributos <- atributos
              registro.MatrizConfusion <- matrizConfusion
              registro.NumeroInstancias <- numeroInstancias
              registro.Correctas <- correctas
              registro.Modelo <- modelo
              registro.Instancias <- instancias
              ctx.SubmitUpdates()