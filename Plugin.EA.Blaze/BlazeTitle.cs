using System.Text.Json;
using System.Text.Json.Serialization;
using ServerShared.Titles;

namespace Plugin.EA.Blaze;

/// <summary>
/// One Blaze-era EA game, read from <c>&lt;ResourceDir&gt;/ea/title.json</c> of a
/// title whose <c>ProtocolFamily</c> is <c>ea-blaze</c>. Blaze games (Fuse, Dead
/// Space 2, the later Battlefields, Mass Effect 3…) reach a redirector first and
/// are then sent to the main server; both run on the shared ProtoSSL layer.
/// </summary>
public sealed class BlazeTitle
{
    public const string ProtocolFamily = "ea-blaze";
    public const string FileName = "ea/title.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() },
    };

    [JsonIgnore]
    public TitleDescriptor Descriptor { get; set; } = new();

    /// <summary>The redirector host the game is pointed at, e.g. <c>gosredirector.ea.com</c>.</summary>
    public string RedirectorHost { get; set; } = "";

    /// <summary>Redirector listener port (ProtoSSL).</summary>
    public int RedirectorPort { get; set; }

    /// <summary>Main Blaze server listener port the redirector sends the client to (ProtoSSL).</summary>
    public int ServerPort { get; set; }

    /// <summary>The Blaze "service name" the client asks the redirector for (per title).</summary>
    public string ServiceName { get; set; } = "";

    public override string ToString() => Descriptor.ToString();

    public static List<BlazeTitle> LoadAll(IEnumerable<TitleDescriptor> titles, List<string> problems)
    {
        var loaded = new List<BlazeTitle>();
        foreach (TitleDescriptor descriptor in titles.Where(t =>
                     string.Equals(t.ProtocolFamily, ProtocolFamily, StringComparison.OrdinalIgnoreCase)))
        {
            string path = Path.Combine(descriptor.ResourceDir, FileName);
            if (!File.Exists(path))
            {
                problems.Add($"{descriptor}: {FileName} not found at {path}");
                continue;
            }
            try
            {
                BlazeTitle title = JsonSerializer.Deserialize<BlazeTitle>(File.ReadAllText(path), JsonOptions)
                                   ?? throw new InvalidDataException("empty title file");
                title.Descriptor = descriptor;
                loaded.Add(title);
            }
            catch (Exception exception) when (exception is JsonException or InvalidDataException)
            {
                problems.Add($"{descriptor}: {FileName} could not be read: {exception.Message}");
            }
        }
        return loaded;
    }
}
