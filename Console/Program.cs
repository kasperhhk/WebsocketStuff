using System.Net.WebSockets;
using Shared;

using ClientWebSocket ws = new ClientWebSocket();
await ws.ConnectAsync(new Uri(args[0]), CancellationToken.None);

var buffer = new byte[1024];

Console.WriteLine("sending start");
await ws.SendStringAsync("start", CancellationToken.None);

var startResult = await ws.ReceiveStringAsync(buffer, CancellationToken.None);
if (startResult != "start ack")
{
    Console.WriteLine("didn't get ack :(");
    return;
}

Console.WriteLine("start ACK'ed");

for (var i = 0; i < 5; i++)
{
    if (i == 3)
    {
        Console.WriteLine("temporary suspend");
        await ws.SendStringAsync("status suspended", CancellationToken.None);

        await Task.Delay(3000);
        
        Console.WriteLine("resume charging");
        await ws.SendStringAsync("status charging", CancellationToken.None);
    }
    
    await Task.Delay(1000);
    Console.WriteLine("sending meter value");
    await ws.SendStringAsync($"charging {(i+1) * 1000}", CancellationToken.None);
}

Console.WriteLine("sending stop");
await ws.SendStringAsync("stop", CancellationToken.None);

var stopResult = await ws.ReceiveStringAsync(buffer, CancellationToken.None);
if (stopResult == "stop ack")
{
    Console.WriteLine("stop ACK'ed, closing connection");
    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
}