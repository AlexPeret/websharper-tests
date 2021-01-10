- [About](#orgfc0b69e)
- [Usage](#orga08e3b7)
- [Issues](#orge68d790)
  - [global reference to SignalR's connection object](#org96691bb)
  - [SignalR ASP.NET Core changes on API](#org51a590c)
  - [HubConnection vs HubConnectionBuilder](#org86dcda5)
- [References](#org75d4df3)


<a id="orgfc0b69e"></a>

# About

This is an attempt to port the [ ChrisDobby/Chrisjdobson.Websharper.SignalR](https://github.com/ChrisDobby/Chrisjdobson.Websharper.SignalR) project to .NET Core 3.1 and WebSharper 4.7.

The application is partially implemented, but is working. PR are welcome.

This is not intended to be used as a reference, but only for testing.


<a id="orga08e3b7"></a>

# Usage

From the solution (.sln) folder:

```shell
$ dotnet run --project ChatSample
```

Open two browsers instance pointing to <http://localhost:5000/> two emulate different users.


<a id="orge68d790"></a>

# Issues


<a id="org96691bb"></a>

## global reference to SignalR's connection object

Looks like the WebSharper compiler changed alot since version 3.x and I couldn't find a way to make a reference to the global object.

For instance, the snippet below won't find the \`\`\`connection\`\`\` object created by the Resource SignalRConnection. Which, by the way, I'm not using as a workaround.

```fsharp

type Connection[<JavaScript>]() =

    ...

    [<JavaScript>]
    // transpiler won't reference to connection object
    [<Inline "connection.baseUrl = $url;">]
    static member New(url : string) = Connection()
  ...
```

The solution was to use the $global fake variable.

```fsharp

type Connection[<JavaScript>]() =

    ...

    [<JavaScript>]
    [<Inline "$global.connection.baseUrl = $url;">]
    static member New(url : string) = Connection()
  ...
```


<a id="org51a590c"></a>

## SignalR ASP.NET Core changes on API

A lot of breaking changes from previous version (ASP.NET Framework).

For this sample, I didn't implemented some methods, like Connection.ConnectionError and others.

I believe it would be better to implement this component with WIG, instead.


<a id="org86dcda5"></a>

## HubConnection vs HubConnectionBuilder

This is regarded to the breaking changes on the API.

Before, creating the connection object as a single step. Now, there are two classes and some configuration must be done with the builder, before calling the build() method to get an instance of the connection object.


<a id="org75d4df3"></a>

# References

-   <https://github.com/aspnet/SignalR-samples> (the ChatSample)
-   <https://stackoverflow.com/questions/21031160/when-is-the-connection-closed-with-signalr-from-browser> (previous API with userConnected and broadcastMessage methods)
-   <https://docs.microsoft.com/en-us/aspnet/core/signalr/version-differences?view=aspnetcore-3.1#differences-on-the-server>
-   <https://docs.microsoft.com/en-us/javascript/api/@microsoft/signalr/?view=signalr-js-latest>
-   <https://docs.microsoft.com/en-us/aspnet/core/signalr/groups?view=aspnetcore-5.0>
-   <https://github.com/ChrisDobby/Chrisjdobson.Websharper.SignalR>
