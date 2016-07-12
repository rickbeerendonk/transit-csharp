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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Beerendonk.Transit.Impl;
using Beerendonk.Transit.Java;
using Beerendonk.Transit.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Beerendonk.Transit.Tests
{
    [TestClass]
    public class TransitTest
    {
        #region Reading

        public IReader Reader(string s)
        {
            Stream input = new MemoryStream(Encoding.Default.GetBytes(s));
            return TransitFactory.Reader(TransitFactory.Format.Json, input);
        }

        [TestMethod]
        public void TestReadString()
        {
            Assert.AreEqual("foo", Reader("\"foo\"").Read<string>());
            Assert.AreEqual("~foo", Reader("\"~~foo\"").Read<string>());
            Assert.AreEqual("`foo", Reader("\"~`foo\"").Read<string>());
            Assert.AreEqual("foo", Reader("\"~#foo\"").Read<Tag>().GetValue());
            Assert.AreEqual("^foo", Reader("\"~^foo\"").Read<string>());
        }

        [TestMethod]
        public void TestReadBoolean()
        {
            Assert.IsTrue(Reader("\"~?t\"").Read<bool>());
            Assert.IsFalse(Reader("\"~?f\"").Read<bool>());

            IDictionary d = Reader("{\"~?t\":1,\"~?f\":2}").Read<IDictionary>();
            Assert.AreEqual(1L, d[true]);
            Assert.AreEqual(2L, d[false]);
        }

        [TestMethod]
        public void TestReadNull()
        {
            IReader r = Reader("\"~_\"");
            object v = r.Read<object>();
            Assert.IsNull(v);
        }

        [TestMethod]
        public void TestReadKeyword()
        {
            IKeyword v = Reader("\"~:foo\"").Read<IKeyword>();
            Assert.AreEqual("foo", v.ToString());

            IReader r = Reader("[\"~:foo\",\"^" + (char)WriteCache.BaseCharIdx + "\",\"^" + (char)WriteCache.BaseCharIdx + "\"]");
            IList v2 = r.Read<IList>();
            Assert.AreEqual("foo", v2[0].ToString());
            Assert.AreEqual("foo", v2[1].ToString());
            Assert.AreEqual("foo", v2[2].ToString());
        }

        [TestMethod]
        public void TestReadInteger()
        {
            IReader r = Reader("\"~i42\"");
            long v = r.Read<long>();
            Assert.AreEqual<long>(42L, v);
        }

        [TestMethod]
        public void TestReadBigInteger()
        {
            BigInteger expected = BigInteger.Parse("4256768765123454321897654321234567");
            IReader r = Reader("\"~n4256768765123454321897654321234567\"");
            BigInteger v = r.Read<BigInteger>();
            Assert.AreEqual<BigInteger>(expected, v);
        }

        [TestMethod]
        public void TestReadDouble()
        {
            Assert.AreEqual<double>(42.5D, Reader("\"~d42.5\"").Read<double>());
        }

        [TestMethod]
        public void TestReadSpecialNumbers()
        {
            Assert.AreEqual<double>(double.NaN, Reader("\"~zNaN\"").Read<double>());
            Assert.AreEqual<double>(double.PositiveInfinity, Reader("\"~zINF\"").Read<double>());
            Assert.AreEqual<double>(double.NegativeInfinity, Reader("\"~z-INF\"").Read<double>());
        }

        [TestMethod]
        public void TestReadBigRational()
        {
            Assert.AreEqual(new BigRational(12.345M), Reader("\"~f12.345\"").Read<BigRational>());
            Assert.AreEqual(new BigRational(-12.345M), Reader("\"~f-12.345\"").Read<BigRational>());
            Assert.AreEqual(new BigRational(0.1001M), Reader("\"~f0.1001\"").Read<BigRational>());
            Assert.AreEqual(new BigRational(0.01M), Reader("\"~f0.01\"").Read<BigRational>());
            Assert.AreEqual(new BigRational(0.1M), Reader("\"~f0.1\"").Read<BigRational>());
            Assert.AreEqual(new BigRational(1M), Reader("\"~f1\"").Read<BigRational>());
            Assert.AreEqual(new BigRational(10M), Reader("\"~f10\"").Read<BigRational>());
            Assert.AreEqual(new BigRational(420.0057M), Reader("\"~f420.0057\"").Read<BigRational>());
        }

        [TestMethod]
        public void TestReadDateTime()
        {
            var d = new DateTime(2014, 8, 9, 10, 6, 21, 497, DateTimeKind.Local);
            var expected = new DateTimeOffset(d).LocalDateTime;
            long javaTime = Beerendonk.Transit.Java.Convert.ToJavaTime(d);

            string timeString = JsonParser.FormatDateTime(d);
            Assert.AreEqual(expected, Reader("\"~t" + timeString + "\"").Read<DateTime>());

            Assert.AreEqual(expected, Reader("{\"~#m\": " + javaTime + "}").Read<DateTime>());

            timeString = new DateTimeOffset(d).UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
            Assert.AreEqual(expected, Reader("\"~t" + timeString + "\"").Read<DateTime>());

            timeString = new DateTimeOffset(d).UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            Assert.AreEqual(expected.AddMilliseconds(-497D), Reader("\"~t" + timeString + "\"").Read<DateTime>());

            timeString = new DateTimeOffset(d).UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff-00:00");
            Assert.AreEqual(expected, Reader("\"~t" + timeString + "\"").Read<DateTime>());
        }

        [TestMethod]
        public void TestReadGuid()
        {
            Guid guid = Guid.NewGuid();
            long hi64 = ((Uuid)guid).MostSignificantBits;
            long lo64 = ((Uuid)guid).LeastSignificantBits;

            Assert.AreEqual(guid, Reader("\"~u" + guid.ToString() + "\"").Read<Guid>());
            Assert.AreEqual(guid, Reader("{\"~#u\": [" + hi64 + ", " + lo64 + "]}").Read<Guid>());
        }

        [TestMethod]
        public void TestReadUri()
        {
            Uri expected = new Uri("http://www.foo.com");
            IReader r = Reader("\"~rhttp://www.foo.com\"");
            Uri v = r.Read<Uri>();
            Assert.AreEqual(expected, v);
        }

        [TestMethod]
        public void TestReadSymbol()
        {
            IReader r = Reader("\"~$foo\"");
            ISymbol v = r.Read<ISymbol>();
            Assert.AreEqual("foo", v.ToString());
        }

        [TestMethod]
        public void TestReadCharacter()
        {
            IReader r = Reader("\"~cf\"");
            char v = r.Read<char>();
            Assert.AreEqual('f', v);
        }

        [TestMethod]
        public void TestReadBinary()
        {
            byte[] bytes = Encoding.ASCII.GetBytes("foobarbaz");
            string encoded = System.Convert.ToBase64String(bytes);
            byte[] decoded = Reader("\"~b" + encoded + "\"").Read<byte[]>();

            Assert.AreEqual(bytes.Length, decoded.Length);

            bool same = true;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != decoded[i])
                    same = false;
            }

            Assert.IsTrue(same);
        }

        [TestMethod]
        public void TestReadUnknown()
        {
            Assert.AreEqual(TransitFactory.TaggedValue("j", "foo"), Reader("\"~jfoo\"").Read<ITaggedValue>());
            IList<object> l = new List<object> { 1L, 2L };

            ITaggedValue expected = TransitFactory.TaggedValue("point", l);
            ITaggedValue result = Reader("{\"~#point\":[1,2]}").Read<ITaggedValue>();
            Assert.AreEqual(expected.Tag, result.Tag);
            CollectionAssert.AreEqual(((IList<object>)expected.Representation).ToArray(), ((IList<object>)result.Representation).ToArray());
        }

        [TestMethod]
        public void TestReadList()
        {
            IList l = Reader("[1, 2, 3]").Read<IList>();

            Assert.IsTrue(l is IList<object>);
            Assert.AreEqual(3, l.Count);

            Assert.AreEqual(1L, l[0]);
            Assert.AreEqual(2L, l[1]);
            Assert.AreEqual(3L, l[2]);
        }

        [TestMethod]
        public void TestReadListWithNested()
        {
            var d = new DateTime(2014, 8, 10, 13, 34, 35);
            String t = JsonParser.FormatDateTime(d);

            IList l = Reader("[\"~:foo\", \"~t" + t + "\", \"~?t\"]").Read<IList>();

            Assert.AreEqual(3, l.Count);

            Assert.AreEqual("foo", l[0].ToString());
            Assert.AreEqual(d, (DateTime)l[1]);
            Assert.IsTrue((bool)l[2]);
        }

        [TestMethod]
        public void TestReadDictionary()
        {
            IDictionary m = Reader("{\"a\": 2, \"b\": 4}").Read<IDictionary>();

            Assert.AreEqual(2, m.Count);

            Assert.AreEqual(2L, m["a"]);
            Assert.AreEqual(4L, m["b"]);
        }

        [TestMethod]
        public void TestReadDictionaryWithNested()
        {
            Guid guid = Guid.NewGuid();

            IDictionary m = Reader("{\"a\": \"~:foo\", \"b\": \"~u" + (Uuid)guid + "\"}").Read<IDictionary>();

            Assert.AreEqual(2, m.Count);

            Assert.AreEqual("foo", m["a"].ToString());
            Assert.AreEqual(guid, m["b"]);
        }

        [TestMethod]
        public void TestReadSet()
        {
            ISet<object> s = Reader("{\"~#set\": [1, 2, 3]}").Read<ISet<object>>();

            Assert.AreEqual(3, s.Count);

            Assert.IsTrue(s.Contains(1L));
            Assert.IsTrue(s.Contains(2L));
            Assert.IsTrue(s.Contains(3L));
        }

        [TestMethod]
        public void TestReadEnumerable()
        {
            IEnumerable l = Reader("{\"~#list\": [1, 2, 3]}").Read<IEnumerable>();
            IEnumerable<object> lo = l.OfType<object>();

            Assert.IsTrue(l is IEnumerable);
            Assert.AreEqual(3, lo.Count());

            Assert.AreEqual(1L, lo.First());
            Assert.AreEqual(2L, lo.Skip(1).First());
            Assert.AreEqual(3L, lo.Skip(2).First());
        }

        [TestMethod]
        public void TestReadRatio()
        {
            IRatio r = Reader("{\"~#ratio\": [\"~n1\",\"~n2\"]}").Read<IRatio>();

            Assert.AreEqual(BigInteger.One, r.Numerator);
            Assert.AreEqual(BigInteger.One + 1, r.Denominator);
            Assert.AreEqual(0.5d, r.GetValue(), 0.01d);
        }

        [TestMethod]
        public void TestReadCDictionary()
        {
            IDictionary m = Reader("{\"~#cmap\": [{\"~#ratio\":[\"~n1\",\"~n2\"]},1,{\"~#list\":[1,2,3]},2]}").Read<IDictionary>();

            Assert.AreEqual(2, m.Count);

            foreach (DictionaryEntry e in m)
            {
                if ((long)e.Value == 1L)
                {
                    Ratio r = (Ratio)e.Key;
                    Assert.AreEqual(new BigInteger(1), r.Numerator);
                    Assert.AreEqual(new BigInteger(2), r.Denominator);
                }
                else
                {
                    if ((long)e.Value == 2L)
                    {
                        IList l = (IList)e.Key;
                        Assert.AreEqual(1L, l[0]);
                        Assert.AreEqual(2L, l[1]);
                        Assert.AreEqual(3L, l[2]);
                    }
                }
            }
        }

        [TestMethod]
        public void TestReadSetTagAsString()
        {
            object o = Reader("{\"~~#set\": [1, 2, 3]}").Read<object>();
            Assert.IsFalse(o is ISet<object>);
            Assert.IsTrue(o is IDictionary);
        }

        [TestMethod]
        public void TestReadMany()
        {
            IReader r;

            // TODO Make sure JSON parser can parse number larger than Int64
            /*
            BigInteger expected = BigInteger.Parse("4256768765123454321897654321234567");
            r = Reader("4256768765123454321897654321234567");
            BigInteger v = r.Read<BigInteger>();
            Assert.AreEqual<BigInteger>(expected, v);
            */

            r = Reader("true null false \"foo\" 42.2 42");
            Assert.IsTrue(Reader("true").Read<bool>());
            Assert.IsNull(Reader("null").Read<object>());
            Assert.IsFalse(Reader("false").Read<bool>());
            Assert.AreEqual("foo", Reader("\"foo\"").Read<string>());
            Assert.AreEqual(42.2, Reader("42.2").Read<double>());
            Assert.AreEqual(42L, Reader("42").Read<long>());
        }

        [TestMethod]
        public void TestReadCache()
        {
            ReadCache rc = new ReadCache();
            Assert.AreEqual("~:foo", rc.CacheRead("~:foo", false));
            Assert.AreEqual("~:foo", rc.CacheRead("^" + (char)WriteCache.BaseCharIdx, false));
            Assert.AreEqual("~$bar", rc.CacheRead("~$bar", false));
            Assert.AreEqual("~$bar", rc.CacheRead("^" + (char)(WriteCache.BaseCharIdx + 1), false));
            Assert.AreEqual("~#baz", rc.CacheRead("~#baz", false));
            Assert.AreEqual("~#baz", rc.CacheRead("^" + (char)(WriteCache.BaseCharIdx + 2), false));
            Assert.AreEqual("foobar", rc.CacheRead("foobar", false));
            Assert.AreEqual("foobar", rc.CacheRead("foobar", false));
            Assert.AreEqual("foobar", rc.CacheRead("foobar", true));
            Assert.AreEqual("foobar", rc.CacheRead("^" + (char)(WriteCache.BaseCharIdx + 3), true));
            Assert.AreEqual("abc", rc.CacheRead("abc", false));
            Assert.AreEqual("abc", rc.CacheRead("abc", false));
            Assert.AreEqual("abc", rc.CacheRead("abc", true));
            Assert.AreEqual("abc", rc.CacheRead("abc", true));
        }

        [TestMethod]
        public void TestReadIdentity()
        {
            IReader r = Reader("\"~\\'42\"");
            string v = r.Read<string>();
            Assert.AreEqual<string>("42", v);
        }

        [TestMethod]
        public void TestReadLink()
        {
            IReader r = Reader("[\"~#link\" , {\"href\": \"~rhttp://www.Beerendonk.nl\", \"rel\": \"a-rel\", \"name\": \"a-name\", \"prompt\": \"a-prompt\", \"render\": \"link or image\"}]");
            ILink v = r.Read<ILink>();
            Assert.AreEqual(new Uri("http://www.Beerendonk.nl"), v.Href);
            Assert.AreEqual("a-rel", v.Rel);
            Assert.AreEqual("a-name", v.Name);
            Assert.AreEqual("a-prompt", v.Prompt);
            Assert.AreEqual("link or image", v.Render);
        }

        #endregion

        #region Writing

        public string Write(object obj, TransitFactory.Format format, IDictionary<Type, IWriteHandler> customHandlers)
        {
            using (Stream output = new MemoryStream())
            {
                IWriter<object> w = TransitFactory.Writer<object>(format, output, customHandlers);
                w.Write(obj);

                output.Position = 0;
                var sr = new StreamReader(output);
                return sr.ReadToEnd();
            }
        }

        public string WriteJsonVerbose(object obj)
        {
            return Write(obj, TransitFactory.Format.JsonVerbose, null);
        }

        public string WriteJsonVerbose(object obj, IDictionary<Type, IWriteHandler> customHandlers)
        {
            return Write(obj, TransitFactory.Format.JsonVerbose, customHandlers);
        }

        public string WriteJson(object obj)
        {
            return Write(obj, TransitFactory.Format.Json, null);
        }

        public string WriteJson(object obj, IDictionary<Type, IWriteHandler> customHandlers)
        {
            return Write(obj, TransitFactory.Format.Json, customHandlers);
        }

        public bool IsEqual(object o1, object o2)
        {
            if (o1 is bool && o2 is bool)
                return (bool)o1 == (bool)o2;
            else
                return false;
        }

        [TestMethod]
        public void TestRoundTrip()
        {
            object inObject = true;
            object outObject;

            string s;

            using (Stream output = new MemoryStream())
            {
                IWriter<object> w = TransitFactory.Writer<object>(TransitFactory.Format.JsonVerbose, output);
                w.Write(inObject);

                output.Position = 0;
                var sr = new StreamReader(output);
                s = sr.ReadToEnd();
            }

            byte[] buffer = Encoding.ASCII.GetBytes(s);
            using (Stream input = new MemoryStream(buffer))
            {
                IReader reader = TransitFactory.Reader(TransitFactory.Format.Json, input);
                outObject = reader.Read<object>();
            }

            Assert.IsTrue(IsEqual(inObject, outObject));
        }

        public string Scalar(string value)
        {
            return "[\"~#'\"," + value + "]";
        }

        public string ScalarVerbose(string value)
        {
            return "{\"~#'\":" + value + "}";
        }

        [TestMethod]
        public void TestWriteNull()
        {
            Assert.AreEqual(ScalarVerbose("null"), WriteJsonVerbose(null));
            Assert.AreEqual(Scalar("null"), WriteJson(null));
        }

        [TestMethod]
        public void TestWriteKeyword()
        {
            Assert.AreEqual(ScalarVerbose("\"~:foo\""), WriteJsonVerbose(TransitFactory.Keyword("foo")));
            Assert.AreEqual(Scalar("\"~:foo\""), WriteJson(TransitFactory.Keyword("foo")));

            IList l = new IKeyword[] 
            {
                TransitFactory.Keyword("foo"),
                TransitFactory.Keyword("foo"),
                TransitFactory.Keyword("foo")
            };
            Assert.AreEqual("[\"~:foo\",\"~:foo\",\"~:foo\"]", WriteJsonVerbose(l));
            Assert.AreEqual("[\"~:foo\",\"^0\",\"^0\"]", WriteJson(l));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestWriteObjectJson()
        {
            WriteJson(new object());
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestWriteObjectJsonVerbose()
        {
            WriteJsonVerbose(new object());
        }

        [TestMethod]
        public void TestWriteString()
        {
            Assert.AreEqual(ScalarVerbose("\"foo\""), WriteJsonVerbose("foo"));
            Assert.AreEqual(Scalar("\"foo\""), WriteJson("foo"));
            Assert.AreEqual(ScalarVerbose("\"~~foo\""), WriteJsonVerbose("~foo"));
            Assert.AreEqual(Scalar("\"~~foo\""), WriteJson("~foo"));
        }

        [TestMethod]
        public void TestWriteBoolean()
        {
            Assert.AreEqual(ScalarVerbose("true"), WriteJsonVerbose(true));
            Assert.AreEqual(Scalar("true"), WriteJson(true));
            Assert.AreEqual(Scalar("false"), WriteJson(false));

            var d = new Dictionary<bool, int>();
            d[true] = 1;
            Assert.AreEqual("{\"~?t\":1}", WriteJsonVerbose(d));
            Assert.AreEqual("[\"^ \",\"~?t\",1]", WriteJson(d));

            var d2 = new Dictionary<bool, int>();
            d2[false] = 1;
            Assert.AreEqual("{\"~?f\":1}", WriteJsonVerbose(d2));
            Assert.AreEqual("[\"^ \",\"~?f\",1]", WriteJson(d2));
        }

        [TestMethod]
        public void TestWriteInteger()
        {
            Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose(42));
            Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose(42L));
            Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose((byte)42));
            Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose((short)42));
            Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose((int)42));
            Assert.AreEqual(ScalarVerbose("42"), WriteJsonVerbose(42L));
            Assert.AreEqual(ScalarVerbose("\"~n42\""), WriteJsonVerbose(BigInteger.Parse("42")));
            Assert.AreEqual(ScalarVerbose("\"~n4256768765123454321897654321234567\""), WriteJsonVerbose(BigInteger.Parse("4256768765123454321897654321234567")));
        }

        [TestMethod]
        public void TestWriteFloatDouble()
        {
            Assert.AreEqual(ScalarVerbose("42.5"), WriteJsonVerbose(42.5));
            Assert.AreEqual(ScalarVerbose("42.5"), WriteJsonVerbose(42.5F));
            Assert.AreEqual(ScalarVerbose("42.5"), WriteJsonVerbose(42.5D));
        }

        [TestMethod]
        public void TestSpecialNumbers()
        {
            Assert.AreEqual(Scalar("\"~zNaN\""), WriteJson(double.NaN));
            Assert.AreEqual(Scalar("\"~zINF\""), WriteJson(double.PositiveInfinity));
            Assert.AreEqual(Scalar("\"~z-INF\""), WriteJson(double.NegativeInfinity));

            Assert.AreEqual(Scalar("\"~zNaN\""), WriteJson(float.NaN));
            Assert.AreEqual(Scalar("\"~zINF\""), WriteJson(float.PositiveInfinity));
            Assert.AreEqual(Scalar("\"~z-INF\""), WriteJson(float.NegativeInfinity));

            Assert.AreEqual(ScalarVerbose("\"~zNaN\""), WriteJsonVerbose(double.NaN));
            Assert.AreEqual(ScalarVerbose("\"~zINF\""), WriteJsonVerbose(double.PositiveInfinity));
            Assert.AreEqual(ScalarVerbose("\"~z-INF\""), WriteJsonVerbose(double.NegativeInfinity));

            Assert.AreEqual(ScalarVerbose("\"~zNaN\""), WriteJsonVerbose(float.NaN));
            Assert.AreEqual(ScalarVerbose("\"~zINF\""), WriteJsonVerbose(float.PositiveInfinity));
            Assert.AreEqual(ScalarVerbose("\"~z-INF\""), WriteJsonVerbose(float.NegativeInfinity));
        }

        [TestMethod]
        public void TestWriteBigDecimal()
        {
            Assert.Inconclusive();

            // TODO
            //Assert.AreEqual(ScalarVerbose("\"~f42.5\""), WriteJsonVerbose(new BigRational(42.5)));
        }

        [TestMethod]
        public void TestWriteDateTime()
        {
            var d = DateTime.Now;
            String dateString = AbstractParser.FormatDateTime(d);
            long dateLong = Beerendonk.Transit.Java.Convert.ToJavaTime(d);
            Assert.AreEqual(ScalarVerbose("\"~t" + dateString + "\""), WriteJsonVerbose(d));
            Assert.AreEqual(Scalar("\"~m" + dateLong + "\""), WriteJson(d));
        }

        [TestMethod]
        public void TestWriteUUID()
        {
            Guid guid = Guid.NewGuid();
            Assert.AreEqual(ScalarVerbose("\"~u" + guid.ToString() + "\""), WriteJsonVerbose(guid));
        }

        [TestMethod]
        public void TestWriteURI()
        {
            Uri uri = new Uri("http://www.foo.com/");

            Assert.AreEqual(ScalarVerbose("\"~rhttp://www.foo.com/\""), WriteJsonVerbose(uri));
        }

        [TestMethod]
        public void TestWriteBinary()
        {
            byte[] bytes = Encoding.ASCII.GetBytes("foobarbaz");
            string encoded = System.Convert.ToBase64String(bytes);

            Assert.AreEqual(ScalarVerbose("\"~b" + encoded + "\""), WriteJsonVerbose(bytes));
        }

        [TestMethod]
        public void TestWriteSymbol()
        {
            Assert.AreEqual(ScalarVerbose("\"~$foo\""), WriteJsonVerbose(TransitFactory.Symbol("foo")));
        }

        [TestMethod]
        public void TestWriteList()
        {
            IList<int> l = new List<int> { 1, 2, 3 };

            Assert.AreEqual("[1,2,3]", WriteJsonVerbose(l));
            Assert.AreEqual("[1,2,3]", WriteJson(l));
        }

        [TestMethod]
        public void TestWritePrimitiveArrays()
        {
            int[] ints = { 1, 2 };
            Assert.AreEqual("[1,2]", WriteJsonVerbose(ints));

            long[] longs = { 1L, 2L };
            Assert.AreEqual("[1,2]", WriteJsonVerbose(longs));

            float[] floats = { 1.5f, 2.78f };
            Assert.AreEqual("[1.5,2.78]", WriteJsonVerbose(floats));

            bool[] bools = { true, false };
            Assert.AreEqual("[true,false]", WriteJsonVerbose(bools));

            double[] doubles = { 1.654d, 2.8765d };
            Assert.AreEqual("[1.654,2.8765]", WriteJsonVerbose(doubles));

            short[] shorts = { 1, 2 };
            Assert.AreEqual("[1,2]", WriteJsonVerbose(shorts));

            char[] chars = { '5', '/' };
            Assert.AreEqual("[\"~c5\",\"~c/\"]", WriteJsonVerbose(chars));
        }

        [TestMethod]
        public void TestWriteDictionary()
        {
            IDictionary<string, int> d = new Dictionary<string, int> { {"foo", 1}, {"bar", 2} };

            Assert.AreEqual("{\"foo\":1,\"bar\":2}", WriteJsonVerbose(d));
            Assert.AreEqual("[\"^ \",\"foo\",1,\"bar\",2]", WriteJson(d));
        }

        [TestMethod]
        public void TestWriteEmptyDictionary()
        {
            IDictionary<object, object> d = new Dictionary<object, object>();
            Assert.AreEqual("{}", WriteJsonVerbose(d));
            Assert.AreEqual("[\"^ \"]", WriteJson(d));
        }

        [TestMethod]
        public void TestWriteSet()
        {
            ISet<string> s = new HashSet<string> { "foo", "bar" };

            Assert.AreEqual("{\"~#set\":[\"foo\",\"bar\"]}", WriteJsonVerbose(s));
            Assert.AreEqual("[\"~#set\",[\"foo\",\"bar\"]]", WriteJson(s));
        }

        [TestMethod]
        public void TestWriteEmptySet()
        {
            ISet<object> s = new HashSet<object>();
            Assert.AreEqual("{\"~#set\":[]}", WriteJsonVerbose(s));
            Assert.AreEqual("[\"~#set\",[]]", WriteJson(s));
        }

        [TestMethod]
        public void TestWriteEnumerable()
        {
            ICollection<string> c = new LinkedList<string>();
            c.Add("foo");
            c.Add("bar");
            IEnumerable<string> e = c;
            Assert.AreEqual("{\"~#list\":[\"foo\",\"bar\"]}", WriteJsonVerbose(e));
            Assert.AreEqual("[\"~#list\",[\"foo\",\"bar\"]]", WriteJson(e));
        }

        [TestMethod]
        public void TestWriteEmptyEnumerable()
        {
            IEnumerable<string> c = new LinkedList<string>();
            Assert.AreEqual("{\"~#list\":[]}", WriteJsonVerbose(c));
            Assert.AreEqual("[\"~#list\",[]]", WriteJson(c));
        }

        [TestMethod]
        public void TestWriteCharacter()
        {
            Assert.AreEqual(ScalarVerbose("\"~cf\""), WriteJsonVerbose('f'));
        }

        [TestMethod]
        public void TestWriteRatio()
        {
            IRatio r = new Ratio(BigInteger.One, new BigInteger(2));
            Assert.AreEqual("{\"~#ratio\":[\"~n1\",\"~n2\"]}", WriteJsonVerbose(r));
            Assert.AreEqual("[\"~#ratio\",[\"~n1\",\"~n2\"]]", WriteJson(r));
        }

        [TestMethod]
        public void TestWriteCDictionary()
        {
            IRatio r = new Ratio(BigInteger.One, new BigInteger(2));
            IDictionary<object, object> d = new Dictionary<object, object>();
            d.Add(r, 1);
            Assert.AreEqual("{\"~#cmap\":[{\"~#ratio\":[\"~n1\",\"~n2\"]},1]}", WriteJsonVerbose(d));
            Assert.AreEqual("[\"~#cmap\",[[\"~#ratio\",[\"~n1\",\"~n2\"]],1]]", WriteJson(d));
        }

        [TestMethod]
        public void TestWriteCache()
        {
            WriteCache wc = new WriteCache(true);
            Assert.AreEqual("~:foo", wc.CacheWrite("~:foo", false));
            Assert.AreEqual("^" + (char)WriteCache.BaseCharIdx, wc.CacheWrite("~:foo", false));
            Assert.AreEqual("~$bar", wc.CacheWrite("~$bar", false));
            Assert.AreEqual("^" + (char)(WriteCache.BaseCharIdx + 1), wc.CacheWrite("~$bar", false));
            Assert.AreEqual("~#baz", wc.CacheWrite("~#baz", false));
            Assert.AreEqual("^" + (char)(WriteCache.BaseCharIdx + 2), wc.CacheWrite("~#baz", false));
            Assert.AreEqual("foobar", wc.CacheWrite("foobar", false));
            Assert.AreEqual("foobar", wc.CacheWrite("foobar", false));
            Assert.AreEqual("foobar", wc.CacheWrite("foobar", true));
            Assert.AreEqual("^" + (char)(WriteCache.BaseCharIdx + 3), wc.CacheWrite("foobar", true));
            Assert.AreEqual("abc", wc.CacheWrite("abc", false));
            Assert.AreEqual("abc", wc.CacheWrite("abc", false));
            Assert.AreEqual("abc", wc.CacheWrite("abc", true));
            Assert.AreEqual("abc", wc.CacheWrite("abc", true));
        }

        [TestMethod]
        public void TestWriteCacheDisabled()
        {
            WriteCache wc = new WriteCache(false);
            Assert.AreEqual("foobar", wc.CacheWrite("foobar", false));
            Assert.AreEqual("foobar", wc.CacheWrite("foobar", false));
            Assert.AreEqual("foobar", wc.CacheWrite("foobar", true));
            Assert.AreEqual("foobar", wc.CacheWrite("foobar", true));
        }

        [TestMethod]
        public void TestWriteUnknown()
        {
            var l = new List<object>();
            l.Add("`jfoo");
            Assert.AreEqual("[\"~`jfoo\"]", WriteJsonVerbose(l));
            Assert.AreEqual(ScalarVerbose("\"~`jfoo\""), WriteJsonVerbose("`jfoo"));

            var l2 = new List<object>();
            l2.Add(1L);
            l2.Add(2L);
            Assert.AreEqual("{\"~#point\":[1,2]}", WriteJsonVerbose(TransitFactory.TaggedValue("point", l2)));
        }

        [TestMethod]
        public void TestWriteWithCustomHandler()
        {
            Mock<IWriteHandler> mock = new Mock<IWriteHandler>();
            mock.Setup(m => m.Tag(It.IsAny<object>())).Returns("s");
            mock.Setup(m => m.Representation(It.IsAny<object>())).Returns("NULL");
            mock.Setup(m => m.StringRepresentation(It.IsAny<object>())).Returns<string>(null);
            mock.Setup(m => m.GetVerboseHandler()).Returns<IWriteHandler>(null);

            IDictionary<Type, IWriteHandler> customHandlers = new Dictionary<Type, IWriteHandler>();
            customHandlers.Add(typeof(NullType), mock.Object);

            // JSON-Verbose
            Assert.AreEqual(ScalarVerbose("\"NULL\""), WriteJsonVerbose(null, customHandlers));
            mock.Verify(m => m.Representation(null));
            mock.Verify(m => m.GetVerboseHandler());

            // JSON
            mock.ResetCalls();
            Assert.AreEqual(Scalar("\"NULL\""), WriteJson(null, customHandlers));
            mock.Verify(m => m.Representation(null));
        }

        #endregion

        [TestMethod]
        public void TestUseIKeywordAsDictionaryKey()
        {
            IDictionary<object, object> d = new Dictionary<object, object>();
            d.Add(TransitFactory.Keyword("foo"), 1);
            d.Add("foo", 2);
            d.Add(TransitFactory.Keyword("bar"), 3);
            d.Add("bar", 4);

            Assert.AreEqual(1, d[TransitFactory.Keyword("foo")]);
            Assert.AreEqual(2, d["foo"]);
            Assert.AreEqual(3, d[TransitFactory.Keyword("bar")]);
            Assert.AreEqual(4, d["bar"]);
        }

        [TestMethod]
        public void TestUseISymbolAsDictionaryKey()
        {
            IDictionary<object, object> d = new Dictionary<object, object>();
            d.Add(TransitFactory.Symbol("foo"), 1);
            d.Add("foo", 2);
            d.Add(TransitFactory.Symbol("bar"), 3);
            d.Add("bar", 4);

            Assert.AreEqual(1, d[TransitFactory.Symbol("foo")]);
            Assert.AreEqual(2, d["foo"]);
            Assert.AreEqual(3, d[TransitFactory.Symbol("bar")]);
            Assert.AreEqual(4, d["bar"]);
        }

        [TestMethod]
        public void TestKeywordEquality()
        {
            string s = "foo";

            IKeyword k1 = TransitFactory.Keyword("foo");
            IKeyword k2 = TransitFactory.Keyword("!foo".Substring(1));
            IKeyword k3 = TransitFactory.Keyword("bar");

            Assert.AreEqual(k1, k2);
            Assert.AreEqual(k2, k1);
            Assert.IsFalse(k1.Equals(k3));
            Assert.IsFalse(k3.Equals(k1));
            Assert.IsFalse(s.Equals(k1));
            Assert.IsFalse(k1.Equals(s));
        }

        [TestMethod]
        public void TestKeywordHashCode()
        {
            string s = "foo";

            IKeyword k1 = TransitFactory.Keyword("foo");
            IKeyword k2 = TransitFactory.Keyword("!foo".Substring(1));
            IKeyword k3 = TransitFactory.Keyword("bar");
            ISymbol symbol = TransitFactory.Symbol("bar");

            Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
            Assert.IsFalse(k3.GetHashCode() == k1.GetHashCode());
            Assert.IsFalse(symbol.GetHashCode() == k1.GetHashCode());
            Assert.IsFalse(s.GetHashCode() == k1.GetHashCode());
        }

        [TestMethod]
        public void TestKeywordComparator()
        {

            List<IKeyword> l = new List<IKeyword> {
                { TransitFactory.Keyword("bbb") },
                { TransitFactory.Keyword("ccc") },
                { TransitFactory.Keyword("abc") },
                { TransitFactory.Keyword("dab") } };

            l.Sort();

            Assert.AreEqual("abc", l[0].ToString());
            Assert.AreEqual("bbb", l[1].ToString());
            Assert.AreEqual("ccc", l[2].ToString());
            Assert.AreEqual("dab", l[3].ToString());
        }

        [TestMethod]
        public void TestSymbolEquality()
        {
            string s = "foo";

            ISymbol sym1 = TransitFactory.Symbol("foo");
            ISymbol sym2 = TransitFactory.Symbol("!foo".Substring(1));
            ISymbol sym3 = TransitFactory.Symbol("bar");

            Assert.AreEqual(sym1, sym2);
            Assert.AreEqual(sym2, sym1);
            Assert.IsFalse(sym1.Equals(sym3));
            Assert.IsFalse(sym3.Equals(sym1));
            Assert.IsFalse(s.Equals(sym1));
            Assert.IsFalse(sym1.Equals(s));
        }

        [TestMethod]
        public void TestSymbolHashCode()
        {
            string s = "foo";

            ISymbol sym1 = TransitFactory.Symbol("foo");
            ISymbol sym2 = TransitFactory.Symbol("!foo".Substring(1));
            ISymbol sym3 = TransitFactory.Symbol("bar");
            IKeyword keyword = TransitFactory.Keyword("bar");

            Assert.AreEqual(sym1.GetHashCode(), sym2.GetHashCode());
            Assert.IsFalse(sym3.GetHashCode() == sym1.GetHashCode());
            Assert.IsFalse(keyword.GetHashCode() == sym1.GetHashCode());
            Assert.IsFalse(s.GetHashCode() == sym1.GetHashCode());
        }

        [TestMethod]
        public void TestSymbolComparator()
        {

            List<ISymbol> l = new List<ISymbol> {
                { TransitFactory.Symbol("bbb") },
                { TransitFactory.Symbol("ccc") },
                { TransitFactory.Symbol("abc") },
                { TransitFactory.Symbol("dab") } };

            l.Sort();

            Assert.AreEqual("abc", l[0].ToString());
            Assert.AreEqual("bbb", l[1].ToString());
            Assert.AreEqual("ccc", l[2].ToString());
            Assert.AreEqual("dab", l[3].ToString());
        }

        [TestMethod]
        public void TestDictionaryWithEscapedKey()
        {
            var d1 = new Dictionary<object, object> { { "~Gfoo", 20L } };
            string str = WriteJson(d1);

            IDictionary d2 = Reader(str).Read<IDictionary>();
            Assert.IsTrue(d2.Contains("~Gfoo"));
            Assert.IsTrue(d2["~Gfoo"].Equals(20L));
        }

        [TestMethod]
        public void TestLink()
        {
            ILink l1 = TransitFactory.Link("http://google.com/", "search", "name", "link", "prompt");
            String str = WriteJson(l1);
            ILink l2 = Reader(str).Read<ILink>();
            Assert.AreEqual("http://google.com/", l2.Href.AbsoluteUri);
            Assert.AreEqual("search", l2.Rel);
            Assert.AreEqual("name", l2.Name);
            Assert.AreEqual("link", l2.Render);
            Assert.AreEqual("prompt", l2.Prompt);
        }

        [TestMethod]
        public void TestEmptySet()
        {
            string str = WriteJson(new HashSet<object>());
            Assert.IsInstanceOfType(Reader(str).Read<ISet<object>>(), typeof(ISet<object>));
        }
    }
}
