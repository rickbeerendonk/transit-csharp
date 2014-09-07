// Copyright © 2014 NForza. All Rights Reserved.
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

using NForza.Transit.Impl;
using NForza.Transit.Spi;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Numerics;

namespace NForza.Transit
{
    /// <summary>
    /// Main entry point for using transit-java library. Provides methods to construct
    /// readers and writers, as well as helpers to make various other values.
    /// </summary>
    public class TransitFactory
    {
        /// <summary>
        /// Transit formats.
        /// </summary>
        public enum Format 
        { 
            /// <summary>
            /// JSON
            /// </summary>
            Json, 

            /// <summary>
            /// MessagePack
            /// </summary>
            MsgPack, 

            /// <summary>
            /// JSON Verbose
            /// </summary>
            JsonVerbose 
        }

        /// <summary>
        /// Creates a writer instance.
        /// </summary>
        /// <param name="type">The format to write in.</param>
        /// <param name="output">The output stream to write to.</param>
        /// <returns>A writer.</returns>
        public static IWriter<T> Writer<T>(Format type, Stream output)
        {
            return Writer<T>(type, output, null);
        }
        
        /// <summary>
        /// Creates a writer instance.
        /// </summary>
        /// <param name="type">The format to write in.</param>
        /// <param name="output">The output stream to write to.</param>
        /// <param name="customHandlers">Additional IWriteHandlers to use in addition 
        /// to or in place of the default IWriteHandlers.</param>
        /// <returns>A writer</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <exception cref="System.ArgumentException">Unknown Writer type:  + type.ToString()</exception>
        public static IWriter<T> Writer<T>(Format type, Stream output, IDictionary<Type, IWriteHandler> customHandlers)
        {
            switch (type) {
                case Format.MsgPack:
                    return WriterFactory.GetMsgPackInstance<T>(output, customHandlers);
                case Format.Json:
                    return WriterFactory.GetJsonInstance<T>(output, customHandlers, false);
                case Format.JsonVerbose:
                    return WriterFactory.GetJsonInstance<T>(output, customHandlers, true);
                default:
                    throw new ArgumentException("Unknown Writer type: " + type.ToString());
            }
        }

        /// <summary>
        /// Creates a reader instance.
        /// </summary>
        /// <param name="type">The format to read in.</param>
        /// <param name="input">The stream to read from.</param>
        /// <returns>A reader</returns>
        public static IReader Reader(Format type, Stream input) 
        {
            return Reader(type, input, DefaultDefaultReadHandler());
        }

        /// <summary>
        /// Creates a reader instance.
        /// </summary>
        /// <param name="type">The format to read in.</param>
        /// <param name="input">The input stream to read from.</param>
        /// <param name="customDefaultHandler">
        /// A DefaultReadHandler to use for processing encoded values for which there is no read handler
        /// </param>
        /// <returns>A reader</returns>
        public static IReader Reader(Format type, Stream input, IDefaultReadHandler<object> customDefaultHandler)
        {
            return Reader(type, input, null, customDefaultHandler);
        }

        private class DeferredJsonReader : IReader, IReaderSpi {
            private Stream input;
            private IImmutableDictionary<string, IReadHandler> customHandlers;
            private IDefaultReadHandler<object> customDefaultHandler;
            private IReader reader;
            private IDictionaryReader dictionaryBuilder;
            private IListReader listBuilder;

            public DeferredJsonReader(Stream input, IImmutableDictionary<string, IReadHandler> customHandlers, IDefaultReadHandler<object> customDefaultHandler)
            {
                this.input = input;
                this.customHandlers = customHandlers;
                this.customDefaultHandler = customDefaultHandler;
            }

            public T Read<T>() 
            {
                if (reader == null) 
                {
                    reader = ReaderFactory.GetJsonInstance(input, customHandlers, customDefaultHandler);
                    if ((dictionaryBuilder != null) || (listBuilder != null)) 
                    {
                        ((IReaderSpi)reader).SetBuilders(dictionaryBuilder, listBuilder);
                    }
                }
                return reader.Read<T>();
            }

            public IReader SetBuilders(
                IDictionaryReader dictionaryBuilder,
                IListReader listBuilder) 
            {
                this.dictionaryBuilder = dictionaryBuilder;
                this.listBuilder = listBuilder;
                return this;
            }
        }

        /// <summary>
        /// Creates a reader instance.
        /// </summary>
        /// <param name="type">The format to read in.</param>
        /// <param name="input">The input stream to read from.</param>
        /// <param name="customHandlers">
        /// A dictionary of custom ReadHandlers to use in addition or in place of the default ReadHandlers.
        /// </param>
        /// <param name="customDefaultHandler">
        /// A DefaultReadHandler to use for processing encoded values for which there is no read handler.
        /// </param>
        /// <returns>A reader.</returns>
        public static IReader Reader(Format type, Stream input,
                                    IImmutableDictionary<string, IReadHandler> customHandlers,
                                    IDefaultReadHandler<object> customDefaultHandler) 
        {
            switch (type) {
                case Format.Json:
                case Format.JsonVerbose:
                    // TODO: Check if this is true in C# too.
                    // JSON parser creation blocks on input stream until 4 bytes
                    // are available to determine character encoding - this is
                    // unexpected, so defer creation until first read
                    return new DeferredJsonReader(input, customHandlers, customDefaultHandler);
                case Format.MsgPack:
                    return ReaderFactory.GetMsgPackInstance(input, customHandlers, customDefaultHandler);
                default:
                    throw new ArgumentException("Unknown Reader type: " + type.ToString());
            }
        }

