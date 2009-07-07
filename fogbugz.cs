using System;
using System.Collections;
using Wv;
using Wv.Schedulator;
using System.Data;
using System.Data.Odbc;

namespace Wv.Schedulator
{
    public class FogBugzSource : Source, IDisposable
    {
	string user; // get the bugs for this username
	WvLog log;
	
	WvDbi db;
	
        public FogBugzSource(Schedulator s, string name, string odbcstring,
			     string user)
	    : base(s, name)
	{
	    if (!wv.isempty(user))
		this.user = user;
	    else
		this.user = s.name;
	    log = new WvLog(String.Format("FogBugz:{0}", name));
	    log.print("Initializing FogBugz source '{0}'.\n", name);
	    log.print("Connecting to: '{0}'\n", odbcstring);
	    db = WvDbi.create(odbcstring);
	}
	
	public void Dispose()
	{
	    if (db != null) 
	    {
		db.Dispose();
		db = null;
	    }
	}
	
	public static Source create(Schedulator s, string name,
				    string prefix, string suffix)
	{
	    string[] bits = suffix.Split(':');
	    if (bits.Length >= 2)
		return new FogBugzSource(s, name, bits[0], bits[1]);
	    else if (bits.Length >= 1)
		return new FogBugzSource(s, name, bits[0], null);
	    else
		throw new ArgumentException("bad moniker for FogBugzSource");
	}

	Hashtable fogpersons = new Hashtable();
	Hashtable fogpersons_byname = new Hashtable();
	Hashtable fogprojects = new Hashtable();
	Hashtable fogfixfors = new Hashtable();

	public override string view_url(string taskid)
	{
	    return wv.fmt("http://fogbugz/?{0}", taskid);
	}
	
	public override void make_basic()
	{
	    log.print("Reading Person table.\n");
	    foreach (var r in db.select("select ixPerson, sFullName, sEmail "
			  + "from Person "
			  + "order by fDeleted, ixPerson "))
	    {
		int ix = r[0];
		string fullname = r[1];
		string email = r[2].IsNull ? "--" : r[2];
		
		string name = email;
		if (name.IndexOf('@') >= 0)
		    name = name.Substring(0, name.IndexOf('@'));
		
		Person p = s.persons.Add(name, fullname);
		fogpersons.Add(ix, p);
		if (!fogpersons_byname.Contains(name))
		    fogpersons_byname.Add(name, ix);
		if (!fogpersons_byname.Contains(fullname))
		    fogpersons_byname.Add(fullname, ix);
	    }
	    
	    log.print("Reading Project table.\n");
	    foreach (var r in db.select("select ixProject, sProject " +
	                                "   from Project"))
	    {
		int ix = r[0];
		string name = r[1];
		
		Project p = s.projects.Add(name);
		fogprojects.Add(ix, p);
	    }
	    
	    log.print("Reading FixFor table.\n");
	    foreach (var r in db.select("select ixFixFor, ixProject, sFixFor, dt "
			  + " from FixFor "))
	    {
		int ix = r[0];
		int projix = r[1];
		string name = r[2];
		DateTime date = r[3];
		
		Project project = (Project)fogprojects[projix];
		if (project == null)
		    project = s.projects.Add("UNKNOWN");
		
		FixFor f = s.fixfors.Add(project, name);
		if (date > DateTime.MinValue)
		    f.add_release(date);
		fogfixfors.Add(ix, f);
	    }
	}
	
	string bug_str(ICollection list)
	{
	    if (list.Count == 0)
		return "-1";
	    else
	    {
		string[] array = new string[list.Count];
		list.CopyTo(array, 0);
		return String.Join(",", array);
	    }
	}
	
	void add_list(ArrayList list, ICollection src)
	{
	    foreach (object o in src)
		list.Add(o);
	}
	
	Task add_task(string ixstr, string title, FixFor fixfor, int pri,
		      bool done, bool halfdone, DateTime donedate)
	{
	    Task t = s.tasks.Add(this, ixstr, title);
	    t.fixfor = fixfor;
	    t.priority = pri;
	    if (done)
	    {
		t.done = true;
		t.donedate = donedate;
	    }
	    t.halfdone = halfdone;
	    return t;
	}

