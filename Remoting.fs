namespace Intranet

open System.Collections.Concurrent
open WebSharper

module Server =
    // Log-in types/vars
    type UserPassword = {
        User : string
        Password : string
    }

    let adminuser = IntranetAccess.loginAdmin ()

    let users = let dictionary = ConcurrentDictionary<string, IntranetAccess.UserData>()
                match adminuser with
                    Some admin -> ignore (dictionary.AddOrUpdate(IntranetAccess.getUserName admin, admin, (fun _ user -> user)))
                                  printfn "Added admin: %s" (IntranetAccess.getUserName admin)
                                  dictionary
                  | None       -> dictionary
    // Log-in types/vars

    // Database commands types/vars
    type Career = {
        ITI  : bool
        ITEM : bool
        ISTI : bool
        ITMA : bool
        LAG  : bool
        LMKT : bool
    }

    let carreras = ["ITI"; "ITEM"; "ISTI"; "ITMA"; "LAG"; "LMKT"]

    let periodos = 
            "20013S" ::
                [for ano in 2002 .. System.DateTime.Now.Year do
                    for s in 1..3 do
                        yield string ano + string s + "S"]
    // Database commands types/vars

    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }

    [<Rpc>]
    let LoginUser (userpass: UserPassword) =
        let ctx = Web.Remoting.GetContext()
        let user = IntranetAccess.login userpass.User userpass.Password
        match user with
            None      -> async.Return ()
          | Some user -> ignore (users.AddOrUpdate(userpass.User, user, (fun _ user -> user)))
                         ctx.UserSession.LoginUser (userpass.User, persistent=true)
                            |> Async.Ignore

    [<Rpc>]
    let LogoutUser username =
        let v : IntranetAccess.UserData ref = ref Unchecked.defaultof<IntranetAccess.UserData>
        ignore (users.TryRemove(username, v))
        let ctx = Web.Remoting.GetContext()
        ctx.UserSession.Logout()

    [<Rpc>]
    let UserFullName username =
        let v : IntranetAccess.UserData ref = ref Unchecked.defaultof<IntranetAccess.UserData>
        if users.TryGetValue(username, v)
        then !v |> IntranetAccess.getFullName
                |> async.Return
        else async.Return ""

    [<Rpc>]
    let UserType username =
        let v : IntranetAccess.UserData ref = ref Unchecked.defaultof<IntranetAccess.UserData>
        if users.TryGetValue(username, v)
        then !v |> IntranetAccess.getUserType
                |> async.Return
        else async.Return "Unknown"

    [<Rpc>]
    let UpdatePrograms (careers : string list) =
        printfn "Executing UpdatePrograms..."
        let ctx = Web.Remoting.GetContext()
        let loggedIn = ctx.UserSession.GetLoggedInUser()
                        |> Async.RunSynchronously
        async {
                match loggedIn with
                    | Some username ->
                        let userdata = let v : IntranetAccess.UserData ref = ref Unchecked.defaultof<IntranetAccess.UserData>
                                       if users.TryGetValue(username, v)
                                       then !v
                                       else failwith "User can not access this resource..."
                        let cookie = IntranetAccess.getCookie userdata
                        let carreras = careers
                        printfn "Carreras: %A" carreras
                        let cookie = Planes.obtener_extracurriculares cookie
                        let cookie =  List.fold Planes.obtener_plan cookie carreras
                        let userdata = IntranetAccess.changeCookie userdata cookie
                        ignore (users.AddOrUpdate(username, userdata, (fun _ user -> user)))
                    | None -> printfn "You must log-in..."
                return ()
        }

