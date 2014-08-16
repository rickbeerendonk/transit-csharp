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

using NForza.Transit.Java;
using System;

namespace NForza.Transit.Impl.ReadHandlers
{
    /// <summary>
    /// Represents a <see cref="DateTime"/> read handler.
    /// </summary>
    internal class DateTimeReadHandler : IReadHandler
    {
        /// <summary>
        /// Converts a transit value to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="representation">The transit value.</param>
        /// <returns>
        /// The converted object.
        /// </returns>
        /// <exception cref="TransitException">Cannot parse representation as a long (DateTime):  + representation</exception>
        public object FromRepresentation(object representation)
        {
            long n;
            if (representation is long)
            {
                n = (long)representation;
            }
            else
            {
                if (!long.TryParse((string)representation, out n))
                {
                    throw new TransitException("Cannot parse representation as a long (DateTime): " + representation);
                }
            }

            return Java.Convert.FromJavaTimeToLocal(n);
        }
    }
}