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

namespace NForza.Transit.Impl.ReadHandlers
{
    public class CmapReadHandler : IListReadHandler
    {
        public IListReader ListReader()
        {
            return new ListReaderImpl();
        }

        public object FromRepresentation(object representation)
        {
            throw new NotSupportedException();
        }

        private class ListReaderImpl : IListReader
        {
            IImmutableDictionary<object, object> d = null;
            object nextKey = null;

            public object Init()
            {
                d = ImmutableDictionary.Create<Object, Object>();
                return this;
            }

            public object Add(object list, object item)
            {
                if (nextKey != null)
                {
                    d = d.Add(nextKey, item);
                    nextKey = null;
                }
                else
                {
                    nextKey = item;
                }
                return this;
            }

            public object Complete(object list)
            {
                return d;
            }
        }
    }
}
