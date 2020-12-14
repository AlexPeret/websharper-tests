namespace WebSharperWebSocketChat

open WebSharper
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating.Runtime.Server
open WebSharper.JavaScript
open WebSharper.AspNetCore.WebSocket

// avoids clashing Message type properties with WebSocketReadState ones.
module WebSocketClient = WebSharper.AspNetCore.WebSocket.Client

[<JavaScript>]
module ChatPage2 =

    type ChatPageTemplate = Templating.Template<"ChatPage2.html">
    type ChatPageEvent = TemplateEvent<ChatPageTemplate.Vars,Dom.MouseEvent>

    let Main (endpoint : WebSocketEndpoint<string, string>) =
        let rvStateLabel = Var.Create "Ready to connect"
        let rvConnIDLabel = Var.Create "N/A"
        let rvSendMessage = Var.Create ""
        let rvRecipients = Var.Create ""

        let rvControlsDisabled = Var.Create false

        let vControlsDisabled = rvControlsDisabled.View |> View.Map id
        let vControlsEnabled = rvControlsDisabled.View |> View.Map not

        let attrConnectionUrlDisabled = Attr.DynamicProp "disabled" vControlsDisabled
        let attrConnectButtonDisabled = Attr.DynamicProp "disabled" vControlsDisabled
        let attrSendMessageEnabled = Attr.DynamicProp "disabled" vControlsEnabled
        let attrSendButtonEnabled = Attr.DynamicProp "disabled" vControlsEnabled
        let attrRecipientsEnabled = Attr.DynamicProp "disabled" vControlsEnabled
        let attrCloseButtonEnabled = Attr.DynamicProp "disabled" vControlsEnabled

        let gServer = ref None

        let isConnID (str:string) =
            if (str.Substring(0, 7) = "ConnID:") then
                Var.Set rvConnIDLabel (str.Substring(8, 45))

        let updateState (socket:WebSocketClient.WebSocketServer<_,_>) msg =

            let disable () =
                Var.Set rvControlsDisabled false

            let enable () =
                Var.Set rvControlsDisabled true

            // NOTE: currently, WIG creates the dll without F# metadata, providing
            // no support for union or record types, active patterns and so on.
            // Find description at:
            //   https://github.com/dotnet-websharper/core/issues/1121#issuecomment-743088088
            let (|Connecting|Open|Closing|Closed|) readyState =
                if readyState = WebSocketReadyState.Connecting then Connecting
                elif readyState = WebSocketReadyState.Open then Open
                elif readyState = WebSocketReadyState.Closing then Closing
                else Closed

            //match socket.Connection.ReadyState with
            match socket.Connection.ReadyState with
            | Connecting ->
                Console.Log("ReadyState:Connecting")
                Var.Set rvStateLabel "Connecting..."
                disable()

            | Open ->
                Console.Log("ReadyState:Open")
                Var.Set rvStateLabel "Open"

                match msg with
                | WebSocketClient.Message data ->
                    isConnID data
                | _ -> ()

                enable()

            | Closing ->
                Console.Log("ReadyState:Closing")
                Var.Set rvStateLabel "Closing..."
                disable()

            | Closed ->
                Console.Log("ReadyState:Closed")
                Var.Set rvStateLabel "Closed"
                Var.Set rvConnIDLabel "N/a"
                disable()

            | _ ->
                Console.Log("ReadyState:unknown")
                let errorMsg =
                    sprintf "Unknown WebSocket State: %A" (socket.Connection.ReadyState)
                Var.Set rvStateLabel errorMsg
                disable()

        let constructJSONPayload() =
            JSON.Stringify(
                {|
                  From = Var.Get rvConnIDLabel
                  To = Var.Get rvRecipients
                  Message = Var.Get rvSendMessage
                |})

        let connectButtonOnClick (_:ChatPageEvent) =
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

        let sendButtonOnClick (_:ChatPageEvent) =
            let data = constructJSONPayload()
            let socket = (!gServer).Value
            socket.Post(data)
            let row =
                tr []
                   [ td [ attr.``class`` "commslog-client"] [ text "Client" ]
                     td [ attr.``class`` "commslog-server"] [ text "Server" ]
                     td [ attr.``class`` "commslog-data"] [ text data ]
                   ]

            row
            |> Doc.RunAppendById "commsLog"

        let closeButtonOnClick (_:ChatPageEvent) =
            // if (!server || server.ReadyState != WebSocket.OPEN) the
            //     JS.Alert("socket not connected")
            let socket = (!gServer).Value
            socket.Connection.Close(1000, "Closing from client")

        // NOTES:
        // 1. although the ConnectionUrl field gets the endpoint's URI, it is
        //    for information only, as WebSharper relies on the endpoint Type.

        ChatPageTemplate()
          .StateLabel(rvStateLabel.View)
          .ConnIDLabel(rvConnIDLabel.View)
          .ConnectionUrl(endpoint.URI)
          .ConnectionUrlDisabled(attrConnectionUrlDisabled)
          .ConnectButtonOnClick(connectButtonOnClick)
          .ConnectButtonDisabled(attrConnectButtonDisabled)
          .SendMessage(rvSendMessage)
          .SendMessageEnabled(attrSendMessageEnabled)
          .SendButtonOnClick(sendButtonOnClick)
          .SendButtonEnabled(attrSendButtonEnabled)
          .Recipients(rvRecipients)
          .RecipientsEnabled(attrRecipientsEnabled)
          .CloseButtonOnClick(closeButtonOnClick)
          .CloseButtonEnabled(attrCloseButtonEnabled)
          .Doc()
