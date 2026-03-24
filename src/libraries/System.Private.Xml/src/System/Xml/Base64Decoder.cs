// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;

namespace System.Xml
{
    internal sealed class Base64Decoder : IncrementalReadDecoder
    {
        private byte[]? _buffer;
        private int _startIndex;
        private int _curIndex;
        private int _endIndex;

        // When the destination buffer has fewer than 3 bytes remaining, we decode into a
        // temporary buffer and store the overflow here for the next SetNextOutputBuffer call.
        private byte _leftoverByte1;
        private byte _leftoverByte2;
        private int _leftoverCount;

        // When Base64.DecodeFromChars returns NeedMoreData (partial group at end of a
        // non-final chunk), we buffer the 1-3 non-whitespace chars here so the caller
        // always sees all input chars consumed — matching the IncrementalReadDecoder contract.
        private char _pendingChar1;
        private char _pendingChar2;
        private char _pendingChar3;
        private int _pendingCharCount;

        internal override int DecodedCount => _curIndex - _startIndex;

        internal override bool IsFull => _curIndex == _endIndex;

        internal override int Decode(char[] chars, int startPos, int len)
        {
            ArgumentNullException.ThrowIfNull(chars);
            ArgumentOutOfRangeException.ThrowIfNegative(len);
            ArgumentOutOfRangeException.ThrowIfNegative(startPos);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(len, chars.Length - startPos);

            if (len == 0)
            {
                return 0;
            }

            Decode(chars.AsSpan(startPos, len), _buffer.AsSpan(_curIndex, _endIndex - _curIndex), out int charsDecoded, out int bytesDecoded);
            _curIndex += bytesDecoded;
            return charsDecoded;
        }

        internal override int Decode(string str, int startPos, int len)
        {
            ArgumentNullException.ThrowIfNull(str);
            ArgumentOutOfRangeException.ThrowIfNegative(len);
            ArgumentOutOfRangeException.ThrowIfNegative(startPos);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(len, str.Length - startPos);

            if (len == 0)
            {
                return 0;
            }

            Decode(str.AsSpan(startPos, len), _buffer.AsSpan(_curIndex, _endIndex - _curIndex), out int charsDecoded, out int bytesDecoded);
            _curIndex += bytesDecoded;
            return charsDecoded;
        }

        internal override void Reset()
        {
            _leftoverCount = 0;
            _pendingCharCount = 0;
        }

        internal override void SetNextOutputBuffer(Array buffer, int index, int count)
        {
            Debug.Assert(buffer != null);
            Debug.Assert(count >= 0);
            Debug.Assert(index >= 0);
            Debug.Assert(buffer.Length - index >= count);
            Debug.Assert((buffer as byte[]) != null);

            _buffer = (byte[])buffer;
            _startIndex = index;
            _curIndex = index;
            _endIndex = index + count;

            // Drain any leftover bytes from a previous small-buffer decode
            // into this new output buffer so they aren't lost.
            if (_leftoverCount > 0 && _curIndex < _endIndex)
            {
                _buffer[_curIndex++] = _leftoverByte1;
                _leftoverCount--;

                if (_leftoverCount > 0 && _curIndex < _endIndex)
                {
                    _buffer[_curIndex++] = _leftoverByte2;
                    _leftoverCount--;
                }
                else if (_leftoverCount > 0)
                {
                    // Output buffer too small for second leftover byte; shift it down.
                    _leftoverByte1 = _leftoverByte2;
                }
            }
        }

        private void Decode(ReadOnlySpan<char> chars, Span<byte> bytes, out int charsDecoded, out int bytesDecoded)
        {
            charsDecoded = 0;
            bytesDecoded = 0;

            if (chars.IsEmpty || bytes.IsEmpty)
            {
                return;
            }

            // Complete any pending partial group from a previous chunk boundary.
            if (_pendingCharCount > 0)
            {
                CompletePendingGroup(chars, bytes, out charsDecoded, out bytesDecoded);

                if (charsDecoded >= chars.Length || bytesDecoded >= bytes.Length)
                {
                    return;
                }

                chars = chars[charsDecoded..];
                bytes = bytes[bytesDecoded..];
            }

            // Determine whether padding ('=') is present in this chunk.
            // When padding is present, we pass everything through the padding
            // to DecodeFromChars with isFinalBlock: true, then manually
            // validate that only whitespace follows.
            int paddingIdx = chars.IndexOf('=');
            bool hasPadding = paddingIdx >= 0;

            ReadOnlySpan<char> decodeSource;
            int afterPaddingEnd;

            if (hasPadding)
            {
                afterPaddingEnd = paddingIdx + 1;
                while ((uint)afterPaddingEnd < (uint)chars.Length && chars[afterPaddingEnd] == '=')
                {
                    afterPaddingEnd++;
                }

                decodeSource = chars[..afterPaddingEnd];
            }
            else
            {
                decodeSource = chars;
                afterPaddingEnd = chars.Length;
            }

            // Decode the base64 data.
            DecodeCore(decodeSource, bytes, hasPadding, out int coreConsumed, out int coreWritten);
            charsDecoded += coreConsumed;
            bytesDecoded += coreWritten;

            // After padding, only whitespace is allowed.
            if (hasPadding && coreConsumed >= decodeSource.Length)
            {
                for (int i = afterPaddingEnd; i < chars.Length; i++)
                {
                    char ch = chars[i];
                    if (!XmlCharType.IsWhiteSpace(ch))
                    {
                        throw new XmlException(SR.Xml_InvalidBase64Value, ch.ToString());
                    }
                }

                charsDecoded += chars.Length - coreConsumed;
            }
        }

