namespace Intranet

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI.Next
open WebSharper.UI.Next.Client
open WebSharper.UI.Next.Html
//open WebSharper.UI.Next.Formlets

[<JavaScript>]
module Client =
    open WebSharper.Forms
    module B = WebSharper.Forms.Bootstrap.Controls

    let periodos () = 
            "20013S" ::
                [for ano in 2002 .. System.DateTime.Now.Year do
                    for s in 1..3 do
                        yield string ano + string s + "S"]
    let vperiodo period = 
            if not (List.exists (fun s -> s = period) ("*" :: periodos ()))
            then printfn "El valor de periodo debe ser uno de los siguientes valores: %A" periodos
                 true
            else false


    let logOut username =
        div [
            p [text "Oprimir para salir:"]
            buttonAttr [
                on.click (fun _ _ ->
                    async {
                        do! Server.LogoutUser username
                        return JS.Window.Location.Reload()
                    } |> Async.Start
                )
            ] [text "Salir"]
        ]

    let logOutAction username =
        div [
            p [text "Oprimir para salir:"]
            buttonAttr [
                on.afterRender (fun _ ->
                    async {
                        do! Server.LogoutUser username
                        return JS.Window.Location.Assign "/"
                    } |> Async.Start
                )
            ] [text "Salir"]
        ]

    let adminComponents username =
        div [
            p [text "Actualizar Base de Datos:"]
            buttonAttr [
                on.click (fun _ _ ->
                    async {
                        return JS.Window.Location.Assign "about"
                    } |> Async.Start
                )
            ] [text "Actualizar"]
        ]


    let getCareers (careers : Server.Career) =
            let result = if careers.ITI then ["ITI"] else []
            let result = if careers.ITEM then "ITEM" :: result else result
            let result = if careers.ISTI then "ISTI" :: result else result
            let result = if careers.ITMA then "ITMA" :: result else result
            let result = if careers.LAG then "LAG" :: result else result
            let result = if careers.LMKT then "LMKT" :: result else result
            result


    let UpdatePrograms () =
        let careers : Server.Career = {ITI = false; ITEM = false; ISTI = false; ITMA = false; LAG = false; LMKT = false}
        Form.Return (fun iti item isti itma lag lmkt -> {ITI = iti; ITEM = item; ISTI = isti; ITMA = itma; LAG = lag; LMKT = lmkt} : Server.Career)
        <*> Form.Yield careers.ITI
        <*> Form.Yield careers.ITEM
        <*> Form.Yield careers.ISTI
        <*> Form.Yield careers.ITMA
        <*> Form.Yield careers.LAG
        <*> Form.Yield careers.LMKT
        |> Form.WithSubmit
        |> Form.Run (fun career ->
            async {
                return! Server.UpdatePrograms (getCareers career)
            } |> Async.Start
        )
        |> Form.Render (fun iti item isti itma lag lmkt submit ->
            form [
                h2 [text "Actualizar Programas de Carreras"]
                div [B.Checkbox "ITI"  [] (iti, [], [])
                     B.Checkbox "ITEM" [] (item, [], [])
                     B.Checkbox "ISTI" [] (isti, [], [])]
                div [B.Checkbox "ITMA" [] (itma, [], [])
                     B.Checkbox "LAG"  [] (lag, [], [])
                     B.Checkbox "LMKT" [] (lmkt, [], [])]
                B.Button "Actualizar Programa(s)" [attr.``class`` "btn btn-primary"] submit.Trigger
            ]
        )

    let UpdateGroups () =
        Form.Return (fun period -> period : string)
        <*> (Form.Yield ""
            |> Validation.IsNotEmpty "Necesitas introducir una periodo no nulo, por ejmplo, 20051S o *"
            |> Validation.Is (not << vperiodo) "Necesitas introducir una periodo válido, por ejmplo, 20051S o *")
        |> Form.WithSubmit
        |> Form.Run (fun period ->
            async {
                let periods = if period = "*"
                              then periodos ()
                              else [period]
                return! Server.UpdateGroups periods
            } |> Async.Start
        )
        |> Form.Render (fun period submit ->
            form [
                h2 [text "Actualizar Grupos"]
                B.Simple.InputWithError "Periodo" period submit.View
                B.Button "Actualizar Grupos" [attr.``class`` "btn btn-primary"] submit.Trigger
                B.ShowErrors [attr.style "margin-top:1em"] submit.View
            ]
        )

    let UpdateProfessors () =
        let careers : Server.Career = {ITI = false; ITEM = false; ISTI = false; ITMA = false; LAG = false; LMKT = false}
        Form.Return (fun iti item isti itma lag lmkt period -> (({ITI = iti; ITEM = item; ISTI = isti; ITMA = itma; LAG = lag; LMKT = lmkt} : Server.Career), period))
        <*> Form.Yield careers.ITI
        <*> Form.Yield careers.ITEM
        <*> Form.Yield careers.ISTI
        <*> Form.Yield careers.ITMA
        <*> Form.Yield careers.LAG
        <*> Form.Yield careers.LMKT
        <*> (Form.Yield ""
            |> Validation.IsNotEmpty "Necesitas introducir una periodo no nulo, por ejmplo, 20051S o *"
            |> Validation.Is (not << vperiodo) "Necesitas introducir una periodo válido, por ejmplo, 20051S o *")
        |> Form.WithSubmit
        |> Form.Run (fun (career, period) ->
            async {
                let periods = if period = "*"
                              then periodos ()
                              else [period]
                return! Server.UpdateProfessors (getCareers career) periods
            } |> Async.Start
        )
        |> Form.Render (fun iti item isti itma lag lmkt period submit ->
            form [
                h2 [text "Actualizar Profesores"]
                div [B.Checkbox "ITI"  [] (iti, [], [])
                     B.Checkbox "ITEM" [] (item, [], [])
                     B.Checkbox "ISTI" [] (isti, [], [])]
                div [B.Checkbox "ITMA" [] (itma, [], [])
                     B.Checkbox "LAG"  [] (lag, [], [])
                     B.Checkbox "LMKT" [] (lmkt, [], [])]
                B.Simple.InputWithError "Periodo" period submit.View
                B.Button "Actualizar Profesores" [attr.``class`` "btn btn-primary"] submit.Trigger
            ]
        )

    let UpdateKardex () =
        let careers : Server.Career = {ITI = false; ITEM = false; ISTI = false; ITMA = false; LAG = false; LMKT = false}
        Form.Return (fun iti item isti itma lag lmkt period -> (({ITI = iti; ITEM = item; ISTI = isti; ITMA = itma; LAG = lag; LMKT = lmkt} : Server.Career), period))
        <*> Form.Yield careers.ITI
        <*> Form.Yield careers.ITEM
        <*> Form.Yield careers.ISTI
        <*> Form.Yield careers.ITMA
        <*> Form.Yield careers.LAG
        <*> Form.Yield careers.LMKT
        <*> (Form.Yield ""
            |> Validation.IsNotEmpty "Necesitas introducir una periodo no nulo, por ejmplo, 20051S o *"
            |> Validation.Is (not << vperiodo) "Necesitas introducir una periodo válido, por ejmplo, 20051S o *")
        |> Form.WithSubmit
        |> Form.Run (fun (career, period) ->
            async {
                let periods = if period = "*"
                              then periodos ()
                              else [period]
                return! Server.UpdateKardex (getCareers career) periods
            } |> Async.Start
        )
        |> Form.Render (fun iti item isti itma lag lmkt period submit ->
            form [
                h2 [text "Actualizar Kardex"]
                div [B.Checkbox "ITI"  [] (iti, [], [])
                     B.Checkbox "ITEM" [] (item, [], [])
                     B.Checkbox "ISTI" [] (isti, [], [])]
                div [B.Checkbox "ITMA" [] (itma, [], [])
                     B.Checkbox "LAG"  [] (lag, [], [])
                     B.Checkbox "LMKT" [] (lmkt, [], [])]
                B.Simple.InputWithError "Periodo" period submit.View
                B.Button "Actualizar Kardex" [attr.``class`` "btn btn-primary"] submit.Trigger
            ]
        )

    let LoggedInUser username =
        div [
            p [text "Click here to log out:"]
            buttonAttr [
                on.click (fun _ _ ->
                    async {
                        do! Server.LogoutUser username
                        return JS.Window.Location.Reload()
                    } |> Async.Start
                )
            ] [text "Logout"]
        ]

