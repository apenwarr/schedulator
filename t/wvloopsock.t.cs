/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
#include "wvtest.cs.h"
using System;
using System.Net.Sockets;
using Wv;
using Wv.Extensions;
using Wv.Test;

[TestFixture]
public class WvLoopSockTests
{
    int try_recv(Socket s)
    {
	try {
	    byte[] b = new byte[100];
	    s.Blocking = true;
	    s.ReceiveTimeout = 1;
	    return s.Receive(b);
	}
	catch (SocketException) {
	    return 0;
	}
    }
    
    [Test] public void basics()
    {
	var l = new WvLoopSock();
	
	WVPASSEQ(try_recv(l.readsock), 0);
	l.set();
	WVPASSEQ(try_recv(l.readsock), 1);
	WVPASSEQ(try_recv(l.readsock), 0);
	l.drain();
	l.set();
	l.set();
	WVPASSEQ(try_recv(l.readsock), 1);
	WVPASSEQ(try_recv(l.readsock), 0);
    }
}
