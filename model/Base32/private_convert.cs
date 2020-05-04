// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System
{
    [Flags]
    public enum Base64FormattingOptions
    {
        None = 0,
        InsertLineBreaks = 1
    }

    // Returns the type code of this object. An implementation of this method
    // must not return TypeCode.Empty (which represents a null reference) or
    // TypeCode.Object (which represents an object that doesn't implement the
    // IConvertible interface). An implementation of this method should return
    // TypeCode.DBNull if the value of this object is a database null. For
    // example, a nullable integer type should return TypeCode.DBNull if the
    // value of the object is the database null. Otherwise, an implementation
    // of this method should return the TypeCode that best describes the
    // internal representation of the object.
    // The Value class provides conversion and querying methods for values. The
    // Value class contains static members only, and it is not possible to create
    // instances of the class.
    //
    // The statically typed conversion methods provided by the Value class are all
    // of the form:
    //
    //    public static XXX ToXXX(YYY value)
    //
    // where XXX is the target type and YYY is the source type. The matrix below
    // shows the set of supported conversions. The set of conversions is symmetric
    // such that for every ToXXX(YYY) there is also a ToYYY(XXX).
    //
    // From:  To: Bol Chr SBy Byt I16 U16 I32 U32 I64 U64 Sgl Dbl Dec Dat Str
    // ----------------------------------------------------------------------
    // Boolean     x       x   x   x   x   x   x   x   x   x   x   x       x
    // Char            x   x   x   x   x   x   x   x   x                   x
    // SByte       x   x   x   x   x   x   x   x   x   x   x   x   x       x
    // Byte        x   x   x   x   x   x   x   x   x   x   x   x   x       x
    // Int16       x   x   x   x   x   x   x   x   x   x   x   x   x       x
    // UInt16      x   x   x   x   x   x   x   x   x   x   x   x   x       x
    // Int32       x   x   x   x   x   x   x   x   x   x   x   x   x       x
    // UInt32      x   x   x   x   x   x   x   x   x   x   x   x   x       x
    // Int64       x   x   x   x   x   x   x   x   x   x   x   x   x       x
    // UInt64      x   x   x   x   x   x   x   x   x   x   x   x   x       x
    // Single      x       x   x   x   x   x   x   x   x   x   x   x       x
    // Double      x       x   x   x   x   x   x   x   x   x   x   x       x
    // Decimal     x       x   x   x   x   x   x   x   x   x   x   x       x
    // DateTime                                                        x   x
    // String      x   x   x   x   x   x   x   x   x   x   x   x   x   x   x
    // ----------------------------------------------------------------------
    //
    // For dynamic conversions, the Value class provides a set of methods of the
    // form:
    //
    //    public static XXX ToXXX(object value)
    //
    // where XXX is the target type (Boolean, Char, SByte, Byte, Int16, UInt16,
    // Int32, UInt32, Int64, UInt64, Single, Double, Decimal, DateTime,
    // or String). The implementations of these methods all take the form:
    //
    //    public static XXX toXXX(object value) {
    //        return value == null? XXX.Default: ((IConvertible)value).ToXXX();
    //    }
    //
    // The code first checks if the given value is a null reference (which is the
    // same as Value.Empty), in which case it returns the default value for type
    // XXX. Otherwise, a cast to IConvertible is performed, and the appropriate ToXXX()
    // method is invoked on the object. An InvalidCastException is thrown if the
    // cast to IConvertible fails, and that exception is simply allowed to propagate out
    // of the conversion method.

    // Constant representing the database null value. This value is used in
    // database applications to indicate the absence of a known value. Note
    // that Value.DBNull is NOT the same as a null object reference, which is
    // represented by Value.Empty.
    //
    // The Equals() method of DBNull always returns false, even when the
    // argument is itself DBNull.
    //
    // When passed Value.DBNull, the Value.GetTypeCode() method returns
    // TypeCode.DBNull.
    //
    // When passed Value.DBNull, the Value.ToXXX() methods all throw an
    // InvalidCastException.

    public static partial class Convert
    {
        // A typeof operation is fairly expensive (does a system call), so we'll cache these here
        // statically.  These are exactly lined up with the TypeCode, eg. ConvertType[TypeCode.Int16]
        // will give you the type of an short.
        internal static readonly Type[] ConvertTypes = {
            typeof(object),
            typeof(System.DBNull),
            typeof(bool),
            typeof(char),
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
            typeof(object), // TypeCode is discontinuous so we need a placeholder.
            typeof(string)
        };

        // Need to special case Enum because typecode will be underlying type, e.g. Int32
        private static readonly Type EnumType = typeof(Enum);

        internal static readonly char[] base64Table = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
                                                        'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
                                                        'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's',
                                                        't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
                                                        '8', '9', '+', '/', '=' };

        private const int base64LineBreakPosition = 76;

        public static string ToBase64String(byte[] inArray)
        {
            if (inArray == null)
            {
                throw new ArgumentNullException(nameof(inArray));
            }
            return ToBase64String(new ReadOnlySpan<byte>(inArray), Base64FormattingOptions.None);
        }

        public static string ToBase64String(byte[] inArray, Base64FormattingOptions options)
        {
            if (inArray == null)
            {
                throw new ArgumentNullException(nameof(inArray));
            }
            return ToBase64String(new ReadOnlySpan<byte>(inArray), options);
        }

        public static string ToBase64String(byte[] inArray, int offset, int length)
        {
            return ToBase64String(inArray, offset, length, Base64FormattingOptions.None);
        }

        public static string ToBase64String(byte[] inArray, int offset, int length, Base64FormattingOptions options)
        {
            if (inArray == null)
                throw new ArgumentNullException(nameof(inArray));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_Index);
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_GenericPositive);
            if (offset > (inArray.Length - length))
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_OffsetLength);

            return ToBase64String(new ReadOnlySpan<byte>(inArray, offset, length), options);
        }

        public static string ToBase64String(ReadOnlySpan<byte> bytes, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            if (options < Base64FormattingOptions.None || options > Base64FormattingOptions.InsertLineBreaks)
            {
                throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, (int)options), nameof(options));
            }

            if (bytes.Length == 0)
            {
                return string.Empty;
            }

            bool insertLineBreaks = (options == Base64FormattingOptions.InsertLineBreaks);
            string result = string.FastAllocateString(ToBase64_CalculateAndValidateOutputLength(bytes.Length, insertLineBreaks));

            unsafe
            {
                fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
                fixed (char* charsPtr = result)
                {
                    int charsWritten = ConvertToBase64Array(charsPtr, bytesPtr, 0, bytes.Length, insertLineBreaks);
                    Debug.Assert(result.Length == charsWritten, $"Expected {result.Length} == {charsWritten}");
                }
            }

            return result;
        }

        public static int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut)
        {
            return ToBase64CharArray(inArray, offsetIn, length, outArray, offsetOut, Base64FormattingOptions.None);
        }

        public static unsafe int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut, Base64FormattingOptions options)
        {
            // Do data verfication
            if (inArray == null)
                throw new ArgumentNullException(nameof(inArray));
            if (outArray == null)
                throw new ArgumentNullException(nameof(outArray));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_Index);
            if (offsetIn < 0)
                throw new ArgumentOutOfRangeException(nameof(offsetIn), SR.ArgumentOutOfRange_GenericPositive);
            if (offsetOut < 0)
                throw new ArgumentOutOfRangeException(nameof(offsetOut), SR.ArgumentOutOfRange_GenericPositive);

            if (options < Base64FormattingOptions.None || options > Base64FormattingOptions.InsertLineBreaks)
            {
                throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, (int)options), nameof(options));
            }

            int retVal;

            int inArrayLength;
            int outArrayLength;
            int numElementsToCopy;

            inArrayLength = inArray.Length;

            if (offsetIn > (int)(inArrayLength - length))
                throw new ArgumentOutOfRangeException(nameof(offsetIn), SR.ArgumentOutOfRange_OffsetLength);

            if (inArrayLength == 0)
                return 0;

            bool insertLineBreaks = (options == Base64FormattingOptions.InsertLineBreaks);
            // This is the maximally required length that must be available in the char array
            outArrayLength = outArray.Length;

            // Length of the char buffer required
            numElementsToCopy = ToBase64_CalculateAndValidateOutputLength(length, insertLineBreaks);

            if (offsetOut > (int)(outArrayLength - numElementsToCopy))
                throw new ArgumentOutOfRangeException(nameof(offsetOut), SR.ArgumentOutOfRange_OffsetOut);

            fixed (char* outChars = &outArray[offsetOut])
            {
                fixed (byte* inData = &inArray[0])
                {
                    retVal = ConvertToBase64Array(outChars, inData, offsetIn, length, insertLineBreaks);
                }
            }

            return retVal;
        }

        public static unsafe bool TryToBase64Chars(ReadOnlySpan<byte> bytes, Span<char> chars, out int charsWritten, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            if (options < Base64FormattingOptions.None || options > Base64FormattingOptions.InsertLineBreaks)
            {
                throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, (int)options), nameof(options));
            }

            if (bytes.Length == 0)
            {
                charsWritten = 0;
                return true;
            }

            bool insertLineBreaks = (options == Base64FormattingOptions.InsertLineBreaks);

            int charLengthRequired = ToBase64_CalculateAndValidateOutputLength(bytes.Length, insertLineBreaks);
            if (charLengthRequired > chars.Length)
            {
                charsWritten = 0;
                return false;
            }

            fixed (char* outChars = &MemoryMarshal.GetReference(chars))
            fixed (byte* inData = &MemoryMarshal.GetReference(bytes))
            {
                charsWritten = ConvertToBase64Array(outChars, inData, 0, bytes.Length, insertLineBreaks);
                return true;
            }
        }

        private static unsafe int ConvertToBase64Array(char* outChars, byte* inData, int offset, int length, bool insertLineBreaks)
        {
            int lengthmod3 = length % 3;
            int calcLength = offset + (length - lengthmod3);
            int j = 0;
            int charcount = 0;
            // Convert three bytes at a time to base64 notation.  This will consume 4 chars.
            int i;

            // get a pointer to the base64Table to avoid unnecessary range checking
            fixed (char* base64 = &base64Table[0])
            {
                for (i = offset; i < calcLength; i += 3)
                {
                    if (insertLineBreaks)
                    {
                        if (charcount == base64LineBreakPosition)
                        {
                            outChars[j++] = '\r';
                            outChars[j++] = '\n';
                            charcount = 0;
                        }
                        charcount += 4;
                    }
                    outChars[j] = base64[(inData[i] & 0xfc) >> 2];
                    outChars[j + 1] = base64[((inData[i] & 0x03) << 4) | ((inData[i + 1] & 0xf0) >> 4)];
                    outChars[j + 2] = base64[((inData[i + 1] & 0x0f) << 2) | ((inData[i + 2] & 0xc0) >> 6)];
                    outChars[j + 3] = base64[inData[i + 2] & 0x3f];
                    j += 4;
                }

                // Where we left off before
                i = calcLength;

                if (insertLineBreaks && (lengthmod3 != 0) && (charcount == base64LineBreakPosition))
                {
                    outChars[j++] = '\r';
                    outChars[j++] = '\n';
                }

                switch (lengthmod3)
                {
                    case 2: // One character padding needed
                        outChars[j] = base64[(inData[i] & 0xfc) >> 2];
                        outChars[j + 1] = base64[((inData[i] & 0x03) << 4) | ((inData[i + 1] & 0xf0) >> 4)];
                        outChars[j + 2] = base64[(inData[i + 1] & 0x0f) << 2];
                        outChars[j + 3] = base64[64]; // Pad
                        j += 4;
                        break;
                    case 1: // Two character padding needed
                        outChars[j] = base64[(inData[i] & 0xfc) >> 2];
                        outChars[j + 1] = base64[(inData[i] & 0x03) << 4];
                        outChars[j + 2] = base64[64]; // Pad
                        outChars[j + 3] = base64[64]; // Pad
                        j += 4;
                        break;
                }
            }

            return j;
        }

        private static int ToBase64_CalculateAndValidateOutputLength(int inputLength, bool insertLineBreaks)
        {
            long outlen = ((long)inputLength) / 3 * 4;          // the base length - we want integer division here.
            outlen += ((inputLength % 3) != 0) ? 4 : 0;         // at most 4 more chars for the remainder

            if (outlen == 0)
                return 0;

            if (insertLineBreaks)
            {
                long newLines = outlen / base64LineBreakPosition;
                if ((outlen % base64LineBreakPosition) == 0)
                {
                    --newLines;
                }
                outlen += newLines * 2;              // the number of line break chars we'll add, "\r\n"
            }

            // If we overflow an int then we cannot allocate enough
            // memory to output the value so throw
            if (outlen > int.MaxValue)
                throw new OutOfMemoryException();

            return (int)outlen;
        }

        /// <summary>
        /// Converts the specified string, which encodes binary data as Base64 digits, to the equivalent byte array.
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <returns>The array of bytes represented by the specified Base64 string.</returns>
        public static byte[] FromBase64String(string s)
        {
            // "s" is an unfortunate parameter name, but we need to keep it for backward compat.

            if (s == null)
                throw new ArgumentNullException(nameof(s));

            unsafe
            {
                fixed (char* sPtr = s)
                {
                    return FromBase64CharPtr(sPtr, s.Length);
                }
            }
        }

        public static bool TryFromBase64String(string s, Span<byte> bytes, out int bytesWritten)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            return TryFromBase64Chars(s.AsSpan(), bytes, out bytesWritten);
        }

        public static bool TryFromBase64Chars(ReadOnlySpan<char> chars, Span<byte> bytes, out int bytesWritten)
        {
            // This is actually local to one of the nested blocks but is being declared at the top as we don't want multiple stackallocs
            // for each iteraton of the loop.
            Span<char> tempBuffer = stackalloc char[4];  // Note: The tempBuffer size could be made larger than 4 but the size must be a multiple of 4.

            bytesWritten = 0;

            while (chars.Length != 0)
            {
                // Attempt to decode a segment that doesn't contain whitespace.
                bool complete = TryDecodeFromUtf16(chars, bytes, out int consumedInThisIteration, out int bytesWrittenInThisIteration);
                bytesWritten += bytesWrittenInThisIteration;
                if (complete)
                    return true;

                chars = chars.Slice(consumedInThisIteration);
                bytes = bytes.Slice(bytesWrittenInThisIteration);

                Debug.Assert(chars.Length != 0); // If TryDecodeFromUtf16() consumed the entire buffer, it could not have returned false.
                if (chars[0].IsSpace())
                {
                    // If we got here, the very first character not consumed was a whitespace. We can skip past any consecutive whitespace, then continue decoding.

                    int indexOfFirstNonSpace = 1;
                    while (true)
                    {
                        if (indexOfFirstNonSpace == chars.Length)
                            break;
                        if (!chars[indexOfFirstNonSpace].IsSpace())
                            break;
                        indexOfFirstNonSpace++;
                    }

                    chars = chars.Slice(indexOfFirstNonSpace);

                    if ((bytesWrittenInThisIteration % 3) != 0 && chars.Length != 0)
                    {
                        // If we got here, the last successfully decoded block encountered an end-marker, yet we have trailing non-whitespace characters.
                        // That is not allowed.
                        bytesWritten = default;
                        return false;
                    }

                    // We now loop again to decode the next run of non-space characters.
                }
                else
                {
                    Debug.Assert(chars.Length != 0 && !chars[0].IsSpace());

                    // If we got here, it is possible that there is whitespace that occurred in the middle of a 4-byte chunk. That is, we still have
                    // up to three Base64 characters that were left undecoded by the fast-path helper because they didn't form a complete 4-byte chunk.
                    // This is hopefully the rare case (multiline-formatted base64 message with a non-space character width that's not a multiple of 4.)
                    // We'll filter out whitespace and copy the remaining characters into a temporary buffer.
                    CopyToTempBufferWithoutWhiteSpace(chars, tempBuffer, out int consumedFromChars, out int charsWritten);
                    if ((charsWritten & 0x3) != 0)
                    {
                        // Even after stripping out whitespace, the number of characters is not divisible by 4. This cannot be a legal Base64 string.
                        bytesWritten = default;
                        return false;
                    }

                    tempBuffer = tempBuffer.Slice(0, charsWritten);
                    if (!TryDecodeFromUtf16(tempBuffer, bytes, out int consumedFromTempBuffer, out int bytesWrittenFromTempBuffer))
                    {
                        bytesWritten = default;
                        return false;
                    }
                    bytesWritten += bytesWrittenFromTempBuffer;
                    chars = chars.Slice(consumedFromChars);
                    bytes = bytes.Slice(bytesWrittenFromTempBuffer);

                    if ((bytesWrittenFromTempBuffer % 3) != 0)
                    {
                        // If we got here, this decode contained one or more padding characters ('='). We can accept trailing whitespace after this
                        // but nothing else.
                        for (int i = 0; i < chars.Length; i++)
                        {
                            if (!chars[i].IsSpace())
                            {
                                bytesWritten = default;
                                return false;
                            }
                        }
                        return true;
                    }

                    // We now loop again to decode the next run of non-space characters.
                }
            }

            return true;
        }

        private static void CopyToTempBufferWithoutWhiteSpace(ReadOnlySpan<char> chars, Span<char> tempBuffer, out int consumed, out int charsWritten)
        {
            Debug.Assert(tempBuffer.Length != 0); // We only bound-check after writing a character to the tempBuffer.

            charsWritten = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!c.IsSpace())
                {
                    tempBuffer[charsWritten++] = c;
                    if (charsWritten == tempBuffer.Length)
                    {
                        consumed = i + 1;
                        return;
                    }
                }
            }
            consumed = chars.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSpace(this char c) => c == ' ' || c == '\t' || c == '\r' || c == '\n';

        /// <summary>
        /// Converts the specified range of a Char array, which encodes binary data as Base64 digits, to the equivalent byte array.
        /// </summary>
        /// <param name="inArray">Chars representing Base64 encoding characters</param>
        /// <param name="offset">A position within the input array.</param>
        /// <param name="length">Number of element to convert.</param>
        /// <returns>The array of bytes represented by the specified Base64 encoding characters.</returns>
        public static byte[] FromBase64CharArray(char[] inArray, int offset, int length)
        {
            if (inArray == null)
                throw new ArgumentNullException(nameof(inArray));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_Index);

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_GenericPositive);

            if (offset > inArray.Length - length)
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_OffsetLength);

            if (inArray.Length == 0)
            {
                return Array.Empty<byte>();
            }

            unsafe
            {
                fixed (char* inArrayPtr = &inArray[0])
                {
                    return FromBase64CharPtr(inArrayPtr + offset, length);
                }
            }
        }

        /// <summary>
        /// Convert Base64 encoding characters to bytes:
        ///  - Compute result length exactly by actually walking the input;
        ///  - Allocate new result array based on computation;
        ///  - Decode input into the new array;
        /// </summary>
        /// <param name="inputPtr">Pointer to the first input char</param>
        /// <param name="inputLength">Number of input chars</param>
        /// <returns></returns>
        private static unsafe byte[] FromBase64CharPtr(char* inputPtr, int inputLength)
        {
            // The validity of parameters much be checked by callers, thus we are Critical here.

            Debug.Assert(0 <= inputLength);

            // We need to get rid of any trailing white spaces.
            // Otherwise we would be rejecting input such as "abc= ":
            while (inputLength > 0)
            {
                int lastChar = inputPtr[inputLength - 1];
                if (lastChar != (int)' ' && lastChar != (int)'\n' && lastChar != (int)'\r' && lastChar != (int)'\t')
                    break;
                inputLength--;
            }

            // Compute the output length:
            int resultLength = FromBase64_ComputeResultLength(inputPtr, inputLength);

            Debug.Assert(0 <= resultLength);

            // resultLength can be zero. We will still enter FromBase64_Decode and process the input.
            // It may either simply write no bytes (e.g. input = " ") or throw (e.g. input = "ab").

            // Create result byte blob:
            byte[] decodedBytes = new byte[resultLength];

            // Convert Base64 chars into bytes:
            if (!TryFromBase64Chars(new ReadOnlySpan<char>(inputPtr, inputLength), decodedBytes, out int _))
                throw new FormatException(SR.Format_BadBase64Char);

            // Note that the number of bytes written can differ from resultLength if the caller is modifying the array
            // as it is being converted. Silently ignore the failure.
            // Consider throwing exception in an non in-place release.

            // We are done:
            return decodedBytes;
        }

        /// <summary>
        /// Compute the number of bytes encoded in the specified Base 64 char array:
        /// Walk the entire input counting white spaces and padding chars, then compute result length
        /// based on 3 bytes per 4 chars.
        /// </summary>
        private static unsafe int FromBase64_ComputeResultLength(char* inputPtr, int inputLength)
        {
            const uint intEq = (uint)'=';
            const uint intSpace = (uint)' ';

            Debug.Assert(0 <= inputLength);

            char* inputEndPtr = inputPtr + inputLength;
            int usefulInputLength = inputLength;
            int padding = 0;

            while (inputPtr < inputEndPtr)
            {
                uint c = (uint)(*inputPtr);
                inputPtr++;

                // We want to be as fast as possible and filter out spaces with as few comparisons as possible.
                // We end up accepting a number of illegal chars as legal white-space chars.
                // This is ok: as soon as we hit them during actual decode we will recognise them as illegal and throw.
                if (c <= intSpace)
                    usefulInputLength--;
                else if (c == intEq)
                {
                    usefulInputLength--;
                    padding++;
                }
            }

            Debug.Assert(0 <= usefulInputLength);

            // For legal input, we can assume that 0 <= padding < 3. But it may be more for illegal input.
            // We will notice it at decode when we see a '=' at the wrong place.
            Debug.Assert(0 <= padding);

            // Perf: reuse the variable that stored the number of '=' to store the number of bytes encoded by the
            // last group that contains the '=':
            if (padding != 0)
            {
                if (padding == 1)
                    padding = 2;
                else if (padding == 2)
                    padding = 1;
                else
                    throw new FormatException(SR.Format_BadBase64Char);
            }

            // Done:
            return (usefulInputLength / 4) * 3 + padding;
        }
    }  // class Convert
}  // namespace
