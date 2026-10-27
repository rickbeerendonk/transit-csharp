// Copyright © 2014 Rick Beerendonk. All Rights Reserved.
//
// This code is a C# port of the Java version created and maintained by Cognitect, therefore
//
// Copyright © 2014 Cognitect. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS-IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Newtonsoft.Json;
using Beerendonk.Transit.Spi;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace Beerendonk.Transit.Impl;

/// <summary>
/// Represents a reader factory.
/// </summary>
internal class ReaderFactory
{
    /// <summary>
    /// Get the default handlers.
    /// </summary>
    /// <returns>The default handlers.</returns>
    public static IImmutableDictionary<string, IReadHandler> DefaultHandlers()
    {
        var builder = ImmutableDictionary.Create<string, IReadHandler>().ToBuilder();

        builder.Add(":", new ReadHandlers.KeywordReadHandler());
        builder.Add("$", new ReadHandlers.SymbolReadHandler());
        builder.Add("i", new ReadHandlers.IntegerReadHandler());
        builder.Add("?", new ReadHandlers.BooleanReadHandler());
        builder.Add("_", new ReadHandlers.NullReadHandler());
        builder.Add("f", new ReadHandlers.BigRationalReadHandler());
        builder.Add("n", new ReadHandlers.BigIntegerReadHandler());
        builder.Add("d", new ReadHandlers.DoubleReadHandler());
        builder.Add("z", new ReadHandlers.SpecialNumberReadHandler());
        builder.Add("c", new ReadHandlers.CharacterReadHandler());
        builder.Add("t", new ReadHandlers.VerboseDateTimeReadHandler());
        builder.Add("m", new ReadHandlers.DateTimeReadHandler());
        builder.Add("r", new ReadHandlers.UriReadHandler());
        builder.Add("u", new ReadHandlers.GuidReadHandler());
        builder.Add("b", new ReadHandlers.BinaryReadHandler());
        builder.Add("\'", new ReadHandlers.IdentityReadHandler());
        builder.Add("set", new ReadHandlers.SetReadHandler());
        builder.Add("list", new ReadHandlers.ListReadHandler());
        builder.Add("ratio", new ReadHandlers.RatioReadHandler());
        builder.Add("cmap", new ReadHandlers.CDictionaryReadHandler());
        builder.Add("link", new ReadHandlers.LinkReadHandler());

        return builder.ToImmutable();
    }

    /// <summary>
    /// THe defaults default handler.
    /// </summary>
    /// <returns>The default read handler.</returns>
    public static IDefaultReadHandler<ITaggedValue> DefaultDefaultHandler()
    {
        return new DefaultReadHandler();
    }

    private static void DisallowOverridingGroundTypes(
        IEnumerable<KeyValuePair<string, IReadHandler>>? handlers)
    {
        if (handlers != null)
        {
            IImmutableDictionary<string, IReadHandler> defaultHandlers = DefaultHandlers();
            string[] groundTypeTags = ["_", "s", "?", "i", "d", "b", "'", "map", "array"];
            foreach (KeyValuePair<string, IReadHandler> handler in handlers)
            {
                foreach (string tag in groundTypeTags)
                {
                    if (handler.Key == tag)
                    {
                        IReadHandler? defaultHandler;
                        if (!defaultHandlers.TryGetValue(tag, out defaultHandler)
                            || handler.Value == null
                            || handler.Value.GetType() != defaultHandler.GetType())
                        {
                            throw new TransitException("Cannot override decoding for transit ground type, tag " + tag);
                        }
                    }
                }
            }
        }
    }

    public static IImmutableDictionary<string, IReadHandler> HandlerMap(
        IEnumerable<KeyValuePair<string, IReadHandler>>? customHandlers)
    {
        DisallowOverridingGroundTypes(customHandlers);
        IImmutableDictionary<string, IReadHandler> handlers = DefaultHandlers();
        if (customHandlers != null)
        {
            foreach (var customHandler in customHandlers)
            {
                handlers = handlers.SetItem(customHandler.Key, customHandler.Value);
            }
        }

        return handlers;
    }

