// Copyright © 2014 Rick Beerendonk. All Rights Reserved.
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
using Beerendonk.Transit.Java;
using System;

namespace Beerendonk.Transit.Tests.Java
{
    [TestClass]
    public class UuidTest
    {
        [TestMethod]
        public void TestConstructor()
        {
            var uuid = new Uuid(1, 2);

            Assert.AreEqual(1, uuid.MostSignificantBits);
            Assert.AreEqual(2, uuid.LeastSignificantBits);
        }

        [TestMethod]
        public void ShouldNotEqualNull()
        {
            var uuid = new Uuid(1, 2);

            Assert.IsFalse(uuid.Equals(null));
        }

        [TestMethod]
        public void ShouldNotEqualOtherType()
        {
            var uuid = new Uuid(1, 2);

            Assert.IsFalse(uuid.Equals(new object()));
        }

        [TestMethod]
        public void ShouldEqualSimilarUuidObject()
        {
            var uuid = new Uuid(1, 2);
            object other = new Uuid(1, 2);

            Assert.IsTrue(uuid.Equals(other));
        }

        [TestMethod]
        public void ShouldNotEqualOtherUuidObject()
        {
            var uuid = new Uuid(1, 2);
            object other = new Uuid(3, 4);

            Assert.IsFalse(uuid.Equals(other));
        }

        [TestMethod]
        public void ShouldEqualSimilarUuid()
        {
            var uuid = new Uuid(1, 2);

            Assert.IsTrue(uuid.Equals(new Uuid(1, 2)));
        }

        [TestMethod]
        public void ShouldNotEqualOtherUuid()
        {
            var uuid = new Uuid(1, 2);

            Assert.IsFalse(uuid.Equals(new Uuid(3, 4)));
        }

        [TestMethod]
        public void OperationOverloadEqual_ShouldEqualSimilarUuid()
        {
            var uuid = new Uuid(1, 2);

            Assert.IsTrue(uuid == new Uuid(1, 2));
        }

        [TestMethod]
        public void OperationOverloadEqual_ShouldNotEqualOtherUuid()
        {
            var uuid = new Uuid(1, 2);

            Assert.IsFalse(uuid == new Uuid(3, 4));
        }

        [TestMethod]
        public void OperationOverloadUnequal_ShouldUnequalOtherUuid()
        {
            var uuid = new Uuid(1, 2);

            Assert.IsTrue(uuid != new Uuid(3, 4));
        }

        [TestMethod]
        public void OperationOverloadUnequal_ShouldNotUnequalSimilarUuid()
        {
            var uuid = new Uuid(1, 2);

            Assert.IsFalse(uuid != new Uuid(1, 2));
        }

        [TestMethod]
        public void HashCodeShouldEqualGuidHashCode()
        {
            var guid = Guid.NewGuid();

            Assert.AreEqual(guid.GetHashCode(), ((Uuid)guid).GetHashCode());
        }

        [TestMethod]
        public void ConvertToGuidOfDefaultUuidShouldReturnDefaultGuid()
        {
            var uuid = default(Uuid);

            Assert.AreEqual(default(Guid), (Guid)uuid);
        }

        [TestMethod]
        public void ConvertToGuidShouldReturnCorrectGuid()
        {
            var uuid = new Uuid(-1714729031470661412L, -8577612382363445748L);

            Assert.AreEqual(new Guid("e8340f07-e924-40dc-88f6-32fc003c160c"), (Guid)uuid);
        }

        [TestMethod]
        public void ConvertToUuidOfDefaultGuidShouldReturnDefaultUuid()
        {
            var guid = default(Guid);

            Assert.AreEqual(default(Uuid), (Uuid)guid);
        }

        [TestMethod]
        public void ConvertToUuidShouldReturnCorrectUuid()
        {
            var guid = new Guid("e8340f07-e924-40dc-88f6-32fc003c160c");

            var uuid = (Uuid)guid; 

            Assert.AreEqual(-1714729031470661412L, uuid.MostSignificantBits);
            Assert.AreEqual(-8577612382363445748L, uuid.LeastSignificantBits);
        }

        [TestMethod]
        public void ToStringShouldReturnCorrectString()
        {
            var uuid = new Uuid(-1714729031470661412L, -8577612382363445748L);

            Assert.AreEqual("e8340f07-e924-40dc-88f6-32fc003c160c", uuid.ToString());
        }

        [TestMethod]
        public void FromStringShouldReturnCorrectUuid()
        {
            var uuid = Uuid.FromString("e8340f07-e924-40dc-88f6-32fc003c160c");

            Assert.AreEqual(-1714729031470661412L, uuid.MostSignificantBits);
            Assert.AreEqual(-8577612382363445748L, uuid.LeastSignificantBits);
        }
    }
}
