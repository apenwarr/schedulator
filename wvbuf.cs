/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wv.Extensions;

namespace Wv
{
    /**
     * A simple structure that allows us to wrap *part* of a byte[] array.
     * This is intended to be an efficient alternative to constantly
     * reallocating and copying arrays just because we want to talk about
     * subsets of them.
     * 
     * In C/C++, manipulations on subsets of byte arrays are common using
     * pointer arithmetic.  We don't want to use pointers because it makes
     * our code unsafe, but there's no need to be copying entire arrays
     * around just because of that!
     */
    public struct WvBytes : IEnumerable<byte>
    {
	public byte[] bytes;
	public int start, len;
	
	/**
	 * Wrap a particular section of the given bytes[] array.
	 */
	public WvBytes(byte[] bytes, int start, int len)
	{
	    this.bytes = bytes;
	    this.start = start;
	    this.len = len;
	    wv.assert(start >= 0);
	    wv.assert(len >= 0);
	    wv.assert(bytes==null || start+len <= bytes.Length);
	}
	
	/**
	 * Any byte array can produce a WvBytes object with no effort.
	 */
	public static implicit operator WvBytes(byte[] b)
	{
	    return new WvBytes(b, 0, b != null ? b.Length : 0);
	}
	
	/**
	 * Any WvBytes object is enough to produce a byte array, but it
	 * might require an array copy.  So we make it explicit to avoid
	 * doing it by accident.
	 */
	public static explicit operator byte[](WvBytes b)
	{
	    return b.ToArray();
	}
	
	/**
	 * Convert this byte array to a simple byte[] structure.  This
	 * *might* imply making a copy, but might not, so don't modify the
	 * resulting array.  Of course, we can't stop you from modifying
	 * the array, because there's no const, so we'll have to trust you.
	 */
	public byte[] ToArray()
	{
	    if (start == 0 && len == bytes.Length)
		return bytes;
	    else
	    {
		byte[] b = new byte[len];
		Array.Copy(bytes, start, b, 0, len);
		return b;
	    }
	}
	
	public bool IsNull {
	    get { return bytes == null; }
	}
	
	/// parallels WvBuf.getall()
	public byte[] getall()
	{
	    return ToArray();
	}
	
	/// get a subset of this set of bytes, without copying any actual data
	public WvBytes sub(int start, int len)
	{
	    wv.assert(start >= 0);
	    wv.assert(len >= 0);
	    wv.assert(start+len <= this.len);
	    if (start == this.start && len == this.len)
		return this;
	    int s = this.start + start;
	    wv.assert(s+len <= bytes.Length);
	    
	    return new WvBytes(bytes, s, len);
	}
	
	public byte this[int i] {
	    get	{
		wv.assert(i < len);
		return bytes[start+i];
	    }
	}
	
	public void put(int offset, WvBytes b)
	{
	    wv.assert(offset >= 0);
	    wv.assert(offset+b.len <= len);
	    Array.Copy(b.bytes, b.start, this.bytes, start+offset, b.len);
	}
	
	// IEnumerable<byte>
	public IEnumerator<byte> GetEnumerator()
	{
	    int end = start+len;
	    for (int i = start; i < end; i++)
		yield return bytes[i];
	}
	
	// IEnumerable
	IEnumerator IEnumerable.GetEnumerator()
	{
	    int end = start+len;
	    for (int i = start; i < end; i++)
		yield return bytes[i];
	}
    }
    
    public class WvMiniBuf
    {
	byte[] bytes;
	int first, next;

	public WvMiniBuf(int size)
	{
	    bytes = new byte[size];
	    first = 0;
	    next = 0;
	}

	public int size { get { return (int)bytes.Length; } }

	public int used { get { return next-first; } }

	public int avail { get { return (int)bytes.Length-next; } }
	
	public WvBytes alloc(int size)
	{
	    wv.assert(size <= avail);
	    var ret = bytes.sub(next, size);
	    next += size;
	    return ret;
	}
	
