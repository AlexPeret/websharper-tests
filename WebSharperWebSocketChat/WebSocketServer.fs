namespace WebSharperWebSocketChat

open System
open WebSharper.AspNetCore.WebSocket.Server

module WebSocketServer =

    let Start() : StatefulAgent<string, string, int> =
        /// print to debug output and stdout
        let dprintfn x =
            Printf.ksprintf (fun s ->
                System.Diagnostics.Debug.WriteLine s
                stdout.WriteLine s
            ) x

        fun client -> async {
            let clientIp = client.Connection.Context.Connection.RemoteIpAddress.ToString()

            let connID = Guid.NewGuid().ToString()
            do! client.PostAsync ("ConnID: " + connID)

            return 0, fun state msg -> async {
                dprintfn "Received message #%i from %s" state clientIp
                (* msg: WebSocket.Client.Message<'S2>
                   There are 4 cases, corresponding to the events defined by
                   WebSockets API (W3C):
                    | Message of 'S2C
                    | Error
                    | Open
                    | Close
                *)

                match msg with
                | Message data ->
                    do! client.PostAsync ("message: " + data)

                    return state + 1

                // | Open ->
                //     //let buffer = Encoding.UTF8.GetBytes("ConnID: " + connID)
                //     do! client.PostAsync ("ConnID: " + connID)
                //     return state

                | Error exn ->
                    eprintfn "Error in WebSocket server connected to %s: %s" clientIp exn.Message
                    do! client.PostAsync ("Error: " + exn.Message)
                    return state
                | Close ->
                    dprintfn "Closed connection to %s" clientIp
                    return state
            }
        }
