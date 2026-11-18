namespace Plugin.EA.Blaze.Tdf;

/// <summary>
/// A Blaze TDF field tag: a short label (up to four characters from a restricted
/// alphabet) packed into three bytes, followed by a one-byte type. Blaze replaced
/// Plasma's <c>key=value</c> text with this tagged binary format. The packing is
/// our own implementation of the documented scheme; the exact character mapping
/// is confirmed against a live capture (docs/BLAZE.md) and kept here as a single
/// pair of inverse functions so a correction touches one place.
/// </summary>
public static class BlazeTag
{
    /// <summary>Packs a label (padded or truncated to four characters) into the low 24 bits of an int.</summary>
    public static uint Encode(string label)
    {
        Span<char> chars = ['\0', '\0', '\0', '\0'];
        for (int i = 0; i < 4 && i < label.Length; i++)
            chars[i] = label[i];

        uint tag = 0;
        foreach (char c in chars)
            tag = (tag << 6) | SixBits(c);
        return tag & 0x00ff_ffff;
    }

    /// <summary>Recovers the label from a packed tag, trimming the trailing padding.</summary>
    public static string Decode(uint tag)
    {
        Span<char> chars = stackalloc char[4];
        for (int i = 3; i >= 0; i--)
        {
            chars[i] = FromSixBits((byte)(tag & 0x3f));
            tag >>= 6;
        }
        return new string(chars).TrimEnd('\0');
    }

    // The alphabet is the printable range Blaze labels use (space/underscore, digits, upper case).
    // '\0' maps to 0 (padding); every other character is offset from the space character.
    private static uint SixBits(char c) => c == '\0' ? 0u : (uint)((c - 0x20) & 0x3f);

    private static char FromSixBits(byte v) => v == 0 ? '\0' : (char)(0x20 + v);
}
