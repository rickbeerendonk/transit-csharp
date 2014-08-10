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

using System.Collections.Immutable;

namespace NForza.Transit.Impl
{
    public class WriteCache
    {
        public const int MinSizeCacheable = 4;
        public const int CacheCodeDigits = 44;
        public const int MaxCacheEntries = CacheCodeDigits * CacheCodeDigits;
        public const int BaseCharIdx = 48;

        private IImmutableDictionary<string, string> cache = ImmutableDictionary.Create<string, string>();
        private int index;
        private bool enabled;

        public WriteCache() 
            : this(true)
        {
        }

        public WriteCache(bool enabled) 
        {
            index = 0;
            this.enabled = enabled;
        }

        public static bool IsCacheable(string s, bool asDictionaryKey) 
        {
            return (s.Length >= MinSizeCacheable) &&
                (asDictionaryKey || 
                    (s[0] == Constants.Esc &&
                    (s[1] == ':' || s[1] == '$' || s[1] == '#')));
        }

        private string IndexToCode(int index) 
        {
            int hi = index / CacheCodeDigits;
            int lo = index % CacheCodeDigits;
            if (hi == 0) 
            {
                return Constants.SubStr + (char)(lo + BaseCharIdx);
            } 
            else 
            {
                return Constants.SubStr + (char)(hi + BaseCharIdx) + (char)(lo + BaseCharIdx);
            }
        }

        public string CacheWrite(string s, bool asDictionaryKey) 
        {
            if (enabled && IsCacheable(s, asDictionaryKey)) 
            {
                string val;
                if (cache.TryGetValue(s, out val))
                {
                    return val;
                }
                else
                {
                    if (index == MaxCacheEntries)
                    {
                        Init();
                    }

                    cache = cache.SetItem(s, IndexToCode(index++));
                }
            }

            // TODO Either s or val is returned. Weird?!
            return s;
        }

	    public WriteCache Init()
        {
		    index = 0;
		    cache = cache.Clear();
		    return this;
	    }
    }
}
