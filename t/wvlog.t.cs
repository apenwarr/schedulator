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
public class WvLogTests
{
    string badrender()
    {
	// If this gets called, the delayedstring got rendered when it
	// shouldn't have.
	wv.assert(false);
	return "BROKEN";
    }
    
    bool b;
    string goodrender()
    {
	b = true;
	return "OK";
    }
    
    [Test] public void delayed_string()
    {
	WvLog.maxlevel = WvLog.L.Debug1;
	var log = new WvLog("foo", WvLog.L.Debug2);
	
	b = true;
	log.print(new WvDelayedString(badrender));
	WVPASS(b);
	b = false;
	log.print(WvLog.L.Debug1, new WvDelayedString(goodrender));
	WVPASS(b);
	b = true;
	log.print(WvLog.L.Debug2, new WvDelayedString(badrender));
	WVPASS(b);
    }
}
