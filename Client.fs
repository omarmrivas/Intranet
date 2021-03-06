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

    let FormItem foo (name, act, placeholder, btn) =
        let rvInput = Var.Create ""
        liAttr [if foo then yield attr.``class`` "active"]
               [formAttr [attr.``class`` "navbar-form navbar-left"
                          Attr.Create "role" "search"
//                          attr.name name
                          attr.``method`` "post"
                          attr.action act]
                         [
                          divAttr [attr.``class`` "form-group"] [Doc.Input [attr.``type`` "text"
                                                                            attr.name name
                                                                            attr.``class`` "form-control"
                                                                            attr.placeholder placeholder] rvInput
                                                                 buttonAttr [attr.``type`` "submit"
                                                                             attr.``class`` "btn btn-default"] [text btn]
                                                                ]
                         ]] :> Doc

    let FormItem2 foo (name, act, placeholder, btn) =
        Form.Return (fun valor -> valor)
        <*> (Form.Yield ""
            |> Validation.IsNotEmpty "Must enter a username")
        |> Form.WithSubmit
        |> Form.Run (fun valor ->
            async {
                return JS.Window.Location.Assign ("/prediction_alumn" + "?" + name + "=" + valor)
            } |> Async.Start
        )
        |> Form.Render (fun valor submit ->
            form [
                B.Simple.InputWithError "Usuario (intranet)" valor submit.View
                B.Button btn [attr.``class`` "btn btn-primary"] submit.Trigger
                B.ShowErrors [attr.style "margin-top:1em"] submit.View
            ]
        )



