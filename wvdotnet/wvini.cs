/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.IO;
using System.Collections.Generic;

namespace Wv
{
    /// Well, it's not exactly UniConf, but...
    public class WvIni
    {
	string filename;
	Dictionary<string,IDictionary<string,string>> sections 
	    = new Dictionary<string,IDictionary<string,string>>
	            (StringComparer.OrdinalIgnoreCase);
	
	public WvIni(string filename)
	{
	    this.filename = filename;
	    
	    StreamReader r;
	    try
	    {
		r = File.OpenText(filename);
	    }
	    catch (IOException)
	    {
		return; // I guess there's no file!
	    }
	    
	    using (r)
	    {
		string section = "";
		string s;
		while ((s = r.ReadLine()) != null)
		{
		    s = s.Trim();
		    if (s.Length == 0) continue; // blank line
		    if (s[0] == '#') continue; // comment
		    if (s[0] == '[' && s[s.Length-1]==']') // section
		    {
			section = s.Substring(1, s.Length-2);
		    }
		    else if (s.IndexOf('=') >= 0)
		    {
			string[] a = s.Split(new char[] {'='}, 2);
			this[section][a[0].Trim()] = a[1].Trim();
		    }
		    else
			continue; // whatever
		}
	    }
	}
	
	public IDictionary<string,string> this[string sectname]
	{
	    get
	    {
		if (!sections.ContainsKey(sectname))
		    sections.Add(sectname, new Dictionary<string,string>
				     (StringComparer.OrdinalIgnoreCase));
		return sections[sectname];
	    }
	}
	
	public string get(string section, string key, string defval)
	{
	    string v;
	    if (!this[section].TryGetValue(key, out v))
		return defval;
	    return v;
	}
	
	public string get(string section, string key)
	{
	    return get(section, key, null);
	}
	
	public void set(string section, string key, string val)
	{
	    this[section][key] = val;
	}
	
	public void maybeset(string section, string key, string val)
	{
	    if (get(section, key) == null)
		set(section, key, val);
	}
	
	public void save(string filename)
	{
	    using (StreamWriter w = File.CreateText(filename))
	    {
		foreach (string section in sections.Keys)
		{
		    w.Write(wv.fmt("\n[{0}]\n", section));
		    foreach (string ent in sections[section].Keys)
			w.Write(wv.fmt("{0} = {1}\n",
				       ent, sections[section][ent]));
		}
	    }
	}
	
	public void save()
	{
	    save(filename);
	}
    }
}
