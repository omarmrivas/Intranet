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

let cabeceraNumerica data (atts : weka.core.FastVector, mapa) materia i =
    // Clave Profesor
    let profesoresVals = weka.core.FastVector()
    data |> List.choose (fun (_, ks) -> obtener_clave_profesor ((List.item i ks).Grupo))
         |> set
         |> Set.toList
         |> List.map string
         |> List.iter (fun clave -> profesoresVals.addElement(clave))
    atts.addElement( weka.core.Attribute(materia + "_profesor", profesoresVals) )
    // C1
    atts.addElement( weka.core.Attribute(materia + "_c1") )
    // I1
    atts.addElement( weka.core.Attribute(materia + "_i1") )
    // C2
    atts.addElement( weka.core.Attribute(materia + "_c2") )
    // I2
    atts.addElement( weka.core.Attribute(materia + "_i2") )
    // C3
    atts.addElement( weka.core.Attribute(materia + "_c3") )
    // I3
    atts.addElement( weka.core.Attribute(materia + "_i3") )
    // Efinal
    atts.addElement( weka.core.Attribute(materia + "_efinal") )
    // Final
    atts.addElement( weka.core.Attribute(materia + "_final") )
    // Inasistencias
    atts.addElement( weka.core.Attribute(materia + "_inasistencias") )
    // Estatus
    let estatusVals = weka.core.FastVector()
    estatusVals.addElement("Aprobado")
    estatusVals.addElement("Reprobado")
    atts.addElement( weka.core.Attribute(materia + "_estatus", estatusVals) )
    // Mapa
    let mapa = Map.add (materia + "_profesor") profesoresVals mapa
    (atts, Map.add (materia + "_estatus") estatusVals mapa)

let instanciaNumerica data (atts : weka.core.FastVector, mapa : Map<string, weka.core.FastVector>) i materia =
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
    let toEstatus v =
        match v with
            | "Aprobado" -> "Aprobado"
            | _ -> "Reprobado"
    data |> List.map (fun (_, instancia) -> List.item i instancia)
         |> List.map (fun k -> let profesoresVals = Map.find (materia + "_profesor") mapa
                               let estatusVals = Map.find (materia + "_estatus") mapa
                               [| (float)(profesoresVals.indexOf( (string << Option.get) (obtener_clave_profesor k.Grupo)))
                                  toDouble k.C1
                                  double k.I1
                                  toDouble k.C2
                                  double k.I2
                                  toDouble k.C3
                                  double k.I3
                                  toDouble k.Efinal
                                  toDouble k.Final
                                  double k.Inasistencias
                                  (float)(estatusVals.indexOf( toEstatus k.Estatus )) |])

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
           |> List.map (fun obj -> let str = obj.ToString()
                                   match str.Split [|' '|] |> Array.toList with
                                      | _ :: name :: _ -> name
                                      | _ -> failwith ("Error on attribute: " + str))

let nueva_ruta codigo (ruta, data) =
    printfn "Procesando ruta: %A" ruta
    printfn "Alumnos: %A" (List.length data)
    let (attrs, m) = List.fold2 (cabeceraNumerica data) (weka.core.FastVector(), Map.empty) ruta [0 .. List.length ruta - 1]
    let instancias = List.mapi (instanciaNumerica data (attrs, m)) ruta
    let valores = [0 .. (List.length << List.head) instancias - 1]
                    |> List.map (fun i -> instancias |> List.map (List.item i)
                                                     |> Array.concat)
    let weka_data = weka.core.Instances(codigo + "_relation", attrs, 0)
    weka_data.setClassIndex(weka_data.numAttributes() - 1)
    valores |> List.iter (fun values -> weka_data.add( weka.core.DenseInstance(1.0, values) ) |> ignore)

    // Filtering
    let filter = new weka.filters.supervised.attribute.AttributeSelection()
    filter.setOptions( weka.core.Utils.splitOptions("-E \"weka.attributeSelection.CfsSubsetEval -P 1 -E 1\" -S \"weka.attributeSelection.BestFirst -D 1 -N 5\"") )
    filter.setInputFormat( weka_data ) |> ignore
    let weka_data = weka.filters.supervised.attribute.AttributeSelection.useFilter( weka_data, filter )
    let attributes = 
            getAttributes [] (weka_data.enumerateAttributes())
    attributes |> List.map (fun attribute -> match attribute.Split [|'_'|] |> Array.toList with
                                                | materia :: i :: _ -> materia + "_" + i
                                                | _ -> failwith ("Error on: " + attribute))
               |> List.fold (fun (s, ruta) materia -> if Set.contains materia s
                                                      then (s, ruta)
                                                      else (Set.add materia s, materia :: ruta)) (Set.empty, [])
               |> (List.rev << snd)


