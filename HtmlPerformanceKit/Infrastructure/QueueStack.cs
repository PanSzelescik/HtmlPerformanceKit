using System;

namespace HtmlPerformanceKit.Infrastructure;

/// <summary>
/// Implements a queue where it is also possible to insert items in the beginning of the queue.
/// </summary>
internal class QueueStack
{
    private int firstIndex;
    private int lastIndex;
    private int[] items;
    private char[] memoryItems;
    private char[] copyBuffer;

    internal QueueStack(int capacity)
    {
        Capacity = capacity;
        items = new int[capacity];
        memoryItems = new char[capacity];
        copyBuffer = new char[capacity];
    }

    internal int Count { get; private set; }

    internal int Capacity { get; private set; }

    internal void Push(int item)
    {
        if (firstIndex == 0)
        {
            firstIndex = Capacity - 1;
        }
        else
        {
            firstIndex--;
        }

        if (firstIndex == lastIndex)
        {
            Resize();
        }

        items[firstIndex] = item;
        memoryItems[firstIndex] = (char)item;
        Count++;
    }

    internal void Enqueue(int item)
    {
        items[lastIndex] = item;
        memoryItems[lastIndex] = (char)item;
        Count++;

        if (lastIndex == Capacity - 1)
        {
            lastIndex = 0;
        }
        else
        {
            lastIndex++;
        }

        if (firstIndex == lastIndex)
        {
            Resize();
        }
    }

    internal int Peek()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Collection is empty.");
        }

        return items[firstIndex];
    }

    internal int Dequeue()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Collection is empty.");
        }

        var item = items[firstIndex];

        if (firstIndex == Capacity - 1)
        {
            firstIndex = 0;
        }
        else
        {
            firstIndex++;
        }

        Count--;

        if (Count == 0)
        {
            firstIndex = 0;
            lastIndex = 0;
        }

        return item;
    }

    internal ReadOnlyMemory<char> AsMemory()
    {
        if (firstIndex < lastIndex)
        {
            var length = lastIndex - firstIndex;
            for (var index = firstIndex; index < lastIndex; index++)
            {
                if (items[index] == -1)
                {
                    length = index - firstIndex;
                    break;
                }
            }

            return new ReadOnlyMemory<char>(memoryItems, firstIndex, length);
        }

        var copyBufferIndex2 = 0;
        for (var index = firstIndex; index < Capacity; index++, copyBufferIndex2++)
        {
            var read = items[index];
            if (read == -1)
            {
                return new ReadOnlyMemory<char>(copyBuffer, 0, copyBufferIndex2);
            }

            copyBuffer[copyBufferIndex2] = memoryItems[index];
        }

        for (var index = 0; index < firstIndex; index++, copyBufferIndex2++)
        {
            var read = items[index];
            if (read == -1)
            {
                break;
            }

            copyBuffer[copyBufferIndex2] = memoryItems[index];
        }

        return new ReadOnlyMemory<char>(copyBuffer, 0, copyBufferIndex2);
    }

    private void Resize()
    {
        var newCapacity = Capacity * 2;
        var newItems = new int[newCapacity];
        var newMemoryItems = new char[newCapacity];

        if (firstIndex < lastIndex)
        {
            Array.Copy(items, newItems, items.Length);
            Array.Copy(memoryItems, newMemoryItems, memoryItems.Length);
        }
        else
        {
            Array.Copy(items, firstIndex, newItems, 0, Capacity - firstIndex);
            Array.Copy(items, 0, newItems, Capacity - firstIndex, lastIndex);
            Array.Copy(memoryItems, firstIndex, newMemoryItems, 0, Capacity - firstIndex);
            Array.Copy(memoryItems, 0, newMemoryItems, Capacity - firstIndex, lastIndex);
            firstIndex = 0;
            lastIndex = Count;
        }

        Capacity = newCapacity;
        copyBuffer = new char[newCapacity];
        items = newItems;
        memoryItems = newMemoryItems;
    }
}