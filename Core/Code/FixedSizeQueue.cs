using System.Collections.Concurrent;

namespace Core.Code;

public class FixedSizeQueue<T> : ConcurrentQueue<T>
{
    public int Size { get; private set; }

    public FixedSizeQueue(int size)
    {
        Size = size;
    }

    public new void Enqueue(T obj)
    {
        base.Enqueue(obj);
        while (base.Count > Size)
        {
            TryDequeue(out _);
        }
    }
}