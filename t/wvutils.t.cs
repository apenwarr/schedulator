/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
#include "wvtest.cs.h"
using System;
using Wv.Test;
using Wv;
using Wv.Extensions;

[TestFixture]
public class WvUtilsTests
{
    [Test] public void string_empty()
    {
	string a = null, b = "", c = "0";
	
	WVPASS(wv.isempty(a));
	WVPASS(wv.isempty(b));
	WVFAIL(wv.isempty(c));
	WVPASS(a.e());
	WVPASS(b.e());
	WVPASS(c.ne());
	WVFAIL(a.ne());
	WVFAIL(b.ne());
	WVFAIL(c.e());
    }
    
    [Test] [Category("shift")] public void shift_test()
    {
	string[] x = {"a", null, "c", "", "e", "f"};
	
	WVPASSEQ(wv.shift(ref x, 0), "a");
	WVPASSEQ(wv.shift(ref x, 0), null);
	WVPASSEQ(wv.shift(ref x, 1), "");
	WVPASSEQ(wv.shift(ref x, 2), "f");
	WVPASSEQ(x.Length, 2);
	WVPASSEQ(wv.shift(ref x, 0), "c");
	WVPASSEQ(wv.shift(ref x, 0), "e");
	WVPASSEQ(x.Length, 0);
    }
    
    [Test] [Category("ini")] public void ini_test()
    {
	WvIni ini = new WvIni("test.ini");
	WVPASSEQ(ini[""].Count, 2);
	WVPASSEQ(ini[""]["global item"], "i");
	WVPASSEQ(ini[""]["global 2"], "i2");
	WVPASSEQ(ini["subsEction"].Count, 3);
	WVPASSEQ(ini["subseCtion"]["2"], "3");
	WVPASSEQ(ini["nonexistent"].Count, 0);
    }

    [Test] [Category("add_breaks_to_newlines")] 
    public void test_add_breaks()
    {
        WVPASSEQ(wv.add_breaks_to_newlines(""), "");
        WVPASSEQ(wv.add_breaks_to_newlines("\n"), "<br/>\n");
        WVPASSEQ(wv.add_breaks_to_newlines("<br/>\n"), "<br/>\n");
        WVPASSEQ(wv.add_breaks_to_newlines("<br />\n"), "<br />\n");
        WVPASSEQ(wv.add_breaks_to_newlines("<br></br>\n"), "<br></br><br/>\n");
        WVPASSEQ(wv.add_breaks_to_newlines("\n\n"), "<br/>\n<br/>\n");
        WVPASSEQ(wv.add_breaks_to_newlines("foo\n\n"), "foo<br/>\n<br/>\n");
        WVPASSEQ(wv.add_breaks_to_newlines("\nfoo\n"), "<br/>\nfoo<br/>\n");
        WVPASSEQ(wv.add_breaks_to_newlines("foo\nfoo\n"), "foo<br/>\nfoo<br/>\n");
        WVPASSEQ(wv.add_breaks_to_newlines("foo\nfoo"), "foo<br/>\nfoo");
    }
    
    [Test] public void until_test()
    {
	DateTime t1 = DateTime.Now;
	bool first = true;
	foreach (var remain in wv.until(1000))
	{
	    if (first)
	    {
		first = false;
		WVPASS(remain > 500);
	    }
	}
	DateTime t2 = DateTime.Now;
	WVPASS((t2-t1).TotalMilliseconds >= 1000);
    }
}
