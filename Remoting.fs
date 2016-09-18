namespace Intranet

//open System.Collections.Concurrent
open WebSharper

module Server =
    type UserPassword = {
        User : string
        Password : string
    }

//    let users = ConcurrentDictionary<string, IntranetAccess.UserData>()

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
          | Some user -> ctx.UserSession.LoginUser (userpass.User, persistent=true)
                            |> Async.Ignore

    [<Rpc>]
    let LogoutUser () =
        let ctx = Web.Remoting.GetContext()
        ctx.UserSession.Logout()
