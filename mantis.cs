using System;
using System.Data;
using System.Data.Odbc;
using System.Collections;
using Wv.Dbi;
using Wv.Utils;
using Wv.Schedulator;

namespace Wv.Schedulator
{
    public class MantisSource : Source
    {
	string user; // get the bugs for this username
	Log log;
	
	Db db;
	
        public MantisSource(Schedulator s, string name, string odbcstring,
			     string user)
	    : base(s, name)
	{
	    // note: the null user doesn't exist, but the "" user is used
	    // for all bugs not assigned to anyone yet.
	    if (user != null)
		this.user = user;
	    else
		this.user = s.name;
	    
	    log = new Log(wv.fmt("Mantis:{0}", name));
	    log.log("Initializing Mantis source '{0}'.", name);
	    log.log("Connecting to: '{0}'", odbcstring);
	    db = new Db(odbcstring);
	}
	
	public static Source create(Schedulator s, string name,
				    string prefix, string suffix)
	{
	    string[] bits = suffix.Split(':');
	    if (bits.Length >= 2)
		return new MantisSource(s, name, bits[0], bits[1]);
	    else if (bits.Length >= 1)
		return new MantisSource(s, name, bits[0], null);
	    else
		throw new ArgumentException("bad moniker for MantisSource");
	}

	Hashtable mantispersons = new Hashtable();
	Hashtable mantispersons_byname = new Hashtable();
	Hashtable mantisprojects = new Hashtable();
	Hashtable mantisfixfors = new Hashtable();

