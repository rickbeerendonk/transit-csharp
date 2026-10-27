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

using Beerendonk.Transit.Java;
using Beerendonk.Transit.Numerics;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Text;
using System;

namespace Beerendonk.Transit.Impl;

internal static class WriteHandlers
{

    internal sealed class BigRationalWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object obj)
        {
            return "f";
        }

        public override object Representation(object obj)
        {
            return ToDecimalString((BigRational)obj);
        }

        private static string ToDecimalString(BigRational value)
        {
            if (value.Denominator.IsZero)
            {
                throw new TransitException("Cannot encode an uninitialized BigRational as a decimal.");
            }

            if (value.Numerator.IsZero)
            {
                return "0";
            }

            BigInteger denominator = value.Denominator;
            int twos = 0;
            int fives = 0;
            while (denominator % 2 == 0)
            {
                denominator /= 2;
                twos++;
            }
            while (denominator % 5 == 0)
            {
                denominator /= 5;
                fives++;
            }

            if (denominator != BigInteger.One)
            {
                throw new TransitException("A BigRational with a non-terminating decimal expansion cannot be encoded as a Transit decimal.");
            }

            int scale = Math.Max(twos, fives);
            BigInteger unscaled = BigInteger.Abs(value.Numerator)
                * BigInteger.Pow(2, scale - twos)
                * BigInteger.Pow(5, scale - fives);
            string digits = unscaled.ToString(CultureInfo.InvariantCulture);

            if (scale > 0)
            {
                if (digits.Length <= scale)
                {
                    digits = digits.PadLeft(scale + 1, '0');
                }

                digits = digits.Insert(digits.Length - scale, ".");
                digits = digits.TrimEnd('0').TrimEnd('.');
            }

            return value.Numerator.Sign < 0 ? "-" + digits : digits;
        }
    }


    internal class BinaryWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "b";
        }

        public override object Representation(object obj)
        {
            return obj;
        }
    }


    internal class BooleanWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "?";
        }

        public override object Representation(object obj)
        {
            return (bool)obj;
        }

        public override string StringRepresentation(object obj)
        {
            return ((bool)obj).ToString();
        }
    }


    internal class DateTimeWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "m";
        }

        public override object Representation(object obj)
        {
            return Java.Convert.ToJavaTime((DateTime)obj);
        }

        public override string StringRepresentation(object obj)
        {
            return Representation(obj).ToString();
        }

        public override IWriteHandler GetVerboseHandler()
        {
            return new VerboseDateTimeWriteHandler();
        }

        private class VerboseDateTimeWriteHandler : IWriteHandler
        {
            public string Tag(object ignored)
            {
                return "t";
            }

            public object Representation(object obj)
            {
                return AbstractParser.FormatDateTime((DateTime)obj);
            }

            public string StringRepresentation(object obj)
            {
                return Representation(obj).ToString();
            }

            public IWriteHandler GetVerboseHandler()
            {
                return this;
            }
        }
    }


    internal sealed class DecimalWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object obj)
        {
            return "f";
        }

        public override object Representation(object obj)
        {
            return ((decimal)obj).ToString(CultureInfo.InvariantCulture);
        }
    }


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
                var entries = new List<object>();
                foreach (var entry in (IEnumerable)obj)
                {
                    entries.Add(entry);
                }

                return entries;
            }
            else
            {
                var l = new List<object>();

                foreach (var item in o)
                {
                    l.Add(item.Key);
                    l.Add(item.Value);
                }

                return TransitFactory.TaggedValue("array", l);
            }
        }
    }


    internal class DoubleWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object obj)
        {
            double d = (double)obj;

            if (double.IsNaN(d) || double.IsInfinity(d))
            {
                return "z";
            }

            return "d";
        }

        public override object Representation(object obj)
        {
            double d = (double)obj;

            if (double.IsNaN(d))
            {
                return "NaN";
            }

            if (double.IsPositiveInfinity(d))
            {
                return "INF";
            }

            if (double.IsNegativeInfinity(d))
            {
                return "-INF";
            }

            return d;
        }

        public override string StringRepresentation(object obj)
        {
            return Representation(obj).ToString();
        }
    }


    internal class EnumerableWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "list";
        }

        public override object Representation(object obj)
        {
            return TransitFactory.TaggedValue("array", obj);
        }
    }


    internal class FloatWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object obj)
        {
            float f = (float)obj;

            if (float.IsNaN(f) || float.IsInfinity(f))
            {
                return "z";
            }

            return "d";
        }

        public override object Representation(object obj)
        {
            float f = (float)obj;

            if (float.IsNaN(f))
            {
                return "NaN";
            }

            if (float.IsPositiveInfinity(f))
            {
                return "INF";
            }

            if (float.IsNegativeInfinity(f))
            {
                return "-INF";
            }

            return f;
        }

        public override string StringRepresentation(object obj)
        {
            return Representation(obj).ToString();
        }
    }


    internal class GuidWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "u";
        }

        public override object Representation(object obj)
        {
            Uuid uuid = (Uuid)(Guid)obj;
            long[] l = new long[2];
            l[0] = uuid.MostSignificantBits;
            l[1] = uuid.LeastSignificantBits;

            return l;
        }

        public override string StringRepresentation(object obj)
        {
            return obj.ToString();
        }
    }


    internal class IntegerWriteHandler(string t) : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return t;
        }

        public override object Representation(object obj)
        {
            return obj;
        }

        public override string StringRepresentation(object obj)
        {
            return obj.ToString();
        }
    }


    internal class LinkWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "link";
        }

        public override object Representation(object obj)
        {
            return ((ILink)obj).ToDictionary();
        }
    }


    internal class ListWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "array";
        }

        public override object Representation(object obj)
        {
            return obj;
        }
    }


    internal class NullWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "_";
        }

        public override object Representation(object ignored)
        {
            return null;
        }

        public override string StringRepresentation(object ignored)
        {
            return string.Empty;
        }
    }


    internal class QuoteWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "'";
        }

        public override object Representation(object obj)
        {
            return ((Quote)obj).Obj;
        }

        public override string StringRepresentation(object ignored)
        {
            throw new TransitException();
        }
    }


    internal class RatioWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "ratio";
        }

        public override object Representation(object obj)
        {
            IRatio r = (IRatio)obj;
            BigInteger[] l = new BigInteger[2];
            l[0] = r.Numerator;
            l[1] = r.Denominator;

            return l;
        }
    }


    internal class SetWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "set";
        }

        public override object Representation(object obj)
        {
            return TransitFactory.TaggedValue("array", obj);
        }
    }


    internal class TaggedValueWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object obj)
        {
            return ((ITaggedValue)obj).Tag;
        }

        public override object Representation(object obj)
        {
            return ((ITaggedValue)obj).Representation;
        }
    }


    internal class ToStringWriteHandler(string t) : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return t;
        }

        public override object Representation(object obj)
        {
            return obj.ToString();
        }

        public override string StringRepresentation(object obj)
        {
            return (string)Representation(obj);
        }
    }


    internal sealed class UriWriteHandler : AbstractWriteHandler
    {
        public override string Tag(object ignored)
        {
            return "r";
        }

        public override object Representation(object obj)
        {
            return ((Uri)obj).OriginalString;
        }

        public override string StringRepresentation(object obj)
        {
            return ((Uri)obj).OriginalString;
        }
    }
}
