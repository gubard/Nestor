using System;
using System.Net.WebSockets;
using System.Threading;
using Nestor.Shared;
using StreamJsonRpc;

const string ServerWebSocketUrl = "ws://localhost:5019/ws-jsonrpc"; // Use ws:// or wss://
using var webSocket = new ClientWebSocket();

try
{
    Console.WriteLine("Client: Connecting to WebSocket server...");
    await webSocket.ConnectAsync(new(ServerWebSocketUrl), CancellationToken.None);
    Console.WriteLine("Client: WebSocket connected.");
    using var jsonRpc = new JsonRpc(new WebSocketMessageHandler(webSocket));
    jsonRpc.StartListening();
    var calculator = jsonRpc.Attach<IExampleService>();

    calculator.CreateExample(
        new() { Id = Guid.NewGuid(), ExampleStringValue = "asl;dasdmal;sals;mdasc" }
    );

    // Keep the client alive to receive any potential server-sent notifications
    Console.WriteLine("Client: Press any key to close WebSocket connection.");
    Console.ReadKey();
}
catch (WebSocketException ex)
{
    Console.WriteLine($"Client WebSocket Error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Client General Error: {ex.Message}");
}

Console.WriteLine("Client: WebSocket disconnected.");
