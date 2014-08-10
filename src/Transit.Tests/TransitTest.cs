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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NForza.Transit.Impl;
using NForza.Transit.Java;
using NForza.Transit.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace NForza.Transit.Tests
{
    [TestClass]
    public class TransitTest
    {
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
            IReader r = Reader("\"~d42.5\"");
            double v = r.Read<double>();
            Assert.AreEqual<double>(42.5D, v);
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
            long javaTime = NForza.Transit.Java.Convert.ToJavaTime(d);

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
            Assert.AreEqual(expected.GetTag(), result.GetTag());
            CollectionAssert.AreEqual(((IList<object>)expected.GetRepresentation()).ToArray(), ((IList<object>)result.GetRepresentation()).ToArray());
        }

        [TestMethod]
        public void TestReadArray()
        {
            IList l = Reader("[1, 2, 3]").Read<IList>();

            Assert.IsTrue(l is IList<object>);
            Assert.AreEqual(3, l.Count);

            Assert.AreEqual(1L, l[0]);
            Assert.AreEqual(2L, l[1]);
            Assert.AreEqual(3L, l[2]);
        }

        [TestMethod]
        public void TestReadArrayWithNested()
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
        public void TestReadList()
        {
            IList l = Reader("{\"~#list\": [1, 2, 3]}").Read<IList>();

            Assert.IsTrue(l is IList<object>);
            Assert.AreEqual(3, l.Count);

            Assert.AreEqual(1L, l[0]);
            Assert.AreEqual(2L, l[1]);
            Assert.AreEqual(3L, l[2]);
        }

        [TestMethod]
        public void TestReadRatio()
        {
            IRatio r = Reader("{\"~#ratio\": [\"~n1\",\"~n2\"]}").Read<IRatio>();

            Assert.AreEqual(BigInteger.One, r.GetNumerator());
            Assert.AreEqual(BigInteger.One + 1, r.GetDenominator());
            Assert.AreEqual(0.5d, r.GetValue(), 0.01d);
        }

        [TestMethod]
        public void TestReadCmap()
        {
            IDictionary m = Reader("{\"~#cmap\": [{\"~#ratio\":[\"~n1\",\"~n2\"]},1,{\"~#list\":[1,2,3]},2]}").Read<IDictionary>();

            Assert.AreEqual(2, m.Count);

            foreach (DictionaryEntry e in m)
        	{
                if ((long)e.Value == 1L) 
                {
                    Ratio r = (Ratio)e.Key;
                    Assert.AreEqual(new BigInteger(1), r.GetNumerator());
                    Assert.AreEqual(new BigInteger(2), r.GetDenominator());
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
            /*
            BigInteger expected = BigInteger.Parse("4256768765123454321897654321234567");
            IReader r = Reader("4256768765123454321897654321234567");
            BigInteger v = r.Read<BigInteger>();
            Assert.AreEqual<BigInteger>(expected, v);
            */

            Assert.Inconclusive();
            // TODO
            /*
            IReader r = Reader("true null false \"foo\" 42.2 42");
            Assert.IsTrue(r.Read<bool>());
            Assert.IsNull(r.Read<object>());
            Assert.IsFalse(r.Read<bool>());
            Assert.AreEqual("foo", r.Read<string>());
            Assert.AreEqual(42.2, r.Read<double>());
            Assert.AreEqual(42L, r.Read<long>());
            */ 
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
            Assert.Inconclusive();
            // TODO
        }

        [TestMethod]
        public void TestReadLink()
        {
            Assert.Inconclusive();
            // TODO
        }





    }
}
