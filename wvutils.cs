#define DEBUG
#define TRACE

using System;
using System.Collections;
using System.Diagnostics;

namespace Wv.Utils
{
    public class wv
    {
	public static string shift(ref string[] array, int index)
	{
	    string s = array[index];
	    string[] outa = new string[array.Length-1];
	    Array.Copy(array, 0, outa, 0, index);
	    Array.Copy(array, index+1, outa, index, array.Length-index-1);
	    array = outa;
	    return s;
	}
	
	public static string shift(ref string[] array)
	{
	    return shift(ref array, 0);
	}
	
	public static void assert(bool b)
	{
	    if (!b)
		throw new System.ArgumentException("assertion failure");
	}
	
	public static void assert()
	{
	    assert(false);
	}
	
	public static DateTime date(string s)
	{
	    try
	    {
		return DateTime.Parse(s);
	    }
	    catch (FormatException)
	    {
		return DateTime.MinValue;
	    }
	}
	
	public static double atod(string s)
	{
	    try
	    {
		return Double.Parse(s);
	    }
	    catch (FormatException)
	    {
		return 0.0;
	    }
	}
	
	public static int atoi(string s)
	{
	    try
	    {
		return Int32.Parse(s);
	    }
	    catch (FormatException)
	    {
		return 0;
	    }
	}
	
	public static long atol(string s)
	{
	    try
	    {
		return Int64.Parse(s);
	    }
	    catch (FormatException)
	    {
		return 0;
	    }
	}
	
	public static bool isempty(string s)
	{
	    return s == null || s == "";
	}
	
	public static bool isempty(DateTime d)
	{
	    return d == DateTime.MinValue;
	}
	
	public static bool isempty(TimeSpan t)
	{
	    return t == TimeSpan.Zero;
	}
	
	public static string fmt(string format, params object[] args)
	{
	    return String.Format(format, args);
	}
	
	public static Array sort(ICollection keys, IComparer comparer)
	{
	    object[] sorted = new object[keys.Count];
	    keys.CopyTo(sorted, 0);
	    Array.Sort(sorted, comparer);
	    return sorted;
	}
	
	public static Array sort(ICollection keys)
	{
	    return sort(keys, Comparer.Default);
	}
	
	public static string[] stringify(ICollection keys)
	{
	    string[] a = new string[keys.Count];
	    int i = 0;
	    foreach (object o in keys)
		a[i++] = o.ToString();
	    return a;
	}
    }
    
    public class Log
    {
	protected string logname;
	
	static int refs = 0;
	static TraceListener mytrace = null;
	
	public Log(string logname)
	{
	    refs++;
	    if (mytrace == null && Trace.Listeners.Count < 2)
	    {
		mytrace = new TextWriterTraceListener(Console.Error);
		Trace.Listeners.Add(mytrace);
	    }
	    
	    //Trace.WriteLine(String.Format("Log constructor for '{0}'.",
	    //   logname));
	    Trace.Flush();
	    this.logname = logname;
	}
	
	~Log()
	{
	    refs--;
	    if (refs == 0)
		mytrace = null;
	}
	
	public virtual void log(string format, params object [] arg)
	{
	    // Console.Error.WriteLine("<" + logname + "> " + format, arg);
	    Trace.WriteLine(String.Format("> " + format, arg));
	    Trace.Flush();
	}
    }

    public class SilentLog : Log
    {
        public SilentLog(string logname) : base(logname)
	{
	}
	
	public override void log(string format, params object [] arg)
	{
	}
    }
    
    // This is intended to work sort of like PHP arrays, which are a mix of
    // hash tables and lists.  Looking up an entry in the hash is O(1), but
    // when you iterate through all the entries, the ordering is well-defined.
    // 
    // So far, this implementation is pretty limited and lacks a bunch of the
    // things PHP can do, such as assigning to a particular array index.
    public class SortedHash : IEnumerable
    {
	ArrayList array = new ArrayList();
	Hashtable hash = new Hashtable();
	
	public IEnumerator GetEnumerator()
	{
	    return array.GetEnumerator();
	}
	
	public void Add(string key, object value)
	{
	    array.Add(value);
	    if (!hash.Contains(key))
		hash.Add(key, value);
	}
	
	public int Count
	{
	    get { return hash.Count; }
	}
	
	public bool Contains(object key)
	{
	    return hash.Contains(key);
	}
	
	public object Find(string key)
	{
	    return hash[key];
	}
	
	public virtual object this[int index]
	{
	    get { return array[index]; }
	}
	
	public virtual void Sort()
	{
	    array.Sort();
	}
	
	public virtual void Sort(IComparer comparer)
	{
	    array.Sort(comparer);
	}
	
	public void Clear()
	{
	    array.Clear();
	    hash.Clear();
	}
	
	public static explicit operator ArrayList(SortedHash x)
	{
	    return ArrayList.ReadOnly(x.array);
	}
    }
    
    public class ObjectCounter : Hashtable
    {
	public new virtual int this [object key]
	{
	    get
	    {
		if (!Contains(key))
		    Add(key, 0);
		return (int)base[key];
	    }
	    
	    set
	    {
		if (Contains(key))
		    Remove(key);
		Add(key, (int)value);
	    }
	}
    }
}

