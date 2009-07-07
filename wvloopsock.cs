/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.Net;
using System.Net.Sockets;

namespace Wv
{
    /**
     * A "loopback" socket that can write to itself, allowing you to wake up
     * an select() call on demand.  Just include this socket in the select()
     * read list, and write to it from another thread when the time comes.
     */
    public class WvLoopSock
    {
	public Socket readsock { get; private set; }
	Socket writesock;
	object socklock = new object();
	bool sent = false;
	byte[] trigger = new byte[] { (byte)'x' };
	
	public WvLoopSock()
	{
	    Exception last_e = null;
	    
	    for (int count = 0; count < 1000; count++)
	    {
		byte[] accessbytes = wv.randombytes(100);
		Socket listensock = new Socket(AddressFamily.InterNetwork,
					       SocketType.Stream,
					       ProtocolType.Tcp);
		
		byte[] b = wv.randombytes(2);
		int port = (ushort)((b[0]/2<<8) + b[1] + 1024);
		var ipe = new IPEndPoint(IPAddress.Loopback, port);
		try {
		    listensock.Bind(ipe);
		}
		catch (SocketException e) {
		    last_e = e;
		    listensock.Close();
		    continue;
		}
		listensock.Listen(1);
	    
		writesock = new Socket(AddressFamily.InterNetwork,
				       SocketType.Stream,
				       ProtocolType.Tcp);
		writesock.Connect(ipe);
		writesock.NoDelay = true;
		writesock.Send(accessbytes);
		
		readsock = listensock.Accept();
		listensock.Close();
		
		// The security code is needed because we can't be certain
		// someone else on the same machine isn't the one connecting
		// to us.  If we could use Unix domain sockets, we could
		// set the permissions to avoid this problem, but sadly,
		// that doesn't work in Windows.  This method is sort of
		// passable.
		byte[] gotbytes = new byte[100];
		readsock.Poll(1000*1000, SelectMode.SelectRead);
		if (readsock.Receive(gotbytes) != 100
		    || !equal(gotbytes, accessbytes))
		{
		    last_e = new Exception(
				   "WvLoopSock: security code mismatch?!");
		    readsock.Close();
		    writesock.Close();
		    continue;
		}
		    
		return;
	    }
	    
	    // if we get here, creation failed for some reason
	    throw last_e;
	}
	
	static bool equal(byte[] a, byte[] b)
	{
	    if (a.Length != b.Length)
		return false;
	    int len = a.Length;
	    for (int i = 0; i < len; i++)
		if (a[i] != b[i])
		    return false;
	    return true;
	}
	
	public void set()
	{
	    lock (socklock)
	    {
		if (!sent)
		    writesock.Send(trigger);
		sent = true;
	    }
	}
	
	public void drain()
	{
	    lock(socklock)
	    {
		byte[] b = new byte[100];
		readsock.Blocking = true;
		try {
		    while (readsock.Poll(0, SelectMode.SelectRead))
			readsock.Receive(b);
		}
		catch (SocketException) {
		    // just a socket timeout
		}
		sent = false;
	    }
	}
    }
}
