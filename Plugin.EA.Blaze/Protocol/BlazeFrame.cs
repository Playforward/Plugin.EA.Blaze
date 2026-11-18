using System.Buffers.Binary;

namespace Plugin.EA.Blaze.Protocol;

/// <summary>
/// The fixed header in front of every Blaze message, over ProtoSSL. Unlike
/// Plasma's text header, Blaze frames carry the component and command as numbers
/// and the body is TDF (see <see cref="Tdf.BlazeTag"/>). The field layout here is
/// the documented Blaze header; its exact size and the message-type encoding are
/// confirmed per title against a live capture (docs/BLAZE.md), which is why the
/// reader returns null rather than guessing when too few bytes are present.
/// </summary>
public readonly record struct BlazeFrame(
    int PayloadLength,
    ushort Component,
    ushort Command,
    ushort ErrorCode,
    BlazeMessageType MessageType,
    ushort MessageId)
{
    /// <summary>Documented base header size, before any extended-length bytes.</summary>
    public const int HeaderSize = 12;

    public static BlazeFrame? TryReadHeader(ReadOnlySpan<byte> buffer, out int headerLength)
    {
        headerLength = HeaderSize;
        if (buffer.Length < HeaderSize)
            return null;

        int payloadLength = BinaryPrimitives.ReadUInt16BigEndian(buffer[..2]);
        ushort component = BinaryPrimitives.ReadUInt16BigEndian(buffer[2..4]);
        ushort command = BinaryPrimitives.ReadUInt16BigEndian(buffer[4..6]);
        ushort error = BinaryPrimitives.ReadUInt16BigEndian(buffer[6..8]);
        var messageType = (BlazeMessageType)buffer[8];
        // buffer[9] carries extended-length / flag bits; modelled but not interpreted yet.
        ushort messageId = BinaryPrimitives.ReadUInt16BigEndian(buffer[10..12]);

        return new BlazeFrame(payloadLength, component, command, error, messageType, messageId);
    }

    public byte[] WriteHeader(int payloadLength)
    {
        byte[] header = new byte[HeaderSize];
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(0, 2), (ushort)payloadLength);
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(2, 2), Component);
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(4, 2), Command);
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(6, 2), ErrorCode);
        header[8] = (byte)MessageType;
        header[9] = 0;
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(10, 2), MessageId);
        return header;
    }
}

/// <summary>How the peer should treat a frame. Values confirmed per title (docs/BLAZE.md).</summary>
public enum BlazeMessageType : byte
{
    Message = 0x00,
    Reply = 0x10,
    Notification = 0x20,
    ErrorReply = 0x30,
}
