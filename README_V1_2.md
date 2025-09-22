# C# 1.2 Add‑On Projects (VS .NET 2003 era)

C# **1.2** introduced a key runtime‑generated code change: **`foreach` now calls `Dispose()`** on the enumerator **if** the enumerator implements `IDisposable`. These projects demonstrate that behavior in different scenarios, while staying within C# 1.x syntax (no generics, no lambdas, etc.).

## Projects
- **P17_ForeachDispose_DisposableEnumerator** — Custom `IEnumerator : IDisposable`; show `foreach` auto‑calls `Dispose`, while a manual `while` loop must do it in `finally`.
- **P18_ForeachDispose_NonDisposableEnumerator** — Enumerator without `IDisposable`; `foreach` **does not** call `Dispose`.
- **P19_ForeachDispose_StructEnumerator** — **Struct** enumerator implementing `IDisposable`; demonstrates that `foreach` still calls `Dispose` (note the call happens on a boxed copy when cast to interfaces).
- **P20_ForeachDispose_StreamReader** — Enumerator that reads file lines and implements `IDisposable`; after `foreach`, file can be deleted because the reader is closed via `Dispose`.

## Build
```bash
dotnet restore
dotnet build
dotnet run --project P17_ForeachDispose_DisposableEnumerator
```

The solution targets `.NET 10.0` and enforces **C# 1.0** syntax globally (via `Directory.Build.props` with `<LangVersion>ISO-1</LangVersion>`). The only 1.2‑specific behavior here is the compiler‑generated `try/finally` in `foreach` that disposes the enumerator when applicable.