(*    let LoginForm (redirectUrl: string) : Formlet<unit> =
        let uName =
            Controls.Input ""
            |> Validator.IsNotEmpty "Enter Username"
            |> Enhance.WithValidationIcon
            |> Enhance.WithTextLabel "Username"
        let pw =
            Controls.Password ""
            |> Validator.IsNotEmpty "Enter Password"
            |> Enhance.WithValidationIcon
            |> Enhance.WithTextLabel "Password"
        let loginF =
            Formlet.Yield (fun n pw -> {Name=n; Password=pw})
            <*> uName <*> pw
 
        Formlet.Do {
            let! uInfo = 
                loginF
                |> Enhance.WithCustomSubmitAndResetButtons
                    {Enhance.FormButtonConfiguration.Default with Label = Some "Login"}
                    {Enhance.FormButtonConfiguration.Default with Label = Some "Reset"}
            return!
                WithLoadingPane (Login uInfo) <| fun loggedIn ->
                    if loggedIn then
                        Redirect redirectUrl
                        C.Formlet.Return ()
                    else
                        WarningPanel "Login failed"
        }
        |> Enhance.WithFormContainer*)

    let AnonymousUser () =
        Form.Return (fun user pass -> ({User = user; Password = pass} : Server.UserPassword))
        <*> (Form.Yield ""
            |> Validation.IsNotEmpty "Must enter a username")
        <*> (Form.Yield ""
            |> Validation.IsNotEmpty "Must enter a password")
        |> Form.WithSubmit
        |> Form.Run (fun userpass ->
            async {
                do! Server.LoginUser userpass
                return JS.Window.Location.Reload()
            } |> Async.Start
        )
        |> Form.Render (fun user pass submit ->
            form [
                B.Simple.InputWithError "Usuario (intranet)" user submit.View
                B.Simple.InputPasswordWithError "Contraseña (intranet)" pass submit.View
                B.Button "Iniciar" [attr.``class`` "btn btn-primary"] submit.Trigger
                B.ShowErrors [attr.style "margin-top:1em"] submit.View
            ]
        )

    let Main () =
        let rvInput = Var.Create ""
        let submit = Submitter.CreateOption rvInput.View
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> Server.DoSomething input
            )
        div [
            Doc.Input [] rvInput
            Doc.Button "Send" [] submit.Trigger
            hr []
            h4Attr [attr.``class`` "text-muted"] [text "The server responded:"]
            divAttr [attr.``class`` "jumbotron"] [h1 [textView vReversed]]
        ]


