// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Globalization;
using System.Tests;
using System.Text;
using Xunit;

// TODO: Once compiler support is available, augment tests to exercise interpolated strings.

namespace System.SpanTests
{
    public class TryWriteTests
    {
        private char[] _largeBuffer = new char[4096];

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(42, 84)]
        [InlineData(-1, 0)]
        [InlineData(-1, -1)]
        [InlineData(-16, 1)]
        public void LengthAndHoleArguments_Valid(int baseLength, int holeCount)
        {
            bool success;

            MemoryExtensions.InterpolatedSpanBuilder.Create(baseLength, holeCount, new char[Math.Max(0, baseLength)], out success);
            Assert.True(success);

            MemoryExtensions.InterpolatedSpanBuilder.Create(baseLength, holeCount, new char[1 + Math.Max(0, baseLength)], out success);
            Assert.True(success);

            if (baseLength > 0)
            {
                MemoryExtensions.InterpolatedSpanBuilder.Create(baseLength, holeCount, new char[baseLength - 1], out success);
                Assert.False(success);
            }

            foreach (IFormatProvider provider in new IFormatProvider[] { null, new ConcatFormatter(), CultureInfo.InvariantCulture, CultureInfo.CurrentCulture, new CultureInfo("en-US"), new CultureInfo("fr-FR") })
            {
                MemoryExtensions.InterpolatedSpanBuilder.Create(baseLength, holeCount, new char[Math.Max(0, baseLength)], out success);
                Assert.True(success);

                MemoryExtensions.InterpolatedSpanBuilder.Create(baseLength, holeCount, new char[1 + Math.Max(0, baseLength)], out success);
                Assert.True(success);

                if (baseLength > 0)
                {
                    MemoryExtensions.InterpolatedSpanBuilder.Create(baseLength, holeCount, new char[baseLength - 1], out success);
                    Assert.False(success);
                }
            }
        }

        [Fact]
        public void TryFormatBaseString()
        {
            var expected = new StringBuilder();
            MemoryExtensions.InterpolatedSpanBuilder actual = MemoryExtensions.InterpolatedSpanBuilder.Create(0, 0, _largeBuffer, out _);

            foreach (string s in new[] { "", "a", "bc", "def", "this is a long string", "!" })
            {
                expected.Append(s);
                actual.TryFormatBaseString(s);
            }

            Assert.True(MemoryExtensions.TryWrite(_largeBuffer, actual, out int charsWritten));
            Assert.Equal(expected.ToString(), _largeBuffer.AsSpan(0, charsWritten).ToString());
        }

        [Fact]
        public void TryFormatInterpolationHole_ReadOnlySpanChar()
        {
            var expected = new StringBuilder();
            MemoryExtensions.InterpolatedSpanBuilder actual = MemoryExtensions.InterpolatedSpanBuilder.Create(0, 0, _largeBuffer, out _);

            foreach (string s in new[] { "", "a", "bc", "def", "this is a longer string", "!" })
            {
                // span
                expected.Append(s);
                actual.TryFormatInterpolationHole((ReadOnlySpan<char>)s);

                // span, format
                expected.AppendFormat("{0:X2}", s);
                actual.TryFormatInterpolationHole((ReadOnlySpan<char>)s, format: "X2");

                foreach (int alignment in new[] { 0, 3, -3 })
                {
                    // span, alignment
                    expected.AppendFormat("{0," + alignment.ToString(CultureInfo.InvariantCulture) + "}", s);
                    actual.TryFormatInterpolationHole((ReadOnlySpan<char>)s, alignment);

                    // span, alignment, format
                    expected.AppendFormat("{0," + alignment.ToString(CultureInfo.InvariantCulture) + ":X2}", s);
                    actual.TryFormatInterpolationHole((ReadOnlySpan<char>)s, alignment, "X2");
                }
            }

            Assert.True(MemoryExtensions.TryWrite(_largeBuffer, actual, out int charsWritten));
            Assert.Equal(expected.ToString(), _largeBuffer.AsSpan(0, charsWritten).ToString());
        }

