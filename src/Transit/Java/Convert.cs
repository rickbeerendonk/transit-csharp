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

using System;

namespace Beerendonk.Transit.Java
{
    /// <summary>
    /// Contains conversations from Java to .NET and vice versa.
    /// </summary>
    internal static class Convert
    {
        private const long JavaTimePrecisionDifference = 10000;
        private static readonly long JavaTimeEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        /// <summary>
        /// Converts a <see cref="DateTime"/> to a Java time.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/></param>
        /// <returns>A Java time</returns>
        public static long ToJavaTime(DateTime value)
        {
            DateTime valueUtc = new DateTimeOffset(value).UtcDateTime;
            return (valueUtc.Ticks - JavaTimeEpochTicks) / JavaTimePrecisionDifference;
        }

        /// <summary>
        /// Converts a Java time to a <see cref="DateTime"/> in the local time zone.
        /// </summary>
        /// <param name="javaTime">A Java time</param>
        /// <returns>A <see cref="DateTime"/> in the local time zone</returns>
        public static DateTime FromJavaTimeToLocal(long javaTime)
        {
            return new DateTimeOffset(FromJavaTimeToUtc(javaTime)).LocalDateTime;
        }

        /// <summary>
        /// Converts a Java time to a <see cref="DateTime"/> in the UTC time zone.
        /// </summary>
        /// <param name="javaTime">A Java time</param>
        /// <returns>A <see cref="DateTime"/> in the UTC time zone</returns>
        public static DateTime FromJavaTimeToUtc(long javaTime)
        {
            return new DateTime(JavaTimePrecisionDifference * javaTime + JavaTimeEpochTicks, DateTimeKind.Utc);
        }
    }
}
