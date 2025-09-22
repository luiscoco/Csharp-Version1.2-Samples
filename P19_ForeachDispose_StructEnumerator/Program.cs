using System;
using System.Collections;

struct StructEnum : IEnumerator, IDisposable
{
    public static int DisposeCalls;
    private int i;
    public object Current { get { return i; } }
    public bool MoveNext() { i++; return i <= 2; }
    public void Reset() { i = 0; }
    public void Dispose() { DisposeCalls++; Console.WriteLine("StructEnum.Dispose() called"); }
}

struct StructEnumerable : IEnumerable
{
    public IEnumerator GetEnumerator() { StructEnum e = new StructEnum(); return e; }
}

class Program
{
    static void Main()
    {
        StructEnum.DisposeCalls = 0;
        Console.WriteLine("Foreach over struct enumerator implementing IDisposable:");
        foreach (int n in new StructEnumerable())
        {
            Console.WriteLine("  " + n);
        }
        Console.WriteLine("Dispose calls = " + StructEnum.DisposeCalls);
    }
}
