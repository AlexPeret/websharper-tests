module Dependencies

open WebSharper
module R = WebSharper.Core.Resources
//module A = WebSharper.Core.Attributes

//[<A.Require(typeof<WebSharper.JQuery.Resources.JQuery>)>]
[<Require(typeof<WebSharper.JQuery.Resources.JQuery>)>]
[<Sealed>]
type SignalRJs() =
    // According to the documentation, the jQuery dependency has been removed.
    // source: https://docs.microsoft.com/en-us/aspnet/core/signalr/version-differences?view=aspnetcore-3.1
    //inherit R.BaseResource("//ajax.aspnetcdn.com/ajax/signalr/", "jquery.signalr-2.1.2.min.js")
    inherit R.BaseResource("/lib/", "signalr.min.js")

// [<Sealed>]
// type SignalRHubConnection() =
//     interface R.IResource with
//         member _.Render _ =
//             fun writer ->
//                 let scriptWriter = writer R.Scripts
//                 scriptWriter.RenderBeginTag "script"
//                 // jQuery dependency has been remove. Find comment at the top.
//             //scriptWriter.WriteLine "var connection = $.hubConnection();"
//                 // scriptWriter.WriteLine 
//                 //     """
//                 //     const connection = new signalR.HubConnectionBuilder()
//                 //           .withUrl("/chathub")
//                 //           .withAutomaticReconnect()
//                 //           .build();
//                 //        """
//                 scriptWriter.WriteLine "const hubConnection = new signalR.HubConnectionBuilder();"
//                 scriptWriter.RenderEndTag()

// [<Sealed>]
// type SignalRConnection() =
//     interface R.IResource with
//         member _.Render _ =
//             fun writer ->
//                 let scriptWriter = writer R.Scripts
//                 scriptWriter.RenderBeginTag "script"
//                 scriptWriter.WriteLine "const connection = hubConnection.build();"
//                 scriptWriter.RenderEndTag()
