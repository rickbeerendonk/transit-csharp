using MessagePack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;

namespace Beerendonk.Transit.Impl;

internal sealed class MessagePackParser(
    Stream input,
    IImmutableDictionary<string, IReadHandler> handlers,
    IDefaultReadHandler<object> defaultHandler,
    IDictionaryReader dictionaryBuilder,
    IListReader listBuilder)
    : AbstractParser(handlers, defaultHandler, dictionaryBuilder, listBuilder)
{
    private readonly MessagePackStreamReader streamReader = new(input, true);
    private ReadOnlySequence<byte> currentMessage;
    private long currentPosition;

    public override object Parse(ReadCache cache)
    {
        ReadOnlySequence<byte>? message = streamReader.ReadAsync(CancellationToken.None).AsTask().GetAwaiter().GetResult();
        if (!message.HasValue)
        {
            return null;
        }

        currentMessage = message.Value;
        currentPosition = 0;
        object value = ParseVal(false, cache);
        if (currentPosition != currentMessage.Length)
        {
            throw new TransitException("MessagePack value contains trailing data.");
        }
        return value;
    }

    public override object ParseVal(bool asDictionaryKey, ReadCache cache)
    {
        var reader = new MessagePackReader(currentMessage.Slice(currentPosition));
        object value = ParseValue(ref reader, asDictionaryKey, cache);
        currentPosition += reader.Consumed;
        return value;
    }

    public override object ParseDictionary(bool asDictionaryKey, ReadCache cache, IDictionaryReadHandler handler)
    {
        var reader = new MessagePackReader(currentMessage.Slice(currentPosition));
        object value = ParseDictionary(ref reader, cache, handler);
        currentPosition += reader.Consumed;
        return value;
    }

    public override object ParseList(bool asDictionaryKey, ReadCache cache, IListReadHandler handler)
    {
        var reader = new MessagePackReader(currentMessage.Slice(currentPosition));
        object value = ParseList(ref reader, cache, handler);
        currentPosition += reader.Consumed;
        return value;
    }

    private object ParseValue(ref MessagePackReader reader, bool asDictionaryKey, ReadCache cache) =>
        reader.NextMessagePackType switch
        {
            MessagePackType.Map => ParseDictionary(ref reader, cache, null),
            MessagePackType.Array => ParseList(ref reader, cache, null),
            MessagePackType.String => cache.CacheRead(reader.ReadString(), asDictionaryKey, this),
            MessagePackType.Integer => reader.ReadInt64(),
            MessagePackType.Float => reader.ReadDouble(),
            MessagePackType.Boolean => reader.ReadBoolean(),
            MessagePackType.Nil => ReadNil(ref reader),
            var type => throw new TransitException("Unsupported MessagePack value type: " + type)
        };

    private static object ReadNil(ref MessagePackReader reader)
    {
        reader.ReadNil();
        return null;
    }

    private object ParseDictionary(
        ref MessagePackReader reader,
        ReadCache cache,
        IDictionaryReadHandler handler)
    {
        int size = reader.ReadMapHeader();
        IDictionaryReader builder = handler != null ? handler.DictionaryReader() : dictionaryBuilder;
        object result = builder.Init();

        for (int i = 0; i < size; i++)
        {
            object key = ParseValue(ref reader, true, cache);
            if (key is Tag tag)
            {
                return ParseTaggedValue(ref reader, tag.GetValue(), cache);
            }

            object value = ParseValue(ref reader, false, cache);
            result = builder.Add(result, key, value);
        }

        return builder.Complete(result);
    }

    private object ParseList(
        ref MessagePackReader reader,
        ReadCache cache,
        IListReadHandler handler)
    {
        int size = reader.ReadArrayHeader();
        IListReader builder = handler != null ? handler.ListReader() : listBuilder;
        object result = builder.Init();
        if (size == 0)
        {
            return builder.Complete(result);
        }

        bool firstIsDirectoryAsList = reader.NextMessagePackType == MessagePackType.String;
        object first;
        if (firstIsDirectoryAsList)
        {
            string firstString = reader.ReadString();
            firstIsDirectoryAsList = firstString == Constants.DirectoryAsList;
            first = cache.CacheRead(firstString, false, this);
        }
        else
        {
            first = ParseValue(ref reader, false, cache);
        }

        if (firstIsDirectoryAsList)
        {
            return ParseDirectoryAsMap(ref reader, size - 1, cache);
        }

        if (first is Tag tag)
        {
            return ParseTaggedValue(ref reader, tag.GetValue(), cache);
        }

        result = builder.Add(result, first);
        for (int i = 1; i < size; i++)
        {
            result = builder.Add(result, ParseValue(ref reader, false, cache));
        }

        return builder.Complete(result);
    }

    private object ParseDirectoryAsMap(ref MessagePackReader reader, int itemCount, ReadCache cache)
    {
        if ((itemCount & 1) != 0)
        {
            throw new TransitException("A map-as-array representation must contain key/value pairs.");
        }

        object result = dictionaryBuilder.Init();
        for (int i = 0; i < itemCount; i += 2)
        {
            object key = ParseValue(ref reader, true, cache);
            object value = ParseValue(ref reader, false, cache);
            result = dictionaryBuilder.Add(result, key, value);
        }

        return dictionaryBuilder.Complete(result);
    }

    private object ParseTaggedValue(ref MessagePackReader reader, string tag, ReadCache cache)
    {
        IReadHandler handler;
        if (TryGetHandler(tag, out handler))
        {
            if (reader.NextMessagePackType == MessagePackType.Map && handler is IDictionaryReadHandler mapHandler)
            {
                return ParseDictionary(ref reader, cache, mapHandler);
            }

            if (reader.NextMessagePackType == MessagePackType.Array && handler is IListReadHandler listHandler)
            {
                return ParseList(ref reader, cache, listHandler);
            }

            return handler.FromRepresentation(ParseValue(ref reader, false, cache));
        }

        return Decode(tag, ParseValue(ref reader, false, cache));
    }
}
