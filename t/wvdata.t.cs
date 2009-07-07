/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
#include "wvtest.cs.h"
using System;
using Wv;
using Wv.Extensions;
using Wv.Test;

[TestFixture]
public class WvDataTests
{
    [Test] public void nulls_test()
    {
	WvAutoCast n = WvAutoCast._null;
	object o = n;
	WVPASS(o != null);
	WVPASS(o.ToString() != null);
	WVPASSEQ((int)n, 0);
	WVPASSEQ((long)n, 0);
	WVPASSEQ((double)n, 0.0);
	
	n = new WvAutoCast("-6");
	o = n;
	WVPASS(o != null);
	WVPASSEQ(o.ToString(), "-6");
	WVPASSEQ((int)n, -6);
	WVPASSEQ((long)n, -6);
	WVPASSEQ((int)(((double)n)*10000), -60000);

	n = new WvAutoCast("-5.5555.p");
	o = n;
	WVPASS(o != null);
	WVPASSEQ(o.ToString(), "-5.5555.p");
	WVPASSEQ((int)n, -5);
	WVPASSEQ((long)n, -5);
	WVPASSEQ((int)(((double)n)*10000), -55555);
    }

    [Test]
    public void bool_test()
    {
        WvAutoCast t = new WvAutoCast(true);
        WvAutoCast f = new WvAutoCast(false);
        WVPASSEQ(t.ToString(), "1");
        WVPASSEQ(f.ToString(), "0");
        WVPASSEQ((double)t, 1.0);
        WVPASSEQ((double)f, 0.0);
        WVPASSEQ((int)t, 1);
        WVPASSEQ((int)f, 0);
    }
    
    [Test]
    public void assignto_test()
    {
	var list = new object[] { 1, "foo", 99.8 };
	
	{
	    int a;
	    string b, c;
	    double d;
	    
	    int num = list.assignto(out a, out b, out c, out d);
	    WVPASSEQ(a, 1);
	    WVPASSEQ(b, "foo");
	    WVPASSEQ(c, "99.8");
	    WVPASSEQ(d, 0);
	    WVPASSEQ(num, 3);
	}
	
	{
	    int a, b, c, d;
	    int num = list.assignto(out a, out b, out c, out d);
	    WVPASSEQ(a, 1);
	    WVPASSEQ(b, 0);
	    WVPASSEQ(c, 99);
	    WVPASSEQ(d, 0);
	    WVPASSEQ(num, 3);
	}
	
	var l2 = new int[] { 0,1,2,3,4,5,6,7,8,9,10 };
	{
	    int a,b,c,d;
	    double e;
	    float f;
	    decimal g;
	    bool h;
	    int num = l2.assignto(out a, out b, out c, out d,
				  out e, out f, out g, out h);
	    WVPASSEQ(a, 0);
	    WVPASSEQ(b, 1);
	    WVPASSEQ(c, 2);
	    WVPASSEQ(d, 3);
	    WVPASSEQ(e, 4);
	    WVPASSEQ(f, 5);
	    WVPASSEQ((decimal)6, 6);
	    WVPASSEQ((decimal)new WvAutoCast((int)6), 6);
	    WVPASSEQ(g, 6);
	    WVPASSEQ(h, true);
	    WVPASSEQ(num, 8);
	}
    }
}
