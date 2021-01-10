namespace WebSharper4.Core31.SignalR

open WebSharper

type TransportType =
    | WebSockets
    | ForeverFrame
    | ServerSentEvents
    | LongPolling

type ConnectionState =
    | Connecting = 0
    | Connected = 1
    | Reconnecting = 2
    | Disconnected = 4

type StateChange =
    {
        [<Name "newState">]
        NewState : ConnectionState

        [<Name "oldState">]
        OldState : ConnectionState
    }

type Error = 
    {
        [<Name "message">]
        Message : string

        [<Name "data">]
        data : string
    }

type StartupConfig[<JavaScript>]() =
    [<JavaScript>]
    let rec transportList transports =
        let transportText =
            function
                | TransportType.WebSockets -> "webSockets"
                | TransportType.ForeverFrame -> "foreverFrame"
                | TransportType.ServerSentEvents -> "serverSentEvents"
                | TransportType.LongPolling -> "longPolling"

        match transports with
            | t::l -> (transportText t)::transportList l
            | _ -> []

    [<Name "transport">]
    [<Stub>]
    member val private T = Unchecked.defaultof<string array> with get, set

    [<JavaScript>]
    member x.Transport with set(v: TransportType list) = x.T <- (transportList v) |> List.toArray

[<Require(typeof<Dependencies.SignalRJs>)>]
//[<Require(typeof<Dependencies.SignalRConnection>)>]
type Connection[<JavaScript>]() =
    // reference: https://docs.microsoft.com/en-us/javascript/api/@microsoft/signalr/hubconnection?view=signalr-js-latest

    [<JavaScript>]
    static member New() = Connection()

    [<JavaScript>]
    [<Inline "$global.connection.baseUrl = $url;">]
    static member New(url : string) = Connection()

    //???
    // [<JavaScript>]
    // [<Inline "connection.qs = $qs">]
    // static member WithQueryString qs (c : Connection) = c

    // [<JavaScript>]
    // [<Inline "connection.logging = false">]
    // static member WithoutLogging (c : Connection) = c

    //???
    // [<JavaScript>]
    // [<Inline "connection.error($f)">]
    // static member ConnectionError (f : string -> unit) (s : Connection) = s

    //???
    // [<JavaScript>]
    // [<Inline "connection.starting($f)">]
    // static member Starting (f : unit -> unit) (c : Connection) = c

    //???
    // [<JavaScript>]
    // [<Inline "connection.received($f)">]
    // static member Received (f : unit -> unit) (c : Connection) = c

    //???
    // [<JavaScript>]
    // [<Inline "connection.connectionSlow($f)">]
    // static member ConnectionSlow (f : unit -> unit) (c : Connection) = c

    [<JavaScript>]
    [<Inline "$global.connection.onreconnecting($f)">]
    static member Reconnecting (f : unit -> unit) (c : Connection) = c

    [<JavaScript>]
    [<Inline "$global.connection.onreconnected($f)">]
    static member Reconnected (f : unit -> unit) (c : Connection) = c

    [<JavaScript>]
    [<Inline "$global.connection.onclose($f)">]
    static member Disconnected (f : unit -> unit) (c : Connection) = c

    [<JavaScript>]
    [<Inline "$global.connection.stateChanged($f)">]
    static member StateChanged (f : StateChange -> unit) (c : Connection) = c

    [<JavaScript>]
    //[<Inline "connection.start($cfg).done($success).catch($fail)">]
    //static member Start (cfg : StartupConfig) (success : unit -> unit) (fail : Error -> unit) (s : Connection) = ()
    [<Inline "$global.connection.start().then($success).catch($fail)">]
    static member Start (success : unit -> unit) (fail : Error -> unit) (s : Connection) = ()

[<Require(typeof<Dependencies.SignalRJs>)>]
//[<Require(typeof<Dependencies.SignalRHubConnection>)>] // must come before SignalRConnection
//[<Require(typeof<Dependencies.SignalRConnection>)>]
type SignalR[<JavaScript>](hubName : string) =
    // reference: https://docs.microsoft.com/en-us/javascript/api/@microsoft/signalr/hubconnectionbuilder?view=signalr-js-latest
    member private x.HubName() = hubName

    [<JavaScript>]
    [<Inline "new signalR.HubConnectionBuilder()">]
    static member New (hub : string) = SignalR(hub)

    [<JavaScript>]
    [<Inline "$s.withUrl($url)">]
    static member WithUrl (url : string) (s: SignalR) = s

    [<JavaScript>]
    [<Inline "$s.withUrl($url,$transportType)">]
    static member WithUrlTransport (url : string) (transportType : StartupConfig) (s: SignalR) = s

    [<JavaScript>]
    [<Inline "$s.configureLogging($logLevel)">]
    static member WithLogging (logLevel:string) (s : SignalR) = s

    [<JavaScript>]
    [<Inline "$s.withAutomaticReconnect()">]
    static member WithAutomaticReconnect (s : SignalR) = s

    [<JavaScript>]
    [<Inline "$global.connection = $s.build();">]
    static member Build (s : SignalR) = s

    [<JavaScript>]
    //[<Inline "connection.createHubProxy($s.hubName).on($name, $f)">]
    [<Inline "$global.connection.on($name, $f)">]
    static member Receive<'a> (name : string) (f : 'a -> unit) (s : SignalR) = s

    [<JavaScript>]
    //[<Inline "connection.createHubProxy($s.hubName).invoke($name, $m).done($success).fail($fail)">]
    [<Inline "$global.connection.invoke($name, $m).then($success).catch($fail)">]
    static member Send<'a> (name : string) (m : 'a) (success : unit -> unit) (fail : Error -> unit) (s : SignalR) = s

