using System.Net;
using System.Net.WebSockets;
using System.Text;
using Web;

var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

app.UseWebSockets();

app.Map("/ws/{identifier}", async context =>
{
    var identifier = context.Request.RouteValues["identifier"] as string;
    if (identifier != null && context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var handler = new Handler(identifier, webSocket);
        
        Console.WriteLine($"Connection established with {identifier}");

        var buffer = new byte[1024];
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            var str = Encoding.UTF8.GetString(buffer, 0, result.Count);

            handler.Handle(str);
        }

        if (webSocket.State == WebSocketState.CloseReceived)
        {
            Console.WriteLine($"Close received from {identifier}");
            await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }
        
        Console.WriteLine($"Connection closed with {identifier}");
    }
    else if (identifier == null)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

app.Run();