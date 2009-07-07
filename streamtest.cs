/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.Collections;
using Wv;
using Wv.Extensions;

public class FooTest
{
    static IEnumerable contprint(WvLog log, WvStream s, string prefix)
    {
	int i = 0;
	while (s.ok)
	{
	    i++;
	    string str = s.read(128).FromUTF8();
	    log.print("{0}#{1}: {2}\n", prefix, i, str);
	    yield return 0;
	}
    }
    
    public static void Main()
    {
	{
	    Console.WriteLine("stdout works.");
	    Console.OpenStandardError().write("stderr works.\n".ToUTF8());
	    
	    WvLog log = new WvLog("main");
	    log.print("Hello");
	    log.print(" world!\n");
	    
	    WvStream s1 = new WvTcp("localhost", 80);
	    WvStream s2 = new WvTcp("localhost", 80);
	    s1.onreadable += contprint(log, s1, "\nA\n").ToAction();
	    s2.onreadable += contprint(log, s2, "\nB\n").ToAction();
	    s1.print("GET / HTTP/1.0\r\n\r\n");
	    s2.print("FOO / HTTP/1.0\r\n\r\n");
	    while (s1.ok || s2.ok)
		WvStream.runonce();
	    log.print("\n");
	    log.print("s1 err: {0}\n", s1.err.Short());
	    log.print("s2 err: {0}\n", s2.err.Short());
	}
    }
}
