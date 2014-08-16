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

namespace NForza.Transit
{
    /// <summary>
    /// Represents a transit tag and value. Returned by default when a reader encounters a tag for 
    /// which there is no registered <see cref="IReadHandler"/>. Can also be used in a 
    /// custom <see cref="IWriteHandler"/> implementation to force representation to use a transit 
    /// ground type using a representation for which there is no registered handler (e.g., an 
    /// enumerable for the representation of an array).
    /// </summary>
    public interface ITaggedValue
    {
        /// <summary>
        /// Gets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        string Tag { get; }

        /// <summary>
        /// Gets the representation.
        /// </summary>
        /// <value>
        /// The representation.
        /// </value>
        object Representation { get; }
    }
}