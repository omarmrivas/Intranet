namespace Intranet

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Server

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About
    | [<EndPoint "/admin">] Admin
    | [<EndPoint "/prediction_group">] PredictionGroup
    | [<EndPoint "/logout">] LogOut
    | [<EndPoint "POST /prediction_alumn">] PredictAlumn of args: PostedMatricula
and PostedMatricula =
    {
        [<FormData>] matricula: string
    }

type EndPoint with
    static member KindCompare p q =
            match (p, q) with
            | (PredictAlumn _, PredictAlumn _) -> true
            | _ -> p = q

type DropdownItem =
    | LinkTo of string * EndPoint
    | FormTo of string * EndPoint * string * string
    | Divider

module Templating =
    open WebSharper.UI.Next.Html

    type MainTemplate = Templating.Template<"Main.html">

    let MenuBarGeneral (ctx: Context<EndPoint>) usertype endpoint : Doc list =
        let ( => ) txt act =
            liAttr [if EndPoint.KindCompare endpoint act then yield attr.``class`` "active"] [
                aAttr [attr.href (ctx.Link act)] [text txt]
            ]

        let ( =>> ) txt acts =
            let MKitem ddi =
                match ddi with
                    | Divider -> liAttr [attr.``class`` "divider"] [] :> Doc
                    | LinkTo (label, act)  -> liAttr [if EndPoint.KindCompare endpoint act then yield attr.``class`` "active"] [aAttr [attr.href (ctx.Link act)] [text label]] :> Doc
                    | FormTo (name, act, placeholder, btn) -> 
                        let foo = EndPoint.KindCompare endpoint act
                        liAttr [if foo then yield attr.``class`` "active"]
                            [formAttr [attr.``class`` "navbar-form navbar-left"
                                       Attr.Create "role" "search"
                                       attr.``method`` "post"
                                       attr.action (ctx.Link act)]
                                      [
                                       divAttr [attr.``class`` "form-group"] 
                                               [
                                                   divAttr [attr.``class`` "row"]
                                                           [
                                                               divAttr [attr.``class`` "col-xs-6"]
                                                                       [
                                                                              inputAttr [attr.``type`` "text"
                                                                                         attr.name name
                                                                                         attr.maxlength "6"
                                                                                         attr.size "6"
                                                                                         attr.``class`` "form-control"
                                                                                         attr.placeholder placeholder] []]
                                                               divAttr [attr.``class`` "col-xs-6"]
                                                                       [
                                                                              buttonAttr [attr.``type`` "submit"
                                                                                          attr.``class`` "btn btn-default"] [text btn]]
                                                           ]
                                               ]
                                      ]] :> Doc
            let getAction ddi = 
                match ddi with
                    | LinkTo (_, act) -> act
                    | FormTo (name, act, placeholder, btn) -> act
                    | Divider -> EndPoint.LogOut
            let submenu () =
                List.map MKitem acts
            liAttr [attr.``class`` (if List.exists (EndPoint.KindCompare endpoint << getAction) acts
                                    then "dropdown active"
                                    else "dropdown")] [
                        aAttr [attr.href "#"; attr.``class`` "dropdown-toggle"; attr.``data-`` "toggle" "dropdown"; Attr.Create "role" "button"]
                              [text txt
                               bAttr [attr.``class`` "caret"] []]
                        ulAttr [attr.``class`` "dropdown-menu"; attr.style "width: 240px"] (submenu ())
                    ]

        match usertype with
            | "Student" ->
                [
                    li ["Inicio" => EndPoint.Home]
                    li ["Acerca de" => EndPoint.About]
                ]
            | "Professor" ->
                [
                    li ["Inicio" => EndPoint.Home]
                    li ["Predicción" =>> [LinkTo ("Por Grupo", EndPoint.PredictionGroup)
                                          Divider
                                          FormTo ("matricula", PredictAlumn {matricula=""} , "Matrícula", "Por Alumno")]]
                    li ["Acerca de" => EndPoint.About]
                    li ["Salir" => EndPoint.LogOut]
                ]
            | "Staff" ->
                [
                    li ["Inicio" => EndPoint.Home]
                    li ["Acerca de" => EndPoint.About]
                ]
            | "Admin" ->
                [
                    li ["Inicio" => EndPoint.Home]
                    li ["Administrar" => EndPoint.Admin]
                    li ["Acerca de" => EndPoint.About]
//                    li [client <@ Client.logOutButton "" @>]
                ]
            | _ ->
                [
                    li ["Inicio" => EndPoint.Home]
                ]

    let MainGeneral ctx action title usertype body =
        Content.Page(
            MainTemplate.Doc(
                title = title,
                menubar = MenuBarGeneral ctx usertype action,
                body = body
            )
        )