[<JavaScript>]
module PredictionProfessor =

    // First, we declare types for predictions and how to order them.

    type PrediccionProfesor = {
        Materia   : string
        Grupo     : string
        Matricula : string
        Nombre    : string
        Estatus   : string
        Precision : string
        NumeroInstancias : string
        Atributos : string
        Descripcion : string
        DescripcionSeleccion : string
    }

    type Order = Alfabetico | Matricula | Estatus

    type Order with

        /// A textual representation of our orderings.
        static member Show order =
            match order with
            | Alfabetico -> "Apellido"
            | Matricula -> "Matrícula"
            | Estatus -> "Estatus"

    type PrediccionProfesor with

        /// A comparison function, based on whether we're sorting by name or matriculation number.
        static member Compare order p1 p2 =
            match order with
            | Alfabetico -> compare p1.Nombre p2.Nombre
            | Matricula -> compare p1.Matricula p2.Matricula
            | Estatus -> compare p1.Estatus p2.Estatus

        /// A filtering function.
        static member MatchesQuery q ph =
            ph.Nombre.Contains(q)
            || ph.Matricula.Contains(q)

        /// A filtering function.
        static member MatchesMateria q ph =
            q = ph.Materia + " (" + ph.Grupo + ")"


    let sty n v = Attr.Style n v
    let cls n = Attr.Class n
    let divc c docs = Doc.Element "div" [cls c] docs


    // This is our prediction widget. We take a list of predictions, and return
    // an document tree which can be rendered.
    let widget (predicciones : PrediccionProfesor list) (planes : Map<string, string>) =
        let materias = List.fold (fun l ph -> let materia = ph.Materia + " (" + ph.Grupo + ")"
                                              if List.exists (fun m -> m = materia) l
                                              then l
                                              else materia :: l) [] predicciones
                                              |> List.rev

        // Búsqueda
        let vquery = Var.Create ""

        // Ordenamiento
        let vorder = Var.Create Alfabetico

        // Por materia
        let vmaterias = match materias with
                            | _ :: _ -> Var.Create (List.head materias)
                            | [] -> Var.Create ""

        // The above vars are our model. Everything else is computed from them.
        // Now, compute visible predictions under the current selection:

        let prediccionesVisibles =
            (View.FromVar vorder)
                |> View.Map2 (fun query order -> (query, order)) (View.FromVar vquery)
                |> View.Map2 (fun materia (query, order) ->
                    predicciones 
                           |> List.filter (PrediccionProfesor.MatchesMateria materia)
                           |> List.filter (PrediccionProfesor.MatchesQuery query)
                           |> List.sortWith (PrediccionProfesor.Compare order)) (View.FromVar vmaterias)

        // A simple function for displaying the details of a prediction:
        let muestraPrediccion i ph =
            trAttr [attr.``class`` ("d" + string (i % 2))] [
//                td [text ph.Materia]
                td [text (string (i + 1))]
                td [text ph.Matricula]
                td [text ph.Nombre]
                td [text ph.Estatus]
//                td [text ph.Precision]
            ] :> Doc

        let putInPanel name comp = 
            divc "panel panel-default" [
                divc "panel-heading" [
                    h3Attr [cls "panel-title"] [
                        text name
                    ]
                ]

                divc "panel-body" [
                    comp
                ]
            ]

        let muestraPredicciones predicciones =
                match predicciones with
                    | _ :: _ -> let info = List.head predicciones
                                let descripcion (txt : string) =
                                    txt.Split [|'\n'|]
                                        |> Array.toList
                                        |> List.filter (fun txt -> txt.Trim() <> "")
                                        |> (fun l -> List.iteri (fun i txt -> printfn "%i, %s" i txt) l
                                                     l)
                                        |> List.map text
                                        |> (fun l -> let pairs = match List.length l with
                                                                    | 0 -> [text "", text ""]
                                                                    | 1 -> [List.head l, text ""]
                                                                    | _ -> List.pairwise l
                                                     let all = pairs |> List.map (fun (_, t) -> [br[] :> Doc; t])
                                                                     |> List.concat
                                                     ((fst << List.head) pairs) :: all)
                                let attrName attr =
                                    match attr with
                                        | "profesor" -> "profesor"
                                        | "c1" -> "calificación del parcial 1"
                                        | "i1" -> "inasistencias del parcial 1"
                                        | "c2" -> "calificación del parcial 2"
                                        | "i2" -> "inasistencias del parcial 2"
                                        | "c3" -> "calificación del parcial 3"
                                        | "i3" -> "inasistencias del parcial 3"
                                        | "efinal" -> "calificación del exámen final"
                                        | "final" -> "calificación final"
                                        | "inasistencias" -> "inasistencias"
                                        | "estatus" -> "estatus"
                                        | _ -> "desconocido"
                                let atributos (txt : string) =
                                    let extraer (atributo : string) = 
                                        match atributo.Split [|'_'|] |> Array.toList with
                                            | (codigo :: num :: atributo :: _) -> 
                                                       match Map.tryFind codigo planes with
                                                         | Some materia -> (codigo, num, materia, attrName atributo)
                                                         | None -> ("", "", "", "")
                                            | _ -> ("", "", "", "")
                                    txt.Split [|','|] 
                                        |> Array.toList
                                        |> List.map extraer
                                        |> List.fold (fun m (codigo, num, materia, atributo) ->
                                                        let key = materia + " (" + codigo + "-" + num + ")"
                                                        match Map.tryFind key m with
                                                            | Some l -> Map.add key (l @ [atributo]) m
                                                            | None -> Map.add key [atributo] m) Map.empty
                                        |> Map.toList
                                        |> List.map (fun (materia, l) -> l |> String.concat ", "
                                                                           |> (fun atributos -> materia + " (" + atributos + ")"))
                                        |> String.concat ", "

                                let modelInfo = ul [
                                                  li [text ("Se consideraron " + string info.NumeroInstancias + " alumnos en la construcción del modelo predictivo.")]
                                                  li [text ("La precisión del modelo fue de " + info.Precision + "% instancias clasificadas correctamente usando validación cruzada.")]
                                                  li [text "El modelo predictivo uso la siguiente información por materia:"
                                                      br []
                                                      text (atributos info.Atributos)]
                                                  li [text "La información del algoritmo de clasificación es la siguiente:"
                                                      br []
                                                      p (descripcion info.Descripcion)]
                                                  li [text "La información del algoritmo de selección de atributos es la siguiente:"
                                                      br []
                                                      p (descripcion info.DescripcionSeleccion)]
                                                  ]
                                (predicciones 
                                    |> List.mapi muestraPrediccion
                                    |> table
                                    |> (fun t -> div [t
                                                      br []
                                                      p [putInPanel "Información de Modelo Predictivo" modelInfo]])) :> Doc
                    | [] -> (div []) :> Doc
                             

        let queryPanel = 
            divc "col-sm-6" [
                text "Materia: "
                Doc.Select [Attr.Create "class" "form-control"] id materias vmaterias

                // We then have a select box, linked to our orders variable
                text "Ordenar por: "
                Doc.Select [Attr.Create "class" "form-control"] Order.Show [Alfabetico; Matricula; Estatus] vorder
                // We specify a label, and an input box linked to our query RVar.
                text "Buscar: "
                Doc.Input [Attr.Create "class" "form-control"] vquery
            ] |> putInPanel "Consulta"

        let resultsPanel =
            tableAttr [cls "table"] [
                        tbody [
                            // We map the tableRow function onto the different
                            // views of the source, and concatenate the resulting
                            // documents.
                            ul [ Doc.EmbedView (View.Map muestraPredicciones prediccionesVisibles) ]
                        ]
                    ]
                    |> putInPanel "Resultados"
        div [
            queryPanel
            resultsPanel
        ]

    let Main predicciones planes =
        // Funcion que extrae las predicciones de una lista de strings
        let prediccion P = 
            match P with 
                | [materia; grupo; matricula; nombre; estatus; precision; instancias; atributos; descripcion; descripcion_seleccion] -> 
                    {Materia   = materia
                     Grupo     = grupo
                     Matricula = matricula
                     Nombre    = nombre
                     Estatus   = estatus
                     Precision = precision
                     NumeroInstancias = instancias
                     Atributos = atributos
                     Descripcion = descripcion
                     DescripcionSeleccion = descripcion_seleccion
                     }
                | _ -> printfn "Error"
                       failwith "Error"

        let plan (P : string list) = 
            match P with
                | [codigo; materia] -> (codigo, materia)
                | _ -> printfn "Error"
                       failwith "Error"

        let predicciones = List.map prediccion predicciones
        let planes = planes
                        |> List.map plan
                        |> List.fold (fun m (codigo, materia) -> Map.add codigo materia m) Map.empty
        widget predicciones planes
