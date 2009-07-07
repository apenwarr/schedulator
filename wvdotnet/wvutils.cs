/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
#define DEBUG
#define TRACE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using Wv.Extensions;

namespace Wv
{
    public partial class wv
    {
	public static void sleep(int msec_delay)
	{
	    if (msec_delay < 0)
		Thread.Sleep(Int32.MaxValue);
	    else
		Thread.Sleep(msec_delay);
	}

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
	
	public static void assert(bool b, object msg)
	{
	    if (!b)
		throw new System.ArgumentException(msg.ToString());
	}
	
	public static void assert(bool b, string fmt, params object[] args)
	{
	    if (!b)
		assert(b, (object)wv.fmt(fmt, args));
	}
	
	public static void assert(bool b)
	{
	    assert(b, "Assertion Failure");
	}
	
	public static void assert()
	{
	    assert(false);
	}
	
	public static DateTime date(object s)
	{
	    try
	    {
		return DateTime.Parse(s.ToString());
	    }
	    catch (FormatException)
	    {
		return DateTime.MinValue;
	    }
	}
	
	public static string httpdate(DateTime d)
	{
	    return d.ToUniversalTime()
		.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT";
	}
	
	static string intclean(object o, bool allow_dot)
	{
	    if (o == null) return "0";
	    
	    // All this nonsense is to make it so that we can parse
	    // strings a little more liberally.  Double.Parse() would
	    // give up in case of invalid characters; we just ignore
	    // anything after that point instead, like C would do.
	    string s = o.ToString();
	    
	    char[] ca = s.ToCharArray();
	    int i = 0;
	    if (ca.Length > 0 && ca[0] == '-')
		i++;
	    bool saw_dot = false;
	    for (; i < ca.Length; i++)
	    {
		if (ca[i] == '.')
		{
		    if (saw_dot || !allow_dot)
			break;
		    saw_dot = true;
		}
		if ("0123456789.".IndexOf(ca[i]) < 0)
		    break;
	    }
	    
	    return s.Substring(0, i);
	}
	
	public static double atod(object o)
	{
	    try {
		return Double.Parse(intclean(o, true));
	    } catch (FormatException) {
		return 0.0;
	    }
	}
	
	public static int atoi(object o)
	{
	    try {
		return Int32.Parse(intclean(o, false));
	    } catch (FormatException) {
		return 0;
	    }
	}
	
