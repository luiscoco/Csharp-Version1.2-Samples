using System;
using System.Collections;

class Simple : IEnumerable
{
    public IEnumerator GetEnumerator() { return new E(); }

    private class E : IEnumerator // NOTE: no IDisposable here
    {
        private int i = 0;
        public object Current { get { return i; } }
        public bool MoveNext() { i++; return i <= 3; }
        public void Reset() { i = 0; }
        // If Dispose were here, C# 1.2 would call it automatically.
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Foreach over enumerator WITHOUT IDisposable:");
        foreach (int n in new Simple())
        {
            Console.WriteLine("  " + n);
        }
        Console.WriteLine("No Dispose() message expected.");
    }
}
