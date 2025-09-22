using System;
using System.Collections;

class MyNumbers : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        return new MyEnumerator(new int[] { 1, 2, 3 });
    }

    private class MyEnumerator : IEnumerator, IDisposable
    {
        private int[] _data;
        private int _index = -1;
        private bool _disposed;

        public MyEnumerator(int[] data) { _data = data; }

        public object Current
        {
            get { return _data[_index]; }
        }

        public bool MoveNext()
        {
            if (_disposed) throw new ObjectDisposedException("MyEnumerator");
            _index++;
            return _index < _data.Length;
        }

        public void Reset()
        {
            if (_disposed) throw new ObjectDisposedException("MyEnumerator");
            _index = -1;
        }

        public void Dispose()
        {
            _disposed = true;
            Console.WriteLine("MyEnumerator.Dispose() called by foreach");
        }
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Foreach over IDisposable enumerator (C# 1.2 calls Dispose):");
        foreach (int n in new MyNumbers())
        {
            Console.WriteLine("  " + n);
        }

        Console.WriteLine();
        Console.WriteLine("Manual while loop (must call Dispose yourself):");
        IEnumerable seq = new MyNumbers();
        IEnumerator e = seq.GetEnumerator();
        try
        {
            while (e.MoveNext())
            {
                Console.WriteLine("  " + (int)e.Current);
            }
        }
        finally
        {
            IDisposable d = e as IDisposable;
            if (d != null) d.Dispose();
        }
    }
}
