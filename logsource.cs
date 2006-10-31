using System;
using System.IO;
using System.Web;
using System.Collections;
using Wv.Utils;
using Wv.Schedulator;

namespace Wv.Schedulator
{
    public class LogSource : Source
    {
	string logstr;
	
        public LogSource(Schedulator s, string name, string logstr)
	    : base(s, name)
	{
	    this.logstr = logstr;
	}
	
	public static Source create(Schedulator s, string name,
				    string prefix, string suffix)
	{
	    return new LogSource(s, name, suffix);
	}
	
	static string clean_filename(string s)
	{
	    string s2 = "";
	    foreach (char c in s)
	    {
		if (Char.IsLetterOrDigit(c))
		    s2 += c;
		else
		    s2 += "_";
	    }
	    return s2;
	}
	
	static string get_file_from_id(string id)
	{
	    try
	    {
		StreamReader r = File.OpenText(
			       String.Format("schedules/{0}.sched.log",
					     clean_filename(id)));
		return r.ReadToEnd();
	    }
	    catch (IOException)
	    {
		// nothing
	    }
	    return "";
	}
    
	public static Source create_from_file_id(Schedulator s, string name,
					      string prefix, string suffix)
	{
	    return new LogSource(s, name, get_file_from_id(suffix));
	}

	public override void cleanup_tasks()
	{
	    Hashtable changes = new Hashtable();
	    
	    string[] lines = logstr.Split("\n".ToCharArray());
	    foreach (string line in lines)
	    {
		string[] words = line.Split(" ".ToCharArray());
		if (words.Length != 2) continue;
		string key = HttpUtility.UrlDecode(words[0]);
		string value = HttpUtility.UrlDecode(words[1]);
		TimeSpan span = StringSource.parse_estimate(0, value);
		changes.Remove(key);
		if (span > TimeSpan.Zero)
		    changes.Add(key, span);
	    }
	    
	    foreach (string key in changes.Keys)
	    {
		string[] words = key.Split("_".ToCharArray(), 3);
		if (words.Length < 3) continue; // invalid
		
		Task t = s.tasks.FindById(words[1] + ":" + words[2]);
		if (t == null)
		    continue; // no longer exists, ignore
		
		TimeSpan span = (TimeSpan)changes[key];
		
		switch (words[0])
		{
		case "currest": t.currest = span; break;
		case "elapsed": t.elapsed = span; break;
		}
	    }
	}
    }
}