(*    let algo enlace =
        let matricula = Var.Create ""
        Form.Return (fun matricula -> matricula)
        <*> (Form.YieldVar user
            |> Validation.IsNotEmpty "Introduce una matrícula"
            |> Form.TransmitView)
        |> Form.WithSubmit
        |> Form.Run (fun matricula ->
            JS.Window.Location.Assign (enlace + "?matricula=" + matricula)
        )
        |> Form.Render (fun user ss submit ->

            div [
                div [label [text "Username: "]; Doc.Input [] user]
                div [label [text "Password: "];
                     Doc.PasswordBox [Attr.DynamicProp "disabled" (userSuccess |> View.Map (function
                        | Failure _ -> true
                        | Success _ -> false))] pass]
                Doc.Button "Log in" [] submit.Trigger
                div [
                    Doc.ShowErrors submit.View (fun errors ->
                        errors
                        |> Seq.map (fun m -> p [text m.Text])
                        |> Seq.cast
                        |> Doc.Concat)
                ]
            ]
        )*)

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
(*        let materias = List.fold (fun l ph -> let materia = ph.Materia + " (" + ph.Grupo + ")"
                                              if List.exists (fun m -> m = materia) l
                                              then l
                                              else materia :: l) [] predicciones
                                              |> List.rev*)

        let materias_mapa = 
                       List.fold (fun m ph -> let materia = ph.Materia + " (" + ph.Grupo + ")"
                                              match Map.tryFind materia m with
                                                | Some l -> Map.add materia (l @ [ph]) m
                                                | None ->   Map.add materia [ph] m) Map.empty predicciones

        let materias = materias_mapa |> Map.toList
                                     |> List.map fst

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
(*                    predicciones 
                           |> List.filter (PrediccionProfesor.MatchesMateria materia)*)
                      materias_mapa
                           |> Map.find materia
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

        let cabecera = 
           (["No."
             "Matrícula"
             "Nombre"
             "Predicción"
             ] |> List.map (fun txt -> (td [text txt]) :> Doc)
               |> trAttr [attr.``class`` "d2"]) :> Doc

        let putInPanel name comp = 
            divc "panel panel-default" [
//            divc "panel panel-inverse" [
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
                                    |> (fun l -> cabecera :: l)
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


[<JavaScript>]
module PredictionAlumn =
    // First, we declare types for predictions and how to order them.
    [<Direct "$element.classList.toggle('show');">]
    let toggle (element: Dom.Element) = X<unit>

    type PrediccionAlumno = {
        materia         : string
        grupo           : string
        periodo         : string
        matricula       : string
        nombreAlumno    : string
        carrera         : string
        nombreProfesor  : string
        apellidos       : string
        estatusPredicho : string
        c1              : string
        i1              : string
        c2              : string
        i2              : string
        c3              : string
        i3              : string
        estatus         : string
        precision       : string
        numero_instancias : string
        atributos       : string
        descripcion     : string
        descripcion_seleccion : string
        periodo_inicial : string
        periodo_final   : string
        parcial   : string
        }

    type Order = Alfabetico | Estatus

    type Order with

        /// A textual representation of our orderings.
        static member Show order =
            match order with
            | Alfabetico -> "Apellido"
//            | Matricula -> "Matrícula"
            | Estatus -> "Estatus"

    type PrediccionAlumno with

        /// A comparison function, based on whether we're sorting by name or matriculation number.
        static member Compare order p1 p2 =
            match order with
            | Alfabetico -> compare p1.apellidos p2.apellidos
//            | Matricula -> compare p1.Matricula p2.Matricula
            | Estatus -> compare p1.estatusPredicho p2.estatusPredicho

        /// A filtering function.
        static member MatchesQuery q ph =
            ph.nombreProfesor.Contains(q)
            || ph.apellidos.Contains(q)
            || ph.materia.Contains(q)
            || ph.periodo.Contains(q)

        /// A filtering function.
        static member MatchesPartial q ph =
            ph.parcial = q

    let sty n v = Attr.Style n v
    let cls n = Attr.Class n
    let divc c docs = Doc.Element "div" [cls c] docs


    // This is our prediction widget. We take a list of predictions, and return
    // an document tree which can be rendered.
    let widget (predicciones : PrediccionAlumno list) (planes : Map<string, string>) =
        let etiqueta = match predicciones with
                        | p :: _ -> p.matricula + " - " + p.nombreAlumno + " (" + p.carrera + ")"
                        | _ -> ""

        let periodos_mapa = 
                       List.fold (fun m ph -> let periodo = ph.periodo + " (" + ph.periodo_inicial + "-" + ph.periodo_final + ")"
                                              match Map.tryFind periodo m with
                                                | Some l -> Map.add periodo (l @ [ph]) m
                                                | None ->   Map.add periodo [ph] m) Map.empty predicciones
                                                |> Map.add "*" predicciones

        let parciales = predicciones |> List.map (fun p -> p.parcial)
                                     |> List.distinct
                                     |> List.sort

        let periodos = periodos_mapa |> Map.toList
                                     |> List.map fst

        // Búsqueda
        let vquery = Var.Create ""

        // Ordenamiento
        let vorder = Var.Create Alfabetico

        // Por periodo
        let vperiodos = match periodos with
                            | p :: _ -> Var.Create p
                            | [] -> Var.Create ""

        let vparciales = match parciales with
                            | p :: _ -> Var.Create p
                            | [] -> Var.Create ""

        // The above vars are our model. Everything else is computed from them.
        // Now, compute visible predictions under the current selection:

        let prediccionesVisibles =
            (View.FromVar vorder)
                |> View.Map2 (fun query order -> (query, order)) (View.FromVar vquery)
                |> View.Map2 (fun periodo (query, order) -> (periodo, query, order)) (View.FromVar vperiodos)
                |> View.Map2 (fun parcial (periodo, query, order) ->
                      periodos_mapa
                           |> Map.find periodo
                           |> List.filter (PrediccionAlumno.MatchesPartial parcial)
                           |> List.filter (PrediccionAlumno.MatchesQuery query)
                           |> List.sortWith (PrediccionAlumno.Compare order)) (View.FromVar vparciales)

        // A simple function for displaying the details of a prediction:
        let muestraPrediccion (getInfo : PrediccionAlumno -> Elt) i ph =
            let ename = "element_" + string i
            let popup = divAttr [attr.``class`` "popup"
                                 on.click (fun element _ -> let target = JS.Document.GetElementById ename
                                                            toggle target)]
                                [text "Modelo"
                                 spanAttr [attr.``class`` "popuptext"; attr.id ename]
                                          [getInfo ph]
                                ]


            trAttr [attr.``class`` ("d" + string (i % 2))] [
                td [text (string (i + 1))]
                td [text ph.materia]
                td [text ph.grupo]
                td [text (ph.apellidos + " / " + ph.nombreProfesor)]
//                td [text ph.precision]
//                td [text ph.parcial]
                td [text ph.c1]
                td [text ph.i1]
                td [text ph.c2]
                td [text ph.i2]
                td [text ph.c3]
                td [text ph.i3]
                td [text ph.estatus]
                td [text ph.estatusPredicho]
                td [popup]
            ] :> Doc

        let cabecera = 
           (["No."
             "Materia"
             "Grupo"
             "Profesor"
//             "Parcial"
             "P1"
             "I1"
             "P2"
             "I2"
             "P3"
             "I3"
             "Estatus"
             "Predicción"
             "Detalles"
             ] |> List.map (fun txt -> (td [text txt]) :> Doc)
               |> trAttr [attr.``class`` "d2"]) :> Doc


        let putInPanel name comp = 
            divc "panel panel-default" [
//            divc "panel panel-inverse" [
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
                    | _ :: _ -> 
                            let getInfo info =
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

                                let modelInfo = 
                                              ul [li [text ("Se consideraron " + string info.numero_instancias + " alumnos en la construcción del modelo predictivo.")]
                                                  li [text ("La precisión del modelo fue de " + info.precision + "% instancias clasificadas correctamente usando validación cruzada.")]
                                                  li [text "El modelo predictivo uso la siguiente información por materia:"
                                                      br []
                                                      text (atributos info.atributos)]
                                                  li [text "La información del algoritmo de clasificación es la siguiente:"
                                                      br []
                                                      p (descripcion info.descripcion)]
                                                  li [text "La información del algoritmo de selección de atributos es la siguiente:"
                                                      br []
                                                      p (descripcion info.descripcion_seleccion)]]
                                modelInfo
                            (predicciones
                                    |> List.mapi (muestraPrediccion getInfo)
                                    |> (fun l -> cabecera :: l)
                                    |> table
                                    |> (fun t -> div [t])) :> Doc
                    | [] -> (div []) :> Doc
                             

        let queryPanel = 
            divc "col-sm-6" [
                text "Periodo predicción (y entrenamiento): "
                Doc.Select [Attr.Create "class" "form-control"] id periodos vperiodos

                text "Parcial: "
                Doc.Select [Attr.Create "class" "form-control"] id parciales vparciales

                // We then have a select box, linked to our orders variable
                text "Ordenar por: "
                Doc.Select [Attr.Create "class" "form-control"] Order.Show [Alfabetico; Estatus] vorder
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
                    |> putInPanel ("Resultados: " + etiqueta)
        div [
            queryPanel
            resultsPanel
        ]

    let Main predicciones planes =
        // Funcion que extrae las predicciones de una lista de strings
        let prediccion P = 
            match P with 
                | [materia; grupo; periodo; matricula; nombreAlumno; carrera; nombreProfesor; apellidos; c1; i1; c2; i2; c3; i3; estatusPredicho; estatus; precision; instancias; atributos; descripcion; descripcion_seleccion; periodo_inicial; periodo_final; parcial] -> 
                    {materia   = materia
                     grupo     = grupo
                     periodo   = periodo
                     matricula = matricula
                     nombreAlumno    = nombreAlumno
                     carrera = carrera
                     nombreProfesor    = nombreProfesor
                     apellidos = apellidos
                     c1 = c1
                     i1 = i1
                     c2 = c2
                     i2 = i2
                     c3 = c3
                     i3 = i3
                     estatusPredicho = estatusPredicho
                     estatus   = estatus
                     precision = precision
                     numero_instancias = instancias
                     atributos = atributos
                     descripcion = descripcion
                     descripcion_seleccion = descripcion_seleccion
                     periodo_inicial = periodo_inicial
                     periodo_final = periodo_final
                     parcial = parcial
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