        [Fact]
        public void TryFormatInterpolationHole_String()
        {
            var expected = new StringBuilder();
            MemoryExtensions.InterpolatedSpanBuilder actual = MemoryExtensions.InterpolatedSpanBuilder.Create(0, 0, _largeBuffer, out _);

            foreach (string s in new[] { null, "", "a", "bc", "def", "this is a longer string", "!" })
            {
                // string
                expected.AppendFormat("{0}", s);
                actual.TryFormatInterpolationHole(s);

                // string, format
                expected.AppendFormat("{0:X2}", s);
                actual.TryFormatInterpolationHole(s, "X2");

                foreach (int alignment in new[] { 0, 3, -3 })
                {
                    // string, alignment
                    expected.AppendFormat("{0," + alignment.ToString(CultureInfo.InvariantCulture) + "}", s);
                    actual.TryFormatInterpolationHole(s, alignment);

                    // string, alignment, format
                    expected.AppendFormat("{0," + alignment.ToString(CultureInfo.InvariantCulture) + ":X2}", s);
                    actual.TryFormatInterpolationHole(s, alignment, "X2");
                }
            }

            Assert.True(MemoryExtensions.TryWrite(_largeBuffer, actual, out int charsWritten));
            Assert.Equal(expected.ToString(), _largeBuffer.AsSpan(0, charsWritten).ToString());
        }

        [Fact]
        public void TryFormatInterpolationHole_String_ICustomFormatter()
        {
            var provider = new ConcatFormatter();

            var expected = new StringBuilder();
            MemoryExtensions.InterpolatedSpanBuilder actual = MemoryExtensions.InterpolatedSpanBuilder.Create(0, 0, _largeBuffer, provider, out _);

            foreach (string s in new[] { null, "", "a" })
            {
                // string
                expected.AppendFormat(provider, "{0}", s);
                actual.TryFormatInterpolationHole(s);

                // string, format
                expected.AppendFormat(provider, "{0:X2}", s);
                actual.TryFormatInterpolationHole(s, "X2");

                // string, alignment
                expected.AppendFormat(provider, "{0,3}", s);
                actual.TryFormatInterpolationHole(s, 3);

                // string, alignment, format
                expected.AppendFormat(provider, "{0,-3:X2}", s);
                actual.TryFormatInterpolationHole(s, -3, "X2");
            }

            Assert.True(MemoryExtensions.TryWrite(_largeBuffer, actual, out int charsWritten));
            Assert.Equal(expected.ToString(), _largeBuffer.AsSpan(0, charsWritten).ToString());
        }

        [Fact]
        public void TryFormatInterpolationHole_ReferenceTypes()
        {
            var expected = new StringBuilder();
            MemoryExtensions.InterpolatedSpanBuilder actual = MemoryExtensions.InterpolatedSpanBuilder.Create(0, 0, _largeBuffer, out _);

            foreach (string rawInput in new[] { null, "", "a", "bc", "def", "this is a longer string", "!" })
            {
                foreach (object o in new object[]
                {
                    rawInput, // raw string directly; ToString will return itself
                    new StringWrapper(rawInput), // wrapper object that returns string from ToString
                    new FormattableStringWrapper(rawInput), // IFormattable wrapper around string
                    new SpanFormattableStringWrapper(rawInput) // ISpanFormattable wrapper around string
                })
                {
                    // object
                    expected.AppendFormat("{0}", o);
                    actual.TryFormatInterpolationHole(o);
                    if (o is IHasToStringState tss1)
                    {
                        Assert.True(string.IsNullOrEmpty(tss1.ToStringState.LastFormat));
                        AssertModeMatchesType(tss1);
                    }

                    // object, format
                    expected.AppendFormat("{0:X2}", o);
                    actual.TryFormatInterpolationHole(o, "X2");
                    if (o is IHasToStringState tss2)
                    {
                        Assert.Equal("X2", tss2.ToStringState.LastFormat);
                        AssertModeMatchesType(tss2);
                    }

                    foreach (int alignment in new[] { 0, 3, -3 })
                    {
                        // object, alignment
                        expected.AppendFormat("{0," + alignment.ToString(CultureInfo.InvariantCulture) + "}", o);
                        actual.TryFormatInterpolationHole(o, alignment);
                        if (o is IHasToStringState tss3)
                        {
                            Assert.True(string.IsNullOrEmpty(tss3.ToStringState.LastFormat));
                            AssertModeMatchesType(tss3);
                        }

                        // object, alignment, format
                        expected.AppendFormat("{0," + alignment.ToString(CultureInfo.InvariantCulture) + ":X2}", o);
                        actual.TryFormatInterpolationHole(o, alignment, "X2");
                        if (o is IHasToStringState tss4)
                        {
                            Assert.Equal("X2", tss4.ToStringState.LastFormat);
                            AssertModeMatchesType(tss4);
                        }
                    }
                }
            }

            Assert.True(MemoryExtensions.TryWrite(_largeBuffer, actual, out int charsWritten));
            Assert.Equal(expected.ToString(), _largeBuffer.AsSpan(0, charsWritten).ToString());
        }

