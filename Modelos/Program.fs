// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

let periodos () = 
        "20013S" ::
            [for ano in 2002 .. System.DateTime.Now.Year do
                for s in 1..3 do
                    yield string ano + string s + "S"]

let carreras = ["ITI"; "ITEM"; "ISTI"; "ITMA"; "LAG"; "LMKT"]

let materias_por_carrera carrera =
    query {for A in BaseDatos.Sql.GetDataContext().Intranet.Planes do
           where (A.Carrera = carrera)
           select A.Clave}
           |> Seq.toList

let verifica_modo modo =
    match modo with
        | "entrenamiento" -> false
        | "prediccion" -> false
        | _ -> printfn "El primer argumento es 'entrenamiento' o 'prediccion'"
               true

let verifica_periodo periodo =
    let vperiodo () = 
        if not (List.exists (fun s -> s = periodo) (periodos ()))
        then printfn "El valor de periodo debe ser uno de los siguientes valores: %A" periodos
             true
        else false
    List.exists (fun f -> f ()) [vperiodo]

let verifica_parcial parcial =
    try let p = int parcial
        not (p >= 0 && p <= 3)
    with | :? System.FormatException -> 
                printfn "Las entradas válidas para parcial son: 0, 1, 2 o 3"
                true

let verifica_carrera carrera =
    let vcarrera () = 
        if not (List.exists (fun s -> s = carrera) ("*" :: carreras))
        then printfn "El valor de carrera debe ser uno de los siguientes valores: %A" ("*" :: carreras)
             true
        else false
    List.exists (fun f -> f ()) [vcarrera]


let ejecutaEntrenamiento periodoInicial periodoFinal parcial carreras =
    let porcentajeAlumnos = 0.8
    let materias = List.map (fun carrera -> (carrera, materias_por_carrera carrera)) carreras
    materias
        |> List.iter (fun (carrera, materias) ->
                 printfn "Calculando modelos para la Carrera %s" carrera

                 materias |> List.iter (fun materia ->
                     printfn "Calculando modelo para la materia %s de la carrera de %s" materia carrera
                     BaseDatos.modelo porcentajeAlumnos
                                      periodoInicial periodoFinal parcial materia
                                |> (fun modelo -> 
                                            match modelo with
                                                | Some k -> // serializar matriz
                                                            let matriz = BaseDatos.serializar k.matrizConfusion
                                                            let continuo = sbyte 0
                                                            let tra ss = String.concat "," ss
                                                            BaseDatos.actualiza_modelo_nominal k.materia k.periodoInicial k.periodoFinal parcial (uint32 k.aId) k.clase continuo 
                                                                                               (tra k.rutaMaterias) (tra k.atributos) matriz (float32 k.precision) (uint32 k.numInstancias)
                                                                                               (uint32 k.correctas) k.modelo k.instancias
                                                | None -> printfn "No se encontró modelo para: %s" materia)))

let ejecutaPrediccion periodoInicial periodoFinal periodoPrediccion parcial carreras =
    let materias = List.map (fun carrera -> (carrera, materias_por_carrera carrera)) carreras
    materias |> List.iter (fun (carrera, materias) ->
                    printfn "Calculando predicciones con modelos para la Carrera %s" carrera
                    materias |> List.iter (fun materia ->
                        printfn "Calculando predicciones para la materia %s de la carrera de %s" materia carrera
                        match BaseDatos.prediccion periodoInicial periodoFinal periodoPrediccion parcial materia with
                            | Some (mId, L) -> List.iter (fun (matricula, estatus) -> 
                                                                BaseDatos.actualiza_prediccion_kardex mId matricula periodoPrediccion estatus) L
                            | None -> ()))

[<EntryPoint>]
let main argv =
(*    ignore (BaseDatos.prediccion "20131S" "20162S" "20163S" (uint32 0) "340O1")
    0*)
    if Array.length argv < 5
    then printfn "Entrada incompleta: %A" argv
         printfn "introducir (entrenamiento, prediccion), periodoInicial, periodoFinal, parcial y carrera"
         1
    else if not(verifica_modo argv.[0] ||
                verifica_periodo argv.[1] ||
                verifica_periodo argv.[2] ||
                verifica_parcial argv.[3] ||
                verifica_carrera argv.[4])
    then let modo = argv.[0]
         let periodoInicial = argv.[1]
         let periodoFinal = argv.[2]
         let periodoPrediccion = let ps = periodos ()
                                 ps |> List.item (List.findIndex (fun periodo -> periodo = periodoFinal) ps + 1)
         let parcial = uint32 argv.[3]
         let carreras = if argv.[4] = "*"
                        then carreras
                        else [argv.[4]]
         match modo with
            | "entrenamiento" -> ejecutaEntrenamiento periodoInicial periodoFinal parcial carreras
                                 0
            | _ -> ejecutaPrediccion periodoInicial periodoFinal periodoPrediccion parcial carreras
                   0
    else printfn "Algun error en la entrada: %A" argv
         1