        /// <summary>
        /// Converts a <see cref="string"/> or <see cref="IKeyword"/> to an <see cref="IKeyword"/>.
        /// </summary>
        /// <param name="obj">A string or a keyword.</param>
        /// <returns>A keyword.</returns>
        public static IKeyword Keyword(object obj)
        {
            if (obj is IKeyword)
            {
                return (IKeyword)obj;
            }
            else
            {
                if (obj is string)
                {
                    string s = (string)obj;

                    if (s[0] == ':')
                        return new Keyword(s.Substring(1));
                    else
                        return new Keyword(s);
                }
                else
                {
                    throw new TransitException("Cannot make keyword from " + obj.GetType().ToString());
                }
            }
        }

        /// <summary>
        /// Converts a <see cref="string"/> or <see cref="ISymbol"/> to an <see cref="ISymbol"/>.
        /// </summary>
        /// <param name="obj">A string or a symbol.</param>
        /// <returns>A symbol.</returns>
        public static ISymbol Symbol(object obj)
        {
            if (obj is ISymbol)
            {
                return (ISymbol)obj;
            }
            else
            {
                if (obj is string)
                {
                    string s = (string)obj;

                    if (s[0] == ':')
                        return new Symbol(s.Substring(1));
                    else
                        return new Symbol(s);
                }
                else
                {
                    throw new TransitException("Cannot make symbol from " + obj.GetType().ToString());
                }
            }
        }
  
        /// <summary>
        /// Creates an <see cref="ITaggedValue"/>.
        /// </summary>
        /// <param name="tag">Tag string.</param>
        /// <param name="representation">Value representation.</param>
        /// <returns>A tagged value.</returns>
        public static ITaggedValue TaggedValue(string tag, object representation) {
            return new TaggedValue(tag, representation);
        }

        /// <summary>
        /// Creates a <see cref="ILink"/>.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="rel">The relative.</param>
        /// <returns>An <see cref="ILink"/> instance.</returns>
        public static ILink Link(string href, string rel)
        {
            return Link(href, rel, null, null, null);
        }

        /// <summary>
        /// Creates a <see cref="ILink"/>.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="rel">The relative.</param>
        /// <returns>An <see cref="ILink"/> instance.</returns>
        public static ILink Link(Uri href, string rel)
        {
            return Link(href, rel, null, null, null);
        }

        /// <summary>
        /// Creates a <see cref="ILink"/>.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="rel">The rel.</param>
        /// <param name="name">The optional name.</param>
        /// <param name="prompt">The optional prompt.</param>
        /// <param name="render">The optional render.</param>
        /// <returns>An <see cref="ILink"/> instance.</returns>
        public static ILink Link(string href, string rel, string name, string prompt, string render)
        {
            return Link(new Uri(href), rel, name, prompt, render);
        }

        /// <summary>
        /// Creates a <see cref="ILink"/>.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="rel">The rel.</param>
        /// <param name="name">The optional name.</param>
        /// <param name="prompt">The optional prompt.</param>
        /// <param name="render">The optional render.</param>
        /// <returns>An <see cref="ILink"/> instance.</returns>
        public static ILink Link(Uri href, string rel, string name, string prompt, string render) 
        {
            return new Link(href, rel, name, prompt, render);
        }

        /// <summary>
        /// Creates a <see cref="IRatio"/>.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <returns>An <see cref="IRatio"/> instance.</returns>
        public IRatio Ratio(BigInteger numerator, BigInteger denominator) 
        {
            return new Ratio(numerator, denominator);
        }

        /// <summary>
        /// Returns a directory of classes to read handlers that is used by default.
        /// </summary>
        /// <returns>Tag to read handler directory.</returns>
        public static IImmutableDictionary<string, IReadHandler> DefaultReadHandlers() 
        { 
            return ReaderFactory.DefaultHandlers(); 
        }

        /// <summary>
        /// Returns a directory of classes to write handlers that is used by default.
        /// </summary>
        /// <returns>Class to write handler directory.</returns>
        public static IImmutableDictionary<Type, IWriteHandler> DefaultWriteHandlers() 
        {
            return WriterFactory.DefaultHandlers(); 
        }

        /// <summary>
        /// Returns the <see cref="IDefaultReadHandler{T}"/> of <see cref="ITaggedValue"/> that is used by default.
        /// </summary>
        /// <returns><see cref="IDefaultReadHandler{T}"/> of <see cref="ITaggedValue"/> instance.</returns>
        public static IDefaultReadHandler<ITaggedValue> DefaultDefaultReadHandler()
        {
            return ReaderFactory.DefaultDefaultHandler();
        }
    }
}
