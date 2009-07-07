/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using Wv;
using Wv.Extensions;

namespace Wv
{
    public class WvMonikerAttribute : Attribute
        { }

    public class WvMonikerBase
    {
	static object lockobj = new Object();
	static bool registered = false;

	protected static void register_all()
	{
	    if (!registered)
	    {
		// feebly attempt to be threadsafe: prevent registrations from
		// running except in one thread.
		lock(lockobj)
		{
		    if (registered) return;
		    
		    foreach (var t in
			     WvReflection.find_types(typeof(WvMonikerAttribute)))
		    {
			t.InvokeMember("wvmoniker_register",
				       BindingFlags.Static
				         | BindingFlags.Public
				         | BindingFlags.InvokeMethod,
				       null, null, null);
		    }
		    registered = true;
		}
	    }
	}
    }
    
    public class WvMoniker<T>: WvMonikerBase
    {
	static List<WvMoniker<T>> registry = new List<WvMoniker<T>>();
	string prefix;
	Func<string,object,T> func;
	
	public static WvMoniker<T>
	    register(string prefix, Func<string,object,T> func)
	{
	    return new WvMoniker<T>(prefix, func);
	}
	
        public WvMoniker(string prefix, Func<string,object,T> func)
	{
	    this.prefix = prefix;
	    this.func = func;
	    registry.Add(this);
	}
	
	// probably nobody will ever call this
	public void unregister()
	{
	    registry.Remove(this);
	}
	
	public static WvMoniker<T> find(string prefix)
	{
	    foreach (WvMoniker<T> m in registry)
		if (m.prefix == prefix)
		    return m;
	    return null;
	}
	
	public static T create(string moniker, object o)
	{
	    register_all();
	    
	    int pos = moniker.IndexOf(':');
	    string prefix, suffix;
	    if (pos >= 0)
	    {
		prefix = moniker.Substring(0, pos);
		suffix = moniker.Substring(pos+1);
	    }
	    else
	    {
		prefix = moniker;
		suffix = "";
	    }
	    
	    WvMoniker<T> m = find(prefix);
	    if (m == null)
		return default(T);
	    else
		return m.func(suffix, o);
	}
	
	public static T create(string moniker)
	{
	    return create(moniker, null);
	}
    }
}
