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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Beerendonk.Transit.Impl
{
    /// <summary>
    /// Represents an abstract emitter.
    /// </summary>
    internal abstract class AbstractEmitter : IEmitter
    {
        private IImmutableDictionary<Type, IWriteHandler> handlers;

        public AbstractEmitter(IImmutableDictionary<Type, IWriteHandler> handlers)
        {
            this.handlers = handlers;
        }

        private IWriteHandler CheckBaseClasses(Type type)
        {
            Type baseType = type.GetTypeInfo().BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                IWriteHandler handler;
                if (handlers.TryGetValue(baseType, out handler))
                {
                    handlers = handlers.Add(type, handler);
                    return handler;
                }

                baseType = baseType.GetTypeInfo().BaseType;
            }

            return null;
        }

        private IWriteHandler CheckBaseTypes(Type type, IEnumerable<Type> baseTypes)
        {
            IDictionary<Type, IWriteHandler> possibles = new Dictionary<Type, IWriteHandler>();

            foreach (Type item in baseTypes)
            {
                // TODO Better way to decide the most appropriate possibility
                if (possibles.Count < 1)
                {
                    IWriteHandler h;
                    if (handlers.TryGetValue(item, out h))
                    {
                        possibles.Add(item, h);
                    }
                }
            }

            switch (possibles.Count)
            {
                case 0: return null;
                case 1:
                    {
                        IWriteHandler h = possibles.First().Value;
                        handlers = handlers.Add(type, h);
                        return h;
                    }
                default:
                    throw new TransitException("More thane one match for " + type);
            }
        }

        private IWriteHandler CheckBaseInterfaces(Type type)
        {
            IEnumerable<Type> implementedInterfaces = type.GetTypeInfo().ImplementedInterfaces;
            return CheckBaseTypes(type, implementedInterfaces);
        }

        private IWriteHandler CheckBaseGenericInterfaces(Type type)
        {
            IEnumerable<Type> genericImplementedInterfaces =
                type.GetTypeInfo().ImplementedInterfaces
                .Select(x => x.GetTypeInfo())
                .Where(x => x.IsGenericType)
                .Select(x => x.GetGenericTypeDefinition());

            return CheckBaseTypes(type, genericImplementedInterfaces);
        }

        private IWriteHandler GetHandler(object obj)
        {
            Type type = (obj != null) ? obj.GetType() : typeof(NullType);
            IWriteHandler handler = null;

            if (!handlers.TryGetValue(type, out handler))
            {
                handler = CheckBaseClasses(type);

                if (handler == null)
                {
                    handler = CheckBaseGenericInterfaces(type);

                    if (handler == null)
                    {
                        handler = CheckBaseInterfaces(type);
                    }
                }
            }

            return handler;
        }

        public string GetTag(object obj)
        {
            IWriteHandler handler = GetHandler(obj);
            if (handler == null)
            {
                return null;
            }

            return handler.Tag(obj);
        }

        protected string Escape(string s)
        {
            int l = s.Length;
            if (l > 0)
            {
                char c = s[0];
                if (c == Constants.Esc || c == Constants.Sub || c == Constants.Reserved)
                {
                    return Constants.Esc + s;
                }
            }
            return s;
        }

        protected virtual void EmitTagged(string t, object obj, bool ignored, WriteCache cache)
        {
            EmitListStart(2L);
            EmitString(Constants.EscTag, t, "", false, cache);
            Marshal(obj, false, cache);
            EmitListEnd();
        }

        protected void EmitEncoded(string t, IWriteHandler handler, object obj, bool asDictionaryKey, WriteCache cache)
        {
            if (t.Length == 1)
            {
                object r = handler.Representation(obj);
                if (r is string)
                {
                    EmitString(Constants.EscStr, t, (string)r, asDictionaryKey, cache);
                }
                else
                    if (PrefersStrings() || asDictionaryKey)
                    {
                        string sr = handler.StringRepresentation(obj);
                        if (sr != null)
                        {
                            EmitString(Constants.EscStr, t, sr, asDictionaryKey, cache);
                        }
                        else
                        {
                            throw new TransitException("Cannot be encoded as a string " + obj);
                        }
                    }
                    else
                    {
                        EmitTagged(t, r, asDictionaryKey, cache);
                    }
            }
            else
            {
                if (asDictionaryKey)
                {
                    throw new TransitException("Cannot be used as a map key " + obj);
                }
                else
                {
                    EmitTagged(t, handler.Representation(obj), asDictionaryKey, cache);
                }
            }
        }

        private void EmitDictionary(dynamic keyValuePairEnumerable, bool ignored, WriteCache cache)
        {
            var d = new Dictionary<object, object>();

            foreach (var item in keyValuePairEnumerable)
            {
                d.Add(item.Key, item.Value);
            }

            EmitDictionary(d, ignored, cache);
        }

        abstract protected void EmitDictionary(IEnumerable<KeyValuePair<object, object>> keyValuePairs, 
            bool ignored, WriteCache cache);

        protected void EmitList(object o, bool ignored, WriteCache cache)
        {
            var enumerable = o as IEnumerable;
            var length = enumerable.Cast<object>().Count();

            EmitListStart(length);

            if (o is IEnumerable<int>)
            {
                foreach (var n in (IEnumerable<int>)o)
	            {
		            EmitInteger(n, false, cache);
	            }
            }
            else if (o is IEnumerable<short>)
            {
                foreach (var n in (IEnumerable<short>)o)
	            {
		            EmitInteger(n, false, cache);
	            }
            }
            else if (o is IEnumerable<long>)
            {
                foreach (var n in (IEnumerable<long>)o)
	            {
		            EmitInteger(n, false, cache);
	            }
            }
            else if (o is IEnumerable<float>)
            {
                foreach (var n in (IEnumerable<float>)o)
	            {
		            EmitDouble(n, false, cache);
	            }
            }
            else if (o is IEnumerable<double>)
            {
                foreach (var n in (IEnumerable<double>)o)
	            {
		            EmitDouble(n, false, cache);
	            }
            }
            else if (o is IEnumerable<bool>)
            {
                foreach (var n in (IEnumerable<bool>)o)
	            {
		            EmitBoolean(n, false, cache);
	            }
            }
            else if (o is IEnumerable<char>)
            {
                foreach (var n in (IEnumerable<char>)o)
	            {
		            Marshal(n, false, cache);
	            }
            }
            else 
            {
                foreach (var n in enumerable)
	            {
		            Marshal(n, false, cache);
	            }
            }

            EmitListEnd();
        }

        protected void Marshal(object o, bool asDictionaryKey, WriteCache cache)
        {
            bool supported = false;

            IWriteHandler h = GetHandler(o);
            if (h != null) 
            { 
                string t = h.Tag(o);
                if (t != null) 
                {
                    supported = true;
                    if(t.Length == 1)
                    {
                        switch (t[0]) 
                        {
                            case '_': EmitNull(asDictionaryKey, cache); break;
                            case 's': EmitString(null, null, Escape((string)h.Representation(o)), asDictionaryKey, cache); break;
                            case '?': EmitBoolean((bool)h.Representation(o), asDictionaryKey, cache); break;
                            case 'i': EmitInteger(h.Representation(o), asDictionaryKey, cache); break;
                            case 'd': EmitDouble(h.Representation(o), asDictionaryKey, cache); break;
                            case 'b': EmitBinary(h.Representation(o), asDictionaryKey, cache); break;
                            case '\'': EmitTagged(t, h.Representation(o), false, cache); break;
                            default: EmitEncoded(t, h, o, asDictionaryKey, cache); break;
                        }
                    }
                    else 
                    {
                        if (t.Equals("array"))
                        {
                            EmitList(h.Representation(o), asDictionaryKey, cache);
                        }
                        else
                            if (t.Equals("map"))
                            {
                                EmitDictionary(h.Representation(o), asDictionaryKey, cache);
                            }
                            else
                            {
                                EmitEncoded(t, h, o, asDictionaryKey, cache);
                            }
                    }
                    FlushWriter();
                }
            }

            if (!supported)
            {
                throw new NotSupportedException("Not supported: " + o.GetType());
            }
        }

        protected void MarshalTop(object obj, WriteCache cache)
        {
            IWriteHandler handler = GetHandler(obj);
            if (handler == null) 
            {
                throw new NotSupportedException(
                    string.Format("Cannot marshal type {0} ({1})", obj != null ? obj.GetType() : null, obj));
            }

            string tag = handler.Tag(obj);
            if (tag == null) 
            {
                throw new NotSupportedException(
                    string.Format("Cannot marshal type {0} ({1})", obj != null ? obj.GetType() : null, obj));
            }

            if (tag.Length == 1)
                obj = new Quote(obj);

            Marshal(obj, false, cache);
        }

        public abstract void Emit(object obj, bool asDictionaryKey, WriteCache cache);

        public abstract void EmitNull(bool asDictionaryKey, WriteCache cache);

        public abstract void EmitString(string prefix, string tag, string s, bool asDictionaryKey, WriteCache cache);

        public abstract void EmitBoolean(bool b, bool asDictionaryKey, WriteCache cache);

        public abstract void EmitInteger(object o, bool asDictionaryKey, WriteCache cache);

        public abstract void EmitInteger(long i, bool asDictionaryKey, WriteCache cache);

        public abstract void EmitDouble(object d, bool asDictionaryKey, WriteCache cache);

        public abstract void EmitDouble(float d, bool asDictionaryKey, WriteCache cache);

        public abstract void EmitDouble(double d, bool asDictionaryKey, WriteCache cache);

        public abstract void EmitBinary(object b, bool asDictionaryKey, WriteCache cache);

        public abstract void EmitListStart(long size);

        public abstract void EmitListEnd();

        public abstract void EmitDictionaryStart(long size);

        public abstract void EmitDictionaryEnd();

        public abstract bool PrefersStrings();

        public abstract void FlushWriter();
    }
}
