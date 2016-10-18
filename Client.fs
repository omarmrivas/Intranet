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

    let logOutButton username =
        liAttr [attr.``class`` "passive"] [
            buttonAttr [
                on.click (fun _ _ ->
                    async {
                        do! Server.LogoutUser username
                        return JS.Window.Location.Reload()
                    } |> Async.Start
                )
                attr.width "100"
                attr.height "100"
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

    let predictionStudent () =
        let sty n v = Attr.Style n v
        let cls n = Attr.Class n
        let divc c docs = Doc.Element "div" [cls c] docs

        // Create a reactive variable and view.
        // Reactive *variables* are data *sources*.
        let elems = ["bar"; "foo"]
        let rvText = Var.Create ""
//        let rvSelect = Var.Create ""
        // Create the components backed by the variable: in this case, an input
        // field and a label to display the contents of such a field.

        // The inputField is created using RD.Input, which takes an RVar as its
        // parameter. Whenever the input field is updated, the new value is
        // automatically placed into the variable.
        let inputField =
            divAttr [cls "panel" ; cls "panel-default"] [
                divAttr [cls "panel-heading"] [
                    h3Attr [cls "panel-title"] [
                        text "Selección por"
                    ]
                ]

                divAttr [cls "panel-body"] [
                    formAttr [cls "form-horizontal" ; Attr.Create "role" "form"] [
                        divAttr [cls "form-group"] [
                            labelAttr [cls "col-sm-2" ; cls "control-label" ; attr.``for`` "inputBox"] [
                                Doc.TextNode "Write something: "
                            ]

                            divAttr [cls "col-sm-10"] [
                                Doc.Select [Attr.Create "class" "form-control"//attr.``class`` "form-control"
                                            attr.id "inputBox"
                                            on.afterRender (fun el ->
                                                let idx = List.findIndex ((=) rvText.Value) elems
                                                el?selectedIndex <- idx)] id elems rvText
//                                Doc.Input [attr.``class`` "form-control" ; attr.id "inputBox"] rvText
                            ]
                        ]
                    ]
                ]
        ]
        // Now, we make views of the text, which we mutate using Map.
        let view = View.FromVar rvText

        let viewCaps =
            view |> View.Map (fun s -> s.ToUpper () )

        let viewReverse =
            view |> View.Map (fun s -> new string ((s.ToCharArray ()) |> Array.rev))

        let viewWordCount =
            view |> View.Map (fun s -> s.Split([| ' ' |]).Length)

        let viewWordCountStr =
            View.Map string viewWordCount

        let viewWordOddEven =
            View.Map (fun i -> if i % 2 = 0 then "Even" else "Odd") viewWordCount

        let views =
            [
                ("Entered Text", view)
                ("Capitalised", viewCaps)
                ("Reversed", viewReverse)
                ("Word Count", viewWordCountStr)
                ("Is the word count odd or even?", viewWordOddEven)
            ]

        let tableRow (lbl, view) =
            tr [
                td [
                    text lbl
                ]
                tdAttr [sty "width" "70%"] [
                    textView view
                ]
            ] :> Doc

        let tbl =
            divc "panel panel-default" [
                divc "panel-heading" [
                    h3Attr [cls "panel-title"] [
                        text "Output"
                    ]
                ]

                divc "panel-body" [
                    tableAttr [cls "table"] [
                        tbody [
                            // We map the tableRow function onto the different
                            // views of the source, and concatenate the resulting
                            // documents.
                            List.map tableRow views |> Doc.Concat
                        ]
                    ]
                ]
            ]
        div [
            inputField
            tbl
        ]

(*        let v = Var.Create "bar"
        let doc = Doc.Select [
                        attr.``class`` "form-control"
                        ] id ["bar"; "foo"] v
        v.Value <- "foo"*)

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
                B.Simple.InputWithError "Usuario" user submit.View
                B.Simple.InputPasswordWithError "Contraseña" pass submit.View
                B.Button "Log in" [attr.``class`` "btn btn-primary"] submit.Trigger
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
    }

    type Order = Alfabetico | Matricula

    type Order with

        /// A textual representation of our orderings.
        static member Show order =
            match order with
            | Alfabetico -> "Apellido"
            | Matricula -> "Matrícula"

    type PrediccionProfesor with

        /// A comparison function, based on whether we're sorting by name or matriculation number.
        static member Compare order p1 p2 =
            match order with
            | Alfabetico -> compare p1.Nombre p2.Nombre
            | Matricula -> compare p1.Matricula p2.Matricula

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
    let widget (predicciones : PrediccionProfesor list) =
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
                td [text ph.Materia]
                td [text ph.Matricula]
                td [text ph.Nombre]
                td [text ph.Estatus]
                td [text ph.Precision]
            ] :> Doc

        let muestraPredicciones predicciones =
                (predicciones 
                    |> List.mapi muestraPrediccion
                    |> table) :> Doc
                             
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

        let queryPanel = 
            divc "col-sm-6" [
                text "Materia: "
                Doc.Select [Attr.Create "class" "form-control"] id materias vmaterias
                // We specify a label, and an input box linked to our query RVar.
                text "Buscar: "
                Doc.Input [Attr.Create "class" "form-control"] vquery

                // We then have a select box, linked to our orders variable
                text "Ordenar por: "
                Doc.Select [Attr.Create "class" "form-control"] Order.Show [Alfabetico; Matricula] vorder
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

    let Main predicciones =
        // Funcion que extrae las predicciones de una lista de strings
        let prediccion P = 
            match P with 
                | [materia; grupo; matricula; nombre; estatus; precision] -> 
                    {Materia   = materia
                     Grupo     = grupo
                     Matricula = matricula
                     Nombre    = nombre
                     Estatus   = estatus
                     Precision = precision}
                | _ -> printfn "Error"
                       failwith "Error"

        let predicciones = List.map prediccion predicciones
        widget predicciones