        /// <summary>
        /// Completes a partial base64 group that was buffered from a previous Decode call.
        /// Collects enough non-whitespace chars from the new input to form one complete group,
        /// decodes it, and reports the new-input chars consumed.
        /// </summary>
        private void CompletePendingGroup(ReadOnlySpan<char> chars, Span<byte> bytes,
            out int charsConsumed, out int bytesWritten)
        {
            charsConsumed = 0;
            bytesWritten = 0;

            // Build a group buffer: pending chars + new non-whitespace chars.
            Span<char> group = stackalloc char[4];
            int gIdx = 0;

            if (_pendingCharCount >= 1) group[gIdx++] = _pendingChar1;
            if (_pendingCharCount >= 2) group[gIdx++] = _pendingChar2;
            if (_pendingCharCount >= 3) group[gIdx++] = _pendingChar3;

            int neededNonWs = 4 - gIdx;
            _pendingCharCount = 0;

            bool hitPadding = false;

            // Scan new input for non-whitespace chars to complete the group.
            while (charsConsumed < chars.Length && neededNonWs > 0)
            {
                char c = chars[charsConsumed];

                if (c == '=')
                {
                    hitPadding = true;
                    break;
                }

                charsConsumed++;

                if (XmlCharType.IsWhiteSpace(c))
                {
                    continue;
                }

                if (!IsBase64Char(c))
                {
                    throw new XmlException(SR.Xml_InvalidBase64Value, c.ToString());
                }

                group[gIdx++] = c;
                neededNonWs--;
            }

            if (hitPadding)
            {
                // Fill the remaining group slots with '=' padding chars.
                while (gIdx < 4 && charsConsumed < chars.Length && chars[charsConsumed] == '=')
                {
                    group[gIdx++] = '=';
                    charsConsumed++;
                }
            }

            if (neededNonWs > 0 && !hitPadding)
            {
                // Not enough chars to complete the group yet — re-buffer what we have.
                Debug.Assert(gIdx is > 0 and <= 3);

                if (gIdx >= 1) _pendingChar1 = group[0];
                if (gIdx >= 2) _pendingChar2 = group[1];
                if (gIdx >= 3) _pendingChar3 = group[2];
                _pendingCharCount = gIdx;

                // Report all new-input chars as consumed so callers always advance.
                charsConsumed = chars.Length;
                return;
            }

            if (gIdx == 0)
            {
                return;
            }

            // Decode the completed group into a temp buffer (handles small output buffers).
            Span<byte> temp = stackalloc byte[3];
            OperationStatus status = Base64.DecodeFromChars(group[..gIdx], temp, out _, out int written, isFinalBlock: true);

            if (status == OperationStatus.InvalidData)
            {
                // Check for genuinely invalid chars; silently consume if none found
                // (e.g., malformed padding like "A===").
                foreach (char ch in group[..gIdx])
                {
                    if (!XmlCharType.IsWhiteSpace(ch) && ch != '=' && !IsBase64Char(ch))
                    {
                        throw new XmlException(SR.Xml_InvalidBase64Value, ch.ToString());
                    }
                }

                return;
            }

            int toCopy = Math.Min(written, bytes.Length);
            temp[..toCopy].CopyTo(bytes);
            bytesWritten = toCopy;

            int overflow = written - toCopy;
            if (overflow >= 1)
            {
                _leftoverByte1 = temp[toCopy];
                if (overflow >= 2)
                {
                    _leftoverByte2 = temp[toCopy + 1];
                }

                _leftoverCount = overflow;
            }

            // If we hit padding, validate that remaining chars are whitespace only.
            if (hitPadding)
            {
                while (charsConsumed < chars.Length)
                {
                    char ch = chars[charsConsumed];
                    if (!XmlCharType.IsWhiteSpace(ch))
                    {
                        throw new XmlException(SR.Xml_InvalidBase64Value, ch.ToString());
                    }

                    charsConsumed++;
                }
            }
        }

