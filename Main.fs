namespace Intranet

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Server

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About

module Templating =
    open WebSharper.UI.Next.Html

    type MainTemplate = Templating.Template<"Main.html">

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

    let HomePage (ctx: Context<_>) =
        async {
            let! loggedIn = ctx.UserSession.GetLoggedInUser()
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
                              div [client <@ Client.userComponents username @>]
                         ]
                        else 
                         div [
                              div [client <@ Client.userComponents username @>]
                         ]
                  | None -> 
                        div [
                            h1 [text "Say Hi to the server!"]
                            div [client <@ Client.AnonymousUser() @>]
                        ]
            return! Templating.Main ctx EndPoint.Home "Home" [content]
        }

    let AboutPage ctx =
        async {
            let! loggedIn = ctx.UserSession.GetLoggedInUser()
            let! fullname = match loggedIn with
                                Some username -> Server.UserFullName username
                              | None -> async.Return ""
            return! Templating.Main ctx EndPoint.About "About" 
                        [
                            h1 [text "About"]
                            h1 [text fullname]
                            p [text "This is a template WebSharper client-server application."]
                        ]
        }
(*        Templating.Main ctx EndPoint.About "About" [
            h1 [text "About"]
            h1 [text ("Hola")]
            p [text "This is a template WebSharper client-server application."]
        ]*)

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
        )
