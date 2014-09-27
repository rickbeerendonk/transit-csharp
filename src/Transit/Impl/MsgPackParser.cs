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

using MsgPack;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Numerics;

namespace NForza.Transit.Impl
{
    /// <summary>
    /// Represents a MessagePack parser.
    /// </summary>
    internal class MsgPackParser : AbstractParser
    {
        private readonly Unpacker mp;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsgPackParser"/> class.
        /// </summary>
        /// <param name="unpacker">The MessagePack unpacker.</param>
        /// <param name="handlers">The handlers.</param>
        /// <param name="defaultHandler">The default handler.</param>
        /// <param name="dictionaryBuilder">The dictionary builder.</param>
        /// <param name="listBuilder">The list builder.</param>
        public MsgPackParser(
            Unpacker unpacker,
            IImmutableDictionary<string, IReadHandler> handlers,
            IDefaultReadHandler<object> defaultHandler,
            IDictionaryReader dictionaryBuilder,
            IListReader listBuilder)
            : base(handlers, defaultHandler, dictionaryBuilder, listBuilder)
        {
            this.mp = unpacker;
        }

        private object ParseLong()
        {
            throw new NotImplementedException();
        }

        public override object Parse(ReadCache cache)
        {
            try
            {
                return ParseVal(false, cache);
            }
            catch (EndOfStreamException)
            {
            }

            return null;
        }

        public override object ParseVal(bool asDictionaryKey, ReadCache cache)
        {
            if (!mp.Read())
            {
                throw new EndOfStreamException();
            }
            
            if (mp.LastReadData.IsRaw)
            {
                return cache.CacheRead(mp.LastReadData.AsString().Trim('"'), asDictionaryKey, this);
            }


            throw new NotImplementedException();
        }

        public override object ParseDictionary(bool asDictionaryKey, ReadCache cache, IDictionaryReadHandler handler)
        {
            throw new NotImplementedException();
        }

        public override object ParseList(bool asDictionaryKey, ReadCache cache, IListReadHandler handler)
        {
            throw new NotImplementedException();
        }
    }
}
