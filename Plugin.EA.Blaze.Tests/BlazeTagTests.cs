using Plugin.EA.Blaze.Protocol;
using Plugin.EA.Blaze.Tdf;

namespace Plugin.EA.Blaze.Tests;

public class BlazeTagTests
{
    [Theory]
    [InlineData("ADDR")]
    [InlineData("SESS")]
    [InlineData("USER")]
    [InlineData("ID")]
    public void Tag_round_trips(string label)
    {
        uint tag = BlazeTag.Encode(label);
        Assert.Equal(label, BlazeTag.Decode(tag));
    }

    [Fact]
    public void Tag_fits_in_24_bits()
    {
        Assert.True(BlazeTag.Encode("CMND") <= 0x00ff_ffff);
    }

    [Fact]
    public void Frame_header_round_trips()
    {
        var frame = new BlazeFrame(0, Component: 0x0005, Command: 0x0001, ErrorCode: 0,
            BlazeMessageType.Reply, MessageId: 7);

        byte[] header = frame.WriteHeader(payloadLength: 0);
        BlazeFrame? read = BlazeFrame.TryReadHeader(header, out int headerLength);

        Assert.NotNull(read);
        Assert.Equal(BlazeFrame.HeaderSize, headerLength);
        Assert.Equal((ushort)0x0005, read!.Value.Component);
        Assert.Equal((ushort)0x0001, read.Value.Command);
        Assert.Equal(BlazeMessageType.Reply, read.Value.MessageType);
        Assert.Equal((ushort)7, read.Value.MessageId);
    }

    [Fact]
    public void Frame_header_needs_all_twelve_bytes()
    {
        Assert.Null(BlazeFrame.TryReadHeader(new byte[8], out _));
    }
}
