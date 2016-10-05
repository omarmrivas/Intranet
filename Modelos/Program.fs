// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.


[<EntryPoint>]
let main argv =
    printfn "%A" argv
    printfn "Calculo pesado empezando..."
    let modelo = BaseDatos.modelo "20141S" "20153S" "510F"
    printfn "%A" modelo
    0 // return an integer exit code