	public static long atol(object o)
	{
	    try {
		return Int64.Parse(intclean(o, false));
	    } catch (FormatException) {
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
	
	public static void print(string format, params object[] args)
	{
	    Console.Write(format, args);
	}
	
	public static void printerr(string format, params object[] args)
	{
	    Console.Error.Write(format, args);
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
	
	public static string getenv(string key)
	{
	    string o = Environment.GetEnvironmentVariable(key);
	    if (o != null)
		return o;
	    else
		return "";
	}

    	public static string urldecode(string s)
	{
	    return HttpUtility.UrlDecode(s);
	}
	
	public static string urlencode(string s)
	{
	    return HttpUtility.UrlEncode(s);
	}
	
	public static void urlsplit(Dictionary<string,string> d, string query)
	{
	    // Multiple values separated by & signs, as in URLs
	    foreach (string ent in query.Split('&'))
	    {
		string[] kv = ent.Split("=".ToCharArray(), 2);
		string k = HttpUtility.UrlDecode(kv[0]);
		string v = 
		    kv.Length>1 ? HttpUtility.UrlDecode(kv[1]) : "1";
		d.Remove(k);
		d.Add(k, v);
	    }
	}
	
	public static void cookiesplit(Dictionary<string,string> d,
				       string query)
	{
	    // Multiple values separated by & signs, as in URLs
	    foreach (string ent in query.Split(';'))
	    {
		string[] kv = ent.Split("=".ToCharArray(), 2);
		string k = wv.urldecode(kv[0].Trim());
		string v = kv.Length>1 ? wv.urldecode(kv[1]) : "1";
		d.Remove(k);
		d.Add(k, v);
	    }
	}
	
	static RandomNumberGenerator randserv = null;
	public static byte[] randombytes(int num)
	{
	    // lazy initialization, since it might be expensive.  But we'll
	    // keep it around to ensure "maximum randomness"
	    if (randserv == null)
		randserv = new RNGCryptoServiceProvider();
	    
	    byte[] b = new byte[num];
	    randserv.GetBytes(b);
	    return b;
	}

	/**
	 * A handy tool for making timeout loops.  Use it like this:
	 * 
	 * foreach (int remain in wv.until(t)) {
	 *     do_stuff();
	 *     if (exit_condition) break;
	 * }
	 * 
	 * New iterations of the loop will continue until DateTime.Now==t,
	 * or you exit the loop by hand.
	 * 
	 * If t==DateTime.MinValue, loop will continue forever.
	 * 
	 * 'remain' at each iteration will be the number of milliseconds
	 * remaining, or -1 if t==DateTime.MinValue.
	 * 
	 * Even if the timeout is in the past, this is guaranteed to return
	 * at least once.
	 */
	public static IEnumerable<int> until(DateTime t)
	{
	    bool forever = (t == DateTime.MinValue);
	    DateTime n = DateTime.Now;
	    bool once = false;
	    
	    while (!once || t==DateTime.MinValue || n < t)
	    {
		once = true;
		if (forever)
		    yield return -1;
		else
		{
		    int s = (int)((t-n).TotalMilliseconds);
		    if (s < 0)
			s = 0;
		    yield return s;
		}
		n = DateTime.Now;
	    }
	}
	
	/**
	 * Like until(DateTime t), but works with a timeout in msec instead
	 * of an exact time.
	 * 
	 * A negative timeout means "forever".
	 */
	public static IEnumerable<int> until(int msec_timeout)
	{
	    DateTime t;
	    if (msec_timeout < 0)
		t = DateTime.MinValue;
	    else
		t = DateTime.Now + TimeSpan.FromMilliseconds(msec_timeout);
	    foreach (var i in until(t))
		yield return i;
	}
	
        public static string add_breaks_to_newlines(string orig)
        {
            StringBuilder retval = new StringBuilder();
            // Add a bit of space, since we expect to get a few newlines.
            retval.EnsureCapacity(orig.Length + 32);
            string[] split = orig.Split('\n');
            for (int i = 0; i < split.Length; i++)
            {
                string s = split[i];
                retval.Append(s);
                // Don't do anything to the very end of the string
                if (i != split.Length - 1)
                {
                    string trimmed = s.Trim();
                    if (!trimmed.EndsWith("<br>") && !trimmed.EndsWith("<br/>")
                        && !trimmed.EndsWith("<br />"))
                    {
                        retval.Append("<br/>\n");
                    }
                    else
                        retval.Append("\n");
                }
            }
            return retval.ToString();
        }

        /// Extend Path.Combine to work on more than two path elements.
        public static string PathCombine(string first, params string[] rest)
        {
            string combined = first;
            foreach (string elem in rest)
                combined = Path.Combine(combined, elem);
            return combined;
        }
	
	/// An alias for PathCombine that follows the Split/Join convention
        public static string PathJoin(string first, params string[] rest)
        {
	    return PathCombine(first, rest);
        }
	
	public static string[] PathSplit(string path)
	{
	    return path.split(new char[] {
		Path.DirectorySeparatorChar,
		Path.AltDirectorySeparatorChar,
		'/'
	    });
	}

	public static bool IsMono()
	{
	    return Type.GetType("Mono.Runtime") != null;
	}
    }
}

namespace Wv.Obsolete
{
    public class Log
    {
	protected string logname;
	
	static int refs = 0;
	static TraceListener mytrace = null;
	static bool disable_mytrace = false;
	
	public Log(string logname)
	{
	    refs++;
	    if (mytrace == null && !disable_mytrace 
		&& Trace.Listeners.Count < 2)
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
	    {
		if (mytrace != null)
		    Trace.Listeners.Remove(mytrace);
		mytrace = null;
	    }
	}
	
	public static void no_default_listener()
	{
	    disable_mytrace = true;
	    if (mytrace != null)
	    {
		Trace.Listeners.Remove(mytrace);
		mytrace = null;
	    }
	}
	
	public virtual void log(string format, params object [] arg)
	{
	    // Console.Error.WriteLine("<" + logname + "> " + format, arg);
	    Trace.WriteLine(String.Format
			    ("[" + logname + "] " + format, arg));
	    Trace.Flush();
	}
	
	public void log(string s)
	{
	    log("{0}", s);
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

