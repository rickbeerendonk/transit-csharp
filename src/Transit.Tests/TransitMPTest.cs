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

using Beerendonk.Transit.Impl;
using Beerendonk.Transit.Java;
using Beerendonk.Transit.Numerics;
using MessagePack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Beerendonk.Transit.Tests;

[TestClass]
public class TransitMPTest
{
    private static IReader ReaderOf(params object?[] values)
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);

        foreach (object? value in values)
        {
            WriteMessagePackValue(ref writer, value);
        }

        writer.Flush();
        return TransitFactory.Reader(
            TransitFactory.Format.MsgPack,
            new MemoryStream(buffer.WrittenMemory.ToArray()));
    }

    private static IReader ReaderOfSingle(object? value)
    {
        return ReaderOf(value);
    }

    private static void WriteMessagePackValue(ref MessagePackWriter writer, object? value)
    {
        if (value == null)
        {
            writer.WriteNil();
        }
        else if (value is string stringValue)
        {
            writer.Write(stringValue);
        }
        else if (value is bool booleanValue)
        {
            writer.Write(booleanValue);
        }
        else if (value is float singleValue)
        {
            writer.Write(singleValue);
        }
        else if (value is double doubleValue)
        {
            writer.Write(doubleValue);
        }
        else if (value is byte || value is sbyte || value is short || value is ushort
            || value is int || value is uint || value is long)
        {
            writer.Write(System.Convert.ToInt64(value, CultureInfo.InvariantCulture));
        }
        else if (value is IDictionary dictionary)
        {
            writer.WriteMapHeader(dictionary.Count);
            foreach (DictionaryEntry entry in dictionary)
            {
                WriteMessagePackValue(ref writer, entry.Key);
                WriteMessagePackValue(ref writer, entry.Value);
            }
        }
        else if (value is IEnumerable enumerable)
        {
            var items = enumerable.Cast<object?>().ToList();
            writer.WriteArrayHeader(items.Count);
            foreach (object? item in items)
            {
                WriteMessagePackValue(ref writer, item);
            }
        }
        else
        {
            throw new ArgumentException("Unsupported MessagePack test value: " + value.GetType());
        }
    }

    private static IReader ReaderOfString(string value)
    {
        return ReaderOf(value);
    }

    [TestMethod]
    public void TestReadString()
    {
        Assert.AreEqual("foo", ReaderOfString("foo").Read<string>()!);
        Assert.AreEqual("~foo", ReaderOfString("~~foo").Read<string>()!);
        Assert.AreEqual("`foo", ReaderOfString("~`foo").Read<string>()!);
        Assert.AreEqual("foo", ReaderOfString("~#foo").Read<Tag>()!.GetValue());
        Assert.AreEqual("^foo", ReaderOfString("~^foo").Read<string>()!);
    }

    [TestMethod]
    public void TestReadBoolean()
    {
        Assert.IsTrue(ReaderOfString("~?t").Read<bool>()!);
        Assert.IsFalse(ReaderOfString("~?f").Read<bool>()!);

        var values = new Dictionary<object, object>
        {
            ["~?t"] = 1,
            ["~?f"] = 2
        };
        IDictionary decoded = ReaderOf(values).Read<IDictionary>()!;
        Assert.AreEqual(1L, decoded[true]);
        Assert.AreEqual(2L, decoded[false]);
    }

    [TestMethod]
    public void TestReadNull()
    {
        Assert.IsNull(ReaderOfString("~_").Read<object>()!);
    }

    [TestMethod]
    public void TestReadKeyword()
    {
        Assert.AreEqual(TransitFactory.Keyword("foo"), ReaderOfString("~:foo").Read<IKeyword>()!);

        IList values = ReaderOfSingle(new object[]
        {
            "~:foo",
            "^" + (char)WriteCache.BaseCharIdx,
            "^" + (char)WriteCache.BaseCharIdx
        }).Read<IList>()!;

        Assert.AreEqual(TransitFactory.Keyword("foo"), values[0]);
        Assert.AreEqual(TransitFactory.Keyword("foo"), values[1]);
        Assert.AreEqual(TransitFactory.Keyword("foo"), values[2]);
    }

    [TestMethod]
    public void TestReadInteger()
    {
        Assert.AreEqual(42L, ReaderOfString("~i42").Read<long>()!);
        Assert.AreEqual(
            BigInteger.Parse("4256768765123454321897654321234567", CultureInfo.InvariantCulture),
            ReaderOfString("~n4256768765123454321897654321234567").Read<BigInteger>()!);
    }

    [TestMethod]
    public void TestReadDouble()
    {
        Assert.AreEqual(42.5D, ReaderOfString("~d42.5").Read<double>()!);
    }

    [TestMethod]
    public void TestReadBigDecimal()
    {
        Assert.AreEqual(new BigRational(42.5M), ReaderOfString("~f42.5").Read<BigRational>()!);
    }

    [TestMethod]
    public void TestReadTime()
    {
        string[] timestamps =
        {
            "2014-04-07T22:17:17.481Z",
            "2014-04-07T22:17:17Z",
            "2014-04-07T22:17:17.481-00:00"
        };

        foreach (string timestamp in timestamps)
        {
            DateTime actual = ReaderOfString("~t" + timestamp).Read<DateTime>()!;
            long expected = DateTimeOffset.Parse(timestamp, CultureInfo.InvariantCulture).ToUnixTimeMilliseconds();
            Assert.AreEqual(expected, Beerendonk.Transit.Java.Convert.ToJavaTime(actual));
        }

        long machineTime = 1396909037481L;
        var machineTimeMap = new Dictionary<object, object> { ["~#m"] = machineTime };
        Assert.AreEqual(machineTime, Beerendonk.Transit.Java.Convert.ToJavaTime(ReaderOf(machineTimeMap).Read<DateTime>()!));
    }

    [TestMethod]
    public void TestReadUUID()
    {
        Guid expected = Guid.NewGuid();
        Uuid uuid = (Uuid)expected;
        Assert.AreEqual(expected, ReaderOfString("~u" + expected.ToString("D")).Read<Guid>()!);

        Guid taggedUuid = ReaderOfSingle(new object[]
        {
            "~#u",
            new object[] { uuid.MostSignificantBits, uuid.LeastSignificantBits }
        }).Read<Guid>()!;
        Assert.AreEqual(expected, taggedUuid);
    }

    [TestMethod]
    public void TestReadUri()
    {
        Assert.AreEqual(
            new Uri("http://www.foo.com"),
            ReaderOfString("~rhttp://www.foo.com").Read<Uri>()!);
    }

    [TestMethod]
    public void TestReadSymbol()
    {
        Assert.AreEqual(TransitFactory.Symbol("foo"), ReaderOfString("~$foo").Read<ISymbol>()!);
    }

    [TestMethod]
    public void TestReadCharacter()
    {
        Assert.AreEqual('f', ReaderOfString("~cf").Read<char>()!);
    }

    [TestMethod]
    public void TestReadBinary()
    {
        byte[] expected = Encoding.ASCII.GetBytes("foobarbaz");
        string encoded = System.Convert.ToBase64String(expected);
        CollectionAssert.AreEqual(expected, ReaderOfString("~b" + encoded).Read<byte[]>()!);
    }

    [TestMethod]
    public void TestReadUnknown()
    {
        Assert.AreEqual(
            TransitFactory.TaggedValue("j", "foo"),
            ReaderOfString("~jfoo").Read<ITaggedValue>()!);

        ITaggedValue point = ReaderOf(new Dictionary<object, object>
        {
            ["~#point"] = new object[] { 1, 2 }
        }).Read<ITaggedValue>()!;
        Assert.AreEqual("point", point.Tag);
        CollectionAssert.AreEqual(new object[] { 1L, 2L }, ((IList)point.Representation!).Cast<object>().ToArray());
    }

    [TestMethod]
    public void TestReadArray()
    {
        IList values = ReaderOf(new long[] { 1, 2, 3 }).Read<IList>()!;
        CollectionAssert.AreEqual(new object[] { 1L, 2L, 3L }, values.Cast<object>().ToArray());
    }

    [TestMethod]
    public void TestReadArrayWithNestedDoubles()
    {
        double[] expected = { -3.14159D, 3.14159D, 4.0E11D, 2.998E8D, 6.626E-34D };
        IList actual = ReaderOfSingle(expected).Read<IList>()!;
        CollectionAssert.AreEqual(expected.Cast<object>().ToArray(), actual.Cast<object>().ToArray());
    }

    [TestMethod]
    public void TestReadArrayWithNested()
    {
        var expectedDate = new DateTime(2014, 4, 7, 22, 17, 17, 481, DateTimeKind.Utc);
        string timestamp = expectedDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);

        IList values = ReaderOfSingle(new object[] { "~:foo", "~t" + timestamp, "~?t" }).Read<IList>()!;
        Assert.AreEqual(TransitFactory.Keyword("foo"), values[0]);
        Assert.AreEqual(
            Beerendonk.Transit.Java.Convert.ToJavaTime(expectedDate),
            Beerendonk.Transit.Java.Convert.ToJavaTime((DateTime)values[1]!));
        Assert.AreEqual(true, values[2]);

        long[] expectedTimes = { -6106017600000L, 0L, 946728000000L, 1396909037000L };
        string[] dates = expectedTimes
            .Select(time => "~t" + DateTimeOffset.FromUnixTimeMilliseconds(time).UtcDateTime.ToString(
                "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture))
            .ToArray();
        IList actualDates = ReaderOfSingle(dates).Read<IList>()!;
        for (int i = 0; i < expectedTimes.Length; i++)
        {
            Assert.AreEqual(
                expectedTimes[i],
                Beerendonk.Transit.Java.Convert.ToJavaTime((DateTime)actualDates[i]!));
        }
    }

    [TestMethod]
    public void TestReadMap()
    {
        var values = new Dictionary<object, object> { ["a"] = 2, ["b"] = 4 };
        IDictionary map = ReaderOf(values).Read<IDictionary>()!;
        Assert.AreEqual(2, map.Count);
        Assert.AreEqual(2L, map["a"]);
        Assert.AreEqual(4L, map["b"]);
    }

    [TestMethod]
    public void TestReadMapWithNested()
    {
        Guid expected = Guid.NewGuid();
        var values = new Dictionary<object, object>
        {
            ["a"] = "~:foo",
            ["b"] = "~u" + expected.ToString("D")
        };

        IDictionary map = ReaderOf(values).Read<IDictionary>()!;
        Assert.AreEqual(TransitFactory.Keyword("foo"), map["a"]);
        Assert.AreEqual(expected, map["b"]);
    }

    [TestMethod]
    public void TestReadSet()
    {
        ISet<object> values = ReaderOf(new Dictionary<object, object>
        {
            ["~#set"] = new int[] { 1, 2, 3 }
        }).Read<ISet<object>>()!;

        Assert.AreEqual(3, values.Count);
        Assert.IsTrue(values.Contains(1L));
        Assert.IsTrue(values.Contains(2L));
        Assert.IsTrue(values.Contains(3L));
    }

    [TestMethod]
    public void TestReadList()
    {
        IEnumerable<object> values = ReaderOf(new Dictionary<object, object>
        {
            ["~#list"] = new int[] { 1, 2, 3 }
        }).Read<IEnumerable<object>>()!;
        CollectionAssert.AreEqual(new object[] { 1L, 2L, 3L }, values.ToArray());
    }

    [TestMethod]
    public void TestReadRatio()
    {
        IRatio ratio = ReaderOf(new Dictionary<object, object>
        {
            ["~#ratio"] = new[] { "~n1", "~n2" }
        }).Read<IRatio>()!;

        Assert.AreEqual(BigInteger.One, ratio.Numerator);
        Assert.AreEqual(new BigInteger(2), ratio.Denominator);
        Assert.AreEqual(0.5D, ratio.GetValue(), 0.01D);
    }

    [TestMethod]
    public void TestReadCmap()
    {
        var ratioRepresentation = new Dictionary<object, object>
        {
            ["~#ratio"] = new[] { "~n1", "~n2" }
        };
        var listRepresentation = new Dictionary<object, object>
        {
            ["~#list"] = new[] { 1, 2, 3 }
        };
        IDictionary map = ReaderOf(new Dictionary<object, object>
        {
            ["~#cmap"] = new object[] { ratioRepresentation, 1, listRepresentation, 2 }
        }).Read<IDictionary>()!;

        Assert.AreEqual(2, map.Count);
        foreach (DictionaryEntry entry in map)
        {
            if ((long)entry.Value! == 1L)
            {
                IRatio ratio = (IRatio)entry.Key!;
                Assert.AreEqual(BigInteger.One, ratio.Numerator);
                Assert.AreEqual(new BigInteger(2), ratio.Denominator);
            }
            else
            {
                CollectionAssert.AreEqual(
                    new object[] { 1L, 2L, 3L },
                    ((IList)entry.Key).Cast<object>().ToArray());
            }
        }
    }

    [TestMethod]
    public void TestReadMany()
    {
        IReader reader = ReaderOf(true, null, false, "foo", 42.2D, 42);
        Assert.IsTrue(reader.Read<bool>()!);
        Assert.IsNull(reader.Read<object>());
        Assert.IsFalse(reader.Read<bool>()!);
        Assert.AreEqual("foo", reader.Read<string>()!);
        Assert.AreEqual(42.2D, reader.Read<double>()!);
        Assert.AreEqual(42L, reader.Read<long>()!);
    }

    [TestMethod]
    public void TestWriteReadTime()
    {
        long[] timestamps = { -6106017600000L, 0L, 946728000000L, 1396909037000L };
        DateTime[] dates = timestamps
            .Select(time => DateTimeOffset.FromUnixTimeMilliseconds(time).UtcDateTime)
            .ToArray();

        using var output = new MemoryStream();
        TransitFactory.Writer<object>(TransitFactory.Format.MsgPack, output).Write(dates);
        output.Position = 0;
        IList actual = TransitFactory.Reader(TransitFactory.Format.MsgPack, output).Read<IList>()!;

        Assert.AreEqual(timestamps.Length, actual.Count);
        for (int i = 0; i < timestamps.Length; i++)
        {
            Assert.AreEqual(
                timestamps[i],
                Beerendonk.Transit.Java.Convert.ToJavaTime((DateTime)actual[i]!));
        }
    }

    [TestMethod]
    public void TestWriteReadSpecialNumbers()
    {
        using var output = new MemoryStream();
        IWriter<object> writer = TransitFactory.Writer<object>(TransitFactory.Format.MsgPack, output);
        writer.Write(double.NaN);
        writer.Write(float.NaN);
        writer.Write(double.PositiveInfinity);
        writer.Write(float.PositiveInfinity);
        writer.Write(double.NegativeInfinity);
        writer.Write(float.NegativeInfinity);

        output.Position = 0;
        IReader reader = TransitFactory.Reader(TransitFactory.Format.MsgPack, output);
        Assert.IsTrue(double.IsNaN(reader.Read<double>()!));
        Assert.IsTrue(double.IsNaN(reader.Read<double>()!));
        Assert.AreEqual(double.PositiveInfinity, reader.Read<double>()!);
        Assert.AreEqual(double.PositiveInfinity, reader.Read<double>()!);
        Assert.AreEqual(double.NegativeInfinity, reader.Read<double>()!);
        Assert.AreEqual(double.NegativeInfinity, reader.Read<double>()!);
    }

    [TestMethod]
    public void TestWritingArrayMarkerEscapesDirectString()
    {
        using var output = new MemoryStream();
        TransitFactory.Writer<object>(TransitFactory.Format.MsgPack, output)
            .Write(new List<string> { "^ " });

        output.Position = 0;
        IList values = TransitFactory.Reader(TransitFactory.Format.MsgPack, output).Read<IList>()!;
        Assert.AreEqual("^ ", values[0]);
    }
}
