/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
#include "wvtest.cs.h"
using System;
using Wv.Test;
using Wv;

[TestFixture]
public class WvMonikerTests
{
    class Message
    {
	string s;
	
	public Message(string s)
	{
	    this.s = s;
	}
	
	public string msg
	    { get { return s; } }
    }

    
    [Test] public void monikers()
    {
	new WvMoniker<Message>("a",
		       (string s, object o) => new Message("a" + s));
	var mb =
	    new WvMoniker<Message>("b",
		       (string s, object o) => new Message("b" + s));
	
	WVPASSEQ(WvMoniker<Message>.create("x:foo"), null);
	WVPASSEQ(WvMoniker<Message>.create("a:hello").msg, "ahello");
	WVPASSEQ(WvMoniker<Message>.create("b:hello2").msg, "bhello2");
	WVPASSEQ(WvMoniker<Message>.create("b").msg, "b");
	WVPASSEQ(WvMoniker<Message>.create("boo"), null);
	WVPASSEQ(WvMoniker<Message>.create("b:hello3").msg, "bhello3");
	mb.unregister();
	WVPASSEQ(WvMoniker<Message>.create("b:hello4"), null);
    }
}
