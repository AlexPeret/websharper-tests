namespace ChatSample

open System
open WebSharper
open Microsoft.AspNetCore.SignalR
open System.Threading.Tasks

type Msg =
    {
        [<Name "sentAt">]
        SentAt : string

        [<Name "name">]
        Name : string

        [<Name "message">]
        Message : string
    }

type ChatHub() =
    inherit Hub()
    override x.OnConnectedAsync() =
        let ctx = x.Context.GetHttpContext()
        let user = ctx.Request.Query.["user"]
        Console.WriteLine(user)
        // let name = x.Context.User.Identity.Name
        // Console.WriteLine(name)
        let t:Task = x.Clients.All.SendAsync("ReceiveMessage", user)

        t.Wait()
        base.OnConnectedAsync()

    member x.Chat (msg : Msg) =
        Console.WriteLine(msg)
        let t:Task = x.Clients.All.SendAsync("Send", msg)
        t.Wait()
