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
public class WvUrlTests
{
    [Test] public void urls()
    {
	WvUrl u;
	
	u = new WvUrl("/foo/blah");
	WVPASSEQ(u.ToString(), "file:/foo/blah");
	WVPASSEQ(u.host, null);
	WVPASSEQ(u.port, 0);
	
	u = new WvUrl("foo/blah");
	WVPASSEQ(u.ToString(), "file:foo/blah");
	WVPASSEQ(u.host, null);
	WVPASSEQ(u.port, 0);
	
	u = new WvUrl("file:foo/blah");
	WVPASSEQ(u.ToString(), "file:foo/blah");
	
	u = new WvUrl("whatever:anything@nothing:stuff");
	WVPASSEQ(u.ToString(), "whatever:anything@nothing:stuff");
	WVPASSEQ(u.host, null);
	WVPASSEQ(u.port, 0);
	WVPASSEQ(u.path, "anything@nothing:stuff");
	
	u = new WvUrl("http:");
	WVPASSEQ(u.ToString(), "http:");
	
	u = new WvUrl("http://");
	WVPASSEQ(u.ToString(), "http:");
	
	u = new WvUrl("http://user:pass@host:57/path/to/stuff");
	WVPASSEQ(u.ToString(), "http://user@host:57/path/to/stuff");
	WVPASSEQ(u.ToString(true), "http://user:pass@host:57/path/to/stuff");
	WVPASSEQ(u.method, "http");
	WVPASSEQ(u.user, "user");
	WVPASSEQ(u.password, "pass");
	WVPASSEQ(u.host, "host");
	WVPASSEQ(u.port, 57);
	WVPASSEQ(u.path, "/path/to/stuff");
	
	// Microsoft UNC-style paths are actually valid URLs by our measure :)
	u = new WvUrl(@"\\server\path\to\stuff");
	WVPASSEQ(u.ToString(), "file://server/path/to/stuff");
	WVPASSEQ(u.method, "file");
	WVPASSEQ(u.user, null);
	WVPASSEQ(u.password, null);
	WVPASSEQ(u.host, "server");
	WVPASSEQ(u.port, 0);
	WVPASSEQ(u.path, "/path/to/stuff");
	
	u = new WvUrl(@"\\ser:9@1:23\path\to\stuff");
	WVPASSEQ(u.ToString(), "file://ser@1:23/path/to/stuff");
	WVPASSEQ(u.ToString(true), "file://ser:9@1:23/path/to/stuff");
	WVPASSEQ(u.method, "file");
	WVPASSEQ(u.user, "ser");
	WVPASSEQ(u.password, "9");
	WVPASSEQ(u.host, "1");
	WVPASSEQ(u.port, 23);
	WVPASSEQ(u.path, "/path/to/stuff");
	
	u = new WvUrl("://:@:");
	WVPASSEQ(u.ToString(), ":");
	WVPASSEQ(u.method, "");
	WVPASSEQ(u.user, null);
	WVPASSEQ(u.password, null);
	WVPASSEQ(u.host, null);
	WVPASSEQ(u.port, 0);
	WVPASSEQ(u.path, "");
	
	// since there's no hostname, we expect it to trim out the
	// username/password as well; they're useless without a host to 
	// log into!
	u = new WvUrl("://:47@:");
	WVPASSEQ(u.ToString(), ":");
	WVPASSEQ(u.ToString(true), ":");
    }
}