        [Fact]
        public void TryFormatInterpolationHole_ReferenceTypes_CreateProviderFlowed()
        {
            var provider = new CultureInfo("en-US");
            MemoryExtensions.InterpolatedSpanBuilder builder = MemoryExtensions.InterpolatedSpanBuilder.Create(1, 2, _largeBuffer, provider, out _);

            foreach (IHasToStringState tss in new IHasToStringState[] { new FormattableStringWrapper("hello"), new SpanFormattableStringWrapper("hello") })
            {
                builder.TryFormatInterpolationHole(tss);
                Assert.Same(provider, tss.ToStringState.LastProvider);

                builder.TryFormatInterpolationHole(tss, 1);
                Assert.Same(provider, tss.ToStringState.LastProvider);

                builder.TryFormatInterpolationHole(tss, "X2");
                Assert.Same(provider, tss.ToStringState.LastProvider);

                builder.TryFormatInterpolationHole(tss, 1, "X2");
                Assert.Same(provider, tss.ToStringState.LastProvider);
            }
        }

        [Fact]
        public void TryFormatInterpolationHole_ReferenceTypes_ICustomFormatter()
        {
            var provider = new ConcatFormatter();

            var expected = new StringBuilder();
            MemoryExtensions.InterpolatedSpanBuilder actual = MemoryExtensions.InterpolatedSpanBuilder.Create(0, 0, _largeBuffer, provider, out _);

            foreach (string s in new[] { null, "", "a" })
            {
                foreach (IHasToStringState tss in new IHasToStringState[] { new FormattableStringWrapper(s), new SpanFormattableStringWrapper(s) })
                {
                    void AssertTss(IHasToStringState tss, string format)
                    {
                        Assert.Equal(format, tss.ToStringState.LastFormat);
                        Assert.Same(provider, tss.ToStringState.LastProvider);
                        Assert.Equal(ToStringMode.ICustomFormatterFormat, tss.ToStringState.ToStringMode);
                    }

                    // object
                    expected.AppendFormat(provider, "{0}", tss);
                    actual.TryFormatInterpolationHole(tss);
                    AssertTss(tss, null);

                    // object, format
                    expected.AppendFormat(provider, "{0:X2}", tss);
                    actual.TryFormatInterpolationHole(tss, "X2");
                    AssertTss(tss, "X2");

                    // object, alignment
                    expected.AppendFormat(provider, "{0,3}", tss);
                    actual.TryFormatInterpolationHole(tss, 3);
                    AssertTss(tss, null);

                    // object, alignment, format
                    expected.AppendFormat(provider, "{0,-3:X2}", tss);
                    actual.TryFormatInterpolationHole(tss, -3, "X2");
                    AssertTss(tss, "X2");
                }
            }

            Assert.True(MemoryExtensions.TryWrite(_largeBuffer, actual, out int charsWritten));
            Assert.Equal(expected.ToString(), _largeBuffer.AsSpan(0, charsWritten).ToString());
        }

