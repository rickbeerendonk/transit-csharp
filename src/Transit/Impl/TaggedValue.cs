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
    /// <summary>
    /// Represents a tagged value.
    /// </summary>
    public class TaggedValue : ITaggedValue 
    {
        private readonly string tag;
        private readonly object representation;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaggedValue"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="representation">The representation.</param>
        public TaggedValue(string tag, object representation)
        {
            this.tag = tag;
            this.representation = representation;
        }

        /// <summary>
        /// Gets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public string Tag
        {
            get { return tag; }
        }

        /// <summary>
        /// Gets the representation.
        /// </summary>
        /// <value>
        /// The representation.
        /// </value>
        public object Representation
        {
            get { return representation; }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is TaggedValue))
            {
                return false;
            }

            var other = (TaggedValue)obj;
            return (this.tag.Equals(other.Tag) && 
                this.representation.Equals(other.Representation));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int result = 17;
            result = 31 * result * tag.GetHashCode();
            result = 31 * result * representation.GetHashCode();

            return result;
        }
    }
}