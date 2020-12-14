namespace WebSharperWebSocketChat

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server

open WebSharper.AspNetCore.WebSocket

type EndPoint =
    | [<EndPoint "/">] ChatJQuery
    | [<EndPoint "/chat-reactive">] ChatReactive
    | [<EndPoint "/about">] About

module Templating =
    open WebSharper.UI.Html

    type MainTemplate = Templating.Template<"Main.html">

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             li [if endpoint = act then yield attr.``class`` "active"] [
                a [attr.href (ctx.Link act)] [text txt]
             ]
        [
            "Chat - JQuery" => EndPoint.ChatJQuery
            "Chat - Reactive" => EndPoint.ChatReactive
            "About" => EndPoint.About
        ]

    let Main ctx action (title: string) (body: Doc list) =
        Content.Page(
            MainTemplate()
                .Title(title)
                .MenuBar(MenuBar ctx action)
                .Body(body)
                .Doc()
        )

module Site =
    open WebSharper.UI.Html

    let MyEndPoint (url: string) : WebSharper.AspNetCore.WebSocket.WebSocketEndpoint<string, string> =
        WebSocketEndpoint.Create(url, "/ws", JsonEncoding.Readable)

    let ChatJqueryPage (ctx: Context<_>) =
        let wsep = MyEndPoint (ctx.RequestUri.ToString())

        Templating.Main ctx EndPoint.ChatJQuery "Chat - JQuery" [
            //div [] [ client <@ ChatPage.Main wsep @> ]
            div [] [ client <@ ChatPage.Main wsep @> ]
        ]

    let ChatReactivePage (ctx: Context<_>) =
        let wsep = MyEndPoint (ctx.RequestUri.ToString())

        Templating.Main ctx EndPoint.ChatReactive "Chat - Reactive" [
            //div [] [ client <@ ChatPage.Main wsep @> ]
            div [] [ client <@ ChatPage2.Main wsep @> ]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            h1 [] [text "About"]
            p [] [text "This is a template WebSharper client-server application."]
        ]

    [<Website>]
    let Main =
        Application.MultiPage (fun (ctx: Context<_>) endpoint ->
            match endpoint with
            | EndPoint.ChatJQuery -> ChatJqueryPage ctx
            | EndPoint.ChatReactive -> ChatReactivePage ctx
            | EndPoint.About -> AboutPage ctx
        )
