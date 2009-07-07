/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Wv.Extensions
{
    public static class StreamHelper
    {
	public static void write(this Stream s, WvBytes buffer)
	{
	    s.Write(buffer.bytes, buffer.start, buffer.len);
	}
    }
    
    public static class ExceptionHelper
    {
	public static string Short(this Exception e)
	{
	    if (e == null)
		return "Success";
	    else
		return e.Message;
	}
    }
    
    public static class WvContExtensions
    {
	public static Action ToAction(this IEnumerator ie)
	{
	    return new Action(delegate() {
		ie.MoveNext();
	    });
	}

	public static Action ToAction(this IEnumerable aie)
	{
	    bool must_reset = false;
	    IEnumerator ie = aie.GetEnumerator();
	    return new Action(delegate() {
		if (must_reset)
		    ie = aie.GetEnumerator();
		must_reset = !ie.MoveNext();
	    });
	}
    }

    public static class WvStreamExtensions
    {
	public static byte[] ToUTF8(this Object o)
	{
	    return Encoding.UTF8.GetBytes(o.ToString());
	}

	public static string FromUTF8(this WvBytes b)
	{
	    return Encoding.UTF8.GetString(b.bytes, b.start, b.len);
	}
	
	public static string ToHex(this WvBytes bytes)
	{
	    StringBuilder sb = new StringBuilder();
	    foreach (byte b in bytes)
		sb.Append(b.ToString("X2"));
	    return sb.ToString();
	}
    }
    
    public static class DictExtensions
    {
	public static string getstr<T1,T2>(this Dictionary<T1,T2> dict,
					   T1 key)
	{
	    if (dict.ContainsKey(key))
		return dict[key].ToString();
	    else
		return "";
	}
	
	public static bool has<T1,T2>(this Dictionary<T1,T2> dict,
					   T1 key)
	{
	    return dict.ContainsKey(key);
	}
    }
    
    public static class DataExtensions
    {
	// true if a string is (e)mpty (null or blank)
	public static bool e(this string s)
	{
	    return wv.isempty(s);
	}
	
	// true if a string is (n)on(e)mpty (nonnull and nonblank)
	public static bool ne(this string s)
	{
	    return !wv.isempty(s);
	}
	
	public static string trim(this string s)
	{
	    if (s != null)
		return s.Trim();
	    else
		return null;
	}
	
	public static IEnumerable<T2> map<T1,T2>(this IEnumerable<T1> list,
					  Func<T1,T2> f)
	{
	    foreach (T1 t in list)
		yield return f(t);
	}
	
	public static string[] ToStringArray<T>(this IEnumerable<T> l)
	{
	    List<string> tmp = new List<string>();
	    foreach (T t in l)
		tmp.Add(t.ToString());
	    return tmp.ToArray();
	}
	
	public static T only<T>(this IEnumerable<T> l)
	    where T: class
	{
	    foreach (T t in l)
		return t;
	    return (T)null;
	}

        public static string join<T>(this IEnumerable<T> list, string sep)
        {
            return String.Join(sep, list.ToStringArray());
        }

        public static string join<T>(this IEnumerable<string> list, string sep)
        {
            return String.Join(sep, list.ToArray());
        }
	
        // Note: it would be nice to take "params string[] splitwords" here,
        // but Mono 1.2 apparently has a bug where that won't get picked up
        // properly.
	public static string[] split(this string s, string splitword)
	{
	    return s.Split(new string[] {splitword}, StringSplitOptions.None);
	}
	
	public static string[] split(this string s, string splitword, int max)
	{
	    return s.Split(new string[] {splitword}, max,
			   StringSplitOptions.None);
	}
	
	public static string[] split(this string s, char[] splitchars)
	{
	    return s.Split(splitchars, StringSplitOptions.None);
	}
	
	public static string[] split(this string s, char[] splitchars, int max)
	{
	    return s.Split(splitchars, max, StringSplitOptions.None);
	}
	
	public static string shorten(this string s, int maxlen)
	{
	    if (s.Length <= maxlen)
		return s;
	    else
		return s.Substring(0, maxlen) + "...";
	}

	public static int atoi(this object o)
	{
	    return wv.atoi(o);
	}

	public static long atol(this object o)
	{
	    return wv.atol(o);
	}

	public static double atod(this object o)
	{
	    return wv.atod(o);
	}
	
	public static T pop<T>(this IEnumerator<T> list)
	{
	    if (list.MoveNext())
		return list.Current;
	    else
		return default(T);
	}
	
	// pray that you never need to use this.
	public static WvAutoCast autocast(this object o)
	{
	    if (o is WvAutoCast)
		return (WvAutoCast)o;
	    else
		return new WvAutoCast(o);
	}
	
	public static V tryget<K,V>(this IDictionary<K,V> dict, K key)
	{
	    return dict.tryget(key, default(V));
	}
	
	public static V tryget<K,V>(this IDictionary<K,V> dict, K key,
				    V defval)
	{
	    V v;
	    if (dict.TryGetValue(key, out v))
		return v;
	    else
		return defval;
	}
	
	// This works if b is a byte[], too, because of the implicit
	// cast.
	public static WvBytes sub(this WvBytes b, int start, int len)
	{
	    return b.sub(start, len);
	}
	
	public static void put(this WvBytes dest, int offset, WvBytes src)
	{
	    dest.put(offset, src);
	}
	
	// usage: val1.or(val2)
	// Returns val2 if val1 is default(T).  So you could write:
	//    int port = get_port().or(80);
        public static T or<T>(this T val1, T val2) where T: IComparable<T>
	{
	    if (val1.Equals(default(T)))
		return val2;
	    else
		return val1;
	}
	
	public static string or(this string val1, string val2)
	{
	    if (val1.e())
		return val2;
	    else
		return val1;
	}
	
	static int acto<TI,T>(this IEnumerator<TI> e, out T o)
	{
	    if (e.MoveNext())
	    {
		o = (T)e.Current.autocast().to(typeof(T));
		return 1;
	    }
	    return 0;
	}
	
	public static int assignto<T,T1>
	    (this IEnumerable<T> ie,
	     out T1 a)
	{
	    using (var en = ie.GetEnumerator())
		return en.acto(out a);
	}
	
	public static int assignto<T,T1,T2>
	    (this IEnumerable<T> ie,
	     out T1 a, out T2 b)
	{
	    using (var en = ie.GetEnumerator())
		return en.acto(out a)
		     + en.acto(out b);
	}
	
	public static int assignto<T,T1,T2,T3>
	    (this IEnumerable<T> ie,
	     out T1 a, out T2 b, out T3 c)
	{
	    using (var en = ie.GetEnumerator())
		return en.acto(out a)
		     + en.acto(out b)
		     + en.acto(out c);
	}
	
	public static int assignto<T,T1,T2,T3,T4>
	    (this IEnumerable<T> ie,
	     out T1 a, out T2 b, out T3 c, out T4 d)
	{
	    using (var en = ie.GetEnumerator())
		return en.acto(out a)
		     + en.acto(out b)
		     + en.acto(out c)
		     + en.acto(out d);
	}
	
	public static int assignto<T,T1,T2,T3,T4,T5>
	    (this IEnumerable<T> ie,
	     out T1 a, out T2 b, out T3 c, out T4 d, out T5 e)
	{
	    using (var en = ie.GetEnumerator())
		return en.acto(out a)
		     + en.acto(out b)
		     + en.acto(out c)
		     + en.acto(out d)
		     + en.acto(out e);
	}
	
	public static int assignto<T,T1,T2,T3,T4,T5,T6>
	    (this IEnumerable<T> ie,
	     out T1 a, out T2 b, out T3 c, out T4 d,
	     out T5 e, out T6 f)
	{
	    using (var en = ie.GetEnumerator())
		return en.acto(out a)
		     + en.acto(out b)
		     + en.acto(out c)
		     + en.acto(out d)
		     + en.acto(out e)
		     + en.acto(out f);
	}
	
	public static int assignto<T,T1,T2,T3,T4,T5,T6,T7>
	    (this IEnumerable<T> ie,
	     out T1 a, out T2 b, out T3 c, out T4 d,
	     out T5 e, out T6 f, out T7 g)
	{
	    using (var en = ie.GetEnumerator())
		return en.acto(out a)
		     + en.acto(out b)
		     + en.acto(out c)
		     + en.acto(out d)
		     + en.acto(out e)
		     + en.acto(out f)
		     + en.acto(out g);
	}
	
	public static int assignto<T,T1,T2,T3,T4,T5,T6,T7,T8>
	    (this IEnumerable<T> ie,
	     out T1 a, out T2 b, out T3 c, out T4 d,
	     out T5 e, out T6 f, out T7 g, out T8 h)
	{
	    using (var en = ie.GetEnumerator())
		return en.acto(out a)
		     + en.acto(out b)
		     + en.acto(out c)
		     + en.acto(out d)
		     + en.acto(out e)
		     + en.acto(out f)
		     + en.acto(out g)
		     + en.acto(out h);
	}
    }
}
