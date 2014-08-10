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

namespace NForza.Transit.Impl
{
    public class ReadCache
    {
        private object[] cache;
        private int index;

        public ReadCache()
        {
            cache = new object[WriteCache.MaxCacheEntries];
            index = 0;
        }

        private bool CacheCode(string s)
        {
            return (s[0] == Constants.Sub) && (!s.Equals(Constants.DirectoryAsList));
        }

        private int CodeToIndex(string s)
        {
            int sz = s.Length;
            if (sz == 2)
            {
                return ((int)s[1] - WriteCache.BaseCharIdx);
            }
            else
            {
                return (((int)s[1] - WriteCache.BaseCharIdx) * WriteCache.CacheCodeDigits) +
                        ((int)s[2] - WriteCache.BaseCharIdx);
            }
        }

        public object CacheRead(string s, bool asDictionaryKey) 
        { 
            return CacheRead(s, asDictionaryKey, null); 
        }

        public object CacheRead(string s, bool asDictionaryKey, AbstractParser p)
        {
            if (s.Length != 0)
            {
                if (CacheCode(s))
                {
                    return cache[CodeToIndex(s)];
                }
                else if (WriteCache.IsCacheable(s, asDictionaryKey))
                {
                    if (index == WriteCache.MaxCacheEntries)
                    {
                        Init();
                    }
                    return cache[index++] = (p != null ? p.ParseString(s) : s);
                }
            }

            return p != null ? p.ParseString(s) : s;
        }

        public ReadCache Init()
        {
            //need not clear array
            index = 0;
            return this;
        }
    }
}