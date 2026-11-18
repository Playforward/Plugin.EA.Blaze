using Plugin.EA.DirtySdk;
using ServerShared.Controllers;
using ServerShared.Plugins;
using ServerShared.Titles;

namespace Plugin.EA.Blaze;

/// <summary>
/// EA's Blaze online services — the stack that followed Plasma from about 2010
/// (Fuse, Dead Space 2, the later Battlefields, Mass Effect 3…). It shares the
/// ProtoSSL transport and the account store with the Plasma plugin through the
/// EA base, but the service layer is different: a redirector plus a binary TDF
/// protocol (<see cref="Tdf.BlazeTag"/>, <see cref="Protocol.BlazeFrame"/>).
/// </summary>
/// <remarks>
/// This is an initial scaffold: it loads Blaze titles, checks the base is
/// present, and reports its listeners. The redirector and server message
/// handling are built out against live captures (docs/BLAZE.md).
/// </remarks>
public sealed class BlazePlugin : ServerPlugin<BlazeSettings>
{
    public override uint Priority => 10;
    public override string Name => "EaBlaze";

    public override void Start()
    {
        if (!DirtySdkRuntime.IsReady)
        {
            Log.Error("[ea/blaze] the EA base plugin (EaDirtySdk) is not loaded; without it there is no ProtoSSL layer. Not starting");
            return;
        }

        var problems = new List<string>();
        List<BlazeTitle> titles = BlazeTitle.LoadAll(TitleRegistry.All, problems);
        foreach (string problem in problems)
            Log.Error("[ea/blaze] {Problem}", problem);
        if (titles.Count == 0)
        {
            Log.Warning("[ea/blaze] no title with ProtocolFamily \"{Family}\"; not starting", BlazeTitle.ProtocolFamily);
            return;
        }

        foreach (BlazeListener listener in Config.Listeners)
        {
            bool present = ServerController.Servers.Any(s => s.Port == listener.Port && !s.IsUdp);
            if (!present)
                Log.Error("[ea/blaze] no TCP server on port {Port} for {Component}; add it to ServerAppSettings.json", listener.Port, listener.Component);
        }

        Log.Information("[ea/blaze] loaded {Titles}; listeners: {Listeners} (redirector and server handling are not implemented yet)",
            string.Join(", ", titles), string.Join(", ", Config.Listeners.Select(l => $"{l.Component} {l.Port}")));
    }

    public override void Stop()
    {
    }
}
