namespace WebSharperWebSocketChat

open WebSharper
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.AspNetCore.WebSocket

// avoids clashing Message type properties with WebSocketReadState ones.
module WebSocketClient = WebSharper.AspNetCore.WebSocket.Client

[<JavaScript>]
module ChatPage =

    type ChatPageTemplate = Templating.Template<"ChatPage.html">

    let private runScript (endpoint : WebSocketEndpoint<string, string>) =

        //TODO: remove, as WebSharper gets it from the endpoint
        let connectionUrl =
            JQuery("#connectionUrl")
              .Val("ws://localhost:5000")

        //TODO: replace JQuery by Html elements
        let connectButton = JQuery("#connectButton")
        let stateLabel = JQuery("#stateLabel")
        let sendMessage = JQuery("#sendMessage")
        let sendButton = JQuery("#sendButton")
        let commsLog = JQuery("#commsLog")
        let closeButton = JQuery("#closeButton")
        let recipients = JQuery("#recipients")
        let connID = JQuery("#connIDLabel")

        let isConnID (str:string) =
            if (str.Substring(0, 7) = "ConnID:") then
                connID.Html("ConnID: " + str.Substring(8, 45)).Ignore

        let updateState (socket:WebSocketClient.WebSocketServer<_,_>) msg =

            let disable() =
                sendMessage.Prop("disabled",true).Ignore
                sendButton.Prop("disabled",true).Ignore
                closeButton.Prop("disabled",true).Ignore
                recipients.Prop("disabled",true).Ignore

            let enable() =
                sendMessage.Prop("disabled",false).Ignore
                sendButton.Prop("disabled",false).Ignore
                closeButton.Prop("disabled",false).Ignore
                recipients.Prop("disabled",false).Ignore

            connectionUrl.Prop("disabled",true).Ignore
            connectButton.Prop("disabled",true).Ignore

            let readyState:WebSocketReadyState =
                socket.Connection.ReadyState
            let test = WebSocketReadyState.Connecting
            // from WebSocketReadyState
            Console.Log "socket.Connection.ReadyState"
            Console.Log socket.Connection.ReadyState
            Console.Log readyState

            // ERROR: WebSharper cannot resolve the WebSocketReadyState type options
            // //match socket.Connection.ReadyState with
            // match readyState with
            // | WebSocketReadyState.Connecting ->
            //     Console.Log("ReadyState:Connecting")
            //     stateLabel.Html("Connecting...").Ignore
            //     disable()

            // | Open ->
            //     Console.Log("ReadyState:Open")
            //     stateLabel.Html("Open").Ignore

            //     match msg with
            //     | WebSocketClient.Message data ->
            //         isConnID data
            //     | _ -> ()

            //     enable()

            // | Closing ->
            //     Console.Log("ReadyState:Closing")
            //     stateLabel.Html("Closing...").Ignore
            //     disable()

            // | Closed ->
            //     Console.Log("ReadyState:Closed")
            //     stateLabel.Html("Closed").Ignore
            //     connID.Html("ConnID: N/a").Ignore
            //     disable()
            //     connectionUrl.Prop("disabled",false).Ignore
            //     connectButton.Prop("disabled",false).Ignore

            // | _ ->
            //     Console.Log("ReadyState:unknown")
            //     let errorMsg =
            //         sprintf "Unknown WebSocket State: %A" (socket.Connection.ReadyState)
            //     stateLabel.Html(errorMsg).Ignore
            //     disable()

            let readyStateS =
                sprintf "%A" socket.Connection.ReadyState
            let readyStateInt =
                JS.ParseInt(readyStateS, 10)
            Console.Log "readyStateS"
            Console.Log readyStateS
            Console.Log "readyStateInt"
            Console.Log readyStateInt
            // GAMBIARRA/WORKAROUND: check error comment above
            match readyStateInt with
            | 0 ->
                Console.Log("ReadyState:Connecting")
                stateLabel.Html("Connecting...").Ignore
                disable()

            | 1 ->
                Console.Log("ReadyState:Open")
                stateLabel.Html("Open").Ignore

                match msg with
                | WebSocketClient.Message data ->
                    isConnID data
                | _ -> ()

                enable()

            | 2 ->
                Console.Log("ReadyState:Closing")
                stateLabel.Html("Closing...").Ignore
                disable()

            | 3 ->
                Console.Log("ReadyState:Closed")
                stateLabel.Html("Closed").Ignore
                connID.Html("ConnID: N/a").Ignore
                disable()
                connectionUrl.Prop("disabled",false).Ignore
                connectButton.Prop("disabled",false).Ignore

            | _ ->
                Console.Log("ReadyState:unknown")
                let errorMsg =
                    sprintf "Unknown WebSocket State: %A" (socket.Connection.ReadyState)
                stateLabel.Html(errorMsg).Ignore
                disable()


        let gServer = ref None

        let connectButtonOnClick _ _ =
            async {
                let! server =
                    WebSocketClient.ConnectStateful endpoint <| fun server -> async {
                        return 0, fun state msg -> async {
                            match msg with
                            | WebSocketClient.Message data ->
                                Console.Log "Msg - Message"
                                updateState server msg
                                return (state + 1)
                            | WebSocketClient.Close ->
                                Console.Log "Msg - Close"
                                updateState server msg
                                return state
                            | WebSocketClient.Open ->
                                Console.Log "Msg - Open"
                                updateState server msg
                                return state
                            | WebSocketClient.Error ->
                                Console.Log "Msg - Error"
                                updateState server msg
                                return state
                        }
                    }
                gServer := Some server
                ()
            }
            |> Async.Start

        JQuery(connectButton).On("click",connectButtonOnClick).Ignore


        let constructJSONPayload() =
            JSON.Stringify(
                {|
                    From = connID.Html().Substring(8, connID.Html().Length);
                    To = recipients.Val()
                    Message = sendMessage.Val()
                |})

        let sendButtonOnClick _ _ =
            let data = constructJSONPayload()
            let socket = (!gServer).Value
            socket.Post(data)
            commsLog.Append(
                "<tr>" +
                "<td class=\"commslog-client\">Client</td>" +
                "<td class=\"commslog-server\">Server</td>" +
                "<td class=\"commslog-data\">" + data + "</td></tr>"
            ).Ignore

        JQuery(sendButton).On("click",sendButtonOnClick).Ignore

        let closeButtonOnClick _ _ =
            // if (!server || server.ReadyState != WebSocket.OPEN) the
            //     JS.Alert("socket not connected")
            let socket = (!gServer).Value
            socket.Connection.Close(1000, "Closing from client")

        JQuery(closeButton).On("click",closeButtonOnClick).Ignore

    let Main (endpoint : WebSocketEndpoint<string, string>) =
        ChatPageTemplate()
          .AfterRender(fun (el:Dom.Element) -> runScript endpoint)
          .Doc()
