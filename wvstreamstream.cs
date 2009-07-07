/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.IO;
using System.Threading;

namespace Wv
{
    public class WvStreamStream : WvStream
    {
	System.IO.Stream inner;
	bool hasinner { get { return inner != null; } }
	
	public WvStreamStream(System.IO.Stream inner)
	{
	    this.inner = inner;
	    if (hasinner)
	    {
		if (!inner.CanWrite)
		    nowrite();
		else
		    post_writable();
		if (!inner.CanRead)
		    noread();
		else
		{
		    lock(readlock) start_reading();
		}
	    }
	}
	
	public override bool ok { 
	    get { return hasinner && base.ok; }
	}
	
	object readlock = new object();
	WvBuf inbuf = new WvBuf();
	bool got_eof = false, reading = false;
	Thread reader = null;

	void read_thread()
	{
	    WvBytes buf = new byte[4096];
	    int len = inner.Read(buf.bytes, 0, buf.len);
	    lock (readlock)
	    {
		if (len > 0)
		    inbuf.put(buf.sub(0, len));
		else
		    got_eof = true;
		reading = false;
		post_readable();
	    }
	}
	
	void start_reading()
	{
	    lock (readlock)
	    {
		if (got_eof)
		{
		    //wv.printerr("eof close!\n");
		    noread();
		    nowrite();
		    return;
		}
		
		if (reading)
		    return;
		if (reader != null)
		    reader.Join();
		reading = true;
		reader = new Thread(read_thread);
		reader.Start();
	    }
	}
	
	public override int read(WvBytes b)
	{
	    try
	    {
		lock (readlock)
		{
		    if (inbuf.used > 0)
		    {
			int max = inbuf.used <= b.len ? inbuf.used : b.len;
			b.put(0, inbuf.get(max));
			if (inbuf.used > 0)
			    post_readable(); // _still_ readable
			return max;
		    }
		    else
			return 0;
		}
	    }
	    finally
	    {
		start_reading();
	    }
	}
	
	public override int write(WvBytes b)
	{
	    if (!ok) return 0;
	    try {
		inner.BeginWrite(b.bytes, b.start, b.len,
				 delegate(IAsyncResult ar) {
				     inner.EndWrite(ar);
				     inner.Flush();
				     post_writable();
				 },
				 null);
	    } catch (Exception e) {
		err = e;
		return 0;
	    }
	    
	    return b.len;
	}
	
	public override bool flush(int msec_timeout)
	{
	    // FIXME: how to implement msec_timeout?
	    if (hasinner) inner.Flush();
	    return base.flush(msec_timeout);
	}
	
	public override void close()
	{
	    base.close();
	    if (reader != null)
		reader.Abort();
	    if (hasinner)
		inner.Dispose();
	    inner = null;
	}
	
	static int c = 0;
	public override bool wait(int msec_timeout,
				  bool readable, bool writable)
	{
	    start_reading();
	    foreach (var remain in wv.until(msec_timeout))
	    {
		lock (readlock)
		{
		    if (readable && inbuf.used > 0)
			return true;
		}
		if (writable)
		    return true;
		if (!ok || got_eof)
		    return false;
		WvStream.runonce(remain);
	    }
	    return false;
	}
	
	static WvInBufStream _wvin = null;
	static WvStream _wvout = null, _wverr = null;
	public static WvInBufStream wvin {
	    get { 
		if (_wvin == null)
		    _wvin = new WvInBufStream(
		      new WvStreamStream(Console.OpenStandardInput()));
		return _wvin;
	    }
	}
	public static WvStream wvout {
	    get { 
		if (_wvout == null)
		    _wvout = new WvStreamStream(Console.OpenStandardOutput());
		return _wvout;
	    }
	}
	public static WvStream wverr {
	    get { 
		if (_wverr == null)
		    _wverr = new WvStreamStream(Console.OpenStandardError());
		return _wverr;
	    }
	}
    }
    
    public class _WvFile : WvStreamStream
    {
	static FileStream openstream(string filename, string modestring)
	{
	    FileMode mode;
	    FileAccess access;
	    bool truncate = false;
	    
	    if (modestring.Length == 0)
		throw new ArgumentException
		  ("WvFile: modestring cannot be empty", "modestring");
	    
	    char first = modestring[0];
	    bool plus = (modestring.Length > 1 && modestring[1] == '+');
		
	    switch (first)
	    {
	    case 'r':
		mode = FileMode.Open;
		access = plus ? FileAccess.ReadWrite : FileAccess.Read;
		break;
	    case 'w':
		mode = FileMode.OpenOrCreate;
		access = plus ? FileAccess.ReadWrite : FileAccess.Write;
		truncate = true;
		break;
	    case 'a':
		mode = FileMode.Append;
		access = plus ? FileAccess.ReadWrite : FileAccess.Write;
		break;
	    default:
		throw new ArgumentException
		    (wv.fmt("WvFile: modestring '{0}' must start " +
			    "with r, w, or a", modestring),
		     "modestring");
	    }
	    
	    var f = new FileStream(filename, mode, access, FileShare.ReadWrite);
	    if (truncate)
		f.SetLength(0);
	    return f;
	}
	
        public _WvFile(string filename, string modestring)
	    : base(openstream(filename, modestring))
	{
	}
    }
    
    public class WvFile : WvInBufStream
    {
	public WvFile(string filename, string modestring)
	    : base(new _WvFile(filename, modestring))
	{
	}
    }
}
