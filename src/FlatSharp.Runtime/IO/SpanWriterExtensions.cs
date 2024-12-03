/*
 * Copyright 2024 James Courtney
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Runtime.InteropServices;

namespace FlatSharp.Internal;

/// <summary>
/// Extension methods that apply to all <see cref="ISpanWriter"/> implementations.
/// </summary>
public static class SpanWriterExtensions
{
    public static int WriteReadOnlyByteMemoryBlock<TSpanWriter>(
        this TSpanWriter spanWriter,
        Span<byte> span,
        ReadOnlyMemory<byte> memory,
        SerializationContext ctx) where TSpanWriter : ISpanWriter
    {
        int numberOfItems = memory.Length;
        int vectorStartOffset = ctx.AllocateVector(itemAlignment: sizeof(byte), numberOfItems, sizePerItem: sizeof(byte));

        spanWriter.WriteInt(span, numberOfItems, vectorStartOffset);

        memory.Span.CopyTo(span.Slice(vectorStartOffset + sizeof(uint)));

        return vectorStartOffset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int UnsafeWriteSpan<TSpanWriter, TElement>(
        this TSpanWriter spanWriter,
        Span<byte> span,
        Span<TElement> buffer,
        int alignment,
        SerializationContext ctx) where TSpanWriter : ISpanWriter where TElement : unmanaged
    {
        // Since we are copying bytes here, only LE is supported.
        FlatSharpInternal.AssertLittleEndian();
        FlatSharpInternal.AssertWellAligned<TElement>(alignment);

        int numberOfItems = buffer.Length;
        int vectorStartOffset = ctx.AllocateVector(
            itemAlignment: alignment,
            numberOfItems,
            sizePerItem: Unsafe.SizeOf<TElement>());

        spanWriter.WriteInt(span, numberOfItems, vectorStartOffset);

        var start = span.Slice(vectorStartOffset + sizeof(uint), checked(numberOfItems * Unsafe.SizeOf<TElement>()));

        MemoryMarshal.Cast<TElement, byte>(buffer).CopyTo(start);

        return vectorStartOffset;
    }

    /// <summary>
    /// Writes the given string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteString<TSpanWriter>(
        this TSpanWriter spanWriter,
        Span<byte> span,
        string value,
        int offset,
        SerializationContext context) where TSpanWriter : ISpanWriter
    {
        int stringOffset = spanWriter.WriteAndProvisionString(span, value, context);
        spanWriter.WriteUOffset(span, stringOffset, offset);
    }

    /// <summary>
    /// Writes the string to the buffer, returning the absolute offset of the string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteAndProvisionString<TSpanWriter>(this TSpanWriter spanWriter, Span<byte> span, string value, SerializationContext context)
        where TSpanWriter : ISpanWriter
    {
        var encoding = SerializationHelpers.Encoding;

        int count = encoding.GetByteCount(value);
        int stringStartOffset = context.AllocateVector(sizeof(byte), count + 1, sizeof(byte));

        int bytesWritten = spanWriter.GetStringBytes(span.Slice(stringStartOffset + sizeof(uint)), value, encoding);
        spanWriter.WriteInt(span, bytesWritten, stringStartOffset);

        // null teriminator
        span[stringStartOffset + bytesWritten + sizeof(uint)] = 0;

        return stringStartOffset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUOffset<TSpanWriter>(this TSpanWriter spanWriter, Span<byte> span, int offsetToWrite, int offset)
        where TSpanWriter : ISpanWriter
    {
        checked
        {
            uint uoffset = (uint)(offsetToWrite - offset);
            spanWriter.WriteUInt(span, uoffset, offset);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBool<TSpanWriter>(this TSpanWriter spanWriter, Span<byte> span, bool b, int offset)
        where TSpanWriter : ISpanWriter
    {
        spanWriter.WriteByte(span, b ? SerializationHelpers.True : SerializationHelpers.False, offset);
    }

    [ExcludeFromCodeCoverage]
    [Conditional("DEBUG")]
    public static void CheckAlignment<TSpanWriter>(this TSpanWriter spanWriter, int offset, int size) where TSpanWriter : ISpanWriter
    {
#if DEBUG
        if (offset % size != 0)
        {
            FSThrow.InvalidOperation($"BugCheck: attempted to read unaligned data at index: {offset}, expected alignment: {size}");
        }
#endif
    }
}
