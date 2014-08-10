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

namespace NForza.Transit.Impl
{
    public class Symbol : ISymbol, INamed, IComparable, IComparable<ISymbol>, IEquatable<ISymbol>
    {
        private const char separator = '/';

        private readonly string ns;
        private readonly string name;
        private string str;

        public Symbol(string nsname)
        {
            int i = nsname.IndexOf(separator);
            if (i == -1 || nsname.Equals(separator))
            {
                ns = null;
                name = nsname;
            }
            else
            {
                ns = nsname.Substring(0, i);
                name = nsname.Substring(i + 1);
            }
        }

        public override string ToString()
        {
            if (str == null)
            {
                if (ns != null)
                {
                    str = ns + separator + name;
                }
                else
                {
                    str = name;
                }
            }

            return str;
        }

        public string GetName()
        {
            return name;
        }

        public string GetNamespace()
        {
            return ns;
        }

        public string GetValue()
        {
            return ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            return obj is Symbol && ((Symbol)obj).GetValue().Equals(GetValue());
        }

        public override int GetHashCode()
        {
            return 19 * GetValue().GetHashCode();
        }

        public int CompareTo(ISymbol other)
        {
            return GetValue().CompareTo(((ISymbol)other).GetValue());
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is ISymbol))
            {
                throw new ArgumentException("obj must be an ISymbol.");
            }

            return CompareTo((ISymbol)obj);
        }

        public bool Equals(ISymbol other)
        {
            return CompareTo(other) == 0;
        }
    }
}
