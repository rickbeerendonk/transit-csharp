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

using System.Collections.Generic;
using System.Linq;

namespace NForza.Transit.Impl.WriteHandlers
{
    internal class DictionaryWriteHandler : AbstractWriteHandler, IAbstractEmitterAware
    {
        private AbstractEmitter abstractEmitter;
        
        public void SetEmitter(AbstractEmitter abstractEmitter)
        {
            this.abstractEmitter = abstractEmitter;
        }

        private bool StringableKeys(dynamic d)
        {
            foreach (var key in d.Keys)
	        {
                string tag = abstractEmitter.GetTag(key);

                if (tag != null && tag.Length > 1)
                {
                    return false;
                }
                else if (tag == null && !(key is string)) 
                {
                    return false;
                }
	        }
            
            return true;
        }

        public override string Tag(object obj)
        {
            if (StringableKeys(obj))
            {
                return "map";
            }
            else
            {
                return "cmap";
            }
        }

        public override object Representation(object obj)
        {
            dynamic o = obj;

            if (StringableKeys(o))
            {
                return Enumerable.ToList(o);
            }
            else
            {
                IList<object> l = new List<object>();

                foreach (var item in o)
                {
                    l.Add(item.Key);
                    l.Add(item.Value);
                }

                return TransitFactory.TaggedValue("array", l);
            }

        }
    }
}
