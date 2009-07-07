/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
#include "wvtest.cs.h"
using System;
using System.Collections.Generic;
using Wv.Test;
using Wv;
using Wv.Extensions;

[TestFixture]
public class WvStreamTests
{
    [Test] public void basics()
    {
	using (WvStream s = new WvStream())
	{
	    WVPASS(s.ok);
	    s.close();
	    WVFAIL(s.ok);
	}
	
	using (WvStream s = new WvStream())
	{
	    int closed_called = 0, closed_called2 = 0;
	    s.onclose += delegate() { closed_called++; };
	    s.onclose += delegate() { closed_called2++; };
	    Exception e1 = new Exception("e1");
	    Exception e2 = new Exception("e2");
	    WVPASS(s.ok);
	    WVPASSEQ(closed_called, 0);
	    WVPASSEQ(closed_called2, 0);
	    s.err = e1;
	    s.err = e2;
	    WVFAIL(s.ok);
	    WVPASS(s.err != null);
	    WVPASS(s.err == e1);
	    WVPASS(s.err.Message == "e1");
	    WVPASSEQ(closed_called, 1);
	    WVPASSEQ(closed_called2, 1);
	}
    }
    
    [Test] public void loopback()
    {
	using (WvLoopback l = new WvLoopback())
	{
	    l.print("bonk\nwonk\nblonk");
	    WVPASSEQ(l.read(5).FromUTF8(), "bonk\n");
	    WVPASSEQ(l.read(2).FromUTF8(), "wo");
	    string got = null, got2 = null;
	    Action d = delegate() {
		Console.WriteLine("onreadable!");
		got = l.read(4096).FromUTF8();
		got2 = l.read(4096).FromUTF8();
	    };
	    l.onreadable += d; 
	    l.print("bah");
	    WVPASSEQ(got, null);
	    WVPASSEQ(got2, null);
	    WvStream.runonce(0);
	    WVPASSEQ(got, "nk\nblonkbah");
	    WVPASSEQ(got2, "");
	    WvStream.runonce(0);
	    WVPASSEQ(got, "nk\nblonkbah");
	    l.print("hah");
	    WVPASSEQ(got, "nk\nblonkbah");
	    WvStream.runonce(0);
	    WVPASSEQ(got, "hah");
	    
	    l.onwritable += delegate() {
		l.print("X");
	    };
	    l.onreadable -= d;
	    
	    WvStream.runonce(0);
	    
	    d();
	    WVPASSEQ(got, "X");
	}
    }
    
    [Test] public void inbufstream()
    {
	using (WvLoopback l = new WvLoopback())
	using (WvInBufStream b = new WvInBufStream(l))
	{
	    string s = null;
	    int wcount = 0, rcount = 0;
	    
	    WVPASS(true);
	    b.onwritable += delegate() {
		Console.WriteLine("writing");
		wcount++;
		b.print("X");
		Console.WriteLine("done writing");
	    };
	    WVPASS(true);
	    b.onreadable += delegate() {
		Console.WriteLine("reading");
		rcount++;
		s = b.getline(0, '\n');
		Console.WriteLine("done reading");
	    };
	    
	    WVPASSEQ(wcount, 0);
	    WVPASSEQ(rcount, 0);
	    WVPASSEQ(s, null);
	    
	    WvStream.runonce(0);
	    WVPASS(true);
	    WvStream.runonce(0);
	    
	    WVPASSEQ(wcount, 2);
	    WVPASSEQ(rcount, 1);
	    WVPASSEQ(s, null);
	    
	    wv.print("putstring\n");
	    b.print("string!\n");
	    WvStream.runonce(10000);
	    
	    WVPASSEQ(wcount, 3);
	    WVPASSEQ(rcount, 2);
	    WVPASS(s != null);
	    WVPASSEQ(s, "XXstring!\n");
	}
    }
    
    IEnumerable<int> checker(WvFile f)
    {
	Console.WriteLine("checker!");
	WVPASSEQ(f.getline(-1, '\n'), "Line 1\n");
	yield return 0;
	WVPASSEQ(f.getline(-1, '\n'), "Line 2!!\n");
	yield return 0;
	WVPASSEQ(f.getline(-1, '\n'), "Line 3\rwith CR\n");
	yield return 0;
	WVPASSEQ(f.getline(-1, '\n'), null);
	yield return 0;
	WVPASS(false); // should never reach here
    }
    
    [Test] public void filestream()
    {
	using (WvFile f = new WvFile("testfile.txt", "r"))
	{
	    f.onreadable += checker(f).ToAction();
	    while (f.ok)
	    {
		Console.WriteLine("iter");
		WvStream.runonce(1000);
	    }
	    
	    if (f.err != null)
	    {
		Console.WriteLine(f.err);
		WVFAIL(true);
	    }
	}
	
	using (WvFile f = new WvFile("testfile.txt", "r"))
	{
	    int rcount = 0;
	    f.onreadable += delegate() {
		rcount++;
	    };
	    WVPASSEQ(rcount, 0);
	    WvStream.runonce(10000);
	    WVPASSEQ(rcount, 1);
	    WVPASSEQ(f.read(1).FromUTF8(), "L");
	    WVPASSEQ(f.getline(-1, '\r'), "ine 1\nLine 2!!\nLine 3\r");
	    WvStream.runonce(10000);
	    WVPASSEQ(f.read(1024).FromUTF8(), "with CR\n");
	    WVPASSEQ(rcount, 2);
	    WvStream.runonce(0);
	    WVPASSEQ(rcount, 3);
	    WvStream.runonce(0);
	    WVPASSEQ(rcount, 3);
	}
    }
}
