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

using System.Collections;
using System.Globalization;
using System.Text;

namespace Beerendonk.Transit.Tests;

[TestClass]
public class TransitJSONMachineModeTest
{
    private static IReader Reader(string json)
    {
        return TransitFactory.Reader(
            TransitFactory.Format.Json,
            new MemoryStream(Encoding.UTF8.GetBytes(json)));
    }

    private static string WriteJson(object value)
    {
        using var output = new MemoryStream();
        TransitFactory.Writer<object>(TransitFactory.Format.Json, output).Write(value);
        return Encoding.UTF8.GetString(output.ToArray());
    }

    private static string WriteJsonVerbose(object value)
    {
        using var output = new MemoryStream();
        TransitFactory.Writer<object>(TransitFactory.Format.JsonVerbose, output).Write(value);
        return Encoding.UTF8.GetString(output.ToArray());
    }

    private static string ScalarVerbose(string value)
    {
        return "{\"~#'\":" + value + "}";
    }

    [TestMethod]
    public void TestWriteIntegerAtJsonBoundaries()
    {
        const long maximumSafeInteger = 9007199254740991L;

        Assert.AreEqual(
            ScalarVerbose(maximumSafeInteger.ToString(CultureInfo.InvariantCulture)),
            WriteJsonVerbose(maximumSafeInteger));
        Assert.AreEqual(
            ScalarVerbose("\"~i9007199254740992\""),
            WriteJsonVerbose(maximumSafeInteger + 1));
        Assert.AreEqual(
            ScalarVerbose((-maximumSafeInteger).ToString(CultureInfo.InvariantCulture)),
            WriteJsonVerbose(-maximumSafeInteger));
        Assert.AreEqual(
            ScalarVerbose("\"~i-9007199254740992\""),
            WriteJsonVerbose(-maximumSafeInteger - 1));
    }

    [TestMethod]
    public void TestReadMachineAndHumanTimeForms()
    {
        IList human = Reader(
            "[\"~t1776-07-04T12:00:00.000Z\",\"~t1970-01-01T00:00:00.000Z\","
            + "\"~t2000-01-01T12:00:00.000Z\",\"~t2014-04-07T22:17:17.000Z\"]")
            .Read<IList>()!;
        IList machine = Reader(
            "[\"~m-6106017600000\",\"~m0\",\"~m946728000000\",\"~m1396909037000\"]")
            .Read<IList>()!;

        Assert.AreEqual(4, human.Count);
        Assert.AreEqual(4, machine.Count);
        for (int i = 0; i < human.Count; i++)
        {
            Assert.AreEqual(
                Beerendonk.Transit.Java.Convert.ToJavaTime((DateTime)human[i]!),
                Beerendonk.Transit.Java.Convert.ToJavaTime((DateTime)machine[i]!));
        }
    }

    [TestMethod]
    public void TestWriteMachineTimeSequence()
    {
        long[] timestamps = { -6106017600000L, 0L, 946728000000L, 1396909037000L };
        DateTime[] dates = timestamps
            .Select(timestamp => DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime)
            .ToArray();

        string json = WriteJson(dates);
        Assert.AreEqual(
            "[\"~m-6106017600000\",\"~m0\",\"~m946728000000\",\"~m1396909037000\"]",
            json);

        IList decoded = Reader(json).Read<IList>()!;
        for (int i = 0; i < timestamps.Length; i++)
        {
            Assert.AreEqual(
                timestamps[i],
                Beerendonk.Transit.Java.Convert.ToJavaTime((DateTime)decoded[i]!));
        }
    }

    [TestMethod]
    public void TestWritingArrayMarkerEscapesDirectString()
    {
        var values = new List<string> { "^ " };
        string json = WriteJson(values);

        Assert.AreEqual("[\"~^ \"]", json);
        Assert.AreEqual("^ ", ((IList)Reader(json).Read<IList>()!)[0]);
    }
}
