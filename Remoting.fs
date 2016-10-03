namespace Intranet

open System.Collections.Concurrent
open WebSharper
open System.Security.Cryptography
open System.Text

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

//    let carreras = ["ITI"; "ITEM"; "ISTI"; "ITMA"; "LAG"; "LMKT"]

(*    let periodos () = 
            "20013S" ::
                [for ano in 2002 .. System.DateTime.Now.Year do
                    for s in 1..3 do
                        yield string ano + string s + "S"]*)
    // Database commands types/vars

    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }


    let Encrypt (planText : string) (passPhrase : string) =
        let clearBytes = Encoding.Unicode.GetBytes(planText)
        let encryptor = Aes.Create()
        ()

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
                        let thread = System.Threading.Thread (fun () ->
                            let cookie = Planes.obtener_extracurriculares cookie
                            let cookie =  List.fold Planes.obtener_plan cookie carreras
                            let cookie = List.fold Seriaciones.obtener_seriacion_plan cookie carreras
                            let userdata = IntranetAccess.changeCookie userdata cookie
                            ignore (users.AddOrUpdate(username, userdata, (fun _ user -> user)))
                            printfn "Excecution of UpdatePrograms finished successfully...")
                        thread.Start()
                    | None -> printfn "You must log-in..."
                return ()
        }

    [<Rpc>]
    let UpdateGroups (periods : string list) =
        printfn "Executing UpdateGroups..."
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
                        printfn "Periodos: %A" periods
                        let thread = System.Threading.Thread (fun () ->
                            let cookie =  List.fold Grupos.obtener_grupos cookie periods
                            let userdata = IntranetAccess.changeCookie userdata cookie
                            ignore (users.AddOrUpdate(username, userdata, (fun _ user -> user)))
                            printfn "Excecution of UpdateGroups finished successfully...")
                        thread.Start()
                    | None -> printfn "You must log-in..."
                return ()
        }

    [<Rpc>]
    let UpdateProfessors (planes : string list) (periods : string list) =
        printfn "Executing UpdateProfessors..."
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
                        printfn "Carreras: %A" planes
                        printfn "Periodos: %A" periods
                        let thread = System.Threading.Thread (fun () ->
                            let muestras = [for carrera in planes do
                                                for periodo in periods do
                                                    yield (carrera, periodo)]
                            let cookie =  List.fold Profesores.obtener_profesores cookie muestras
                            let userdata = IntranetAccess.changeCookie userdata cookie
                            ignore (users.AddOrUpdate(username, userdata, (fun _ user -> user)))
                            printfn "Excecution of UpdateGroups finished successfully...")
                        thread.Start()
                    | None -> printfn "You must log-in..."
                return ()
        }

    [<Rpc>]
    let UpdateKardex (planes : string list) (periods : string list) =
        printfn "Executing UpdateKardex..."
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
                        printfn "Periodos: %A" periods
                        let thread = System.Threading.Thread (fun () ->
                            let muestras = [for carrera in planes do
                                                for periodo in periods do
                                                    yield (carrera, periodo)]
                            let alumnos_kardex cookie muestra =
                                Kardex.obtener_kardex (Alumnos.obtener_alumnos cookie muestra) muestra

                            let cookie = List.fold alumnos_kardex cookie muestras
(*                            let alumnos_kardex muestra =
                                let cookie = Option.get (IntranetAccess.newAdminCookie ())
                                Kardex.obtener_kardex (Alumnos.obtener_alumnos cookie muestra) muestra
                            muestras |> List.toArray
                                     |> Array.Parallel.iter (ignore << alumnos_kardex)*)
//                            let cookie = List.fold Alumnos.obtener_alumnos cookie muestras
//                            let cookie = List.fold Kardex.obtener_kardex cookie muestras
//                            let cookie = Option.get (IntranetAccess.newAdminCookie ())
                            let userdata = IntranetAccess.changeCookie userdata cookie
                            ignore (users.AddOrUpdate(username, userdata, (fun _ user -> user)))
                            printfn "Excecution of UpdateKardex finished successfully...")
                        thread.Start()
                    | None -> printfn "You must log-in..."
                return ()
        }

