using System;
using System.IO;
using System.Net;
using System.Web;
using System.Collections;
using System.Text.RegularExpressions;
using Wv;
using Wv.Schedulator;

namespace Wv.Schedulator
{
    public class GoogleCodeSource : Source
    {
	string project; // get the bugs from this googlecode project
	string user; // get the bugs for this username
	WvLog log;
	
	class Row
	{
	    public string id = "", type = "", status = "",
		priority = "", milestone = "", owner = "", summary = "";
	}
	ArrayList rows;
	
        public GoogleCodeSource(Schedulator s, string name,
				string project, string user)
	    : base(s, name)
	{
	    this.project = project;
	    if (user != null)
		this.user = user;
	    else
		this.user = s.name;
	    
	    log = new WvLog(wv.fmt("GoogleCode:{0}", name));
	    log.print("Initializing GoogleCode source '{0}'.", name);
	}
	
	public static Source create(Schedulator s, string name,
				    string prefix, string suffix)
	{
	    string[] bits = suffix.Split(':');
	    if (bits.Length >= 2)
		return new GoogleCodeSource(s, name, bits[0], bits[1]);
	    else if (bits.Length >= 1)
		return new GoogleCodeSource(s, name, bits[0], null);
	    else
		throw new ArgumentException("bad moniker for GoogleCodeSource");
	}

	public override string view_url(string taskid)
	{
	    return wv.fmt("http://code.google.com/p/{0}/issues/detail?id={1}",
			  project, taskid);
	}
	
	int download_page(int start)
	{
	    int count = 0;
	    
	    string url = wv.fmt("http://code.google.com/p/{0}/issues/list"
		+ "?can=1"
		+ "&q=owner:{1}"
		+ "&start={2}"
		+ "&colspec=ID+Type+Status+Priority+Milestone+Owner+Summary",
		project, user, start);
	    
	    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
	    log.print("Created request (start={0}).", start);
	    
	    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
	    log.print("Created response.");
	    
	    StreamReader rr = new StreamReader(resp.GetResponseStream());
	    log.print("Created reader.");
	    
	    string s = rr.ReadToEnd();
	    
	    RegexOptions SL = RegexOptions.Singleline;
	    MatchCollection mc = Regex.Matches(s,
		       "<td class=\"vt[^\"]* col_(.)\"[^>]*>(.*?)</td>", SL);
	    Row r = new Row();
	    foreach (Match m in mc)
	    {
		string colid = m.Groups[1].Value, content = m.Groups[2].Value;
		content = Regex.Replace(content, "<[^>]*>", "", SL);
		content = Regex.Replace(content, "[ \t\r\n]+", " ", SL);
		content = Regex.Replace(content, "^ | $", "");
		if (content == "&nbsp;") continue;
		content = HttpUtility.HtmlDecode(content);
		
		// log.print("Match: {0}: [{1}]", colid, content);
		
		switch (wv.atoi(colid))
		{
		case 0:
		    r = new Row();
		    r.id = content;
		    break;
		case 1: r.type = content; break;
		case 2: r.status = content; break;
		case 3: r.priority = content; break;
		case 4: r.milestone = content; break;
		case 5: r.owner = content; break;
		case 6: 
		    r.summary = content;
		    rows.Add(r);
		    count++;
		    break;
		}
	    }
	    
	    return count;
	}
	
	void download_content()
	{
	    int start = 0, added;
	    rows = new ArrayList();
	    do
	    {
		added = download_page(start);
		start += added;
	    } while (added > 0);
	}
	
	public override void make_basic()
	{
	    download_content();
	    
	    Project p = s.projects.Add(project);
	    
	    foreach (Row r in rows)
		s.fixfors.Add(p, r.milestone);
	}
	
	int priority_map(string type, string pristr)
	{
	    if (type == "Defect")
	    {
		// Defects are always pretty high priority
		if (pristr == "Critical")
		    return 1;
		else if (pristr == "High")
		    return 2;
		else if (pristr == "Medium")
		    return 3;
		else if (pristr == "Low")
		    return 4;
	    }
	    else
	    {
		// Enhancements are lower priority than defects
		if (pristr == "Critical")
		    return 3;
		else if (pristr == "High")
		    return 4;
		else if (pristr == "Medium")
		    return 5;
		else if (pristr == "Low")
		    return 6;
	    }
	    return 0; // unknown priority, default == highest
	}

	public override Task[] make_tasks()
	{
	    Project p = s.projects.Find(project);
	    
	    foreach (Row r in rows)
	    {
		Task t = s.tasks.Add(this, r.id, r.summary);
		t.fixfor = s.fixfors.Find(p, r.milestone);
		t.priority = priority_map(r.type, r.priority);
		t.done = ! (r.status == "New"
			    || r.status == "Accepted"
			    || r.status == "Started");
	    }
	    
	    return null;
	}
    }
}
