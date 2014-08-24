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

using System.Text;
namespace NForza.Transit.Impl
{
    internal static class Util
    {
        public static long NumberToPrimitiveLong(object o)
        {
            long i;

            if (o is long)
                i = (long)o;
            else if (o is int)
                i = (int)o;
            else if (o is short)
                i = (short)o;
            else if (o is byte)
                i = (byte)o;
            else
                throw new TransitException("Unknown integer type: " + o.GetType());

            return i;
        }

	    public static string MaybePrefix(string prefix, string tag, string s)
        {
            if (prefix == null && tag == null)
            {
                return s;
            }

		    prefix = (prefix == null) ? "" : prefix;
		    tag = (tag == null) ? "" : tag;
		    StringBuilder sb = new StringBuilder(prefix.Length + tag.Length + s.Length);

		    return sb.Append(prefix).Append(tag).Append(s).ToString();
	    }

        /*
	    public static long arraySize(Object a) {
	        if(a instanceof Collection)
	            return ((Collection)a).size();
	        else if (a.getClass().isArray())
	            return Array.getLength(a);
	        else if (a instanceof Iterable) {
	            int i = 0;
	            for (Object o : (Iterable) a) {
	                i++;
	            }
	            return i;
	        } else if (a instanceof List) {
                return ((List)a).size();
            }
	        else
	            throw new UnsupportedOperationException("arraySize not supported on this type " + a.getClass().getSimpleName());

	    }

	    public static long mapSize(Object m) {
	        if(m instanceof Collection)
	            return ((Collection) m).size();
            else if (m instanceof Map)
                return ((Map)m).size();
	        else
	            throw new UnsupportedOperationException("mapSize not supported on this type " + m.getClass().getSimpleName());
	    }
        */
    }
}
