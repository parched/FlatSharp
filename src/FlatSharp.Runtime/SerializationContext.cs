﻿/*
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

using System.Threading;

namespace FlatSharp.Internal;

/// <summary>
/// A context object for a FlatBuffer serialize operation. The context is responsible for allocating space in the buffer
/// and managing the latest offset.
/// </summary>
public sealed class SerializationContext
{
    /// <summary>
    /// A delegate to invoke after the serialization process has completed. Used for sorting vectors.
    /// </summary>
    public delegate void PostSerializeAction(Span<byte> span, SerializationContext context);

    internal static readonly ThreadLocal<SerializationContext> ThreadLocalContext = new ThreadLocal<SerializationContext>(() => new SerializationContext());

    private int offset;
    private int capacity;
    private readonly List<PostSerializeAction> postSerializeActions;
    private readonly List<int> vtableOffsets;

    public Dictionary<object, int> ObjectOffsets { get; }= new Dictionary<object, int>(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Initializes a new serialization context.
    /// </summary>
    public SerializationContext()
    {
        this.postSerializeActions = new List<PostSerializeAction>();
        this.vtableOffsets = new List<int>();
    }

    /// <summary>
    /// The maximum offset within the buffer.
    /// </summary>
    public int Offset
    {
        get => this.offset;
        set => this.offset = value;
    }

    /// <summary>
    /// The shared string writer used for this serialization operation.
    /// </summary>
    public ISharedStringWriter? SharedStringWriter { get; set; }

    /// <summary>
    /// Resets the context.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset(int capacity)
    {
        this.offset = capacity;
        this.capacity = capacity;
        this.SharedStringWriter = null;
        this.postSerializeActions.Clear();
        this.vtableOffsets.Clear();
        this.ObjectOffsets.Clear();
    }

    /// <summary>
    /// Invokes any post-serialize actions.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InvokePostSerializeActions(Span<byte> span)
    {
        var actions = this.postSerializeActions;
        int count = actions.Count;

        for (int i = 0; i < count; ++i)
        {
            actions[i](span, this);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddPostSerializeAction(PostSerializeAction action)
    {
        this.postSerializeActions.Add(action);
    }

    /// <summary>
    /// Allocate a vector and return the index. Does not populate any details of the vector.
    /// </summary>
    public int AllocateVector(int itemAlignment, int numberOfItems, int sizePerItem)
    {
        if (numberOfItems < 0)
        {
            FSThrow.ArgumentOutOfRange(nameof(numberOfItems));
        }

        int bytesNeededForItems = checked(numberOfItems * sizePerItem);

        // Vectors have a size uoffset_t, followed by N items.
        // The uoffset_t needs to be 4 byte aligned, while the items need to be N byte aligned.
        this.AllocateSpace(bytesNeededForItems, alignment: itemAlignment);
        int offset = this.AllocateSpace(sizeof(uint), sizeof(uint));

        Debug.Assert(offset % 4 == 0);
        Debug.Assert((offset + 4) % itemAlignment == 0);

        return offset;
    }

    /// <summary>
    /// Allocates a block of memory. Returns the offset.
    /// </summary>
    public int AllocateSpace(int bytesNeeded, int alignment)
    {
        Debug.Assert((alignment & (alignment - 1)) == 0, "Alignment must be a power of 2.");

        int offset = this.offset;
        if (offset < bytesNeeded)
        {
            FSThrow.BufferTooSmall(0);
        }

        int newOffset = SerializationHelpers.AlignBackwards(offset - bytesNeeded, alignment);

        this.offset = newOffset;
        return newOffset;
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // Common method; don't inline
    public int FinishVTable(
        Span<byte> buffer,
        Span<byte> vtable)
    {
        var offsets = this.vtableOffsets;
        int count = offsets.Count;

        for (int i = 0; i < count; ++i)
        {
            int offset = offsets[i];

            ReadOnlySpan<byte> existingVTable = buffer.Slice(offset);
            existingVTable = existingVTable.Slice(0, ScalarSpanReader.ReadUShort(existingVTable));

            if (existingVTable.SequenceEqual(vtable))
            {
                // Slowly bubble used things towards the front of the list.
                // This is not exact, but should keep frequently used
                // items towards the front.
                Promote(i, offsets);

                return offset;
            }
        }

        // Oh, well. Write the new table.
        int newVTableOffset = this.AllocateSpace(vtable.Length, sizeof(ushort));
        vtable.CopyTo(buffer.Slice(newVTableOffset));
        offsets.Add(newVTableOffset);

        // "Insert" this item in the middle of the list.
        int maxIndex = offsets.Count - 1;
        Promote(maxIndex, offsets);

        return newVTableOffset;

        // Promote frequently-used items to be closer to the front of the list.
        // This is done with a swap to avoid shuffling the whole list by inserting
        // at a given index. An alternative might be an unrolled linked list data structure.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Promote(int i, List<int> offsets)
        {
            int swapIndex = i / 2;

            int temp = offsets[i];
            offsets[i] = offsets[swapIndex];
            offsets[swapIndex] = temp;
        }
    }
}

internal sealed class ReferenceEqualityComparer : IEqualityComparer<object>
{
    public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

    public new bool Equals(object x, object y) => ReferenceEquals(x, y);

    public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
}