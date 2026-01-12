using System;
using System.Collections;
using System.IO;

class LineReader : IEnumerable
{
    private string _path;
    public LineReader(string path) { _path = path; }

    public IEnumerator GetEnumerator() { return new LineEnumerator(_path); }

    private class LineEnumerator : IEnumerator, IDisposable
    {
        private StreamReader _reader;
        private string _current;

        public LineEnumerator(string path)
        {
            _reader = new StreamReader(path);
        }

        public object Current { get { return _current; } }

        public bool MoveNext()
        {
            _current = _reader.ReadLine();
            return _current != null;
        }

        public void Reset()
        {
            throw new NotSupportedException("Reset not supported in this simple demo.");
        }

        public void Dispose()
        {
            Console.WriteLine("Closing StreamReader via foreach Dispose...");
            if (_reader != null) { _reader.Close(); _reader = null; }
        }
    }
}

class Program
{
    static void Main()
    {
        string path = "demo_lines.txt";
        // Create a file
        StreamWriter w = null;
        try
        {
            w = new StreamWriter(path);
            w.WriteLine("alpha");
            w.WriteLine("beta");
            w.WriteLine("gamma");
        }
        finally
        {
            if (w != null) w.Close();
        }

        Console.WriteLine("Reading lines with foreach (will auto-close):");
        foreach (string line in new LineReader(path))
        {
            Console.WriteLine("  " + line);
        }

        // Verify the file is not locked anymore
        try
        {
            File.Delete(path);
            Console.WriteLine("File successfully deleted after foreach (disposed).");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not delete file (likely not disposed): " + ex.Message);
        }
    }
}
