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
using Beerendonk.Transit;
using Beerendonk.Transit.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Numerics;

namespace Beerendonk.Transit.Impl;

/// <summary>
/// Implements a writer factory.
/// </summary>
internal class WriterFactory
{
    /// <summary>
    /// Get the default handlers.
    /// </summary>
    /// <returns>The default handlers.</returns>
    public static IImmutableDictionary<Type, IWriteHandler> DefaultHandlers()
    {
        var builder = ImmutableDictionary.Create<Type, IWriteHandler>().ToBuilder();

        var integerHandler = new WriteHandlers.IntegerWriteHandler("i");
        var dictionaryHandler = new WriteHandlers.DictionaryWriteHandler();

        builder.Add(typeof(bool), new WriteHandlers.BooleanWriteHandler());
        builder.Add(typeof(NullType), new WriteHandlers.NullWriteHandler());
        builder.Add(typeof(string), new WriteHandlers.ToStringWriteHandler("s"));
        builder.Add(typeof(int), integerHandler);
        builder.Add(typeof(long), integerHandler);
        builder.Add(typeof(short), integerHandler);
        builder.Add(typeof(byte), integerHandler);
        builder.Add(typeof(BigInteger), new WriteHandlers.ToStringWriteHandler("n"));
        builder.Add(typeof(float), new WriteHandlers.FloatWriteHandler());
        builder.Add(typeof(double), new WriteHandlers.DoubleWriteHandler());
        builder.Add(typeof(decimal), new WriteHandlers.DecimalWriteHandler());
        builder.Add(typeof(BigRational), new WriteHandlers.BigRationalWriteHandler());
        builder.Add(typeof(char), new WriteHandlers.ToStringWriteHandler("c"));
        builder.Add(typeof(IKeyword), new WriteHandlers.ToStringWriteHandler(":"));
        builder.Add(typeof(ISymbol), new WriteHandlers.ToStringWriteHandler("$"));
        builder.Add(typeof(byte[]), new WriteHandlers.BinaryWriteHandler());
        builder.Add(typeof(Guid), new WriteHandlers.GuidWriteHandler());
        builder.Add(typeof(Uri), new WriteHandlers.UriWriteHandler());
        builder.Add(typeof(DateTime), new WriteHandlers.DateTimeWriteHandler());
        builder.Add(typeof(IRatio), new WriteHandlers.RatioWriteHandler());
        builder.Add(typeof(ILink), new WriteHandlers.LinkWriteHandler());
        builder.Add(typeof(Quote), new WriteHandlers.QuoteWriteHandler());
        builder.Add(typeof(ITaggedValue), new WriteHandlers.TaggedValueWriteHandler());

        builder.Add(typeof(IDictionary), dictionaryHandler);
        builder.Add(typeof(IList), new WriteHandlers.ListWriteHandler());
        builder.Add(typeof(ISet<>), new WriteHandlers.SetWriteHandler());
        builder.Add(typeof(IEnumerable), new WriteHandlers.EnumerableWriteHandler());
        builder.Add(typeof(IList<>), new WriteHandlers.ListWriteHandler());
        builder.Add(typeof(IDictionary<,>), dictionaryHandler);
        builder.Add(typeof(IReadOnlyDictionary<,>), dictionaryHandler);

        return builder.ToImmutable();
    }

    private static IImmutableDictionary<Type, IWriteHandler> Handlers(IDictionary<Type, IWriteHandler>? customHandlers)
    {
        IImmutableDictionary<Type, IWriteHandler> handlers = DefaultHandlers();

        if (customHandlers != null)
        {
            handlers = handlers.RemoveRange(customHandlers.Keys).AddRange(customHandlers);
        }

        return handlers;
    }

    public static IImmutableDictionary<Type, IWriteHandler> HandlerMap(
        IDictionary<Type, IWriteHandler>? customHandlers)
    {
        return Handlers(customHandlers);
    }

    private static void SetSubHandler(IImmutableDictionary<Type, IWriteHandler> handlers, AbstractEmitter abstractEmitter)
    {
        foreach (var handler in handlers)
        {
            if (handler.Value is IAbstractEmitterAware)
            {
                ((IAbstractEmitterAware)handler.Value).SetEmitter(abstractEmitter);
            }
        }
    }

    private static IImmutableDictionary<Type, IWriteHandler> GetVerboseHandlers(IImmutableDictionary<Type, IWriteHandler> handlers)
    {
        var verboseHandlersBuilder = ImmutableDictionary.Create<Type, IWriteHandler>().ToBuilder();

        foreach (var item in handlers)
        {
            verboseHandlersBuilder.Add(item.Key, item.Value.GetVerboseHandler() ?? item.Value);
	        }

        return verboseHandlersBuilder.ToImmutable();
    }

    public static IWriter<T> GetJsonInstance<T>(
        Stream output,
        IDictionary<Type, IWriteHandler> customHandlers,
        bool verboseMode)
    {
        return GetJsonInstance<T>(output, customHandlers, null, verboseMode, null);
    }

    public static IWriter<T> GetJsonInstance<T>(
        Stream output,
        IDictionary<Type, IWriteHandler>? customHandlers,
        IWriteHandler? defaultWriteHandler,
        bool verboseMode,
        Func<object, object>? transform)
    {
        TextWriter textWriter = new StreamWriter(output);
        JsonWriter jsonWriter = new JsonTextWriter(textWriter);
        IImmutableDictionary<Type, IWriteHandler> handlers = Handlers(customHandlers);
        AbstractEmitter emitter;
        if (verboseMode)
        {
            emitter = new JsonVerboseEmitter(jsonWriter, GetVerboseHandlers(handlers), defaultWriteHandler, transform);
        }
        else
        {
            emitter = new JsonEmitter(jsonWriter, handlers, defaultWriteHandler, transform);
        }

        SetSubHandler(handlers, emitter);
        WriteCache wc = new WriteCache(!verboseMode);

        return new Writer<T>(output, emitter, wc);
    }

    public static IWriter<T> GetMsgPackInstance<T>(
        Stream output,
        IDictionary<Type, IWriteHandler>? customHandlers)
    {
        return GetMsgPackInstance<T>(output, customHandlers, null, null);
    }

    public static IWriter<T> GetMsgPackInstance<T>(
        Stream output,
        IDictionary<Type, IWriteHandler>? customHandlers,
        IWriteHandler? defaultWriteHandler,
        Func<object, object>? transform)
    {
        IImmutableDictionary<Type, IWriteHandler> handlers = Handlers(customHandlers);
        var emitter = new MessagePackEmitter(output, handlers, defaultWriteHandler, transform);
        SetSubHandler(handlers, emitter);
        return new Writer<T>(output, emitter, new WriteCache());
    }

    public static IWriteHandler DefaultDefaultHandler()
    {
        return new DefaultWriteHandler();
    }

    private sealed class DefaultWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object obj)
        {
            throw new NotSupportedException("Not supported: " + obj);
        }

        public override object Representation(object obj)
        {
            throw new NotSupportedException("Not supported: " + obj);
        }

        public override string StringRepresentation(object obj)
        {
            throw new NotSupportedException("Not supported: " + obj);
        }
    }

    private class Writer<T> : IWriter<T>
    {
        private Stream output;
        private AbstractEmitter emitter;
        private WriteCache wc;

        public Writer (Stream output, AbstractEmitter emitter, WriteCache wc)
	        {
            this.output = output;
            this.emitter = emitter;
            this.wc = wc;
	        }

        public void Write(T? value)
        {
            emitter.Emit(value, false, wc.Init());
            output.Flush();
        }
    }
}