	public override Task[] make_tasks()
	{
	    Hashtable abugs = new Hashtable(); // active
	    Hashtable vbugs = new Hashtable(); // needs-verify
	    Hashtable rbugs = new Hashtable(); // resolved-by-me
	    Hashtable sbugs = new Hashtable(); // stolen by someone else
	    
	    if (!fogpersons_byname.Contains(user))
	    {
		log.print("No user '{0}' exists!\n", user);
		return null;
	    }
	    
	    int userix = (int)fogpersons_byname[user];
	    
	    log.print("Listing active bugs.\n");
	    foreach (var r in db.select(String.Format
			  ("select ixBug, ixStatus "
			   + "from Bug "
			   + "where ixPersonAssignedTo={0} "
			   + "  and fOpen=1 ", userix)))
	    {
		int ix = r[0];
		int status = r[1];
		if (status > 1)
		    vbugs.Add(ix, ix.ToString());
		else
		    abugs.Add(ix, ix.ToString());
	    }
	    
	    log.print("Reading BugEvent table (1).\n");
	    foreach (var r in db.select(String.Format
			  ("select distinct ixBug "
			   + "   from BugEvent "
			   + "   where ixPerson={0} "
			   + "     and sVerb like 'Resolved %' ", userix)))
		rbugs.Add((int)r[0], (string)r[0]);
	    log.print("  {0} bugs to check.\n", rbugs.Count);
	    string rbugs_str = bug_str(rbugs.Values);
	    //log.print("rbugs: {0}\n", rbugs_str);
	    
	    log.print("Reading BugEvent table (2).\n");
	    rbugs.Clear();
	    int last_bug = -1;
	    bool resolved_by_me_once = false, resolved_away = false;
	    foreach (var r in db.select(String.Format
			  ("select ixBug, ixPerson, sVerb "
			   + "   from BugEvent "
			   + "   where sVerb like 'Resolved %' "
			   + "     and ixBug in ({1}) "
			   + "   order by ixBug, ixBugEvent ",
			   userix, rbugs_str)))
	    {
		int bug = r[0];
		int ixperson = r[1];
		string verb = r[2];
		
		if (last_bug != bug)
		{
		    if (resolved_away)
			sbugs.Add(last_bug, last_bug.ToString());
		    else if (resolved_by_me_once)
			rbugs.Add(last_bug, last_bug.ToString());
		    
		    // FIXME: implement "stolen bugs" feature here!
		    // resolved_away bugs should show up in my schedule
		    // to remind me that someone has re-resolved them, thereby
		    // possibly taking away the time I had assigned to that
		    // bug.
		    // 
		    // FIXME2: that's stupid anyway, we should find a way to
		    // not steal the time in that case.
		    last_bug = bug;
		    resolved_by_me_once = resolved_away = false;
		}
		
		if (verb == "Resolved (Again)")
		    continue; // skipping this is exactly what (Again) is for
		
		if (userix == ixperson)
		{
		    resolved_by_me_once = true;
		    resolved_away = false; // I stole it back!
		}
		else if (resolved_by_me_once)
		    resolved_away = true;  // Someone re-resolved my bug!
	    }
	    
	    // finish the very last bug from that query
	    if (resolved_away)
		sbugs.Add(last_bug, last_bug.ToString());
	    else if (resolved_by_me_once)
		rbugs.Add(last_bug, last_bug.ToString());
	    
	    log.print("{0} rbugs and {1} sbugs.\n", rbugs.Count, sbugs.Count);
	    
	    log.print("Reading Bug details.\n");
	    ArrayList all_bugs = new ArrayList();
	    add_list(all_bugs, abugs.Values);
	    add_list(all_bugs, vbugs.Values);
	    add_list(all_bugs, rbugs.Values);
	    add_list(all_bugs, sbugs.Values);
	    string all_str = bug_str(all_bugs);
	    foreach (var r in db.select(String.Format
			  ("select ixBug, sTitle, ixFixFor, ixPriority, "
			   + "   dtResolved, "
			   + "   hrsOrigEst, hrsCurrEst, hrsElapsed "
			   + "from Bug "
			   + "where ixBug in ({0})", all_str)))
	    {
		int ix = r[0];
		string ixstr = ix.ToString();
		string title = r[1].IsNull ? "--" : r[1];
		int ixfixfor = r[2];
		int pri = r[3];
		DateTime resolvedate = r[4];
		double origest = r[5];
		double currest = r[6];
		double elapsed = r[7];
		
		FixFor fixfor = (FixFor)fogfixfors[ixfixfor];
		
		if (abugs.Contains(ix))
		{
		    Task t = add_task(ixstr, title, fixfor, pri,
				      false, false, resolvedate);
			
		    // FIXME: we ignore estimates on all non-active bugs,
		    // because the estimate field is used for the person
		    // originally fixing the bug, not the person verifying
		    // it.  We'll have to store the verifier's time
		    // elsewhere.
		    if (origest > 0) t.origest = TimeSpan.FromHours(origest);
		    if (currest > 0) t.currest = TimeSpan.FromHours(currest);
		    if (elapsed > 0) t.elapsed = TimeSpan.FromHours(elapsed);
		}
		else if (rbugs.Contains(ix))
		    add_task(ixstr, title, fixfor, pri, true, true,
			     resolvedate);
		else if (sbugs.Contains(ix))
		    add_task(ixstr, title, fixfor, pri, true, true,
			     resolvedate);
		
		// a bug can be *both* active and needing verification, if the
		// same person opened it and resolved it.
		// 
		// FIXME: "verify" tasks will disappear once closed,
		// since they'll no longer be assigned to the user.
		// We should read "closed" verbs from BugEvent too.
		if (vbugs.Contains(ix))
		{
		    string x = abugs.Contains(ix) ? "v"+ixstr : ixstr;
		    add_task(x, "VERIFY: " + title, fixfor, pri,
			     false, true, resolvedate);
		}
	    }
	    
	    return null;
	}
    }
}
