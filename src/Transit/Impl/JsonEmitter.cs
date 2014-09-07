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
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace NForza.Transit.Impl
{
    /// <summary>
    /// Represents a JSON emitter.
    /// </summary>
    internal class JsonEmitter : AbstractEmitter
    {
        private static readonly long JsonIntMax = (long)Math.Pow(2, 53);
        private static readonly long JsonIntMin = -JsonIntMax;

        protected readonly JsonWriter jsonWriter;

        public JsonEmitter(JsonWriter jsonWriter, IImmutableDictionary<Type, IWriteHandler> handlers)
            : base(handlers)
        {
            this.jsonWriter = jsonWriter;
        }

        public override void Emit(object obj, bool asDictionaryKey, WriteCache cache)
        {
            MarshalTop(obj, cache);
        }

        public override void EmitNull(bool asDictionaryKey, WriteCache cache)
        {
            if (asDictionaryKey)
            {
                EmitString(Constants.EscStr, "_", "", asDictionaryKey, cache);
            }
            else
            {
                jsonWriter.WriteNull();
            }
        }

        public override void EmitString(string prefix, string tag, string s, bool asDictionaryKey, WriteCache cache)
        {
            string outString = cache.CacheWrite(Util.MaybePrefix(prefix, tag, s), asDictionaryKey);
            jsonWriter.WriteValue(outString);
        }

        public override void EmitBoolean(bool b, bool asDictionaryKey, WriteCache cache)
        {
            if (asDictionaryKey)
            {
                EmitString(Constants.EscStr, "?", b ? "t" : "f", asDictionaryKey, cache);
            }
            else
            {
                jsonWriter.WriteValue(b);
            }
        }

        public override void EmitInteger(object i, bool asDictionaryKey, WriteCache cache)
        {
 	        EmitInteger(Util.NumberToPrimitiveLong(i), asDictionaryKey, cache);
        }

        public override void EmitInteger(long i, bool asDictionaryKey, WriteCache cache)
        {
            if (asDictionaryKey || i > JsonIntMax || i < JsonIntMin)
                EmitString(Constants.EscStr, "i", i.ToString(), asDictionaryKey, cache);
            else
                jsonWriter.WriteValue(i);
        }

        public override void EmitDouble(object d, bool asDictionaryKey, WriteCache cache)
        {
            if (d is double)
                EmitDouble((double)d, asDictionaryKey, cache);
            else if (d is float)
                EmitDouble((float)d, asDictionaryKey, cache);
            else
                throw new TransitException("Unknown double type: " + d.GetType());
        }

        public override void EmitDouble(float d, bool asDictionaryKey, WriteCache cache)
        {
            if (asDictionaryKey)
                EmitString(Constants.EscStr, "d", d.ToString(), asDictionaryKey, cache);
            else
                jsonWriter.WriteValue(d);
        }

        public override void EmitDouble(double d, bool asDictionaryKey, WriteCache cache)
        {
            if (asDictionaryKey)
                EmitString(Constants.EscStr, "d", d.ToString(), asDictionaryKey, cache);
            else
                jsonWriter.WriteValue(d);
        }

        public override void EmitBinary(object b, bool asDictionaryKey, WriteCache cache)
        {
            EmitString(Constants.EscStr, "b", Convert.ToBase64String((byte[])b), asDictionaryKey, cache);
        }

        public override void EmitListStart(long size)
        {
            jsonWriter.WriteStartArray();
        }

        public override void EmitListEnd()
        {
            jsonWriter.WriteEndArray();
        }

        public override void EmitDictionaryStart(long size)
        {
            jsonWriter.WriteStartObject();
        }

        public override void EmitDictionaryEnd()
        {
            jsonWriter.WriteEndObject();
        }

        public override void FlushWriter()
        {
            jsonWriter.Flush();
        }

        public override bool PrefersStrings()
        {
            return true;
        }

        protected override void EmitDictionary(IEnumerable<KeyValuePair<object, object>> keyValuePairs, 
            bool ignored, WriteCache cache)
        {
            long sz = Enumerable.Count(keyValuePairs);

            EmitListStart(sz);
            EmitString(null, null, Constants.DirectoryAsList, false, cache);

            foreach (var kvp in keyValuePairs)
        	{
                Marshal(kvp.Key, true, cache);
                Marshal(kvp.Value, false, cache);
            }

            EmitListEnd();
        }
    }
}
