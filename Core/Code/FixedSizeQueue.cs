using System.Collections.Concurrent;

namespace Core.Code;

public class FixedSizeQueue<T> : ConcurrentQueue<T>
{
    public int Size { get; private init; }

    public FixedSizeQueue(int size)
    {
        Size = size;
    }

    public FixedSizeQueue(int size, IEnumerable<T> items) : base(items) 
    { 
        Size = size;
    }

    public new void Enqueue(T obj)
    {
        base.Enqueue(obj);
        while (Count > Size)
        {
            TryDequeue(out _);
        }
    }
}