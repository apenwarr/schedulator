/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.IO;
using System.Threading;
using Wv.Extensions;

namespace Wv
{
    public abstract class WvLogRcv
    {
	string open_header = null;
	byte[] nl = "\n".ToUTF8();
	
	protected abstract void w(WvBytes b);
	
	void w(string s)
	{
	    w(s.ToUTF8());
	}
	
	// note: outbuf is always completely empty when we return
	public void writelines(string header, WvBuf outbuf)
	{
	    // force ending of previous partial line, if necessary
	    if (outbuf.used > 0 
		&& open_header != null && open_header != header)
	    {
		w(nl);
		open_header = null;
	    }
	    
	    // zero or more full lines (terminated by newlines)
	    int i;
	    while ((i = outbuf.strchr('\n')) > 0)
	    {
		if (open_header == null)
		    w(header);
		w(outbuf.get(i));
		open_header = null;
	    }
	    
	    // end-of-buffer partial line (ie. no newline terminator yet)
	    if (outbuf.used > 0)
	    {
		if (open_header == null)
		    w(header);
		w(outbuf.get(outbuf.used));
		open_header = header;
	    }
	}
    }
    
    public class WvLogConsole : WvLogRcv
    {
	Stream outstr = Console.OpenStandardError();
	
	protected override void w(WvBytes b)
	{
	    outstr.Write(b.bytes, b.start, b.len);
	}
    }
    
    public class WvLogStream : WvLogRcv
    {
	WvStream outstr;
	
	public WvLogStream(WvStream outstr)
	{
	    this.outstr = outstr;
	}
	
	protected override void w(WvBytes b)
	{
	    outstr.write(b);
	}
    }
    
    public class WvLogFile : WvLogStream
    {
	public WvLogFile(string filename, string filemode)
	    : base(new WvFile(filename, filemode))
	{
	}
    }
    
    public class WvLog : WvStream
    {
	public enum L {
	    Critical = 0,
	    Error,
	    Warning,
	    Notice,
	    Info,
	    Debug, Debug1=Debug,
	    Debug2,
	    Debug3,
	    Debug4,
	    Debug5,
	};
	
	static L _maxlevel = L.Info;
	public static L maxlevel { 
	    get { return _maxlevel; }
	    set { _maxlevel = value; }
	}
	public static WvLogRcv recv = new WvLogConsole();
	
	string name;
	L level;
	
	string levelstr(L level)
	{
	    switch (level)
	    {
	    case L.Critical: return "!";
	    case L.Error:    return "E";
	    case L.Warning:  return "W";
	    case L.Notice:   return "N";
	    case L.Info:     return "I";
	    case L.Debug1:   return "1";
	    case L.Debug2:   return "2";
	    case L.Debug3:   return "3";
	    case L.Debug4:   return "4";
	    case L.Debug5:   return "5";
	    default:
		wv.assert(false, "Unknown loglevel??"); 
		return "??";
	    }
	}
	
	public WvLog(string name, L level)
	{
	    this.name = name;
	    this.level = level;
	}
	
	public WvLog(string name)
	    : this(name, L.Info)
	    { }
	
	public override int write(WvBytes b)
	{
	    if (level > maxlevel)
		return b.len; // pretend it's written
	    
	    WvBuf outbuf = new WvBuf();
	    outbuf.put(b);
	    recv.writelines(wv.fmt("{0}<{1}>: ", name, levelstr(level)),
			    outbuf);
	    return b.len;
	}
	
	public void print(L level, object s)
	{
	    L old = this.level;
	    try {
		this.level = level;
		print(s);
	    }
	    finally {
		this.level = old;
	    }
	}
	
	public void print(L level, string fmt, params object[] args)
	{
	    if (level > maxlevel)
		return;
	    print(level, (object)String.Format(fmt, args));
	}
	
	public override void print(object o)
	{
	    if (level > maxlevel)
		return;
	    base.print(o);
	}

	public WvLog split(L level)
	{
	    return new WvLog(name, level);
	}
    }
}

