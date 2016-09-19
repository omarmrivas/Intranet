namespace Intranet

open System.Collections.Concurrent
open WebSharper

module Server =
    type UserPassword = {
        User : string
        Password : string
    }

    let users = ConcurrentDictionary<string, IntranetAccess.UserData>()

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