        [Fact]
        public void TryFormatInterpolationHole_ValueTypes()
        {
            void Test<T>(T t)
            {
                var expected = new StringBuilder();
                MemoryExtensions.InterpolatedSpanBuilder actual = MemoryExtensions.InterpolatedSpanBuilder.Create(0, 0, _largeBuffer, out _);

                // struct
                expected.AppendFormat("{0}", t);
                actual.TryFormatInterpolationHole(t);
                Assert.True(string.IsNullOrEmpty(((IHasToStringState)t).ToStringState.LastFormat));
                AssertModeMatchesType(((IHasToStringState)t));

                // struct, format
                expected.AppendFormat("{0:X2}", t);
                actual.TryFormatInterpolationHole(t, "X2");
                Assert.Equal("X2", ((IHasToStringState)t).ToStringState.LastFormat);
                AssertModeMatchesType(((IHasToStringState)t));

                foreach (int alignment in new[] { 0, 3, -3 })
                {
                    // struct, alignment
                    expected.AppendFormat("{0," + alignment.ToString(CultureInfo.InvariantCulture) + "}", t);
                    actual.TryFormatInterpolationHole(t, alignment);
                    Assert.True(string.IsNullOrEmpty(((IHasToStringState)t).ToStringState.LastFormat));
                    AssertModeMatchesType(((IHasToStringState)t));

                    // struct, alignment, format
                    expected.AppendFormat("{0," + alignment.ToString(CultureInfo.InvariantCulture) + ":X2}", t);
                    actual.TryFormatInterpolationHole(t, alignment, "X2");
                    Assert.Equal("X2", ((IHasToStringState)t).ToStringState.LastFormat);
                    AssertModeMatchesType(((IHasToStringState)t));
                }

                Assert.True(MemoryExtensions.TryWrite(_largeBuffer, actual, out int charsWritten));
                Assert.Equal(expected.ToString(), _largeBuffer.AsSpan(0, charsWritten).ToString());
            }

            Test(new FormattableInt32Wrapper(42));
            Test(new SpanFormattableInt32Wrapper(84));
            Test((FormattableInt32Wrapper?)new FormattableInt32Wrapper(42));
            Test((SpanFormattableInt32Wrapper?)new SpanFormattableInt32Wrapper(84));
        }

        [Fact]
        public void TryFormatInterpolationHole_ValueTypes_CreateProviderFlowed()
        {
            void Test<T>(T t)
            {
                var provider = new CultureInfo("en-US");
                MemoryExtensions.InterpolatedSpanBuilder builder = MemoryExtensions.InterpolatedSpanBuilder.Create(1, 2, _largeBuffer, provider, out _);

                builder.TryFormatInterpolationHole(t);
                Assert.Same(provider, ((IHasToStringState)t).ToStringState.LastProvider);

                builder.TryFormatInterpolationHole(t, 1);
                Assert.Same(provider, ((IHasToStringState)t).ToStringState.LastProvider);

                builder.TryFormatInterpolationHole(t, "X2");
                Assert.Same(provider, ((IHasToStringState)t).ToStringState.LastProvider);

                builder.TryFormatInterpolationHole(t, 1, "X2");
                Assert.Same(provider, ((IHasToStringState)t).ToStringState.LastProvider);
            }

            Test(new FormattableInt32Wrapper(42));
            Test(new SpanFormattableInt32Wrapper(84));
            Test((FormattableInt32Wrapper?)new FormattableInt32Wrapper(42));
            Test((SpanFormattableInt32Wrapper?)new SpanFormattableInt32Wrapper(84));
        }

        [Fact]
        public void TryFormatInterpolationHole_ValueTypes_ICustomFormatter()
        {
            var provider = new ConcatFormatter();

            void Test<T>(T t)
            {
                void AssertTss(T tss, string format)
                {
                    Assert.Equal(format, ((IHasToStringState)tss).ToStringState.LastFormat);
                    Assert.Same(provider, ((IHasToStringState)tss).ToStringState.LastProvider);
                    Assert.Equal(ToStringMode.ICustomFormatterFormat, ((IHasToStringState)tss).ToStringState.ToStringMode);
                }

                var expected = new StringBuilder();
                MemoryExtensions.InterpolatedSpanBuilder actual = MemoryExtensions.InterpolatedSpanBuilder.Create(0, 0, _largeBuffer, provider, out _);

                // struct
                expected.AppendFormat(provider, "{0}", t);
                actual.TryFormatInterpolationHole(t);
                AssertTss(t, null);

                // struct, format
                expected.AppendFormat(provider, "{0:X2}", t);
                actual.TryFormatInterpolationHole(t, "X2");
                AssertTss(t, "X2");

                // struct, alignment
                expected.AppendFormat(provider, "{0,3}", t);
                actual.TryFormatInterpolationHole(t, 3);
                AssertTss(t, null);

                // struct, alignment, format
                expected.AppendFormat(provider, "{0,-3:X2}", t);
                actual.TryFormatInterpolationHole(t, -3, "X2");
                AssertTss(t, "X2");

                Assert.True(MemoryExtensions.TryWrite(_largeBuffer, actual, out int charsWritten));
                Assert.Equal(expected.ToString(), _largeBuffer.AsSpan(0, charsWritten).ToString());
            }

            Test(new FormattableInt32Wrapper(42));
            Test(new SpanFormattableInt32Wrapper(84));
            Test((FormattableInt32Wrapper?)new FormattableInt32Wrapper(42));
            Test((SpanFormattableInt32Wrapper?)new SpanFormattableInt32Wrapper(84));
        }