    private static IImmutableDictionary<string, IReadHandler> Handlers(
        IImmutableDictionary<string, IReadHandler>? customHandlers)
    {
        return HandlerMap(customHandlers);
    }

    private static IDefaultReadHandler<object> DefaultHandler(IDefaultReadHandler<object>? customDefaultHandler)
    {
        return customDefaultHandler != null ? customDefaultHandler : DefaultDefaultHandler();
    }

    /// <summary>
    /// Gets the JSON instance.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="customHandlers">The custom handlers.</param>
    /// <param name="customDefaultHandler">The custom default handler.</param>
    /// <returns>A reader.</returns>
    public static IReader GetJsonInstance(Stream input,
        IImmutableDictionary<string, IReadHandler> customHandlers,
        IDefaultReadHandler<object>? customDefaultHandler)
    {
        return new JsonReader(input, Handlers(customHandlers), DefaultHandler(customDefaultHandler));
    }

    /// <summary>
    /// Gets the MessagePack instance.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="customHandlers">The custom handlers.</param>
    /// <param name="customDefaultHandler">The custom default handler.</param>
    /// <returns>A reader.</returns>
    public static IReader GetMsgPackInstance(Stream input,
        IImmutableDictionary<string, IReadHandler> customHandlers,
        IDefaultReadHandler<object>? customDefaultHandler)
    {
        return new MessagePackReader(input, Handlers(customHandlers), DefaultHandler(customDefaultHandler));
    }

    private class DefaultReadHandler : IDefaultReadHandler<ITaggedValue>
    {
        public ITaggedValue? FromRepresentation(string tag, object? representation)
        {
            return TransitFactory.TaggedValue(tag, representation);
        }
    }

    private abstract class Reader(
        Stream input,
        IImmutableDictionary<string, IReadHandler> handlers,
        IDefaultReadHandler<object> defaultHandler) : IReader, IReaderSpi
    {
        protected Stream Input { get; } = input;
        protected IImmutableDictionary<string, IReadHandler> Handlers { get; } = handlers;
        protected IDefaultReadHandler<object> DefaultHandler { get; } = defaultHandler;
        protected IDictionaryReader? dictionaryBuilder;
        protected IListReader? listBuilder;
        private ReadCache cache = new();
        private IParser? parser;
        private bool initialized;

        public T? Read<T>()
        {
            if (!initialized)
            {
                Initialize();
            }

            return (T?)parser!.Parse(cache.Init());
        }

        public IReader SetBuilders(IDictionaryReader? dictionaryBuilder, IListReader? listBuilder)
        {
            if (initialized)
            {
                throw new TransitException("Cannot set builders after read has been called.");
            }

            this.dictionaryBuilder = dictionaryBuilder;
            this.listBuilder = listBuilder;
            return this;
        }

        private void EnsureBuilders()
        {
            if (dictionaryBuilder == null)
            {
                dictionaryBuilder = new MapBuilderImpl();
            }

            if (listBuilder == null)
            {
                listBuilder = new ListBuilderImpl();
            }
        }

        protected void Initialize()
        {
            EnsureBuilders();
            parser = CreateParser();
            initialized = true;
        }

        protected abstract IParser CreateParser();
    }

    private sealed class JsonReader(
        Stream input,
        IImmutableDictionary<string, IReadHandler> handlers,
        IDefaultReadHandler<object> defaultHandler) : Reader(input, handlers, defaultHandler)
    {
        protected override IParser CreateParser()
        {
            var streamReader = new StreamReader(Input);
            var jsonTextReader = new JsonTextReader(streamReader);
            jsonTextReader.SupportMultipleContent = true;
            return new JsonParser(jsonTextReader, Handlers, DefaultHandler,
                dictionaryBuilder!, listBuilder!);
        }
    }

    private sealed class MessagePackReader(
        Stream input,
        IImmutableDictionary<string, IReadHandler> handlers,
        IDefaultReadHandler<object> defaultHandler) : Reader(input, handlers, defaultHandler)
    {
        protected override IParser CreateParser()
        {
            return new MessagePackParser(Input, Handlers, DefaultHandler, dictionaryBuilder!, listBuilder!);
        }
    }
}
