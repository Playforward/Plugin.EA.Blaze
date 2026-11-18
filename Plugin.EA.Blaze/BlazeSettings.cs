using System.Net;
using System.Net.Sockets;

namespace Plugin.EA.Blaze;

/// <summary>Plugin-wide settings, kept in <c>Configs/EaBlazeConfig.json</c>. Per-game ports come from title data.</summary>
public sealed class BlazeSettings
{
    /// <summary>Address the redirector sends clients to. Empty = this machine's first non-loopback IPv4.</summary>
    public string PublicHost { get; set; } = "";

    /// <summary>Which service answers on which ServerApp listener. Both redirector and server are ProtoSSL.</summary>
    public List<BlazeListener> Listeners { get; set; } = [];

    public bool LogTraffic { get; set; } = true;

    public IPAddress PublicAddress()
    {
        if (!string.IsNullOrWhiteSpace(PublicHost))
        {
            if (IPAddress.TryParse(PublicHost, out IPAddress? parsed))
                return parsed;
            return Dns.GetHostAddresses(PublicHost).First(a => a.AddressFamily == AddressFamily.InterNetwork);
        }
        return Dns.GetHostAddresses(Dns.GetHostName())
                  .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a))
               ?? IPAddress.Loopback;
    }
}

public sealed class BlazeListener
{
    public string Component { get; set; } = "";
    public int Port { get; set; }
}

public static class BlazeComponents
{
    /// <summary>The redirector (gosredirector): hands the client the main server's address.</summary>
    public const string Redirector = "REDIRECTOR";

    /// <summary>The main Blaze server: authentication, matchmaking, game manager.</summary>
    public const string Server = "SERVER";
}