        [Fact]
        public void TryFormatInterpolationHole_EmptyBuffer_ZeroLengthWritesSuccessful()
        {
            var buffer = new char[100];

            MemoryExtensions.InterpolatedSpanBuilder b = MemoryExtensions.InterpolatedSpanBuilder.Create(0, 0, buffer.AsSpan(0, 0), out bool success);
            Assert.True(success);

            Assert.True(b.TryFormatBaseString(""));
            Assert.True(b.TryFormatInterpolationHole((object)"", alignment: 0, format: "X2"));
            Assert.True(b.TryFormatInterpolationHole(null));
            Assert.True(b.TryFormatInterpolationHole(""));
            Assert.True(b.TryFormatInterpolationHole("", alignment: 0, format: "X2"));
            Assert.True(b.TryFormatInterpolationHole<string>(""));
            Assert.True(b.TryFormatInterpolationHole<string>("", alignment: 0));
            Assert.True(b.TryFormatInterpolationHole<string>("", format: "X2"));
            Assert.True(b.TryFormatInterpolationHole<string>("", alignment: 0, format: "X2"));
            Assert.True(b.TryFormatInterpolationHole("".AsSpan()));
            Assert.True(b.TryFormatInterpolationHole("".AsSpan(), alignment: 0, format: "X2"));

            Assert.True(MemoryExtensions.TryWrite(buffer.AsSpan(0, 0), b, out int charsWritten));
            Assert.Equal(0, charsWritten);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        public void TryFormatInterpolationHole_BufferTooSmall(int bufferLength)
        {
            var buffer = new char[bufferLength];

            for (int i = 0; i <= 29; i++)
            {
                MemoryExtensions.InterpolatedSpanBuilder b = MemoryExtensions.InterpolatedSpanBuilder.Create(0, 0, buffer, out bool success);
                Assert.True(success);

                Assert.True(b.TryFormatBaseString(new string('s', bufferLength)));

                bool result = i switch
                {
                    0 => b.TryFormatBaseString(" "),
                    1 => b.TryFormatInterpolationHole((object)" ", alignment: 0, format: "X2"),
                    2 => b.TryFormatInterpolationHole(" "),
                    3 => b.TryFormatInterpolationHole(" ", alignment: 0, format: "X2"),
                    4 => b.TryFormatInterpolationHole<string>(" "),
                    5 => b.TryFormatInterpolationHole<string>(" ", alignment: 0),
                    6 => b.TryFormatInterpolationHole<string>(" ", format: "X2"),
                    7 => b.TryFormatInterpolationHole<string>(" ", alignment: 0, format: "X2"),
                    8 => b.TryFormatInterpolationHole(" ".AsSpan()),
                    9 => b.TryFormatInterpolationHole(" ".AsSpan(), alignment: 0, format: "X2"),
                    10 => b.TryFormatInterpolationHole(new FormattableStringWrapper(" ")),
                    11 => b.TryFormatInterpolationHole(new FormattableStringWrapper(" "), alignment: 0),
                    12 => b.TryFormatInterpolationHole(new FormattableStringWrapper(" "), format: "X2"),
                    13 => b.TryFormatInterpolationHole(new FormattableStringWrapper(" "), alignment: 0, format: "X2"),
                    14 => b.TryFormatInterpolationHole(new SpanFormattableStringWrapper(" ")),
                    15 => b.TryFormatInterpolationHole(new SpanFormattableStringWrapper(" "), alignment: 0),
                    16 => b.TryFormatInterpolationHole(new SpanFormattableStringWrapper(" "), format: "X2"),
                    17 => b.TryFormatInterpolationHole(new SpanFormattableStringWrapper(" "), alignment: 0, format: "X2"),
                    18 => b.TryFormatInterpolationHole(new FormattableInt32Wrapper(1)),
                    19 => b.TryFormatInterpolationHole(new FormattableInt32Wrapper(1), alignment: 0),
                    20 => b.TryFormatInterpolationHole(new FormattableInt32Wrapper(1), format: "X2"),
                    21 => b.TryFormatInterpolationHole(new FormattableInt32Wrapper(1), alignment: 0, format: "X2"),
                    22 => b.TryFormatInterpolationHole(new SpanFormattableInt32Wrapper(1)),
                    23 => b.TryFormatInterpolationHole(new SpanFormattableInt32Wrapper(1), alignment: 0),
                    24 => b.TryFormatInterpolationHole(new SpanFormattableInt32Wrapper(1), format: "X2"),
                    25 => b.TryFormatInterpolationHole(new SpanFormattableInt32Wrapper(1), alignment: 0, format: "X2"),
                    26 => b.TryFormatInterpolationHole<string>("", alignment: 1),
                    27 => b.TryFormatInterpolationHole<string>("", alignment: -1),
                    28 => b.TryFormatInterpolationHole<string>(" ", alignment: 1, format: "X2"),
                    29 => b.TryFormatInterpolationHole<string>(" ", alignment: -1, format: "X2"),
                    _ => throw new Exception(),
                };
                Assert.False(result);

                Assert.False(MemoryExtensions.TryWrite(buffer.AsSpan(0, 0), b, out int charsWritten));
                Assert.Equal(0, charsWritten);
            }
        }
        [Fact]
        public void TryFormatInterpolationHole_BufferTooSmall_CustomFormatter()
        {
            var buffer = new char[100];
            var provider = new ConstFormatter(" ");

            {
                MemoryExtensions.InterpolatedSpanBuilder b = MemoryExtensions.InterpolatedSpanBuilder.Create(0, 0, buffer.AsSpan(0, 0), provider, out bool success);
                Assert.True(success);

                // don't use custom formatter
                Assert.True(b.TryFormatBaseString(""));
                Assert.True(b.TryFormatInterpolationHole("".AsSpan()));
                Assert.True(b.TryFormatInterpolationHole("".AsSpan(), alignment: 0, format: "X2"));

                // do use custom formatter
                Assert.False(b.TryFormatInterpolationHole((object)"", alignment: 0, format: "X2"));
                Assert.False(b.TryFormatInterpolationHole(null));
                Assert.False(b.TryFormatInterpolationHole(""));
                Assert.False(b.TryFormatInterpolationHole("", alignment: 0, format: "X2"));
                Assert.False(b.TryFormatInterpolationHole<string>(""));
                Assert.False(b.TryFormatInterpolationHole<string>("", alignment: 0));
                Assert.False(b.TryFormatInterpolationHole<string>("", format: "X2"));
                Assert.False(b.TryFormatInterpolationHole<string>("", alignment: 0, format: "X2"));

                Assert.False(MemoryExtensions.TryWrite(buffer.AsSpan(0, 0), b, out int charsWritten));
                Assert.Equal(0, charsWritten);
            }
        }

        private static void AssertModeMatchesType<T>(T tss) where T : IHasToStringState
        {
            ToStringMode expected =
                tss is ISpanFormattable ? ToStringMode.ISpanFormattableTryFormat :
                tss is IFormattable ? ToStringMode.IFormattableToString :
                ToStringMode.ObjectToString;
            Assert.Equal(expected, tss.ToStringState.ToStringMode);
        }

        private sealed class SpanFormattableStringWrapper : IFormattable, ISpanFormattable, IHasToStringState
        {
            private readonly string _value;
            public ToStringState ToStringState { get; } = new ToStringState();

            public SpanFormattableStringWrapper(string value) => _value = value;

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
            {
                ToStringState.LastFormat = format.ToString();
                ToStringState.LastProvider = provider;
                ToStringState.ToStringMode = ToStringMode.ISpanFormattableTryFormat;

                if (_value is null)
                {
                    charsWritten = 0;
                    return true;
                }

                if (_value.Length > destination.Length)
                {
                    charsWritten = 0;
                    return false;
                }

                charsWritten = _value.Length;
                _value.AsSpan().CopyTo(destination);
                return true;
            }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                ToStringState.LastFormat = format;
                ToStringState.LastProvider = formatProvider;
                ToStringState.ToStringMode = ToStringMode.IFormattableToString;
                return _value;
            }

            public override string ToString()
            {
                ToStringState.LastFormat = null;
                ToStringState.LastProvider = null;
                ToStringState.ToStringMode = ToStringMode.ObjectToString;
                return _value;
            }
        }

