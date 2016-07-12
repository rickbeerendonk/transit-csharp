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

using Beerendonk.Transit.Numerics;
using System;
using System.Numerics;

namespace Beerendonk.Transit.Impl
{
    /// <summary>
    /// Represents a ratio.
    /// </summary>
    internal class Ratio : IRatio, IComparable, IComparable<IRatio>, IEquatable<IRatio>
    {
        private readonly BigInteger numerator;
        private readonly BigInteger denominator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ratio"/> class.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        public Ratio(BigInteger numerator, BigInteger denominator) 
        {
            this.numerator = numerator;
            this.denominator = denominator;
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
            return obj is Ratio && ((Ratio)obj).Numerator == numerator && ((Ratio)obj).Denominator == denominator;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        /// <summary>
        /// The value of the ratio as double.
        /// </summary>
        /// <returns>
        /// A double.
        /// </returns>
        public double GetValue()
        {
            return (double)(new BigRational(numerator, denominator));
        }

        /// <summary>
        /// Gets the numerator.
        /// </summary>
        /// <value>
        /// The numerator.
        /// </value>
        public BigInteger Numerator
        {
            get { return numerator; }
        }

        /// <summary>
        /// Gets the denominator.
        /// </summary>
        /// <value>
        /// The denominator.
        /// </value>
        public BigInteger Denominator
        {
            get { return denominator; }
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.
        /// </returns>
        public int CompareTo(IRatio other)
        {
            return GetValue().CompareTo(other.GetValue());
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        /// <exception cref="System.ArgumentException">obj must be an IRatio.</exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is IRatio))
            {
                throw new ArgumentException("obj must be an IRatio.");
            }

            return CompareTo((IRatio)obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(IRatio other)
        {
            return CompareTo(other) == 0;
        }
    }
}
