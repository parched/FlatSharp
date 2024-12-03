/*
 * Copyright 2020 James Courtney
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

namespace FlatSharp;

/// <summary>
/// A shared string writer that uses a direct-map hash table.
/// </summary>
public class SharedStringWriter : ISharedStringWriter
{
    private const int DefaultCapacity = 1019;
    private readonly WriteCacheEntry[] sharedStringOffsetCache;

    /// <summary>
    /// Initializes a new shared string writer with the default capacity.
    /// </summary>
    public SharedStringWriter() : this(null)
    {
    }

    /// <summary>
    /// Initializes a new shared string writer with the given capacity.
    /// </summary>
    /// <param name="hashTableCapacity">The size of the hash table.</param>
    public SharedStringWriter(int? hashTableCapacity = null)
    {
        if (hashTableCapacity <= 0)
        {
            FSThrow.ArgumentOutOfRange(nameof(hashTableCapacity));
        }

        this.sharedStringOffsetCache = new WriteCacheEntry[hashTableCapacity ?? DefaultCapacity];
        this.IsDirty = true; // force reset to be called the first time.
    }

    public bool IsDirty { get; private set; }

    /// <summary>
    /// Resets the internal state to prepare for a new write operation.
    /// </summary>
    public void Reset()
    {
        var cache = this.sharedStringOffsetCache;
        for (int i = 0; i < cache.Length; ++i)
        {
            cache[i] = default;
        }

        this.IsDirty = false;
    }

    /// <summary>
    /// Writes a shared string.
    /// </summary>
    public int WriteSharedString<TSpanWriter>(
        TSpanWriter spanWriter,
        Span<byte> data,
        string value,
        SerializationContext context) where TSpanWriter : ISpanWriter
    {
        // Find the associative set that must contain our key.
        var cache = this.sharedStringOffsetCache;
        int lineIndex = (int.MaxValue & value.GetHashCode()) % cache.Length;
        ref WriteCacheEntry line = ref cache[lineIndex];

        if (value.Equals(line.String))
        {
            return line.Offset;
        }

        int stringOffset = spanWriter.WriteAndProvisionString(data, value, context);

        line.String = value;
        line.Offset = stringOffset;
        
        this.IsDirty = true;

        return stringOffset;
    }

    // Cache entry. Stored as struct to increase data locality in the array.
    private struct WriteCacheEntry
    {
        // The string
        public string? String;

        public int Offset;
    }
}