        private struct SpanFormattableInt32Wrapper : IFormattable, ISpanFormattable, IHasToStringState
        {
            private readonly int _value;
            public ToStringState ToStringState { get; }

            public SpanFormattableInt32Wrapper(int value)
            {
                ToStringState = new ToStringState();
                _value = value;
            }

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
            {
                ToStringState.LastFormat = format.ToString();
                ToStringState.LastProvider = provider;
                ToStringState.ToStringMode = ToStringMode.ISpanFormattableTryFormat;

                return _value.TryFormat(destination, out charsWritten, format, provider);
            }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                ToStringState.LastFormat = format;
                ToStringState.LastProvider = formatProvider;
                ToStringState.ToStringMode = ToStringMode.IFormattableToString;
                return _value.ToString(format, formatProvider);
            }

            public override string ToString()
            {
                ToStringState.LastFormat = null;
                ToStringState.LastProvider = null;
                ToStringState.ToStringMode = ToStringMode.ObjectToString;
                return _value.ToString();
            }
        }

        private sealed class FormattableStringWrapper : IFormattable, IHasToStringState
        {
            private readonly string _value;
            public ToStringState ToStringState { get; } = new ToStringState();

            public FormattableStringWrapper(string s) => _value = s;

            public string ToString(string format, IFormatProvider formatProvider)
            {
                ToStringState.LastFormat = format;
                ToStringState.LastProvider = formatProvider;
                ToStringState.ToStringMode = ToStringMode.IFormattableToString;
                return _value;
            }

            public override string ToString()
            {
                ToStringState.LastFormat = null;
                ToStringState.LastProvider = null;
                ToStringState.ToStringMode = ToStringMode.ObjectToString;
                return _value;
            }
        }

        private struct FormattableInt32Wrapper : IFormattable, IHasToStringState
        {
            private readonly int _value;
            public ToStringState ToStringState { get; }

            public FormattableInt32Wrapper(int i)
            {
                ToStringState = new ToStringState();
                _value = i;
            }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                ToStringState.LastFormat = format;
                ToStringState.LastProvider = formatProvider;
                ToStringState.ToStringMode = ToStringMode.IFormattableToString;
                return _value.ToString(format, formatProvider);
            }

            public override string ToString()
            {
                ToStringState.LastFormat = null;
                ToStringState.LastProvider = null;
                ToStringState.ToStringMode = ToStringMode.ObjectToString;
                return _value.ToString();
            }
        }

        private sealed class ToStringState
        {
            public string LastFormat { get; set; }
            public IFormatProvider LastProvider { get; set; }
            public ToStringMode ToStringMode { get; set; }
        }

        private interface IHasToStringState
        {
            ToStringState ToStringState { get; }
        }

        private enum ToStringMode
        {
            ObjectToString,
            IFormattableToString,
            ISpanFormattableTryFormat,
            ICustomFormatterFormat,
        }

        private sealed class StringWrapper
        {
            private readonly string _value;

            public StringWrapper(string s) => _value = s;

            public override string ToString() => _value;
        }

        private sealed class ConcatFormatter : IFormatProvider, ICustomFormatter
        {
            public object GetFormat(Type formatType) => formatType == typeof(ICustomFormatter) ? this : null;

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                string s = format + " " + arg + formatProvider;

                if (arg is IHasToStringState tss)
                {
                    // Set after using arg.ToString() in concat above
                    tss.ToStringState.LastFormat = format;
                    tss.ToStringState.LastProvider = formatProvider;
                    tss.ToStringState.ToStringMode = ToStringMode.ICustomFormatterFormat;
                }

                return s;
            }
        }

        private sealed class ConstFormatter : IFormatProvider, ICustomFormatter
        {
            private readonly string _value;

            public ConstFormatter(string value) => _value = value;

            public object GetFormat(Type formatType) => formatType == typeof(ICustomFormatter) ? this : null;

            public string Format(string format, object arg, IFormatProvider formatProvider) => _value;
        }
    }
}
