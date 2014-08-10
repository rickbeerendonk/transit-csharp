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

using System;
using System.Collections.Immutable;

namespace NForza.Transit.Impl
{
    public abstract class AbstractParser : IParser
    {
        public static string FormatDateTime(DateTime value)
        {
            const string dateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
            return new DateTimeOffset(value).UtcDateTime.ToString(dateTimeFormat);
        }

        protected readonly IImmutableDictionary<string, IReadHandler> handlers;
        private readonly IDefaultReadHandler<object> defaultHandler;
        protected IDictionaryReader dictionaryBuilder;
        protected IListReader listBuilder;

        protected AbstractParser(IImmutableDictionary<string, IReadHandler> handlers,
            IDefaultReadHandler<object> defaultHandler,
            IDictionaryReader dictionaryBuilder,
            IListReader listBuilder)
        {
            this.handlers = handlers;
            this.defaultHandler = defaultHandler;
            this.dictionaryBuilder = (IDictionaryReader)dictionaryBuilder;
            this.listBuilder = (IListReader)listBuilder;
        }

        protected bool TryGetHandler(string tag, out IReadHandler readHandler)
        {
            return handlers.TryGetValue(tag, out readHandler);
        }

        protected object Decode(string tag, object representation)
        {
            IReadHandler d;
            if (TryGetHandler(tag, out d))
            {
                return d.FromRepresentation(representation);
            }
            else
            {
                if (defaultHandler != null)
                {
                    return defaultHandler.FromRepresentation(tag, representation);
                }
                else
                {
                    throw new TransitException("Cannot FromRepresentation " + tag + ": " + representation.ToString());
                }
            }
        }

        protected internal object ParseString(object o)
        {
            if (o is string)
            {
                string s = (string)o;
                if (s.Length > 1)
                {
                    switch (s[0])
                    {
                        case Constants.Esc:
                            switch (s[1])
                            {
                                case Constants.Esc:
                                case Constants.Sub:
                                case Constants.Reserved:
                                    return s.Substring(1);
                                case Constants.Tag:
                                    return new Tag(s.Substring(2));
                                default:
                                    {
                                        string tag = s.Substring(1, 1);
                                        string representation = s.Length > 2 ? s.Substring(2) : string.Empty;
                                        return Decode(tag, representation);
                                    }
                            }
                        case Constants.Sub:
                            if (s[1] == ' ')
                            {
                                return Constants.DirectoryAsList;
                            }
                            break;
                    }
                }
            }

            return o;
        }

        public abstract object Parse(ReadCache cache);

        public abstract object ParseVal(bool asDictionaryKey, ReadCache cache);

        public abstract object ParseDictionary(bool asDictionaryKey, ReadCache cache, IDictionaryReadHandler handler);

        public abstract object ParseList(bool asDictionaryKey, ReadCache cache, IListReadHandler handler);
    }
}