module Site =
    open WebSharper.UI.Next.Html

    let periodo () = 
        match System.DateTime.Now.Month with
            | 1 | 2 | 3 | 4 | 5 -> string System.DateTime.Now.Year + "1S"
            | 6 | 7 -> string System.DateTime.Now.Year + "2S"
            | 8 | 9 | 10 | 11 | 12 -> string System.DateTime.Now.Year + "3S"
            | _ -> printfn "Error en la lectura delse mes, retornando semestre primavera"
                   string System.DateTime.Now.Year + "1S"

    let extraerNombre (info : string) =
            match info.Split [|'/'|] |> Array.toList with
                | apellidos :: nombre :: _ -> (apellidos.Trim(), nombre.Trim())
                | _ -> printfn "Información de profesor inválida: %s" info
                       ("", "")

    let LogOutPage ctx =
        async {
            let! loggedIn = ctx.UserSession.GetLoggedInUser()
            let username = match loggedIn with
                                Some username -> username
                              | None -> ""
            return! Templating.MainGeneral ctx EndPoint.LogOut "About" "" [client <@ Client.logOutAction username @>]
        }

    let AdminPage (ctx: Context<_>) =
        async {
            let! loggedIn = ctx.UserSession.GetLoggedInUser()
            let! usertype = match loggedIn with
                                Some username -> Server.UserType username
                              | None -> async.Return ""
            let content = 
                match loggedIn with
                    Some username ->
                        let isAdmin = match Server.adminuser with
                                        Some admin -> if IntranetAccess.getUserName admin = username
                                                      then true
                                                      else false
                                      | None       -> false
                        let adminPrograms = [client <@ Client.UpdatePrograms () @>]
                        let adminGroups = [client <@ Client.UpdateGroups () @>]
                        let adminProfessors = [client <@ Client.UpdateProfessors () @>]
                        let adminKardex = [client <@ Client.UpdateKardex () @>]
                        if isAdmin
                        then 
                         div [
                              divAttr [attr.``class`` "jumbotron"] adminPrograms
                              divAttr [attr.``class`` "jumbotron"] adminGroups
                              divAttr [attr.``class`` "jumbotron"] adminProfessors
                              divAttr [attr.``class`` "jumbotron"] adminKardex
                              divAttr [attr.``class`` "jumbotron"] [client <@ Client.logOut username @>]
                         ]
                        else 
                         div [
                              div [client <@ Client.logOut username @>]
                         ]
                  | None -> 
                        div [
                            div [client <@ Client.AnonymousUser() @>]
                        ]
            return! Templating.MainGeneral ctx EndPoint.Home "Home" usertype [content]
        }

    let AboutPage' ctx =
        async {
            let! loggedIn = ctx.UserSession.GetLoggedInUser()
            let! fullname = match loggedIn with
                                Some username -> Server.UserFullName username
                              | None -> async.Return ""
            let (apellidos, nombre) = extraerNombre fullname
            let! usertype = match loggedIn with
                                Some username -> Server.UserType username
                              | None -> async.Return ""
            return! Templating.MainGeneral ctx EndPoint.About "Acerca de" usertype
                        [
                            h2 [text ("Bienvenido(a) " + nombre + " al Sistema de \"Predicción del Desempeño Académico de Estudiantes\" de la Universidad !")]
                            h4 [text ("El sistema tiene como objetivo el apoyar el trabajo de los profesores(as) y estudiantes, mediante la aplicación de la mineria de datos " +
                                      " para predecir el aprovechamiento académico de los alumnos en cada una de las materias que conforman " +
                                      " la retícula de las diferentes carreras que ofrece la UPSLP. ")
                                br []
                                text ("Nota: los modelo predictivos son un elemento importante para la toma de decisiones en el área educativa siguiendo " +
                                      " el método científico. Para mejorar el aprovechamiento de dichos modelos, hemos de saber que ningún modelo es perfecto," + 
                                      " y por tanto, son herramientas que no deben dejar de lado el conocimiento personal o la experiencia en un grupo de trabajo concreto.")
                                ]
                        ]
        }

    let PredictionPage' ctx =
        async {
            let! loggedIn = ctx.UserSession.GetLoggedInUser()
            let! usertype = match loggedIn with
                                Some username -> Server.UserType username
                              | None -> async.Return ""
            let! fullname = match loggedIn with
                                | Some username -> Server.UserFullName username
                                | None -> async.Return ""

            let (apellidos, nombre) = extraerNombre fullname

            let periodo = periodo ()
            let parcial = 0u

            let! predictions = BaseDatos.obtener_prediccion_profesor periodo parcial nombre apellidos
            match predictions with
                | x :: _ -> printfn "Length: %i" (List.length predictions)
                            printfn "Length 2: %i" (List.length x)
                | _ -> printfn "Length 0!"
            let! planes = BaseDatos.obtener_planes ()
            return! Templating.MainGeneral ctx EndPoint.PredictionGroup "Predicción por Grupos" usertype
                        [
                            div [client <@ PredictionProfessor.Main predictions planes @>]
//                            div [client <@ Client.predictionStudent () @>]
                        ]
        }

    let GenericPage view endpoint title ctx =
        async {
            let! loggedIn = ctx.UserSession.GetLoggedInUser()
            let! fullname = match loggedIn with
                                Some username -> Server.UserFullName username
                              | None -> async.Return ""
            let! usertype = match loggedIn with
                                Some username -> Server.UserType username
                              | None -> async.Return ""
            return!
                match (fullname, loggedIn) with
                    | ("", Some username) -> LogOutPage ctx
                    | (_, Some username) ->
                            let (apellidos, nombre) = extraerNombre fullname
                            Templating.MainGeneral ctx endpoint title usertype (view username usertype nombre apellidos)
                    | (_, None) -> [client <@ Client.AnonymousUser() @>]
                                     |> Templating.MainGeneral ctx EndPoint.Home "Inicio" usertype
        }

    let HomePage ctx =
        GenericPage (fun username usertype nombre apellidos ->
                        ([
                            h1 [text "Bienvenido!"]
                            h1 [text (nombre  + " " + apellidos + "...")]
                         ] : list<Doc>))
                     EndPoint.Home
                     "Inicio"
                     ctx

    let AboutPage ctx =
        GenericPage (fun username usertype nombre apellidos ->
                        [
                            h2 [text ("Bienvenido(a) " + nombre + " al Sistema de \"Predicción del Desempeño Académico de Estudiantes\" de la Universidad !")]
                            h4 [text ("El sistema tiene como objetivo el apoyar el trabajo de los profesores(as) y estudiantes, mediante la aplicación de la mineria de datos " +
                                      " para predecir el aprovechamiento académico de los alumnos en cada una de las materias que conforman " +
                                      " la retícula de las diferentes carreras que ofrece la UPSLP. ")
                                br []
                                text ("Nota: los modelo predictivos son un elemento importante para la toma de decisiones en el área educativa siguiendo " +
                                      " el método científico. Para mejorar el aprovechamiento de dichos modelos, hemos de saber que ningún modelo es perfecto," + 
                                      " y por tanto, son herramientas que no deben dejar de lado el conocimiento personal o la experiencia en un grupo de trabajo concreto.")
                                ]
                        ])
                     EndPoint.About
                     "Acerca de"
                     ctx

    let PredictionPage ctx =
        GenericPage (fun username usertype nombre apellidos ->
                        let periodo = periodo ()
                        let parcial = 0u

                        let predictions = BaseDatos.obtener_prediccion_profesor periodo parcial nombre apellidos
                                            |> Async.RunSynchronously
                        let planes = BaseDatos.obtener_planes ()
                                        |> Async.RunSynchronously
                        [ div [client <@ PredictionProfessor.Main predictions planes @>] ])
                     EndPoint.PredictionGroup
                     "Predicción por Grupos"
                     ctx

    let PredictAlumnPage matricula ctx =
        GenericPage (fun username usertype nombre apellidos ->
                        let predictions = BaseDatos.obtener_prediccion_alumno matricula.matricula
                                            |> Async.RunSynchronously
                        let planes = BaseDatos.obtener_planes ()
                                        |> Async.RunSynchronously
                        [ div [client <@ PredictionAlumn.Main predictions planes @>] ])
                     (EndPoint.PredictAlumn matricula)
                     "Predicción por Alumno"
                     ctx

    let PredictAlumnPage' matricula ctx =
        async {
            let! loggedIn = ctx.UserSession.GetLoggedInUser()
            let! fullname = match loggedIn with
                                Some username -> Server.UserFullName username
                              | None -> async.Return ""
            let! usertype = match loggedIn with
                                Some username -> Server.UserType username
                              | None -> async.Return ""
            return!
                match (fullname, loggedIn) with
                    | ("", Some username) -> LogOutPage ctx
                    | (_, Some username) ->
                       ([
                            h1 [text "Bienvenido!"]
                            h1 [text fullname]
                            h1 [text ("matricula: " + matricula.matricula)]
                        ] : list<Doc>)
                            |> Templating.MainGeneral ctx (EndPoint.PredictAlumn matricula) "Predicción por Alumno" usertype
                    | (_, None) -> [client <@ Client.AnonymousUser() @>]
                                     |> Templating.MainGeneral ctx EndPoint.Home "Inicio" usertype
        }


    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
            | EndPoint.Admin -> AdminPage ctx
            | EndPoint.PredictionGroup -> PredictionPage ctx
            | EndPoint.PredictAlumn matricula -> PredictAlumnPage matricula ctx
            | EndPoint.LogOut -> LogOutPage ctx
        )
