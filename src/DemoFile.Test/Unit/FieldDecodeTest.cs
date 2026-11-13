namespace DemoFile.Test.Unit;

[TestFixture]
public class FieldDecodeTest
{
    private enum ByteEnum : byte
    {
        Low = 1,
        High = 0xFE
    }

    private static FieldEncodingInfo Encoder(string? varEncoder) => new(varEncoder, 0, 0, null, null);

    [Test]
    public void FixedWidthBits()
    {
        Assert.That(FieldDecode.FixedWidthBits(Encoder("fixed8")), Is.EqualTo(8));
        Assert.That(FieldDecode.FixedWidthBits(Encoder("fixed16")), Is.EqualTo(16));
        Assert.That(FieldDecode.FixedWidthBits(Encoder("fixed32")), Is.EqualTo(32));
        Assert.That(FieldDecode.FixedWidthBits(Encoder("fixed64")), Is.EqualTo(0));
        Assert.That(FieldDecode.FixedWidthBits(Encoder(null)), Is.EqualTo(0));
    }

    [Test]
    public void Fixed8_ReadsOneRawByte()
    {
        // 0xFE has the varint continuation bit set: read as a varint it would consume the next byte too.
        var data = new byte[] { 0xFE, 0x05 };

        var buffer = new BitBuffer(data);
        Assert.That(FieldDecode.CreateDecoder_byte(Encoder("fixed8"))(ref buffer), Is.EqualTo((byte)0xFE));
        Assert.That(FieldDecode.CreateDecoder_byte(Encoder("fixed8"))(ref buffer), Is.EqualTo((byte)0x05));

        buffer = new BitBuffer(data);
        Assert.That(FieldDecode.CreateDecoder_sbyte(Encoder("fixed8"))(ref buffer), Is.EqualTo((sbyte)-2));
        Assert.That(FieldDecode.CreateDecoder_sbyte(Encoder("fixed8"))(ref buffer), Is.EqualTo((sbyte)5));

        buffer = new BitBuffer(data);
        Assert.That(FieldDecode.CreateDecoder_enum<ByteEnum>(Encoder("fixed8"))(ref buffer), Is.EqualTo(ByteEnum.High));
    }

    [Test]
    public void Fixed16And32_ReadRawLittleEndianBits()
    {
        var data = new byte[] { 0xFE, 0xFF, 0x34, 0x12, 0xFF, 0xFF, 0xFF, 0x80 };

        var buffer = new BitBuffer(data);
        Assert.That(FieldDecode.CreateDecoder_Int16(Encoder("fixed16"))(ref buffer), Is.EqualTo((short)-2));
        Assert.That(FieldDecode.CreateDecoder_UInt16(Encoder("fixed16"))(ref buffer), Is.EqualTo((ushort)0x1234));
        Assert.That(FieldDecode.CreateDecoder_Int32(Encoder("fixed32"))(ref buffer), Is.EqualTo(unchecked((int)0x80FFFFFF)));

        buffer = new BitBuffer(data);
        Assert.That(FieldDecode.CreateDecoder_UInt32(Encoder("fixed32"))(ref buffer), Is.EqualTo(0x1234FFFEu));
        Assert.That(FieldDecode.CreateDecoder_Int32(Encoder("fixed16"))(ref buffer), Is.EqualTo(-1));
    }

    [Test]
    public void NoEncoder_StillReadsVarints()
    {
        // Varint 0xFE 0x05 = 0x7E | (0x05 << 7) = 766
        var data = new byte[] { 0xFE, 0x05, 0x01 };

        var buffer = new BitBuffer(data);
        Assert.That(FieldDecode.CreateDecoder_UInt32(Encoder(null))(ref buffer), Is.EqualTo(766u));
        Assert.That(FieldDecode.CreateDecoder_byte(Encoder(null))(ref buffer), Is.EqualTo((byte)1));

        buffer = new BitBuffer(data);
        Assert.That(FieldDecode.CreateDecoder_enum<ByteEnum>(Encoder(null))(ref buffer), Is.EqualTo((ByteEnum)(766 & 0xFF)));
    }
}