        private void DecodeCore(ReadOnlySpan<char> source, Span<byte> dest, bool isFinalBlock,
            out int totalConsumed, out int totalWritten)
        {
            OperationStatus status = Base64.DecodeFromChars(source, dest, out totalConsumed, out totalWritten, isFinalBlock);

            switch (status)
            {
                case OperationStatus.Done:
                    break;

                case OperationStatus.InvalidData:
                    HandleInvalidData(source, isFinalBlock, ref totalConsumed);
                    break;

                case OperationStatus.NeedMoreData:
                    // Buffer the trailing non-whitespace chars (partial group) so the caller
                    // always sees all input chars consumed, matching the IncrementalReadDecoder contract.
                    BufferPendingChars(source[totalConsumed..]);
                    totalConsumed = source.Length;
                    break;

                case OperationStatus.DestinationTooSmall:
                    // The remaining destination may have 1-2 bytes that the BCL API cannot
                    // fill (it needs >= 3 bytes per non-padded group). Decode one more group
                    // via a temporary buffer and store the overflow.
                    int remaining = dest.Length - totalWritten;
                    if (remaining > 0 && totalConsumed < source.Length)
                    {
                        DecodeWithOverflow(source[totalConsumed..], dest[totalWritten..], isFinalBlock,
                            ref totalConsumed, ref totalWritten);
                    }

                    break;
            }
        }

        private void BufferPendingChars(ReadOnlySpan<char> remaining)
        {
            _pendingCharCount = 0;

            foreach (char c in remaining)
            {
                if (!XmlCharType.IsWhiteSpace(c))
                {
                    Debug.Assert(_pendingCharCount < 3);

                    switch (_pendingCharCount)
                    {
                        case 0: _pendingChar1 = c; break;
                        case 1: _pendingChar2 = c; break;
                        case 2: _pendingChar3 = c; break;
                    }

                    _pendingCharCount++;
                }
            }
        }

        private void DecodeWithOverflow(ReadOnlySpan<char> source, Span<byte> dest, bool isFinalBlock,
            ref int totalConsumed, ref int totalWritten)
        {
            Debug.Assert(dest.Length is > 0 and < 3);

            Span<byte> temp = stackalloc byte[3];
            OperationStatus status = Base64.DecodeFromChars(source, temp, out int consumed, out int written, isFinalBlock);

            if (status == OperationStatus.InvalidData)
            {
                HandleInvalidData(source, isFinalBlock, ref consumed);
                totalConsumed += consumed;
                return;
            }

            if (status == OperationStatus.NeedMoreData)
            {
                BufferPendingChars(source[consumed..]);
                consumed = source.Length;
            }

            int toCopy = Math.Min(written, dest.Length);
            temp[..toCopy].CopyTo(dest);
            totalConsumed += consumed;
            totalWritten += toCopy;

            // Buffer any bytes that didn't fit into the destination.
            int overflow = written - toCopy;
            if (overflow >= 1)
            {
                _leftoverByte1 = temp[toCopy];
                if (overflow >= 2)
                {
                    _leftoverByte2 = temp[toCopy + 1];
                }

                _leftoverCount = overflow;
            }
        }

        private static void HandleInvalidData(ReadOnlySpan<char> source, bool isFinalBlock,
            ref int consumed)
        {
            if (consumed == 0 && isFinalBlock)
            {
                // The BCL rejected the entire input (e.g., malformed padding like "==" alone).
                // Scan for genuinely invalid characters; if there are none, silently consume
                // to maintain the original lenient XML behavior.
                foreach (char ch in source)
                {
                    if (!XmlCharType.IsWhiteSpace(ch) && ch != '=' && !IsBase64Char(ch))
                    {
                        throw new XmlException(SR.Xml_InvalidBase64Value, ch.ToString());
                    }
                }

                consumed = source.Length;
                return;
            }

            ThrowForInvalidChar(source[consumed..]);
        }

        private static void ThrowForInvalidChar(ReadOnlySpan<char> chars)
        {
            foreach (char ch in chars)
            {
                if (!XmlCharType.IsWhiteSpace(ch) && ch != '=' && !IsBase64Char(ch))
                {
                    throw new XmlException(SR.Xml_InvalidBase64Value, ch.ToString());
                }
            }

            // Fallback: the BCL flagged InvalidData but we couldn't find the bad char
            // (shouldn't happen in practice).
            throw new XmlException(SR.Xml_InvalidBase64Value, string.Empty);
        }

        private static bool IsBase64Char(char ch) =>
            char.IsAsciiLetterOrDigit(ch) || ch is '+' or '/';
    }
}
