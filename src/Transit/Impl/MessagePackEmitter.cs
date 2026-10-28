using MessagePack;
using System.Buffers;
using System.Collections.Immutable;
using System.Globalization;

namespace Beerendonk.Transit.Impl;

internal sealed class MessagePackEmitter : AbstractEmitter
{
    private readonly Stream output;
    private readonly ArrayBufferWriter<byte> buffer;

    public MessagePackEmitter(Stream output, IImmutableDictionary<Type, IWriteHandler> handlers)
        : base(handlers)
    {
        this.output = output;
        buffer = new ArrayBufferWriter<byte>();
    }

    public MessagePackEmitter(
        Stream output,
        IImmutableDictionary<Type, IWriteHandler> handlers,
        IWriteHandler? defaultWriteHandler,
        Func<object, object>? transform)
        : base(handlers, defaultWriteHandler, transform)
    {
        this.output = output;
        buffer = new ArrayBufferWriter<byte>();
    }

    public override void Emit(object? obj, bool asDictionaryKey, WriteCache cache)
    {
        MarshalTop(obj, cache);
    }

    public override void EmitNull(bool asDictionaryKey, WriteCache cache)
    {
        if (asDictionaryKey)
        {
            EmitString(Constants.EscStr, "_", string.Empty, true, cache);
        }
        else
        {
            var writer = new MessagePackWriter(buffer);
            writer.WriteNil();
            writer.Flush();
        }
    }

    public override void EmitString(string? prefix, string? tag, string value, bool asDictionaryKey, WriteCache cache)
    {
        var writer = new MessagePackWriter(buffer);
        writer.Write(cache.CacheWrite(Util.MaybePrefix(prefix, tag, value), asDictionaryKey));
        writer.Flush();
    }

    public override void EmitBoolean(bool value, bool asDictionaryKey, WriteCache cache)
    {
        if (asDictionaryKey)
        {
            EmitString(Constants.EscStr, "?", value ? "t" : "f", true, cache);
        }
        else
        {
            var writer = new MessagePackWriter(buffer);
            writer.Write(value);
            writer.Flush();
        }
    }

    public override void EmitInteger(object value, bool asDictionaryKey, WriteCache cache)
    {
        EmitInteger(Util.NumberToPrimitiveLong(value), asDictionaryKey, cache);
    }

    public override void EmitInteger(long value, bool asDictionaryKey, WriteCache cache)
    {
        if (asDictionaryKey)
        {
            EmitString(Constants.EscStr, "i", value.ToString(CultureInfo.InvariantCulture), true, cache);
        }
        else
        {
            var writer = new MessagePackWriter(buffer);
            writer.Write(value);
            writer.Flush();
        }
    }

    public override void EmitDouble(object value, bool asDictionaryKey, WriteCache cache)
    {
        if (value is double doubleValue)
        {
            EmitDouble(doubleValue, asDictionaryKey, cache);
        }
        else if (value is float floatValue)
        {
            EmitDouble(floatValue, asDictionaryKey, cache);
        }
        else
        {
            throw new TransitException("Unknown floating point type: " + value.GetType());
        }
    }

    public override void EmitDouble(float value, bool asDictionaryKey, WriteCache cache)
    {
        if (asDictionaryKey)
        {
            EmitString(Constants.EscStr, "d", value.ToString(CultureInfo.InvariantCulture), true, cache);
        }
        else
        {
            var writer = new MessagePackWriter(buffer);
            writer.Write(value);
            writer.Flush();
        }
    }

    public override void EmitDouble(double value, bool asDictionaryKey, WriteCache cache)
    {
        if (asDictionaryKey)
        {
            EmitString(Constants.EscStr, "d", value.ToString(CultureInfo.InvariantCulture), true, cache);
        }
        else
        {
            var writer = new MessagePackWriter(buffer);
            writer.Write(value);
            writer.Flush();
        }
    }

    public override void EmitBinary(object value, bool asDictionaryKey, WriteCache cache)
    {
        EmitString(Constants.EscStr, "b", Convert.ToBase64String((byte[])value), asDictionaryKey, cache);
    }

    public override void EmitListStart(long size)
    {
        var writer = new MessagePackWriter(buffer);
        writer.WriteArrayHeader(checked((int)size));
        writer.Flush();
    }

    public override void EmitListEnd()
    {
    }

    public override void EmitDictionaryStart(long size)
    {
        var writer = new MessagePackWriter(buffer);
        writer.WriteMapHeader(checked((int)size));
        writer.Flush();
    }

    public override void EmitDictionaryEnd()
    {
    }

    public override bool PrefersStrings()
    {
        return false;
    }

    public override void FlushWriter()
    {
        output.Write(buffer.WrittenSpan);
        buffer.Clear();
    }

    protected override void EmitDictionary(
        IEnumerable<KeyValuePair<object, object>> keyValuePairs,
        bool ignored,
        WriteCache cache)
    {
        int count = 0;
        foreach (var _ in keyValuePairs)
        {
            count++;
        }

        EmitDictionaryStart(count);
        foreach (KeyValuePair<object, object> pair in keyValuePairs)
        {
            Marshal(pair.Key, true, cache);
            Marshal(pair.Value, false, cache);
        }
        EmitDictionaryEnd();
    }
}
