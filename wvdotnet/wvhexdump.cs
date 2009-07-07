/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.Text;
using System.Linq;

namespace Wv
{
    public class WvDelayedString
    {
	Func<string> a;
	
	public WvDelayedString(Func<string> a)
	{
	    this.a = a;
	}
	
	public override string ToString()
	{
	    return a();
	}
    }

    public partial class wv
    {
	static string hexbyte(WvBytes b, int ofs)
	{
	    if (ofs >= b.start && ofs < b.start+b.len)
		return b.bytes[ofs].ToString("x2");
	    else
		return "  ";
	}
	
	static char printable(WvBytes b, int ofs)
	{
	    if (ofs >= b.start && ofs < b.start+b.len)
	    {
		byte n = b.bytes[ofs];
		if (31 < n && n < 127)
		    return (char)n;
		else
		    return '.';
	    }
	    else
		return ' ';
	}
	
	public static string _hexdump(WvBytes b)
	{
	    if (b.bytes == null)
		return "(nil)";
	    
            var sb = new StringBuilder();
	    
	    // This is overly complicated so that the body and header of
	    // the same buffer can be printed separately yet still show the
	    // proper alignment
 	    
	    int rowoffset = b.start & (~0xf);
	    
            // Note: it's important to set the right capacity when dealing 
            // with large quantities of data.  Assume about 80 chars per line.
            sb.EnsureCapacity((b.len / 16 + 2) * 80);

	    for (int i = rowoffset; i < b.len; i += 16)
	    {
		sb.Append('[').Append(i.ToString("x4")).Append("]");
		
		for (int j = 0; j < 16; j++)
		{
		    if ((j % 4)==0)
			sb.Append(' ');
		    sb.Append(hexbyte(b, i+j));
		}
		
		sb.Append(' ');
		
		for (int j = 0; j < 16; j++)
		{
		    if ((j % 4)==0)
			sb.Append(' ');
		    sb.Append(printable(b, i+j));
		}
		
		sb.Append('\n');
	    }
	
	    return sb.ToString();
	}
	
	public static object hexdump(WvBytes b)
	{
	    return new WvDelayedString(() => _hexdump(b));
	}
    }
}
