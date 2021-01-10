namespace ChatSample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html

open WebSharper4.Core31.SignalR

[<JavaScript>]
module Client =

    type ViewModel =
        {
            User : Var<string>
            Message : Var<string>
            MessageList : ListModel<string, Msg>
            ConnectionList : ListModel<string, string * string>
        }

    type ConnectionParams[<JavaScript>]() =
        [<Name "user">]
        [<Stub>]
        [<JavaScript>]
        member val User = Unchecked.defaultof<string> with get, set

    [<Inline "prompt($question, $defaultResponse)">]
    let Prompt (question: string) (defaultResponse: string) = null : string

    let CreateModel() =
        {
            User = Var.Create ""
            Message = Var.Create ""
            MessageList = ListModel.Create (fun m -> m.SentAt + m.Name)[]
            ConnectionList = ListModel.Create (fun s -> fst s)[]
        }

    let StateText =
        function
            | ConnectionState.Connected -> "connected"
            | ConnectionState.Connecting -> "connecting"
            | ConnectionState.Disconnected -> "disconnected"
            | ConnectionState.Reconnecting -> "reconnecting"
            | _ -> "unknown"

    let Main () =
        let model = CreateModel()
        let renderMessage (m : Msg) : Doc =
            Console.Log m
            Console.Log m.Name
            Console.Log m.Message
            li [] [
               strong [] [ text m.Name]
               text (": " + m.Message)
            ]

        let renderConnectionMessage (s : string * string) : Doc =
            li [] [ text (snd s) ]

        let connectionList =
            ListModel.View model.ConnectionList
                |> Doc.BindSeqCachedBy (fun s -> fst s) (renderConnectionMessage)

        let messageList = ListModel.View model.MessageList
                            |> Doc.BindSeqCachedBy (fun m -> m.SentAt + m.Name) (renderMessage)


        Var.Set model.User (Prompt "Enter your name:" "")

        let startup = StartupConfig()
        let url = sprintf "/chathub/?user=%s" (model.User.Value)
        let s =
            SignalR.New "chatHub"
            |> SignalR.WithUrlTransport url startup
            |> SignalR.WithAutomaticReconnect
            |> SignalR.WithLogging ("warn")
            |> SignalR.Build

        Connection.New()
            //|> Connection.WithQueryString (ConnectionParams(User = model.User.Value))
            //|> Connection.ConnectionError (fun e -> JavaScript.JS.Alert e)
            //|> Connection.Starting (fun _ -> model.ConnectionList.Add (JavaScript.Date.Now().ToString(), "Connection starting"))
            //|> Connection.Received (fun _ -> model.ConnectionList.Add (JavaScript.Date.Now().ToString(), "Connection received"))
            //|> Connection.ConnectionSlow (fun _ -> model.ConnectionList.Add (JavaScript.Date.Now().ToString(), "Slow connection"))
            |> Connection.Reconnecting (fun _ -> model.ConnectionList.Add (JavaScript.Date.Now().ToString(), "Connection reconnecting"))
            |> Connection.Reconnected (fun _ -> model.ConnectionList.Add (JavaScript.Date.Now().ToString(), "Connection reconnected"))
            |> Connection.Disconnected (fun _ -> model.ConnectionList.Add (JavaScript.Date.Now().ToString(), "Connection disconnected"))
            //|> Connection.StateChanged (fun s -> model.ConnectionList.Add(JavaScript.Date.Now().ToString(), ("from " + StateText s.OldState + " to " + StateText s.NewState)))
            |> Connection.Start (fun _ -> ()) (fun e -> JavaScript.JS.Alert ("connection error: " + e.Message))


        s
        |> SignalR.Receive<Msg> "Send" (fun m ->
                                        let message : Msg = Json.Decode m
                                        model.MessageList.Add message)
        |> SignalR.Receive<string> "ReceiveMessage" (fun u -> model.ConnectionList.Add (JavaScript.Date.Now().ToString() + u, "User " + u + " connected"))
        |> ignore

        div [] [
            div [attr.``class`` "container" ] [
                Doc.Input [] model.Message
                Doc.Button "Send" [] (fun _ -> s |> SignalR.Send
                                                        "chat"
                                                        {SentAt = JavaScript.Date.Now().ToString(); Name = model.User.Value; Message = model.Message.Value}
                                                        (fun _ -> ()) // called when successfully sent
                                                        (fun e -> JavaScript.JS.Alert e.Message) // called when error sending
                                                        |> ignore)
                ul [] [messageList]
            ]
            div [] [
                ul [] [connectionList]
            ]
        ]
