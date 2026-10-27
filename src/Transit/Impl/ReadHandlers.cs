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

using Beerendonk.Transit.Java;
using Beerendonk.Transit.Numerics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using System;

namespace Beerendonk.Transit.Impl;

internal static class ReadHandlers
{

    /// <summary>
    /// Represents a <see cref="BigInteger"/> read handler.
    /// </summary>
    internal class BigIntegerReadHandler : IReadHandler
    {
        /// <summary>
        /// Converts a transit value to an instance of <see cref="BigInteger"/>.
        /// </summary>
        /// <param name="representation">The transit value.</param>
        /// <returns>
        /// The converted object.
        /// </returns>
        /// <exception cref="Beerendonk.Transit.TransitException">Cannot parse representation as a BigInteger:  + representation</exception>
        public object FromRepresentation(object representation)
        {
            BigInteger result;
            if (!BigInteger.TryParse((string)representation, out result))
            {
                throw new TransitException("Cannot parse representation as a BigInteger: " + representation);
            }

            return result;
        }
    }


    /// <summary>
    /// Represents a <see cref="BigRational"/> read handler.
    /// </summary>
    internal class BigRationalReadHandler : IReadHandler
    {
        /// <summary>
        /// Converts a transit value to an instance of <see cref="BigRational"/>.
        /// </summary>
        /// <param name="representation">The transit value.</param>
        /// <returns>
        /// The converted object.
        /// </returns>
        /// <exception cref="Beerendonk.Transit.TransitException">Cannot parse representation as a BigRational.</exception>
        public object FromRepresentation(object representation)
        {
            BigRational result;
            if (!BigRational.TryParse((string)representation, out result))
            {
                throw new TransitException("Cannot parse representation as a BigRational: " + representation);
            }

            return result;
        }
    }


    /// <summary>
    /// Represents a binary read handler.
    /// </summary>
    internal class BinaryReadHandler : IReadHandler
    {
        /// <summary>
        /// Converts a transit value to a byte array.
        /// </summary>
        /// <param name="representation">The transit value.</param>
        /// <returns>
        /// The converted object.
        /// </returns>
        public object FromRepresentation(object representation)
        {
            return System.Convert.FromBase64String((string)representation);
        }
    }


    /// <summary>
    /// Represents a <see cref="bool"/> read handler.
    /// </summary>
    internal class BooleanReadHandler : IReadHandler
    {
        /// <summary>
        /// Converts a transit value to a <see cref="bool"/>.
        /// </summary>
        /// <param name="representation">The transit value.</param>
        /// <returns>
        /// The converted object.
        /// </returns>
        public object FromRepresentation(object representation)
        {
            return ((string)representation).Equals("t");
        }
    }


    /// <summary>
    /// Represents a read handler of a dictionary with composite keys.
    /// </summary>
    internal class CDictionaryReadHandler : IListReadHandler
    {
        /// <summary>
        /// Provides an <see cref="IListReader" /> that
        /// a parser can use to convert a list representation to
        /// an instance of a type incrementally.
        /// </summary>
        /// <returns>
        /// A ListReader.
        /// </returns>
        public IListReader ListReader()
        {
            return new ListReaderImpl();
        }

        /// <summary>
        /// Converts a transit value to an instance of dictionary with composite keys.
        /// </summary>
        /// <param name="representation">The transit value.</param>
        /// <returns>
        /// The converted object.
        /// </returns>
        /// <exception cref="System.NotSupportedException">This method is not supported.</exception>
        public object FromRepresentation(object representation)
        {
            throw new NotSupportedException();
        }

        private class ListReaderImpl : IListReader
        {
            private readonly object noPendingKey = new object();
            private TransitDictionary dictionary;
            private object nextKey;

            public object Init()
            {
                dictionary = new TransitDictionary();
                nextKey = noPendingKey;
                return this;
            }

            public object Add(object list, object item)
            {
                if (!ReferenceEquals(nextKey, noPendingKey))
                {
                    dictionary.Add(nextKey, item);
                    nextKey = noPendingKey;
                }
                else
                {
                    nextKey = item;
                }
                return this;
            }

            public object Complete(object list)
            {
                if (!ReferenceEquals(nextKey, noPendingKey))
                {
                    throw new TransitException("A composite map representation must contain key/value pairs.");
                }

                return dictionary;
            }
        }
    }


    /// <summary>
    /// Represents a <see cref="char"/> read handler.
    /// </summary>
    internal class CharacterReadHandler : IReadHandler
    {
        /// <summary>
        /// Converts a transit value to a <see cref="char"/>.
        /// </summary>
        /// <param name="representation">The transit value.</param>
        /// <returns>
        /// The converted object.
        /// </returns>
        public object FromRepresentation(object representation)
        {
            return ((string)representation)[0];
        }
    }


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


    /// <summary>
    /// Represents a <see cref="double"/> read handler.
    /// </summary>
    internal class DoubleReadHandler : IReadHandler
    {
        /// <summary>
        /// Converts a transit value to a <see cref="double"/>.
        /// </summary>
        /// <param name="representation">The transit value.</param>
        /// <returns>
        /// The converted object.
        /// </returns>
        /// <exception cref="TransitException">Cannot parse representation as a double:  + representation</exception>
        public object FromRepresentation(object representation)
        {
            double result;

