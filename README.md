# C# 1.2 Add‑On Projects (VS .NET 2003 era)

C# **1.2** introduced a key runtime‑generated code change: **`foreach` now calls `Dispose()`** on the enumerator **if** the enumerator implements `IDisposable`. These projects demonstrate that behavior in different scenarios, while staying within C# 1.x syntax (no generics, no lambdas, etc.).

## Projects
- **P25_ForeachDispose_DisposableEnumerator** — Custom `IEnumerator : IDisposable`; show `foreach` auto‑calls `Dispose`, while a manual `while` loop must do it in `finally`.
- **P26_ForeachDispose_NonDisposableEnumerator** — Enumerator without `IDisposable`; `foreach` **does not** call `Dispose`.
- **P27_ForeachDispose_StructEnumerator** — **Struct** enumerator implementing `IDisposable`; demonstrates that `foreach` still calls `Dispose` (note the call happens on a boxed copy when cast to interfaces).
- **P28_ForeachDispose_StreamReader** — Enumerator that reads file lines and implements `IDisposable`; after `foreach`, file can be deleted because the reader is closed via `Dispose`.

## Build
```bash
dotnet restore
dotnet build
dotnet run --project P17_ForeachDispose_DisposableEnumerator
```

The solution targets `.NET 10.0` and enforces **C# 1.0** syntax globally (via `Directory.Build.props` with `<LangVersion>ISO-1</LangVersion>`). The only 1.2‑specific behavior here is the compiler‑generated `try/finally` in `foreach` that disposes the enumerator when applicable.

# C# 1.2 `foreach` and `Dispose()` - Core Samples (P25–P28)

In **C# 1.2**, the compiler gained a key guarantee:  
If the enumerator returned by `GetEnumerator()` implements `IDisposable`, the compiler automatically generates a hidden `try/finally` around the loop and calls `Dispose()` when the loop ends.  

That means that database readers, sockets, file streams, and other resource-holding enumerators are **automatically cleaned up** at the end of `foreach` — even on `break`, `return`, or exceptions.  

Here’s the compiler-equivalent expansion:

```csharp
var e = source.GetEnumerator();
try
{
    while (e.MoveNext())
    {
        var item = e.Current;
        // loop body
    }
}
finally
{
    (e as IDisposable)?.Dispose();
}
```

The following four samples (P17–P20) illustrate different cases of enumerator disposal.

---

## P25_ForeachDispose_DisposableEnumerator  
**Custom `IEnumerator : IDisposable`; show foreach auto-calls Dispose, while a manual while loop must do it in finally.**

```csharp
class DisposableEnumerator : IEnumerator<int>, IDisposable
{
    private int i = -1;
    public int Current => i;
    object IEnumerator.Current => Current;

    public bool MoveNext() => ++i < 3;
    public void Reset() => i = -1;

    public void Dispose()
    {
        Console.WriteLine("DisposableEnumerator disposed");
    }
}

class MyCollection : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() => new DisposableEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

static void Main()
{
    Console.WriteLine("foreach:");
    foreach (var x in new MyCollection())
        Console.WriteLine(x);

    Console.WriteLine("manual while:");
    var e = new MyCollection().GetEnumerator();
    try
    {
        while (e.MoveNext())
            Console.WriteLine(e.Current);
    }
    finally
    {
        (e as IDisposable)?.Dispose(); // must be explicit here
    }
}
```

**Takeaway:** `foreach` does the `Dispose()` for you; manual enumeration requires your own `try/finally`.

---

## P26_ForeachDispose_NonDisposableEnumerator  
**Enumerator without `IDisposable`; foreach does not call Dispose.**

```csharp
struct NonDisposableEnumerator : IEnumerator<int>
{
    private int i;
    public int Current => i;
    object IEnumerator.Current => Current;

    public bool MoveNext() => ++i < 3;
    public void Reset() => i = 0;
}

class NonDisposableCollection : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() => new NonDisposableEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

static void Main()
{
    foreach (var x in new NonDisposableCollection())
        Console.WriteLine(x);

    // No Dispose() is ever invoked, because enumerator doesn't implement IDisposable
}
```

**Takeaway:** If `IDisposable` isn’t present, `foreach` won’t generate a call to `Dispose()`.

---

## P27_ForeachDispose_StructEnumerator  
**Struct enumerator implementing `IDisposable`; demonstrates that foreach still calls Dispose (note the call happens on a boxed copy when cast to interfaces).**

```csharp
struct StructEnumerator : IEnumerator<int>, IDisposable
{
    private int i;
    public int Current => i;
    object IEnumerator.Current => Current;

    public bool MoveNext() => ++i < 3;
    public void Reset() => i = 0;

    public void Dispose()
    {
        Console.WriteLine("StructEnumerator disposed");
    }
}

class StructEnumerable : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() => new StructEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

static void Main()
{
    foreach (var x in new StructEnumerable())
        Console.WriteLine(x);
}
```

**Takeaway:** Even struct enumerators get `Dispose()` calls, though under the hood the struct may be boxed when accessed via `IEnumerator`.

---

## P28_ForeachDispose_StreamReader  
**Enumerator that reads file lines and implements `IDisposable`; after foreach, file can be deleted because the reader is closed via Dispose.**

```csharp
class FileLines : IEnumerable<string>
{
    private readonly string path;
    public FileLines(string path) => this.path = path;

    public IEnumerator<string> GetEnumerator()
    {
        using var reader = new StreamReader(path);
        string? line;
        while ((line = reader.ReadLine()) != null)
            yield return line;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

static void Main()
{
    var file = "test.txt";
    File.WriteAllLines(file, new[] { "alpha", "beta", "gamma" });

    foreach (var line in new FileLines(file))
        Console.WriteLine(line);

    File.Delete(file); // works, reader was disposed after foreach
}
```

**Takeaway:** `foreach` disposal ensures the `StreamReader` is closed when iteration ends, so the file can be safely deleted.

---

# Why this matters in practice
- `foreach` **always disposes** enumerators that implement `IDisposable` — normal completion, early exit, or exception.  
- If you build custom enumerators for resource management, **always implement `IDisposable`**.  
- If your enumerator doesn’t implement it, `foreach` won’t dispose — safe for pure in-memory iteration, unsafe for resource handles.  
- Applies equally to class and struct enumerators.  
- Yield-based iterators also generate disposable enumerators internally.

These four samples (P25–P28) form the **core demonstrations** of the C# 1.2 foreach-dispose guarantee.