	public override void make_basic()
	{
	    IDataReader r;
	    
	    log.log("Reading mantis_user_table.");
	    r = db.select("select id, username, realname "
			  + "from mantis_user_table "
			  + "order by enabled desc, id ");
	    while (r.Read())
	    {
		int ix = r.GetInt32(0);
		string name = r.IsDBNull(1) ? "--" : r.GetString(1);
		string fullname = r.IsDBNull(2) ? null : r.GetString(2);
		if (wv.isempty(fullname))
		    fullname = name;
		name = name.ToLower();
		
		Person p = s.persons.Add(name, fullname);
		mantispersons.Add(ix, p);
		if (!mantispersons_byname.Contains(name))
		    mantispersons_byname.Add(name, ix);
	    }
	    
	    // the "Undecided" user
	    mantispersons.Add(0, s.persons.Add("", "-Undecided-"));
	    mantispersons_byname.Remove("");
	    mantispersons_byname.Add("", 0);
	    
	    log.log("Reading mantis_project_table.");
	    r = db.select("select id, name from mantis_project_table");
	    while (r.Read())
	    {
		int ix = r.GetInt32(0);
		string name = r.IsDBNull(1) ? null : r.GetString(1);
		
		Project p = s.projects.Add(name);
		mantisprojects.Add(ix, p);
	    }
	    
	    log.log("Reading list of versions.");
	    string[] cols = {"fixed_in_version", "version"};
	    foreach (string col in cols)
	    {
		r = db.select(wv.fmt("select distinct project_id, {0} "
				     + "from mantis_bug_table "
				     + "order by project_id, {0} ", col));
		while (r.Read())
		{
		    int projix = r.GetInt32(0);
		    string name = r.GetString(1);
		    if (wv.isempty(name)) name = "-Undecided-";
		    string ix = projix.ToString() + "." + name;
		
		    Project project = (Project)mantisprojects[projix];
		    if (project == null)
			project = s.projects.Add("UNKNOWN");
		    
		    FixFor f = s.fixfors.Add(project, name);
		    if (!mantisfixfors.Contains(ix))
			mantisfixfors.Add(ix, f);
		}
	    }
	    
	    log.log("Reading mantis_project_version_table.");
	    r = db.select("select project_id, version, date_order "
			  + " from mantis_project_version_table ");
	    while (r.Read())
	    {
		int projix = r.GetInt32(0);
		string name = r.GetString(1);
		DateTime date = r.GetDateTime(2);
		string ix = projix.ToString() + "." + name;
		
		Project project = (Project)mantisprojects[projix];
		if (project == null)
		    project = s.projects.Add("UNKNOWN");
		
		FixFor f = s.fixfors.Add(project, name);
		if (!mantisfixfors.Contains(ix))
		    mantisfixfors.Add(ix, f);
		f.add_release(date);
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
	    Task t = s.tasks.Add(this, ixstr,
				 String.Format("({0}) {1}", ixstr, title));
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

	enum Status {
	    New=10, Feedback=20, Acknowledged=30, Confirmed=40,
		Assigned=50, Resolved=80, Closed=90,
	}
	
	enum Resolution {
	    Open=10, Fixed=20, Reopened=30, NotReproducible=40,
		NotFixable=50, Duplicate=60, NotABug=70, Suspended=80,
		WontFix=80,
	}
	
	enum Priority {
	    None=10, Low=20, Normal=30, High=40, Urgent=50, Immediate=60,
	}
	
	int priority_map(int pri)
	{
	    if (pri <= (int)Priority.None)
		return 7;
	    else if (pri <= (int)Priority.Low)
		return 5;
	    else if (pri <= (int)Priority.Normal)
		return 4;
	    else if (pri <= (int)Priority.High)
		return 3;
	    else if (pri <= (int)Priority.Urgent)
		return 2;
	    else if (pri <= (int)Priority.Immediate)
		return 1;
	    else
		return 1;
	}
	
	public override Task[] make_tasks()
	{
	    IDataReader r;
	    Hashtable abugs = new Hashtable(); // active
	    Hashtable vbugs = new Hashtable(); // needs-verify
	    Hashtable rbugs = new Hashtable(); // resolved-by-me
	    Hashtable sbugs = new Hashtable(); // stolen by someone else
	    
	    if (!mantispersons_byname.Contains(user))
	    {
		log.log("No user '{0}' exists!", user);
		return null;
	    }
	    
	    int userix = (int)mantispersons_byname[user];
	    
	    log.log("Listing active bugs.");
	    r = db.select(wv.fmt("select id, status "
				 + "from mantis_bug_table "
				 + "where handler_id={0} "
				 + "  and status != {1} ",
				 userix, (int)Status.Closed));
	    while (r.Read())
	    {
		int ix = r.GetInt32(0);
		int status = r.GetInt32(1);
		if (status >= (int)Status.Resolved)
		    vbugs.Add(ix, ix.ToString());
		else
		    abugs.Add(ix, ix.ToString());
	    }
	    
	    log.log("Reading mantis_bug_history_table (1).");
	    r = db.select(wv.fmt("select distinct bug_id "
				 + "   from mantis_bug_history_table "
				 + "   where user_id={0} "
				 + "     and field_name='resolution' ",
				 userix));
	    while (r.Read())
		rbugs.Add(r.GetInt32(0), r.GetInt32(0).ToString());
	    log.log("  {0} bugs to check.", rbugs.Count);
	    string rbugs_str = bug_str(rbugs.Values);
	    // log.log("rbugs: {0}", rbugs_str);

	    log.log("Reading mantis_bug_history_table (2).");
	    rbugs.Clear();
	    r = db.select(wv.fmt
			  ("select bug_id, user_id, new_value "
			   + "   from mantis_bug_history_table "
			   + "   where field_name='resolution' "
			   + "     and new_value is not null "
			   + "     and bug_id in ({1}) "
			   + "   order by bug_id, id ",
			   userix, rbugs_str));
	    int last_bug = -1;
	    bool resolved_by_me_once = false, resolved_away = false;
	    while (r.Read())
	    {
		int bug = r.GetInt32(0);
		int ixperson = r.GetInt32(1);
		int newval = wv.atoi(r.GetString(2));
		
		if (last_bug != bug)
		{
		    if (resolved_away)
			sbugs.Add(last_bug, last_bug.ToString());
		    else if (resolved_by_me_once)
			rbugs.Add(last_bug, last_bug.ToString());
		    
		    last_bug = bug;
		    resolved_by_me_once = resolved_away = false;
		}

		// hacky attempt at Resolved (Again) from FogBugz
		if (newval == (int)Resolution.Suspended)
		    continue;
		
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
	    
	    log.log("{0} abugs, {1} vbugs, {2} rbugs and {3} sbugs.", 
		    abugs.Count, vbugs.Count, rbugs.Count, sbugs.Count);
	    
	    log.log("Reading bug details.");
	    ArrayList all_bugs = new ArrayList();
	    add_list(all_bugs, abugs.Values);
	    add_list(all_bugs, vbugs.Values);
	    add_list(all_bugs, rbugs.Values);
	    add_list(all_bugs, sbugs.Values);
	    string all_str = bug_str(all_bugs);
	    r = db.select(wv.fmt
			  ("select id, summary, project_id, "
			   + "   fixed_in_version, version, "
			   + "   priority, last_updated "
			   + "from mantis_bug_table "
			   + "where id in ({0})", all_str));
	    while (r.Read())
	    {
		int ix = r.GetInt32(0);
		string ixstr = ix.ToString();
		string title = r.IsDBNull(1) ? "--" : r.GetString(1);
		int ixproject = r.GetInt32(2);
		string ffname
		    = (r.IsDBNull(3)
		       ? (r.IsDBNull(4) ? "" : r.GetString(4))
		       : r.GetString(3));
		if (wv.isempty(ffname)) ffname = "-Undecided-";
		int pri = priority_map(r.GetInt32(5));
		string ixfixfor = ixproject.ToString() + "." + ffname;
		DateTime resolvedate 
		    = r.IsDBNull(6) ? DateTime.MinValue : r.GetDateTime(6);
		
		FixFor fixfor = (FixFor)mantisfixfors[ixfixfor];
		
		if (abugs.Contains(ix))
		    add_task(ixstr, title, fixfor, pri, false, false,
			     DateTime.MinValue);
		else if (rbugs.Contains(ix) && !vbugs.Contains(ix))
		    add_task(ixstr, title, fixfor, pri, true, true,
			     resolvedate);
		else if (sbugs.Contains(ix))
		    add_task(ixstr, title, fixfor, pri, true, true,
			     resolvedate);
		
		// FIXME: "verify" tasks will disappear once closed,
		// since they'll no longer be assigned to the user.
		// We should read "closed" verbs from BugEvent too.
		if (vbugs.Contains(ix))
		{
		    string x = abugs.Contains(ix) ? "v"+ixstr : ixstr;
		    add_task(x, "VERIFY: " + title, fixfor, pri,
			     false, true, DateTime.MinValue);
		}
	    }

	    return null;
	}
    }
}