            if (!double.TryParse((string)representation, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
            {
                throw new TransitException("Cannot parse representation as a double: " + representation);
            }

            return result;
        }
    }


    internal class GuidReadHandler : IReadHandler
    {
        public object FromRepresentation(object representation)
        {
            if (representation is string)
            {
                return new Guid((string)representation);
            }
            else
            {
                IImmutableList<object> l = (IImmutableList<object>)representation;
                return (Guid)new Uuid((long)l[0], (long)l[1]);
            }
        }
    }


    internal class IdentityReadHandler : IReadHandler
    {
        public object FromRepresentation(object representation)
        {
            return representation;
        }
    }


    internal class IntegerReadHandler : IReadHandler
    {
        public object FromRepresentation(object representation)
        {
            long result;
            if (!long.TryParse((string)representation, out result))
            {
                throw new TransitException("Cannot parse representation as an integer: " + representation);
            }

            return result;
        }
    }


    internal class KeywordReadHandler : IReadHandler
    {
        public object FromRepresentation(object representation)
        {
            return TransitFactory.Keyword((string)representation);
        }
    }


    internal class LinkReadHandler : IReadHandler
    {
        public object FromRepresentation(object representation)
        {
            var dictionaryRepresentation = (IImmutableDictionary<object, object>)representation;

            var dic = ImmutableDictionary.Create<string, object>();

            foreach (var item in dictionaryRepresentation)
            {
                dic = dic.Add((string)item.Key, item.Value);
            }

            return new Link(dic);
        }
    }


    /// <summary>
    /// Represents a list read handler.
    /// </summary>
    internal class ListReadHandler : IListReadHandler
    {
        /// <summary>
        /// Provides an <see cref="IListReader" /> that
        /// a parser can use to convert a list representation to
        /// an instance of a type incrementally.
        /// </summary>
        /// <returns>
        /// A ListReader.
        /// </returns>
        public IListReader ListReader()
        {
            return new ListReaderImpl();
        }

        /// <summary>
        /// Converts a transit value to an instance of a list.
        /// </summary>
        /// <param name="representation">The transit value.</param>
        /// <returns>
        /// The converted object.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public object FromRepresentation(object representation)
        {
            throw new NotSupportedException();
        }

        private class ListReaderImpl : IListReader
        {
            public object Init()
            {
                return ImmutableList.Create<Object>();
            }

            public object Add(object list, object item)
            {
                return ((IImmutableList<object>)list).Add(item);
            }

            public object Complete(object list)
            {
                return list;
            }
        }
    }


    internal class NullReadHandler : IReadHandler
    {
        public object FromRepresentation(object ignored)
        {
            return null;
        }
    }


    /// <summary>
    /// Represents a ratio read handler.
    /// </summary>
    internal class RatioReadHandler : IReadHandler
    {
        /// <summary>
        /// Converts a transit value to an instance of <see cref="Ratio"/>.
        /// </summary>
        /// <param name="representation">The transit value.</param>
        /// <returns>
        /// The converted object.
        /// </returns>
        public object FromRepresentation(object representation)
        {
            var rep = (IImmutableList<object>)representation;
            return new Ratio((BigInteger)rep[0], (BigInteger)rep[1]);
        }
    }


    /// <summary>
    /// Represents a set read handler.
    /// </summary>
    internal class SetReadHandler : IListReadHandler
    {
        /// <summary>
        /// Provides an <see cref="IListReader" /> that
        /// a parser can use to convert a list representation to
        /// an instance of a type incrementally.
        /// </summary>
        /// <returns>
        /// A ListReader.
        /// </returns>
        public IListReader ListReader()
        {
            return new ListReaderImpl();
        }

        /// <summary>
        /// Converts a transit value to an instance of a set.
        /// </summary>
        /// <param name="representation">The transit value.</param>
        /// <returns>
        /// The converted object.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public object FromRepresentation(object representation)
        {
            throw new NotSupportedException();
        }

        private class ListReaderImpl : IListReader
        {
            public object Init()
            {
                return ImmutableHashSet.Create<Object>();
            }

            public object Add(object list, object item)
            {
                return ((IImmutableSet<object>)list).Add(item);
            }

            public object Complete(object list)
            {
                return list;
            }
        }
    }


    internal class SpecialNumberReadHandler : IReadHandler
    {
        public object FromRepresentation(object representation)
        {
            if (representation.Equals("NaN"))
            {
                return double.NaN;
            }
            else if (representation.Equals("INF"))
            {
                return double.PositiveInfinity;
            }
            else if (representation.Equals("-INF"))
            {
                return double.NegativeInfinity;
            }

            throw new TransitException();
        }
    }


    internal class SymbolReadHandler : IReadHandler
    {
        public object FromRepresentation(object representation)
        {
            return TransitFactory.Symbol((string)representation);
        }
    }


    internal class UriReadHandler : IReadHandler
    {
        public object FromRepresentation(object representation)
        {
            return new Uri((string)representation, UriKind.RelativeOrAbsolute);
        }
    }


    internal class VerboseDateTimeReadHandler : IReadHandler
    {
        public object FromRepresentation(object representation)
        {
            DateTime result;
            if (!DateTime.TryParse((string)representation, out result))
            {
                throw new TransitException("Cannot parse representation as a DateTime: " + representation);
            }

            return result;
        }
    }
}
