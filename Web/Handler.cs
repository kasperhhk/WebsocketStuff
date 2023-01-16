using System.Net.WebSockets;
using Shared;

namespace Web;

public class Handler
{
    private readonly string _identifier;
    private readonly WebSocket _ws;

    public Handler(string identifier, WebSocket ws)
    {
        _identifier = identifier;
        _ws = ws;
    }

    public void Handle(string message)
    {
        if (message.StartsWith("start"))
            HandleStart(message);
        else if (message.StartsWith("charging"))
            HandleCharging(message);
        else if (message.StartsWith("status"))
            HandleStatus(message);
        else if (message.StartsWith("stop"))
            HandleStop(message);
    }

    private void HandleStart(string message)
    {
        Console.WriteLine($"{_identifier}: Start transaction!");
        _ws.SendStringAsync("start ack", CancellationToken.None);
    }

    private void HandleCharging(string message)
    {
        Console.WriteLine($"{_identifier}: Charging update - {message}");
    }

    private void HandleStatus(string message)
    {
        Console.WriteLine($"{_identifier}: Status change - {message}");
    }

    private void HandleStop(string message)
    {
        Console.WriteLine($"{_identifier}: Stop transaction!");
        _ws.SendStringAsync("stop ack", CancellationToken.None);
    }
}