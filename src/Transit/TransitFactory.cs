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
using System.Collections.Immutable;
using System.IO;

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
        /// <typeparam name="T"></typeparam>
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
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Rep"></typeparam>
        /// <param name="type">The format to read in</param>
        /// <param name="input">The input stream to read from</param>
        /// <param name="customHandlers">
        /// A dictionary of custom ReadHandlers to use in addition or in place of the default ReadHandlers
        /// </param>
        /// <param name="customDefaultHandler">
        /// A DefaultReadHandler to use for processing encoded values for which there is no read handler
        /// </param>
        /// <returns>A reader</returns>
        public static IReader Reader(Format type, Stream input,
                                    IImmutableDictionary<string, IReadHandler> customHandlers,
                                    IDefaultReadHandler<object> customDefaultHandler) 
        {
            switch (type) {
                case Format.Json:
                case Format.JsonVerbose:
                    // JSON parser creation blocks on input stream until 4 bytes
                    // are available to determine character encoding - this is
                    // unexpected, so defer creation until first read
                    return new DeferredJsonReader(input, customHandlers, customDefaultHandler);
                case Format.MsgPack:
                    // TODO:
                    throw new NotImplementedException();
                    //return ReaderFactory.GetMsgPackInstance(input, customHandlers, customDefaultHandler);
                default:
                    throw new ArgumentException("Unknown Reader type: " + type.ToString());
            }
        }

        /// <summary>
        /// Converts a <see cref="string"/> or <see cref="IKeyword"/> to an <see cref="IKeyword"/>.
        /// </summary>
        /// <param name="obj">A string or a keyword</param>
        /// <returns>A keyword</returns>
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
        /// <param name="obj">A string or a symbol</param>
        /// <returns>A symbol</returns>
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
        /// <typeparam name="T">Tagged value type.</typeparam>
        /// <param name="tag">Tag string</param>
        /// <param name="representation">Value representation</param>
        /// <returns>A tagged value</returns>
        public static ITaggedValue TaggedValue(string tag, object representation) {
            return new TaggedValue(tag, representation);
        }

        /// <summary>
        /// Returns the <see cref="IDefaultReadHandler"/> that is used by default.
        /// </summary>
        /// <returns><see cref="IDefaultReadHandler"/> instance</returns>
        public static IDefaultReadHandler<ITaggedValue> DefaultDefaultReadHandler()
        {
            return ReaderFactory.DefaultDefaultHandler();
        }
    }
}
