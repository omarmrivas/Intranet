// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

type AlgoritmoClasificador =
    {Clase               : string
     ComandoSeleccion    : string
     ComandoConstruccion : string
     ComandoPredecir     : string
     Descripcion         : string}

let periodos () = 
        "20013S" ::
            [for ano in 2002 .. System.DateTime.Now.Year do
                for s in 1..3 do
                    yield string ano + string s + "S"]

let carreras = ["ITI"; "ITEM"; "ISTI"; "ITMA"; "LAG"; "LMKT"]

let algoritmos_clasificadores () =
    query {for A in BaseDatos.ctx.Intranet.AlgoritmosClasificadores do
           select A}
           |> Seq.toList
           |> List.map (fun A -> A.MapTo<AlgoritmoClasificador>())

let materias_por_carrera carrera =
    query {for A in BaseDatos.ctx.Intranet.Planes do
           where (A.Carrera = carrera)
           select A.Clave}
           |> Seq.toList

let verifica_periodo periodo =
    let vperiodo () = 
        if not (List.exists (fun s -> s = periodo) (periodos ()))
        then printfn "El valor de periodo debe ser uno de los siguientes valores: %A" periodos
             true
        else false
    List.exists (fun f -> f ()) [vperiodo]

[<EntryPoint>]
let main argv =
    printfn "%A" argv

    (*let porcentajeAlumnos = 0.8

    if Array.length argv < 2
    then printfn "Entrada incompleta: %A" argv
         printfn "introducir periodoInicial y periodoFinal"
         1
    else let periodoInicial = argv.[0]
         let periodoFinal = argv.[1]
         let algoritmos = algoritmos_clasificadores ()
         let materias = List.map (fun carrera -> (carrera, materias_por_carrera carrera)) carreras
         let algoritmos = algoritmos_clasificadores ()
         materias
             |> List.iter (fun (carrera, materias) ->
                 printfn "Calculando modelos para la Carrera %s" carrera

                 materias |> List.iter (fun materia ->
                     printfn "Calculando modelo para la materia %s de la carrera de %s" materia carrera
                     algoritmos |> List.choose (fun algoritmo ->
                         BaseDatos.modelo porcentajeAlumnos
                                          algoritmo.Clase
                                          algoritmo.ComandoSeleccion
                                          algoritmo.ComandoConstruccion
                                          periodoInicial periodoFinal materia)
                                |> (fun modelos -> if List.isEmpty modelos
                                                   then None
                                                   else modelos |> List.maxBy (fun modelo -> modelo.correctas / modelo.numInstancias)
                                                                |> Some)
                                |> (fun modelo -> 
                                            match modelo with
                                                | Some k -> // serializar matriz
                                                            let matriz = BaseDatos.serializar k.matrizConfusion
                                                            let parcial = uint32 0
                                                            let continuo = sbyte 0
                                                            let tra ss = String.concat "," ss
                                                            BaseDatos.actualiza_modelo_nominal k.materia k.periodoInicial k.periodoFinal parcial k.clase continuo 
                                                                                               (tra k.rutaMaterias) (tra k.atributos) matriz (uint32 k.numInstancias)
                                                                                               (uint32 k.correctas) k.modelo k.instancias
                                                | None -> printfn "No se encontró modelo para: %s" materia)))
         0*)


    printfn "Calculo pesado empezando..."
    let modelo = BaseDatos.prediccion "20131S" "20161S" (uint32 0) "510F"
    printfn "%A" modelo

    0 // return an integer exit code