let to_weka codigo (ruta, data) =
    printfn "Procesando ruta: %A" ruta
    printfn "Alumnos: %A" (List.length data)
    let (attrs, m) = List.fold2 (cabeceraNumerica data) (weka.core.FastVector(), Map.empty) ruta [0 .. List.length ruta - 1]
    let instancias = List.mapi (instanciaNumerica data (attrs, m)) ruta
    let valores = [0 .. (List.length << List.head) instancias - 1]
                    |> List.map (fun i -> instancias |> List.map (List.item i)
                                                     |> Array.concat)
    let weka_data = weka.core.Instances(codigo + "_relation", attrs, 0)
    weka_data.setClassIndex(weka_data.numAttributes() - 1)
    valores |> List.iter (fun values -> weka_data.add( weka.core.DenseInstance(1.0, values) ) |> ignore)


    // Filtering
    let filter = new weka.filters.supervised.attribute.AttributeSelection()
    filter.setOptions( weka.core.Utils.splitOptions("-E \"weka.attributeSelection.CfsSubsetEval -P 1 -E 1\" -S \"weka.attributeSelection.BestFirst -D 1 -N 5\"") )
    filter.setInputFormat( weka_data ) |> ignore
    let weka_data = weka.filters.supervised.attribute.AttributeSelection.useFilter( weka_data, filter )
(*    let attributes = 
            (weka_data.enumerateAttributes()) |> getEnumerators []
                                              |> List.map (fun obj -> obj.ToString())
    printfn "Attributes: %A" attributes*)

   // Build Model
    let j48 = new weka.classifiers.trees.J48()
    j48.setOptions(weka.core.Utils.splitOptions("-C 0.25 -M 2"))
    j48.buildClassifier( weka_data )


    // Cross validation (10 folds)
    let evaluation = new weka.classifiers.Evaluation( weka_data )
    evaluation.crossValidateModel(j48, weka_data, 10, new java.util.Random(int64 1))

    evaluation

let modelo periodoInicial periodoFinal codigo =
    let datos = codigo |> obtener_datos periodoInicial periodoFinal
                       |> Seq.toList
    if List.isEmpty datos
    then None
    else 
         let por_alumnos = 
             datos |> List.groupBy (fun registro -> registro.Matricula)
                   |> List.map (fun (matricula, registros) -> (matricula, registros |> List.sortBy (fun r -> (r.Materia, r.Periodo))))
         printfn "Alumnos: %i" (List.length por_alumnos)
         let matriculas = List.map fst por_alumnos
         let por_matricula_mapa = 
                List.fold (fun m (matricula, registros) -> registros |> List.map (fun k -> k.Materia)
                                                                     |> agrega_indice
                                                                     |> List.map2 (fun registro key -> (key, registro)) registros
                                                                     |> List.fold (fun m (key, registro) -> Map.add key registro m) Map.empty
                                                                     |> (fun m' -> Map.add matricula m' m)) Map.empty por_alumnos

         let rec actualiza_rutas rutas =
             let rutas_datos =
                rutas |> List.map (fun ruta -> matriculas |> List.choose (verifica_ruta ruta por_matricula_mapa)
                                                          |> (fun s -> (ruta, s)))
                      |> List.choose (fun (ruta, s) -> if List.length s > 20
                                                       then Some (ruta, s)
                                                       else None)
             printfn "Datos calculados"
             let rutas' =
                 rutas_datos |> List.map (fun (ruta, data) -> nueva_ruta codigo (ruta, data))
                             |> set
                             |> Set.toList
             if rutas = rutas'
             then rutas
             else actualiza_rutas rutas'

         printfn "Mapa calculado"
         let rutas = 
             por_alumnos
                |> List.groupBy (fun (matricula, registros) -> List.map (fun r -> r.Materia) registros)
                |> List.map fst
                |> List.map agrega_indice
                |> actualiza_rutas

         let modelos = 
             rutas |> List.map (fun ruta -> matriculas |> List.choose (verifica_ruta ruta por_matricula_mapa)
                                                       |> (fun s -> (ruta, s)))
                   |> List.choose (fun (ruta, s) -> if List.length s > 20
                                                    then Some (ruta, s)
                                                    else None)
                   |> List.map (fun (ruta, data) -> 
                                        let model = to_weka codigo (ruta, data)
                                        let error = model.meanAbsoluteError()
                                        printfn "Error: %A" error
                                        (ruta, List.length data, error, to_weka codigo (ruta, data)))

         modelos |> List.minBy (fun (_, _, error, _) -> error)
                 |> Some