	public void unalloc(int size)
	{
	    wv.assert(size <= used);
	    next -= size;
	}
	
	public void put(WvBytes b)
	{
	    wv.assert(b.len <= avail);
	    alloc(b.len).put(0, b);
	}

	public WvBytes peek(int len)
	{
	    return bytes.sub(first, len);
	}

	public WvBytes get(int len)
	{
	    var ret = peek(len);
	    first += len;
	    return ret;
	}

	public void unget(int len)
	{
	    wv.assert(first >= len);
	    first -= len;
	}

	// Returns the number of bytes that would have to be read in order to
	// get the first instance of 'b', or 0 if 'b' is not in the buffer.
	public int strchr(byte b)
	{
	    for (int i = first; i < next; i++)
		if (bytes[i] == b)
		    return i-first+1;
	    return 0;
	}
	
	public void zap()
	{
	    first = next = 0;
	}
    }

    public class WvBuf
    {
	List<WvMiniBuf> list = new List<WvMiniBuf>();
	int startsize = 10;

	public WvBuf(int startsize)
	{
	    this.startsize = startsize;
	    zap();
	}
	
	public WvBuf()
	    : this(10)
	{
	}

	public int used {
	    get {
		return (int)list.Select(b => (long)b.used).Sum();
	    }
	}

	WvMiniBuf last { get { return list[list.Count-1]; } }

	public void zap()
	{
	    list.Clear();
	    list.Add(new WvMiniBuf(startsize));
	}

	void addbuf(int len)
	{
	    int s = last.size * 2;
	    while (s < len*2)
		s *= 2;
	    list.Add(new WvMiniBuf(s));
	}
	
	public WvBytes alloc(int size)
	{
	    if (last.avail < size)
		addbuf(size);
	    return last.alloc(size);
	}
	
	public void unalloc(int size)
	{
	    wv.assert(last.used >= size);
	    last.unalloc(size);
	}

	public void put(WvBytes b)
	{
	    alloc(b.len).put(0, b);
	}
	
	public void put(char c)
	{
	    put(c.ToUTF8());
	}
	
	public void put(string s)
	{
	    put(s.ToUTF8());
	}
	
	public void put(byte b)
	{
	    // FIXME: this could be much more optimal :)
	    put(new byte[1] { b });
	}
	
	public void put(string fmt, params object[] args)
	{
	    put(String.Format(fmt, args));
	}
	
	public void eat(WvBuf buf)
	{
	    list.AddRange(buf.list);
	    buf.zap();
	}

	int min(int a, int b)
	{
	    return (a < b) ? a : b;
	}

	void coagulate(int len)
	{
	    if (list[0].used < len)
	    {
		WvMiniBuf n = new WvMiniBuf(len);
		while (len > 0)
		{
		    int got = min(len, list[0].used);
		    n.put(list[0].get(got));
		    len -= got;
		    if (list[0].used == 0)
			list.Remove(list[0]);
		}
		list.Insert(0, n);
	    }
	}

	public WvBytes peek(int len)
	{
	    wv.assert(used >= len);
	    coagulate(len);
	    return list[0].peek(len);
	}
	
	public WvBytes peekall()
	{
	    return peek(used);
	}

	public WvBytes get(int len)
	{
	    wv.assert(used >= len);
	    coagulate(len);
	    return list[0].get(len);
	}

	public WvBytes getall()
	{
	    return get(used);
	}
	
	public string getstr()
	{
	    return getall().FromUTF8();
	}

	public void unget(int len)
	{
	    list[0].unget(len);
	}

	// Returns the number of bytes that would have to be read in order to
	// get the first instance of 'b', or 0 if 'b' is not in the buffer.
	public int strchr(byte b)
	{
	    int i = 0;
	    foreach (WvMiniBuf mb in list)
	    {
		int r = mb.strchr(b);
		if (r > 0)
		    return i + r;
		else
		    i += mb.used;
	    }
	    return 0;
	}

	public int strchr(char b)
	{
	    return strchr(Convert.ToByte(b));
	}
    }
}
