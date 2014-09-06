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

using Newtonsoft.Json;
using NForza.Transit;
using NForza.Transit.Impl.WriteHandlers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace NForza.Transit.Impl
{
    /// <summary>
    /// Implements a writer factory.
    /// </summary>
    internal class WriterFactory
    {
        /// <summary>
        /// Get the default handlers.
        /// </summary>
        /// <returns>The default handlers.</returns>
        public static IImmutableList<IWriteHandler> DefaultHandlers()
        {
            var builder = ImmutableList.Create<IWriteHandler>().ToBuilder();

            builder.Add(new NullWriteHandler());

            // First single types
            var integerHandler = new IntegerWriteHandler();

            builder.Add(new KeywordWriteHandler());
            builder.Add(new QuoteWriteHandler());
            builder.Add(new StringWriteHandler());
            builder.Add(integerHandler);
            builder.Add(new BigIntegerWriteHandler());
            builder.Add(new SingleWriteHandler());
            builder.Add(new DoubleWriteHandler());
            builder.Add(new TaggedValueWriteHandler());
            builder.Add(new BigIntegerWriteHandler());


            // Second enumerables (since string is both single and enumerable of char)
            builder.Add(new DictionaryWriteHandler());
            builder.Add(new ListWriteHandler());

            return builder.ToImmutable();
        }

        private static IImmutableList<IWriteHandler> Handlers(IEnumerable<IWriteHandler> customHandlers) 
        {
            IImmutableList<IWriteHandler> handlers = DefaultHandlers();

            if (customHandlers != null)
            {
                handlers.AddRange(customHandlers);
            }

            return handlers;
        }

        private static void SetSubHandler(IImmutableList<IWriteHandler> handlers, AbstractEmitter abstractEmitter) 
        {
            foreach (IWriteHandler handler in handlers)
        	{
		        if (handler is IAbstractEmitterAware)
                {
                    ((IAbstractEmitterAware)handler).SetEmitter(abstractEmitter);
                }
        	}
        }

        private static IImmutableList<IWriteHandler> GetVerboseHandlers(IEnumerable<IWriteHandler> handlers) 
        {
            var verboseHandlersBuilder = ImmutableList.Create<IWriteHandler>().ToBuilder();

            foreach (var item in handlers)
	        {
                verboseHandlersBuilder.Add(item.GetVerboseHandler() ?? item);
	        }

            return verboseHandlersBuilder.ToImmutable();
        }

        public static IWriter<T> GetJsonInstance<T>(Stream output, IEnumerable<IWriteHandler> customHandlers, bool verboseMode)
        {
            TextWriter textWriter = new StreamWriter(output);
            JsonWriter jsonWriter = new JsonTextWriter(textWriter);
            IImmutableList<IWriteHandler> handlers = Handlers(customHandlers);
            JsonEmitter emitter;
            if (verboseMode) 
            {
                emitter = new JsonVerboseEmitter(jsonWriter, GetVerboseHandlers(handlers));
            } 
            else 
            {
                emitter = new JsonEmitter(jsonWriter, handlers);
            }

            SetSubHandler(handlers, emitter);
            WriteCache wc = new WriteCache(!verboseMode);

            return new Writer<T>(output, emitter, wc);
        }

        private class Writer<T> : IWriter<T>
        {
            private Stream output; 
            private JsonEmitter emitter;
            private WriteCache wc;

            public Writer (Stream output, JsonEmitter emitter, WriteCache wc)
	        {
                this.output = output;
                this.emitter = emitter;
                this.wc = wc;
	        }

            public void Write(T value)
            {
                emitter.Emit(value, false, wc.Init());
                output.Flush();
            }
        }


    }
}
