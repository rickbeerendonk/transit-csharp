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

namespace Beerendonk.Transit
{
    /// <summary>
    /// Converts an instance of an type to a transit representation.
    /// </summary>
    public interface IWriteHandler
    {
        /// <summary>
        /// The tag to use for the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The tag.</returns>
        string Tag(object obj);

        /// <summary>
        /// Gets the representation to use for the object, either an instance of transit ground type,
        /// or object for which there is a Handler (including an instance of <see cref="ITaggedValue"/>).
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The representation.</returns>
        object Representation(object obj);

        /// <summary>
        /// Gets the string representation to use for the object; can return null.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The representation.</returns>
        string StringRepresentation(object obj);

        /// <summary>
        /// Gets an alternative handler which provides more readable representations for use in
        /// verbose mode; can return null.
        /// </summary>
        /// <returns>A handler.</returns>
        IWriteHandler GetVerboseHandler();
    }
}
