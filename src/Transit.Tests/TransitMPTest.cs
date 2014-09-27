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
using MsgPack;
using NForza.Transit.Impl;
using System.Collections;
using System.IO;

namespace NForza.Transit.Tests
{
    [TestClass]
    public class TransitMPTest
    {
        #region Reading

        public IReader Reader(params object[] things)
        {
            Stream input = new MemoryStream();

            Packer packer = Packer.Create(input);
            foreach (object o in things)
            {
                packer.Pack(o);
            }

            input.Position = 0;
            return TransitFactory.Reader(TransitFactory.Format.MsgPack, input);
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

            // TODO
            /*
            IDictionary thing = new Hashtable { { "\"~?t\"", 1 }, { "\"~?f\"", 2 } };
            IDictionary d = Reader(thing).Read<IDictionary>();
            Assert.AreEqual(1L, d[true]);
            Assert.AreEqual(2L, d[false]);
            */
        }

        // TODO

        #endregion
    }
}
