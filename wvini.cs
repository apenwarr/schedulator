using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace Wv.Utils
{
    /// Well, it's not exactly UniConf, but...
    public class Ini
    {
	Hashtable sections 
	    = new Hashtable(10, new CaseInsensitiveHashCodeProvider(),
			    new CaseInsensitiveComparer());
	
	public Ini(string filename)
	{
	    StreamReader r;
	    
	    try
	    {
		r = File.OpenText(filename);
	    }
	    catch (IOException)
	    {
		return; // I guess there's no file!
	    }
	    
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
	
	public StringDictionary this[string sectname]
	{
	    get
	    {
		if (!sections.Contains(sectname))
		    sections.Add(sectname, new StringDictionary());
		return (StringDictionary)sections[sectname];
	    }
	}
    }
}
