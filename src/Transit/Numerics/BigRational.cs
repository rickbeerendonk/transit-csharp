// Copyright (c) Microsoft Corporation.  All rights reserved.
// Source: http://bcl.codeplex.com/

// TryParse source: http://codereview.stackexchange.com/questions/24002/how-to-make-this-bigrational-parsing-method-more-efficient

/*============================================================
** Class: BigRational
**
** Purpose: 
** --------
** This class is used to represent an arbitrary precision
** BigRational number
**
** A rational number (commonly called a fraction) is a ratio
** between two integers.  For example (3/6) = (2/4) = (1/2)
**
** Arithmetic
** ----------
** a/b = c/d, iff ad = bc
** a/b + c/d  == (ad + bc)/bd
** a/b - c/d  == (ad - bc)/bd
** a/b % c/d  == (ad % bc)/bd
** a/b * c/d  == (ac)/(bd)
** a/b / c/d  == (ad)/(bc)
** -(a/b)     == (-a)/b
** (a/b)^(-1) == b/a, if a != 0
**
** Reduction Algorithm
** ------------------------
** Euclid's algorithm is used to simplify the fraction.
** Calculating the greatest common divisor of two n-digit
** numbers can be found in
**
** O(n(log n)^5 (log log n)) steps as n -> +infinity
============================================================*/

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Beerendonk.Transit.Numerics
{
    /// <summary>
    /// Represents a rational with infinite precision.
    /// </summary>
    [ComVisible(false)]
    public struct BigRational : IComparable, IComparable<BigRational>, IEquatable<BigRational>
    {

        // ---- SECTION:  members supporting exposed properties -------------*
        private BigInteger m_numerator;
        private BigInteger m_denominator;

        private static readonly BigRational s_brZero = new BigRational(BigInteger.Zero);
        private static readonly BigRational s_brOne = new BigRational(BigInteger.One);
        private static readonly BigRational s_brMinusOne = new BigRational(BigInteger.MinusOne);

        // ---- SECTION:  members for internal support ---------*
        #region Members for Internal Support
        [StructLayout(LayoutKind.Explicit)]
        internal struct DoubleUlong
        {
            [FieldOffset(0)]
            public double dbl;
            [FieldOffset(0)]
            public ulong uu;
        }
        private const int DoubleMaxScale = 308;
        private static readonly BigInteger s_bnDoublePrecision = BigInteger.Pow(10, DoubleMaxScale);
        private static readonly BigInteger s_bnDoubleMaxValue = (BigInteger)Double.MaxValue;
        private static readonly BigInteger s_bnDoubleMinValue = (BigInteger)Double.MinValue;

        [StructLayout(LayoutKind.Explicit)]
        internal struct DecimalUInt32
        {
            [FieldOffset(0)]
            public Decimal dec;
            [FieldOffset(0)]
            public int flags;
        }
        private const int DecimalScaleMask = 0x00FF0000;
        private const int DecimalSignMask = unchecked((int)0x80000000);
        private const int DecimalMaxScale = 28;
        private static readonly BigInteger s_bnDecimalPrecision = BigInteger.Pow(10, DecimalMaxScale);
        private static readonly BigInteger s_bnDecimalMaxValue = (BigInteger)Decimal.MaxValue;
        private static readonly BigInteger s_bnDecimalMinValue = (BigInteger)Decimal.MinValue;

        private const String c_solidus = @"/";
        #endregion Members for Internal Support

        // ---- SECTION: public properties --------------*
        #region Public Properties
        /// <summary>
        /// Gets zero as a big rational.
        /// </summary>
        /// <value>
        /// The zero.
        /// </value>
        public static BigRational Zero
        {
            get
            {
                return s_brZero;
            }
        }

        /// <summary>
        /// Gets one as a big rational.
        /// </summary>
        /// <value>
        /// The one.
        /// </value>
        public static BigRational One
        {
            get
            {
                return s_brOne;
            }
        }

        /// <summary>
        /// Gets minus one as a big rational.
        /// </summary>
        /// <value>
        /// The minus one.
        /// </value>
        public static BigRational MinusOne
        {
            get
            {
                return s_brMinusOne;
            }
        }

        /// <summary>
        /// Gets the sign.
        /// </summary>
        /// <value>
        /// The sign.
        /// </value>
        public Int32 Sign
        {
            get
            {
                return m_numerator.Sign;
            }
        }

        /// <summary>
        /// Gets the numerator.
        /// </summary>
        /// <value>
        /// The numerator.
        /// </value>
        public BigInteger Numerator
        {
            get
            {
                return m_numerator;
            }
        }

        /// <summary>
        /// Gets the denominator.
        /// </summary>
        /// <value>
        /// The denominator.
        /// </value>
        public BigInteger Denominator
        {
            get
            {
                return m_denominator;
            }
        }

        #endregion Public Properties

        // ---- SECTION: public instance methods --------------*
        #region Public Instance Methods

        /// <summary>
        /// <code>
        /// GetWholePart() and GetFractionPart()
        /// BigRational == Whole, Fraction
        /// 0/2        ==     0,  0/2
        /// 1/2        ==     0,  1/2
        /// -1/2        ==     0, -1/2
        /// 1/1        ==     1,  0/1
        /// -1/1        ==    -1,  0/1
        /// -3/2        ==    -1, -1/2
        /// 3/2        ==     1,  1/2
        /// </code>
        /// </summary>
        /// <returns></returns>
        public BigInteger GetWholePart()
        {
            return BigInteger.Divide(m_numerator, m_denominator);
        }

        /// <summary>
        /// <code>
        /// GetWholePart() and GetFractionPart()
        /// BigRational == Whole, Fraction
        /// 0/2        ==     0,  0/2
        /// 1/2        ==     0,  1/2
        /// -1/2        ==     0, -1/2
        /// 1/1        ==     1,  0/1
        /// -1/1        ==    -1,  0/1
        /// -3/2        ==    -1, -1/2
        /// 3/2        ==     1,  1/2
        /// </code>
        /// </summary>
        /// <returns></returns>
        public BigRational GetFractionPart()
        {
            return new BigRational(BigInteger.Remainder(m_numerator, m_denominator), m_denominator);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is BigRational))
                return false;
            return this.Equals((BigRational)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return (m_numerator / Denominator).GetHashCode();
        }

        // IComparable
        int IComparable.CompareTo(Object obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is BigRational))
                throw new ArgumentException("Argument must be of type BigRational", "obj");
            return Compare(this, (BigRational)obj);
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.
        /// </returns>
        public int CompareTo(BigRational other)
        {
            return Compare(this, other);
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String" /> containing a fully qualified type name.
        /// </returns>
        public override String ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append(m_numerator.ToString("R", CultureInfo.InvariantCulture));
            ret.Append(c_solidus);
            ret.Append(Denominator.ToString("R", CultureInfo.InvariantCulture));
            return ret.ToString();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// a/b = c/d, iff ad = bc
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public Boolean Equals(BigRational other)
        {
            if (this.Denominator == other.Denominator)
            {
                return m_numerator == other.m_numerator;
            }
            else
            {
                return (m_numerator * other.Denominator) == (Denominator * other.m_numerator);
            }
        }

        #endregion Public Instance Methods

        // -------- SECTION: constructors -----------------*
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BigRational"/> struct.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        public BigRational(BigInteger numerator)
        {
            m_numerator = numerator;
            m_denominator = BigInteger.One;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigRational"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentException">
        /// Argument is not a number;value
        /// or
        /// Argument is infinity;value
        /// </exception>
        public BigRational(Double value)
        {
            if (Double.IsNaN(value))
            {
                throw new ArgumentException("Argument is not a number", "value");
            }
            else if (Double.IsInfinity(value))
            {
                throw new ArgumentException("Argument is infinity", "value");
            }

            bool isFinite;
            int sign;
            int exponent;
            ulong significand;
            SplitDoubleIntoParts(value, out sign, out exponent, out significand, out isFinite);

            if (significand == 0)
            {
                this = BigRational.Zero;
                return;
            }

            m_numerator = significand;
            m_denominator = 1 << 52;

            if (exponent > 0)
            {
                m_numerator = BigInteger.Pow(m_numerator, exponent);
            }
            else if (exponent < 0)
            {
                m_denominator = BigInteger.Pow(m_denominator, -exponent);
            }
            if (sign < 0)
            {
                m_numerator = BigInteger.Negate(m_numerator);
            }
            Simplify();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigRational"/> struct.
        /// The Decimal type represents floating point numbers exactly, with no rounding error.
        /// Values such as "0.1" in Decimal are actually representable, and convert cleanly
        /// to BigRational as "11/10"
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentException">invalid Decimal;value</exception>
        public BigRational(Decimal value)
        {
            int[] bits = Decimal.GetBits(value);
            if (bits == null || bits.Length != 4 || (bits[3] & ~(DecimalSignMask | DecimalScaleMask)) != 0 || (bits[3] & DecimalScaleMask) > (28 << 16))
            {
                throw new ArgumentException("invalid Decimal", "value");
            }

            if (value == Decimal.Zero)
            {
                this = BigRational.Zero;
                return;
            }

            // build up the numerator
            ulong ul = (((ulong)(uint)bits[2]) << 32) | ((ulong)(uint)bits[1]);   // (hi    << 32) | (mid)
            m_numerator = (new BigInteger(ul) << 32) | (uint)bits[0];             // (hiMid << 32) | (low)

            bool isNegative = (bits[3] & DecimalSignMask) != 0;
            if (isNegative)
            {
                m_numerator = BigInteger.Negate(m_numerator);
            }

            // build up the denominator
            int scale = (bits[3] & DecimalScaleMask) >> 16;     // 0-28, power of 10 to divide numerator by
            m_denominator = BigInteger.Pow(10, scale);

            Simplify();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigRational"/> struct.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <exception cref="System.DivideByZeroException"></exception>
        public BigRational(BigInteger numerator, BigInteger denominator)
        {
            if (denominator.Sign == 0)
            {
                throw new DivideByZeroException();
            }
            else if (numerator.Sign == 0)
            {
                // 0/m -> 0/1
                m_numerator = BigInteger.Zero;
                m_denominator = BigInteger.One;
            }
            else if (denominator.Sign < 0)
            {
                m_numerator = BigInteger.Negate(numerator);
                m_denominator = BigInteger.Negate(denominator);
            }
            else
            {
                m_numerator = numerator;
                m_denominator = denominator;
            }
            Simplify();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigRational"/> struct.
        /// </summary>
        /// <param name="whole">The whole.</param>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <exception cref="System.DivideByZeroException"></exception>
        public BigRational(BigInteger whole, BigInteger numerator, BigInteger denominator)
        {
            if (denominator.Sign == 0)
            {
                throw new DivideByZeroException();
            }
            else if (numerator.Sign == 0 && whole.Sign == 0)
            {
                m_numerator = BigInteger.Zero;
                m_denominator = BigInteger.One;
            }
            else if (denominator.Sign < 0)
            {
                m_denominator = BigInteger.Negate(denominator);
                m_numerator = (BigInteger.Negate(whole) * m_denominator) + BigInteger.Negate(numerator);
            }
            else
            {
                m_denominator = denominator;
                m_numerator = (whole * denominator) + numerator;
            }
            Simplify();
        }
        #endregion Constructors

        // -------- SECTION: public static methods -----------------*
        #region Public Static Methods

        /// <summary>
        /// Get the absolute value of the specified r.
        /// </summary>
        /// <param name="r">The big rational.</param>
        /// <returns>The absolute value.</returns>
        public static BigRational Abs(BigRational r)
        {
            return (r.m_numerator.Sign < 0 ? new BigRational(BigInteger.Abs(r.m_numerator), r.Denominator) : r);
        }

        /// <summary>
        /// Negates the specified r.
        /// </summary>
        /// <param name="r">The big rational.</param>
        /// <returns>The negate.</returns>
        public static BigRational Negate(BigRational r)
        {
            return new BigRational(BigInteger.Negate(r.m_numerator), r.Denominator);
        }

        /// <summary>
        /// Inverts the specified r.
        /// </summary>
        /// <param name="r">The big rational.</param>
        /// <returns>The invert.</returns>
        public static BigRational Invert(BigRational r)
        {
            return new BigRational(r.Denominator, r.m_numerator);
        }

        /// <summary>
        /// Adds the specified x and y.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>x + y</returns>
        public static BigRational Add(BigRational x, BigRational y)
        {
            return x + y;
        }

        /// <summary>
        /// Subtracts y from the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>x - y</returns>
        public static BigRational Subtract(BigRational x, BigRational y)
        {
            return x - y;
        }


        /// <summary>
        /// Multiplies the specified x and y.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>x * y</returns>
        public static BigRational Multiply(BigRational x, BigRational y)
        {
            return x * y;
        }

        /// <summary>
        /// Divides the specified dividend.
        /// </summary>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        /// <returns>dividend / divisor</returns>
        public static BigRational Divide(BigRational dividend, BigRational divisor)
        {
            return dividend / divisor;
        }

        /// <summary>
        /// Calculates the remainder.
        /// </summary>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        /// <returns>dividend % divisor</returns>
        public static BigRational Remainder(BigRational dividend, BigRational divisor)
        {
            return dividend % divisor;
        }

        /// <summary>
        /// Get the remainder of the division.
        /// </summary>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        /// <param name="remainder">The remainder.</param>
        /// <returns></returns>
        public static BigRational DivRem(BigRational dividend, BigRational divisor, out BigRational remainder)
        {
            // a/b / c/d  == (ad)/(bc)
            // a/b % c/d  == (ad % bc)/bd

            // (ad) and (bc) need to be calculated for both the division and the remainder operations.
            BigInteger ad = dividend.m_numerator * divisor.Denominator;
            BigInteger bc = dividend.Denominator * divisor.m_numerator;
            BigInteger bd = dividend.Denominator * divisor.Denominator;

            remainder = new BigRational(ad % bc, bd);
            return new BigRational(ad, bc);
        }


        /// <summary>
        /// Gets the powers the specified base value.
        /// </summary>
        /// <param name="baseValue">The base value.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">cannot raise zero to a negative power;baseValue</exception>
        public static BigRational Pow(BigRational baseValue, BigInteger exponent)
        {
            if (exponent.Sign == 0)
            {
                // 0^0 -> 1
                // n^0 -> 1
                return BigRational.One;
            }
            else if (exponent.Sign < 0)
            {
                if (baseValue == BigRational.Zero)
                {
                    throw new ArgumentException("cannot raise zero to a negative power", "baseValue");
                }
                // n^(-e) -> (1/n)^e
                baseValue = BigRational.Invert(baseValue);
                exponent = BigInteger.Negate(exponent);
            }

            BigRational result = baseValue;
            while (exponent > BigInteger.One)
            {
                result = result * baseValue;
                exponent--;
            }

            return result;
        }

        /// <summary>
        /// Least Common Denominator (LCD)
        /// The LCD is the least common multiple of the two denominators.  For instance, the LCD of
        /// {1/2, 1/4} is 4 because the least common multiple of 2 and 4 is 4.  Likewise, the LCD
        /// of {1/2, 1/3} is 6.
        /// To find the LCD:
        /// 1) Find the Greatest Common Divisor (GCD) of the denominators
        /// 2) Multiply the denominators together
        /// 3) Divide the product of the denominators by the GCD
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public static BigInteger LeastCommonDenominator(BigRational x, BigRational y)
        {
            // LCD( a/b, c/d ) == (bd) / gcd(b,d)
            return (x.Denominator * y.Denominator) / BigInteger.GreatestCommonDivisor(x.Denominator, y.Denominator);
        }

        /// <summary>
        /// Compares the specified r1.
        /// </summary>
        /// <param name="r1">The r1.</param>
        /// <param name="r2">The r2.</param>
        /// <returns></returns>
        public static int Compare(BigRational r1, BigRational r2)
        {
            //     a/b = c/d, iff ad = bc
            return BigInteger.Compare(r1.m_numerator * r2.Denominator, r2.m_numerator * r1.Denominator);
        }
        #endregion Public Static Methods

        #region Operator Overloads
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(BigRational x, BigRational y)
        {
            return Compare(x, y) == 0;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(BigRational x, BigRational y)
        {
            return Compare(x, y) != 0;
        }

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <(BigRational x, BigRational y)
        {
            return Compare(x, y) < 0;
        }

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <=(BigRational x, BigRational y)
        {
            return Compare(x, y) <= 0;
        }

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >(BigRational x, BigRational y)
        {
            return Compare(x, y) > 0;
        }

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >=(BigRational x, BigRational y)
        {
            return Compare(x, y) >= 0;
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static BigRational operator +(BigRational r)
        {
            return r;
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static BigRational operator -(BigRational r)
        {
            return new BigRational(-r.m_numerator, r.Denominator);
        }

        /// <summary>
        /// Implements the operator ++.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static BigRational operator ++(BigRational r)
        {
            return r + BigRational.One;
        }

        /// <summary>
        /// Implements the operator --.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static BigRational operator --(BigRational r)
        {
            return r - BigRational.One;
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="r1">The r1.</param>
        /// <param name="r2">The r2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static BigRational operator +(BigRational r1, BigRational r2)
        {
            // a/b + c/d  == (ad + bc)/bd
            return new BigRational((r1.m_numerator * r2.Denominator) + (r1.Denominator * r2.m_numerator), (r1.Denominator * r2.Denominator));
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="r1">The r1.</param>
        /// <param name="r2">The r2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static BigRational operator -(BigRational r1, BigRational r2)
        {
            // a/b - c/d  == (ad - bc)/bd
            return new BigRational((r1.m_numerator * r2.Denominator) - (r1.Denominator * r2.m_numerator), (r1.Denominator * r2.Denominator));
        }

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="r1">The r1.</param>
        /// <param name="r2">The r2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static BigRational operator *(BigRational r1, BigRational r2)
        {
            // a/b * c/d  == (ac)/(bd)
            return new BigRational((r1.m_numerator * r2.m_numerator), (r1.Denominator * r2.Denominator));
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="r1">The r1.</param>
        /// <param name="r2">The r2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static BigRational operator /(BigRational r1, BigRational r2)
        {
            // a/b / c/d  == (ad)/(bc)
            return new BigRational((r1.m_numerator * r2.Denominator), (r1.Denominator * r2.m_numerator));
        }

        /// <summary>
        /// Implements the operator %.
        /// </summary>
        /// <param name="r1">The r1.</param>
        /// <param name="r2">The r2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static BigRational operator %(BigRational r1, BigRational r2)
        {
            // a/b % c/d  == (ad % bc)/bd
            return new BigRational((r1.m_numerator * r2.Denominator) % (r1.Denominator * r2.m_numerator), (r1.Denominator * r2.Denominator));
        }
        #endregion Operator Overloads

        // ----- SECTION: explicit conversions from BigRational to numeric base types  ----------------*
        #region explicit conversions from BigRational
        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="SByte"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator SByte(BigRational value)
        {
            return (SByte)(BigInteger.Divide(value.m_numerator, value.m_denominator));
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="UInt16"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator UInt16(BigRational value)
        {
            return (UInt16)(BigInteger.Divide(value.m_numerator, value.m_denominator));
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="UInt32"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator UInt32(BigRational value)
        {
            return (UInt32)(BigInteger.Divide(value.m_numerator, value.m_denominator));
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="UInt64"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator UInt64(BigRational value)
        {
            return (UInt64)(BigInteger.Divide(value.m_numerator, value.m_denominator));
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="Byte"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Byte(BigRational value)
        {
            return (Byte)(BigInteger.Divide(value.m_numerator, value.m_denominator));
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="Int16"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Int16(BigRational value)
        {
            return (Int16)(BigInteger.Divide(value.m_numerator, value.m_denominator));
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="Int32"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Int32(BigRational value)
        {
            return (Int32)(BigInteger.Divide(value.m_numerator, value.m_denominator));
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="Int64"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Int64(BigRational value)
        {
            return (Int64)(BigInteger.Divide(value.m_numerator, value.m_denominator));
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="BigInteger"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator BigInteger(BigRational value)
        {
            return BigInteger.Divide(value.m_numerator, value.m_denominator);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="Single"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Single(BigRational value)
        {
            // The Single value type represents a single-precision 32-bit number with
            // values ranging from negative 3.402823e38 to positive 3.402823e38      
            // values that do not fit into this range are returned as Infinity
            return (Single)((Double)value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="Double"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Double(BigRational value)
        {
            // The Double value type represents a double-precision 64-bit number with
            // values ranging from -1.79769313486232e308 to +1.79769313486232e308
            // values that do not fit into this range are returned as +/-Infinity
            if (SafeCastToDouble(value.m_numerator) && SafeCastToDouble(value.m_denominator))
            {
                return (Double)value.m_numerator / (Double)value.m_denominator;
            }

            // scale the numerator to preseve the fraction part through the integer division
            BigInteger denormalized = (value.m_numerator * s_bnDoublePrecision) / value.m_denominator;
            if (denormalized.IsZero)
                return (value.Sign < 0) ? BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000)) : 0d; // underflow to -+0

            Double result = 0;
            bool isDouble = false;
            int scale = DoubleMaxScale;

            while (scale > 0)
            {
                if (!isDouble)
                {
                    if (SafeCastToDouble(denormalized))
                    {
                        result = (Double)denormalized;
                        isDouble = true;
                    }
                    else
                    {
                        denormalized = denormalized / 10;
                    }
                }
                result = result / 10;
                scale--;
            }

            if (!isDouble)
                return (value.Sign < 0) ? Double.NegativeInfinity : Double.PositiveInfinity;
            else
                return result;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BigRational"/> to <see cref="Decimal"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        /// <exception cref="System.OverflowException">Value was either too large or too small for a Decimal.</exception>
        public static explicit operator Decimal(BigRational value)
        {
            // The Decimal value type represents decimal numbers ranging
            // from +79,228,162,514,264,337,593,543,950,335 to -79,228,162,514,264,337,593,543,950,335
            // the binary representation of a Decimal value is of the form, ((-2^96 to 2^96) / 10^(0 to 28))
            if (SafeCastToDecimal(value.m_numerator) && SafeCastToDecimal(value.m_denominator))
            {
                return (Decimal)value.m_numerator / (Decimal)value.m_denominator;
            }

            // scale the numerator to preseve the fraction part through the integer division
            BigInteger denormalized = (value.m_numerator * s_bnDecimalPrecision) / value.m_denominator;
            if (denormalized.IsZero)
            {
                return Decimal.Zero; // underflow - fraction is too small to fit in a decimal
            }
            for (int scale = DecimalMaxScale; scale >= 0; scale--)
            {
                if (!SafeCastToDecimal(denormalized))
                {
                    denormalized = denormalized / 10;
                }
                else
                {
                    DecimalUInt32 dec = new DecimalUInt32();
                    dec.dec = (Decimal)denormalized;
                    dec.flags = (dec.flags & ~DecimalScaleMask) | (scale << 16);
                    return dec.dec;
                }
            }
            throw new OverflowException("Value was either too large or too small for a Decimal.");
        }
        #endregion explicit conversions from BigRational

        // ----- SECTION: implicit conversions from numeric base types to BigRational  ----------------*
        #region implicit conversions to BigRational

        /// <summary>
        /// Performs an implicit conversion from <see cref="SByte"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(SByte value)
        {
            return new BigRational((BigInteger)value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UInt16"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(UInt16 value)
        {
            return new BigRational((BigInteger)value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UInt32"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(UInt32 value)
        {
            return new BigRational((BigInteger)value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UInt64"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(UInt64 value)
        {
            return new BigRational((BigInteger)value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Byte"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(Byte value)
        {
            return new BigRational((BigInteger)value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Int16"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(Int16 value)
        {
            return new BigRational((BigInteger)value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Int32"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(Int32 value)
        {
            return new BigRational((BigInteger)value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Int64"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(Int64 value)
        {
            return new BigRational((BigInteger)value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="BigInteger"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(BigInteger value)
        {
            return new BigRational(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Single"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(Single value)
        {
            return new BigRational((Double)value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Double"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(Double value)
        {
            return new BigRational(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Decimal"/> to <see cref="BigRational"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BigRational(Decimal value)
        {
            return new BigRational(value);
        }

        #endregion implicit conversions to BigRational

        // ----- SECTION: private instance utility methods ----------------*
        #region instance helper methods
        private void Simplify()
        {
            // * if the numerator is {0, +1, -1} then the fraction is already reduced
            // * if the denominator is {+1} then the fraction is already reduced
            if (m_numerator == BigInteger.Zero)
            {
                m_denominator = BigInteger.One;
            }

            BigInteger gcd = BigInteger.GreatestCommonDivisor(m_numerator, m_denominator);
            if (gcd > BigInteger.One)
            {
                m_numerator = m_numerator / gcd;
                m_denominator = Denominator / gcd;
            }
        }
        #endregion instance helper methods

        // ----- SECTION: private static utility methods -----------------*
        #region static helper methods
        private static bool SafeCastToDouble(BigInteger value)
        {
            return s_bnDoubleMinValue <= value && value <= s_bnDoubleMaxValue;
        }

        private static bool SafeCastToDecimal(BigInteger value)
        {
            return s_bnDecimalMinValue <= value && value <= s_bnDecimalMaxValue;
        }

        private static void SplitDoubleIntoParts(double dbl, out int sign, out int exp, out ulong man, out bool isFinite)
        {
            DoubleUlong du;
            du.uu = 0;
            du.dbl = dbl;

            sign = 1 - ((int)(du.uu >> 62) & 2);
            man = du.uu & 0x000FFFFFFFFFFFFF;
            exp = (int)(du.uu >> 52) & 0x7FF;
            if (exp == 0)
            {
                // Denormalized number.
                isFinite = true;
                if (man != 0)
                    exp = -1074;
            }
            else if (exp == 0x7FF)
            {
                // NaN or Infinite.
                isFinite = false;
                exp = Int32.MaxValue;
            }
            else
            {
                isFinite = true;
                man |= 0x0010000000000000; // mask in the implied leading 53rd significand bit
                exp -= 1075;
            }
        }

        private static double GetDoubleFromParts(int sign, int exp, ulong man)
        {
            DoubleUlong du;
            du.dbl = 0;

            if (man == 0)
            {
                du.uu = 0;
            }
            else
            {
                // Normalize so that 0x0010 0000 0000 0000 is the highest bit set
                int cbitShift = CbitHighZero(man) - 11;
                if (cbitShift < 0)
                    man >>= -cbitShift;
                else
                    man <<= cbitShift;

                // Move the point to just behind the leading 1: 0x001.0 0000 0000 0000
                // (52 bits) and skew the exponent (by 0x3FF == 1023)
                exp += 1075;

                if (exp >= 0x7FF)
                {
                    // Infinity
                    du.uu = 0x7FF0000000000000;
                }
                else if (exp <= 0)
                {
                    // Denormalized
                    exp--;
                    if (exp < -52)
                    {
                        // Underflow to zero
                        du.uu = 0;
                    }
                    else
                    {
                        du.uu = man >> -exp;
                    }
                }
                else
                {
                    // Mask off the implicit high bit
                    du.uu = (man & 0x000FFFFFFFFFFFFF) | ((ulong)exp << 52);
                }
            }

            if (sign < 0)
            {
                du.uu |= 0x8000000000000000;
            }

            return du.dbl;
        }

        private static int CbitHighZero(ulong uu)
        {
            if ((uu & 0xFFFFFFFF00000000) == 0)
                return 32 + CbitHighZero((uint)uu);
            return CbitHighZero((uint)(uu >> 32));
        }

        private static int CbitHighZero(uint u)
        {
            if (u == 0)
                return 32;

            int cbit = 0;
            if ((u & 0xFFFF0000) == 0)
            {
                cbit += 16;
                u <<= 16;
            }
            if ((u & 0xFF000000) == 0)
            {
                cbit += 8;
                u <<= 8;
            }
            if ((u & 0xF0000000) == 0)
            {
                cbit += 4;
                u <<= 4;
            }
            if ((u & 0xC0000000) == 0)
            {
                cbit += 2;
                u <<= 2;
            }
            if ((u & 0x80000000) == 0)
                cbit += 1;
            return cbit;
        }

        #endregion static helper methods

        #region parse

        private static Regex DigitDotDigit = new Regex(@"^(\-|\+)?(\d+\.\d+)$");
        private static Regex PlainDigit = new Regex(@"^(\-|\+)?\d+$");
        private static Regex DigitSlashDigit = new Regex(@"^(\-|\+)?\d+/\d+$");
        private static Regex DotDigit = new Regex(@"^(\-|\+)?(\.\d+)");

        /// <summary>
        /// Tries to parse.
        /// </summary>
        /// <param name="parse">The parse.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c>.</returns>
        public static bool TryParse(string parse, out BigRational result)
        {
            if (DigitDotDigit.IsMatch(parse))
            {
                int toSubtract;
                bool isNegative = false;
                string[] parts = parse.TrimStart('+').Split('.');
                parts[1] = parts[1].TrimEnd('0');
                if (parts[0].StartsWith("-"))
                {
                    isNegative = true;
                    parts[0] = parts[0].Substring(1);
                }

                BigRational whole = new BigRational(BigInteger.Parse(parts[0]), BigInteger.Zero, BigInteger.One);
                BigRational decimalPart = new BigRational(BigInteger.Parse(parts[1]), BigInteger.Zero, BigInteger.One);
                toSubtract = parts[1].Length;
                while (toSubtract != 0)
                {
                    decimalPart /= 10;
                    toSubtract = toSubtract - 1;
                }

                result = whole + decimalPart;
                if (isNegative)
                {
                    result = -result;
                }

                return true;
            }
            else if (DotDigit.IsMatch(parse))
            {
                return TryParse("0" + parse, out result);
            }
            else if (PlainDigit.IsMatch(parse))
            {
                parse = parse.TrimStart('+');
                if (parse.StartsWith("-"))
                {
                    result = new BigRational(-BigInteger.Parse(parse), BigInteger.Zero, BigInteger.One);
                    return true;
                }

                result = new BigRational(BigInteger.Parse(parse), BigInteger.Zero, BigInteger.One);
                return true;
            }
            else if (DigitSlashDigit.IsMatch(parse))
            {
                string[] parts = parse.TrimStart('+').Split('/');
                if (parts[0].StartsWith("-"))
                {
                    parts[0] = parts[0].Substring(1);
                    result = -(new BigRational(BigInteger.Parse(parts[0]), BigInteger.Parse(parts[1])));
                    return true;
                }

                result = new BigRational(BigInteger.Parse(parts[0]), BigInteger.Parse(parts[1]));
                return true;
            }
            result = BigInteger.Zero;
            return false;
        }

        #endregion parse

    }
}
