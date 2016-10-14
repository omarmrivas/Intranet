namespace Intranet

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Server

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About
    | [<EndPoint "/admin">] Admin

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
                    li ["Home" => EndPoint.Home]
                    li ["About" => EndPoint.About]
                ]
            | "Professor" ->
                [
                    li ["Home" => EndPoint.Home]
                    li ["About" => EndPoint.About]
                ]
            | "Staff" ->
                [
                    li ["Home" => EndPoint.Home]
                    li ["About" => EndPoint.About]
                ]
            | _ ->
                [
                    li ["Home" => EndPoint.Home]
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

    let MainGeneral ctx action title body usertype =
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
                              divAttr [attr.``class`` "jumbotron"] [client <@ Client.userComponents username @>]
                         ]
                        else 
                         div [
                              div [client <@ Client.userComponents username @>]
                         ]
                  | None -> 
                        div [
                            div [client <@ Client.AnonymousUser() @>]
                        ]
            return! Templating.MainGeneral ctx EndPoint.Home "Home" [content] usertype
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
            return! Templating.MainGeneral ctx EndPoint.About "About" 
                        [
                            h1 [text "Bienvenido!"]
                            h1 [text fullname]
                        ]
                        usertype
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
            let content =
                match loggedIn with
                    | Some username ->
                        [
                            h1 [text "Bienvenido!"]
                            h1 [text fullname]
                        ] : list<Doc>
                    | None -> [client <@ Client.AnonymousUser() @>]
            return! Templating.MainGeneral ctx EndPoint.About "About" 
                        content
                        usertype
        }


    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
            | EndPoint.Admin -> AdminPage ctx
        )
