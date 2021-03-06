﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI.Tests.Utilities
{
    /// <summary>Unit tests for <see cref="SDate"/>.</summary>
    [TestFixture]
    internal class SDateTests
    {
        /*********
        ** Properties
        *********/
        /// <summary>All valid seasons.</summary>
        private static readonly string[] ValidSeasons = { "spring", "summer", "fall", "winter" };

        /// <summary>All valid days of a month.</summary>
        private static readonly int[] ValidDays = Enumerable.Range(1, 28).ToArray();

        /// <summary>Sample relative dates for test cases.</summary>
        private static class Dates
        {
            /// <summary>The base date to which other dates are relative.</summary>
            public const string Now = "02 summer Y2";

            /// <summary>The day before <see cref="Now"/>.</summary>
            public const string PrevDay = "01 summer Y2";

            /// <summary>The month before <see cref="Now"/>.</summary>
            public const string PrevMonth = "02 spring Y2";

            /// <summary>The year before <see cref="Now"/>.</summary>
            public const string PrevYear = "02 summer Y1";

            /// <summary>The day after <see cref="Now"/>.</summary>
            public const string NextDay = "03 summer Y2";

            /// <summary>The month after <see cref="Now"/>.</summary>
            public const string NextMonth = "02 fall Y2";

            /// <summary>The year after <see cref="Now"/>.</summary>
            public const string NextYear = "02 summer Y3";
        }


        /*********
        ** Unit tests
        *********/
        /****
        ** Constructor
        ****/
        [Test(Description = "Assert that the constructor sets the expected values for all valid dates.")]
        public void Constructor_SetsExpectedValues([ValueSource(nameof(SDateTests.ValidSeasons))] string season, [ValueSource(nameof(SDateTests.ValidDays))] int day, [Values(1, 2, 100)] int year)
        {
            // act
            SDate date = new SDate(day, season, year);

            // assert
            Assert.AreEqual(day, date.Day);
            Assert.AreEqual(season, date.Season);
            Assert.AreEqual(year, date.Year);
        }

        [Test(Description = "Assert that the constructor throws an exception if the values are invalid.")]
        [TestCase(01, "Spring", 1)] // seasons are case-sensitive
        [TestCase(01, "springs", 1)] // invalid season name
        [TestCase(-1, "spring", 1)] // day < 0
        [TestCase(29, "spring", 1)] // day > 28
        [TestCase(01, "spring", -1)] // year < 1
        [TestCase(01, "spring", 0)] // year < 1
        [SuppressMessage("ReSharper", "AssignmentIsFullyDiscarded", Justification = "Deliberate for unit test.")]
        public void Constructor_RejectsInvalidValues(int day, string season, int year)
        {
            // act & assert
            Assert.Throws<ArgumentException>(() => _ = new SDate(day, season, year), "Constructing the invalid date didn't throw the expected exception.");
        }

        /****
        ** ToString
        ****/
        [Test(Description = "Assert that ToString returns the expected string.")]
        [TestCase("14 spring Y1", ExpectedResult = "14 spring Y1")]
        [TestCase("01 summer Y16", ExpectedResult = "01 summer Y16")]
        [TestCase("28 fall Y10", ExpectedResult = "28 fall Y10")]
        [TestCase("01 winter Y1", ExpectedResult = "01 winter Y1")]
        public string ToString(string dateStr)
        {
            return this.GetDate(dateStr).ToString();
        }

        /****
        ** AddDays
        ****/
        [Test(Description = "Assert that AddDays returns the expected date.")]
        [TestCase("01 spring Y1", 15, ExpectedResult = "16 spring Y1")] // day transition
        [TestCase("01 spring Y1", 28, ExpectedResult = "01 summer Y1")] // season transition
        [TestCase("01 spring Y1", 28 * 4, ExpectedResult = "01 spring Y2")] // year transition
        [TestCase("01 spring Y1", 28 * 7 + 17, ExpectedResult = "18 winter Y2")] // year transition
        [TestCase("15 spring Y1", -14, ExpectedResult = "01 spring Y1")] // negative day transition
        [TestCase("15 summer Y1", -28, ExpectedResult = "15 spring Y1")] // negative season transition
        [TestCase("15 summer Y2", -28 * 4, ExpectedResult = "15 summer Y1")] // negative year transition
        [TestCase("01 spring Y3", -(28 * 7 + 17), ExpectedResult = "12 spring Y1")] // negative year transition
        public string AddDays(string dateStr, int addDays)
        {
            return this.GetDate(dateStr).AddDays(addDays).ToString();
        }

        /****
        ** GetHashCode
        ****/
        [Test(Description = "Assert that GetHashCode returns a unique ordered value for every date.")]
        public void GetHashCode_ReturnsUniqueOrderedValue()
        {
            IDictionary<int, SDate> hashes = new Dictionary<int, SDate>();
            int lastHash = int.MinValue;
            for (int year = 1; year <= 4; year++)
            {
                foreach (string season in SDateTests.ValidSeasons)
                {
                    foreach (int day in SDateTests.ValidDays)
                    {
                        SDate date = new SDate(day, season, year);
                        int hash = date.GetHashCode();
                        if (hashes.TryGetValue(hash, out SDate otherDate))
                            Assert.Fail($"Received identical hash code {hash} for dates {otherDate} and {date}.");
                        if (hash < lastHash)
                            Assert.Fail($"Received smaller hash code for date {date} ({hash}) relative to {hashes[lastHash]} ({lastHash}).");

                        lastHash = hash;
                        hashes[hash] = date;
                    }
                }
            }
        }

        [Test(Description = "Assert that the == operator returns the expected values. We only need a few test cases, since it's based on GetHashCode which is tested more thoroughly.")]
        [TestCase(Dates.Now, null, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.PrevDay, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.PrevMonth, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.PrevYear, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.Now, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.NextDay, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.NextMonth, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.NextYear, ExpectedResult = false)]
        public bool Operators_Equals(string now, string other)
        {
            return this.GetDate(now) == this.GetDate(other);
        }

        [Test(Description = "Assert that the != operator returns the expected values. We only need a few test cases, since it's based on GetHashCode which is tested more thoroughly.")]
        [TestCase(Dates.Now, null, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.PrevDay, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.PrevMonth, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.PrevYear, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.Now, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.NextDay, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.NextMonth, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.NextYear, ExpectedResult = true)]
        public bool Operators_NotEquals(string now, string other)
        {
            return this.GetDate(now) != this.GetDate(other);
        }

        [Test(Description = "Assert that the < operator returns the expected values. We only need a few test cases, since it's based on GetHashCode which is tested more thoroughly.")]
        [TestCase(Dates.Now, null, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.PrevDay, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.PrevMonth, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.PrevYear, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.Now, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.NextDay, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.NextMonth, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.NextYear, ExpectedResult = true)]
        public bool Operators_LessThan(string now, string other)
        {
            return this.GetDate(now) < this.GetDate(other);
        }

        [Test(Description = "Assert that the <= operator returns the expected values. We only need a few test cases, since it's based on GetHashCode which is tested more thoroughly.")]
        [TestCase(Dates.Now, null, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.PrevDay, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.PrevMonth, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.PrevYear, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.Now, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.NextDay, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.NextMonth, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.NextYear, ExpectedResult = true)]
        public bool Operators_LessThanOrEqual(string now, string other)
        {
            return this.GetDate(now) <= this.GetDate(other);
        }

        [Test(Description = "Assert that the > operator returns the expected values. We only need a few test cases, since it's based on GetHashCode which is tested more thoroughly.")]
        [TestCase(Dates.Now, null, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.PrevDay, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.PrevMonth, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.PrevYear, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.Now, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.NextDay, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.NextMonth, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.NextYear, ExpectedResult = false)]
        public bool Operators_MoreThan(string now, string other)
        {
            return this.GetDate(now) > this.GetDate(other);
        }

        [Test(Description = "Assert that the > operator returns the expected values. We only need a few test cases, since it's based on GetHashCode which is tested more thoroughly.")]
        [TestCase(Dates.Now, null, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.PrevDay, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.PrevMonth, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.PrevYear, ExpectedResult = true)]
        [TestCase(Dates.Now, Dates.Now, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.NextDay, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.NextMonth, ExpectedResult = false)]
        [TestCase(Dates.Now, Dates.NextYear, ExpectedResult = false)]
        public bool Operators_MoreThanOrEqual(string now, string other)
        {
            return this.GetDate(now) > this.GetDate(other);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Convert a string date into a game date, to make unit tests easier to read.</summary>
        /// <param name="dateStr">The date string like "dd MMMM yy".</param>
        private SDate GetDate(string dateStr)
        {
            if (dateStr == null)
                return null;

            void Fail(string reason) => throw new AssertionException($"Couldn't parse date '{dateStr}' because {reason}.");

            // parse
            Match match = Regex.Match(dateStr, @"^(?<day>\d+) (?<season>\w+) Y(?<year>\d+)$");
            if (!match.Success)
                Fail("it doesn't match expected pattern (should be like 28 spring Y1)");

            // extract parts
            string season = match.Groups["season"].Value;
            if (!int.TryParse(match.Groups["day"].Value, out int day))
                Fail($"'{match.Groups["day"].Value}' couldn't be parsed as a day.");
            if (!int.TryParse(match.Groups["year"].Value, out int year))
                Fail($"'{match.Groups["year"].Value}' couldn't be parsed as a year.");

            // build date
            return new SDate(day, season, year);
        }
    }
}
