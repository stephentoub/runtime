// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace System.SpanTests
{
    public class ReplaceSequenceTests
    {
        [Fact]
        public void EmptySource()
        {
            ReadOnlySpan<char> source = ReadOnlySpan<char>.Empty;
            Span<char> destination = new char[10];
            int written = source.Replace(destination, "abc".AsSpan(), "xyz".AsSpan());
            Assert.Equal(0, written);
        }

        [Fact]
        public void EmptyOldValue_ThrowsArgumentException()
        {
            char[] src = "hello".ToCharArray();
            char[] dst = new char[10];
            AssertExtensions.Throws<ArgumentException>("oldValue", () => ((ReadOnlySpan<char>)src).Replace(dst, ReadOnlySpan<char>.Empty, "x".AsSpan()));
        }

        [Fact]
        public void NoMatches_CopiesSourceVerbatim()
        {
            ReadOnlySpan<char> source = "hello world".AsSpan();
            Span<char> destination = new char[11];
            int written = source.Replace(destination, "xyz".AsSpan(), "abc".AsSpan());
            Assert.Equal(11, written);
            Assert.Equal("hello world", destination.Slice(0, written).ToString());
        }

        [Theory]
        [InlineData("hello world", "hello", "hi", "hi world")]
        [InlineData("hello world", "world", "earth", "hello earth")]
        [InlineData("hello world", "o", "0", "hell0 w0rld")]
        [InlineData("aaa", "a", "bb", "bbbbbb")]
        [InlineData("aaa", "aa", "b", "ba")]
        [InlineData("abcabc", "abc", "", "")]
        [InlineData("abcabc", "abc", "x", "xx")]
        [InlineData("abc", "abc", "abc", "abc")]
        [InlineData("abc", "abc", "abcd", "abcd")]
        [InlineData("abc", "abc", "ab", "ab")]
        [InlineData("aababaa", "ab", "x", "axxaa")]
        public void Replace_Char_Various(string sourceStr, string oldStr, string newStr, string expected)
        {
            ReadOnlySpan<char> source = sourceStr.AsSpan();
            Span<char> destination = new char[expected.Length];
            int written = source.Replace(destination, oldStr.AsSpan(), newStr.AsSpan());
            Assert.Equal(expected.Length, written);
            Assert.Equal(expected, destination.Slice(0, written).ToString());
        }

        [Theory]
        [InlineData("hello world", "hello", "hi", "hi world")]
        [InlineData("aaa", "a", "bb", "bbbbbb")]
        [InlineData("abcabc", "abc", "", "")]
        public void TryReplace_Char_Success(string sourceStr, string oldStr, string newStr, string expected)
        {
            ReadOnlySpan<char> source = sourceStr.AsSpan();
            Span<char> destination = new char[expected.Length];
            Assert.True(source.TryReplace(destination, oldStr.AsSpan(), newStr.AsSpan(), out int written));
            Assert.Equal(expected.Length, written);
            Assert.Equal(expected, destination.Slice(0, written).ToString());
        }

        [Fact]
        public void TryReplace_DestinationTooSmall_ReturnsFalse()
        {
            ReadOnlySpan<char> source = "hello world".AsSpan();
            Span<char> destination = new char[3];
            Assert.False(source.TryReplace(destination, "o".AsSpan(), "00".AsSpan(), out int written));
            Assert.Equal(0, written);
        }

        [Fact]
        public void Replace_DestinationTooSmall_ThrowsArgumentException()
        {
            char[] src = "hello world".ToCharArray();
            char[] dst = new char[3];
            Assert.Throws<ArgumentException>("destination", () => ((ReadOnlySpan<char>)src).Replace(dst, "o".AsSpan(), "00".AsSpan()));
        }

        [Fact]
        public void Replace_NewValueEmpty_RemovesOccurrences()
        {
            ReadOnlySpan<char> source = "hello world".AsSpan();
            Span<char> destination = new char[20];
            int written = source.Replace(destination, "o".AsSpan(), ReadOnlySpan<char>.Empty);
            Assert.Equal("hell wrld", destination.Slice(0, written).ToString());
        }

        [Fact]
        public void Replace_OldEqualsNew_CopiesVerbatim()
        {
            ReadOnlySpan<char> source = "hello world".AsSpan();
            Span<char> destination = new char[11];
            int written = source.Replace(destination, "hello".AsSpan(), "hello".AsSpan());
            Assert.Equal(11, written);
            Assert.Equal("hello world", destination.Slice(0, written).ToString());
        }

        [Fact]
        public void Replace_SingleCharOldValue()
        {
            ReadOnlySpan<char> source = "banana".AsSpan();
            Span<char> destination = new char[20];
            int written = source.Replace(destination, "a".AsSpan(), "o".AsSpan());
            Assert.Equal("bonono", destination.Slice(0, written).ToString());
        }

        [Fact]
        public void Replace_MatchAtEnd()
        {
            ReadOnlySpan<char> source = "hello abc".AsSpan();
            Span<char> destination = new char[20];
            int written = source.Replace(destination, "abc".AsSpan(), "world".AsSpan());
            Assert.Equal("hello world", destination.Slice(0, written).ToString());
        }

        [Fact]
        public void Replace_EntireSourceIsMatch()
        {
            ReadOnlySpan<char> source = "abc".AsSpan();
            Span<char> destination = new char[5];
            int written = source.Replace(destination, "abc".AsSpan(), "xyz".AsSpan());
            Assert.Equal(3, written);
            Assert.Equal("xyz", destination.Slice(0, written).ToString());
        }

        [Fact]
        public void Replace_ConsecutiveMatches()
        {
            ReadOnlySpan<char> source = "ababab".AsSpan();
            Span<char> destination = new char[20];
            int written = source.Replace(destination, "ab".AsSpan(), "c".AsSpan());
            Assert.Equal("ccc", destination.Slice(0, written).ToString());
        }

        [Fact]
        public void Replace_GenericByte()
        {
            ReadOnlySpan<byte> source = new byte[] { 1, 2, 3, 1, 2, 3, 4 };
            Span<byte> destination = new byte[20];
            ReadOnlySpan<byte> oldValue = new byte[] { 1, 2 };
            ReadOnlySpan<byte> newValue = new byte[] { 9, 8, 7 };
            int written = source.Replace(destination, oldValue, newValue);
            Assert.Equal(9, written);
            Assert.Equal(new byte[] { 9, 8, 7, 3, 9, 8, 7, 3, 4 }, destination.Slice(0, written).ToArray());
        }

        [Fact]
        public void Replace_GenericInt()
        {
            ReadOnlySpan<int> source = new int[] { 10, 20, 30, 10, 20 };
            Span<int> destination = new int[10];
            ReadOnlySpan<int> oldValue = new int[] { 10, 20 };
            ReadOnlySpan<int> newValue = new int[] { 99 };
            int written = source.Replace(destination, oldValue, newValue);
            Assert.Equal(3, written);
            Assert.Equal(new int[] { 99, 30, 99 }, destination.Slice(0, written).ToArray());
        }

        [Fact]
        public void TryReplace_EmptySource()
        {
            ReadOnlySpan<char> source = ReadOnlySpan<char>.Empty;
            Span<char> destination = Span<char>.Empty;
            Assert.True(source.TryReplace(destination, "abc".AsSpan(), "xyz".AsSpan(), out int written));
            Assert.Equal(0, written);
        }

        [Fact]
        public void TryReplace_EmptyOldValue_ThrowsArgumentException()
        {
            char[] src = "hello".ToCharArray();
            char[] dst = new char[10];
            AssertExtensions.Throws<ArgumentException>("oldValue", () => ((ReadOnlySpan<char>)src).TryReplace(dst, ReadOnlySpan<char>.Empty, "x".AsSpan(), out _));
        }

        [Fact]
        public void Replace_LargerNewValue_DestinationExactSize()
        {
            ReadOnlySpan<char> source = "ab".AsSpan();
            Span<char> destination = new char[4];
            int written = source.Replace(destination, "a".AsSpan(), "xyz".AsSpan());
            Assert.Equal(4, written);
            Assert.Equal("xyzb", destination.Slice(0, written).ToString());
        }

        [Fact]
        public void TryReplace_DestinationExactSize_Succeeds()
        {
            string expected = "xyzb";
            ReadOnlySpan<char> source = "ab".AsSpan();
            Span<char> destination = new char[expected.Length];
            Assert.True(source.TryReplace(destination, "a".AsSpan(), "xyz".AsSpan(), out int written));
            Assert.Equal(expected.Length, written);
            Assert.Equal(expected, destination.Slice(0, written).ToString());
        }

        [Fact]
        public void TryReplace_DestinationOneShort_ReturnsFalse()
        {
            ReadOnlySpan<char> source = "ab".AsSpan();
            Span<char> destination = new char[3]; // "xyzb" needs 4
            Assert.False(source.TryReplace(destination, "a".AsSpan(), "xyz".AsSpan(), out int written));
            Assert.Equal(0, written);
        }

        [Fact]
        public void Replace_SameLengthReplacement_AliasedSourceDest()
        {
            char[] buffer = "hello world".ToCharArray();
            Span<char> span = buffer;
            ReadOnlySpan<char> source = buffer;
            int written = source.Replace(span, "o".AsSpan(), "0".AsSpan());
            Assert.Equal(11, written);
            Assert.Equal("hell0 w0rld", new string(buffer));
        }

        [Fact]
        public void Replace_GenericByte_EmptyNewValue_RemovesOccurrences()
        {
            ReadOnlySpan<byte> source = new byte[] { 1, 2, 3, 1, 2, 4 };
            Span<byte> destination = new byte[10];
            ReadOnlySpan<byte> oldValue = new byte[] { 1, 2 };
            int written = source.Replace(destination, oldValue, ReadOnlySpan<byte>.Empty);
            Assert.Equal(2, written);
            Assert.Equal(new byte[] { 3, 4 }, destination.Slice(0, written).ToArray());
        }
    }
}
