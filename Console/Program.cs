using System.Net.WebSockets;
using System.Text.Json;
using Shared;

using ClientWebSocket ws = new ClientWebSocket();
await ws.ConnectAsync(new Uri("ws://localhost:5110/ws/PEWPEW"), CancellationToken.None);

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

var transactionStart = new DateTimeOffset(2023, 1, 17, 15, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
var transactionId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - transactionStart;
var meter = Random.Shared.NextInt64(0, 12345);

var buffer = new byte[1024];

var startPayload = JsonSerializer.Serialize(new
{
    TransactionId = transactionId,
    Timestamp = DateTimeOffset.UtcNow,
    MeterStart = meter
}, jsonOptions);
Console.WriteLine($"sending start: {startPayload}");
await ws.SendStringAsync($"start {startPayload}", CancellationToken.None);

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
        var suspendPayload = JsonSerializer.Serialize(new
        {
            TransactionId = transactionId,
            Timestamp = DateTimeOffset.UtcNow,
            Status = ChargingStationStatus.Suspended.ToString()
        }, jsonOptions);
        Console.WriteLine($"temporary suspend: {suspendPayload}");
        await ws.SendStringAsync($"status {suspendPayload}", CancellationToken.None);

        await Task.Delay(3000);
        
        var resumePaylod = JsonSerializer.Serialize(new
        {
            TransactionId = transactionId,
            Timestamp = DateTimeOffset.UtcNow,
            Status = ChargingStationStatus.Charging.ToString()
        }, jsonOptions);
        Console.WriteLine($"resume charging: {resumePaylod}");
        await ws.SendStringAsync($"status {resumePaylod}", CancellationToken.None);
    }
    
    await Task.Delay(1000);
    meter += Random.Shared.NextInt64(500, 1200);
    
    var chargingPayload = JsonSerializer.Serialize(new
    {
        TransactionId = transactionId,
        Timestamp = DateTimeOffset.UtcNow,
        MeterValue = meter
    }, jsonOptions);
    Console.WriteLine($"sending meter value: {chargingPayload}");
    await ws.SendStringAsync($"charging {chargingPayload}", CancellationToken.None);
}

await Task.Delay(500);
meter += Random.Shared.NextInt64(100, 500);

var stopPayload = JsonSerializer.Serialize(new
{
    TransactionId = transactionId,
    Timestamp = DateTimeOffset.UtcNow,
    MeterStop = meter
}, jsonOptions);
Console.WriteLine($"sending stop: {stopPayload}");
await ws.SendStringAsync($"stop {stopPayload}", CancellationToken.None);

var stopResult = await ws.ReceiveStringAsync(buffer, CancellationToken.None);
if (stopResult == "stop ack")
{
    Console.WriteLine("stop ACK'ed, closing connection");
    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
}