/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Wv
{
    public interface IWvEventer
    {
	void runonce(int msec_timeout);
	void runonce();
	void onreadable(Socket s, Action a);
	void onwritable(Socket s, Action a);
	void addpending(Object cookie, Action a);
	void delpending(Object cookie);
	void addtimeout(Object cookie, DateTime t, Action a);
	void deltimeout(Object cookie);
    }
    
    public class WvEventer : IWvEventer
    {
	WvLoopSock loop = new WvLoopSock();
	
	// CAREFUL! The 'pending' structure might be accessed from other
	// threads!
	Dictionary<object, Action> 
	    pending = new Dictionary<object, Action>();
	
	Dictionary<Socket, Action> 
	    r = new Dictionary<Socket, Action>(),
            w = new Dictionary<Socket, Action>();
	
	class TimeAction
	{
	    public DateTime t;
	    public Action a;
	    
	    public TimeAction(DateTime t, Action a)
	    {
		this.t = t;
		this.a = a;
	    }
	}
	Dictionary<Object, TimeAction>
	    ta = new Dictionary<Object, TimeAction>();
    
	public WvEventer()
	{
	}
	
	public void onreadable(Socket s, Action a)
	{
	    if (s == null) return;
	    r.Remove(s);
	    if (a != null)
		r.Add(s, a);
	}
	
	public void onwritable(Socket s, Action a)
	{
	    if (s == null) return;
	    w.Remove(s);
	    if (a != null)
		w.Add(s, a);
	}
	
	public void addtimeout(Object cookie, DateTime t, Action a)
	{
	    ta.Remove(cookie);
	    if (a != null)
		ta.Add(cookie, new TimeAction(t, a));
	    loop.set();
	}
    
	// NOTE: 
	// This is the only kind of event you can enqueue from a thread other
	// than the one doing runonce()!
	// It will run your Action in the runonce() thread on the next pass.
	public void addpending(Object cookie, Action a)
	{
	    lock(pending)
	    {
		pending.Remove(cookie);
		pending.Add(cookie, a);
		loop.set();
	    }
	}
    
	public void delpending(Object cookie)
	{
	    lock(pending)
	    {
		pending.Remove(cookie);
	    }
	}
	
	public void deltimeout(Object cookie)
	{
	    ta.Remove(cookie);
	}
    
	public void runonce()
	{
	    runonce(-1);
	}
    
	public void runonce(int msec_timeout)
	{
	    // we do this first; anybody who has enqueued any events will
	    // be processed below before select().  If anyone does
	    // loop.set() between now and select(), we'll end up returning
	    // right away, which is harmless.
	    loop.drain();
	    
	    IList<Socket> rlist = r.Keys.ToList();
	    IList<Socket> wlist = w.Keys.ToList();
	    rlist.Add(loop.readsock);
	    IList<TimeAction> talist = ta.Values.ToList();
	    if (msec_timeout < 0)
		msec_timeout = 1000000;
	    TimeAction first 
		= new TimeAction(DateTime.Now
			 + TimeSpan.FromMilliseconds(msec_timeout), null);
	
	    foreach (TimeAction t in talist)
		if (t.t < first.t)
		    first = t;
	    
	    TimeSpan timeout = first.t - DateTime.Now;
	    if (timeout < TimeSpan.Zero)
		timeout = TimeSpan.Zero;
	    
	    lock(pending)
	    {
		if (pending.Count > 0)
		    timeout = TimeSpan.Zero;
	    }
	
	    if (rlist.Count == 0 && wlist.Count == 0)
	    {
		// Socket.Select throws an exception if all lists are empty;
		// idiots.
		if (timeout > TimeSpan.Zero)
		    Thread.Sleep((int)timeout.TotalMilliseconds);
	    }
	    else
	    {
		Socket.Select((IList)rlist, (IList)wlist, null,
			      (int)timeout.TotalMilliseconds * 1000);
	    }
	
	    DateTime now = DateTime.Now;
	    foreach (Socket s in rlist)
		if (s != loop.readsock)
		    r[s]();
	    foreach (Socket s in wlist)
		w[s]();
	    foreach (Object cookie in ta.Keys)
	    {
		TimeAction t = ta[cookie];
		if (t.t <= now)
		{
		    t.a();
		    ta.Remove(cookie);
		}
	    }
	
	    Action[] nowpending;
	    lock(pending)
	    {
		nowpending = pending.Values.ToArray();
		pending.Clear();
	    }
	    // Console.WriteLine("NowPending: {0}", nowpending.Length);
	    foreach (Action a in nowpending)
		a();
	}
    }
}
