namespace Intranet

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Server

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About
    | [<EndPoint "/admin">] Admin
    | [<EndPoint "/prediction">] Prediction
    | [<EndPoint "/logout">] LogOut

module Templating =
    open WebSharper.UI.Next.Html

    type MainTemplate = Templating.Template<"Main.html">

    let MenuBarGeneral (ctx: Context<EndPoint>) usertype endpoint : Doc list =
        let ( => ) txt act =
            liAttr [if endpoint = act then yield attr.``class`` "active"] [
                aAttr [attr.href (ctx.Link act)] [text txt]
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
                    li ["Predicción" => EndPoint.Prediction]
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

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             liAttr [if endpoint = act then yield attr.``class`` "active"] [
                aAttr [attr.href (ctx.Link act)] [text txt]
             ]
        [
            li ["Home" => EndPoint.Home]
            li ["About" => EndPoint.About]
        ]

    let MainGeneral ctx action title usertype body =
        Content.Page(
            MainTemplate.Doc(
                title = title,
                menubar = MenuBarGeneral ctx usertype action,
                body = body
            )
        )

    let Main ctx action title body =
        Content.Page(
            MainTemplate.Doc(
                title = title,
                menubar = MenuBar ctx action,
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

    let AboutPage ctx =
        async {
            let! loggedIn = ctx.UserSession.GetLoggedInUser()
            let! fullname = match loggedIn with
                                Some username -> Server.UserFullName username
                              | None -> async.Return ""
            let! usertype = match loggedIn with
                                Some username -> Server.UserType username
                              | None -> async.Return ""
            return! Templating.MainGeneral ctx EndPoint.About "About" usertype
                        [
                            h1 [text "Bienvenido!"]
                            h1 [text fullname]
                        ]
        }

    let PredictionPage ctx =
        let extraerNombre (info : string) =
            match info.Split [|'/'|] |> Array.toList with
                | apellidos :: nombre :: _ -> (apellidos.Trim(), nombre.Trim())
                | _ -> printfn "Información de profesor inválida: %s" info
                       ("", "")
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
            let! planes = BaseDatos.obtener_planes ()
            return! Templating.MainGeneral ctx EndPoint.Prediction "Prediction" usertype
                        [
                            div [client <@ PredictionProfessor.Main predictions planes @>]
//                            div [client <@ Client.predictionStudent () @>]
                        ]
        }

    let HomePage ctx =
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
                    | ("", Some username) -> [client <@ Client.AnonymousUser() @>]
                                                |> Templating.MainGeneral ctx EndPoint.Home "Inicio" usertype
                    | (_, Some username) ->
                       ([
                            h1 [text "Bienvenido!"]
                            h1 [text fullname]
                        ] : list<Doc>)
                            |> Templating.MainGeneral ctx EndPoint.Home "Inicio" usertype
                    | (_, None) -> [client <@ Client.AnonymousUser() @>]
                                     |> Templating.MainGeneral ctx EndPoint.Home "Inicio" usertype
//            return! Templating.MainGeneral ctx EndPoint.About "About" usertype content
        }

    let LogOutPage ctx =
        async {
            do! ctx.UserSession.Logout()
            return! HomePage ctx
        }

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
            | EndPoint.Admin -> AdminPage ctx
            | EndPoint.Prediction -> PredictionPage ctx
            | EndPoint.LogOut -> LogOutPage ctx
        )
