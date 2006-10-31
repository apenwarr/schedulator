using System;
using System.Collections;
using Wv.Utils;
using Wv.Schedulator;
using System.Data;
using System.Data.Odbc;

namespace Wv.Schedulator
{
    public class FogBugzSource : Source
    {
	string user; // get the bugs for this username
	Log log;
	
	IDbConnection db;
	
        public FogBugzSource(Schedulator s, string name, string odbcstring,
			     string user)
	    : base(s, name)
	{
	    this.user = user;
	    log = new Log(String.Format("FogBugz:{0}", name));
	    log.log("Initializing FogBugz source '{0}'.", name);
	    log.log("Connecting to: '{0}'", odbcstring);
	    db = new OdbcConnection(odbcstring);
	    db.Open();
	}
	
	public static Source create(Schedulator s, string name,
				    string prefix, string suffix)
	{
	    string[] bits = suffix.Split(':');
	    if (bits.Length >= 2)
		return new FogBugzSource(s, name, bits[0], bits[1]);
	    else
		throw new ArgumentException("bad moniker for FogBugzSource");
	}

	IDataReader select(string sql)
	{
	    IDbCommand cmd = db.CreateCommand();
	    cmd.CommandText = sql;
	    IDataReader reader = cmd.ExecuteReader();
	    return reader;
	}
	
	Hashtable fogpersons = new Hashtable();
	Hashtable fogpersons_byname = new Hashtable();
	Hashtable fogprojects = new Hashtable();
	Hashtable fogfixfors = new Hashtable();

	public override void make_basic()
	{
	    IDataReader r;
	    
	    log.log("Reading Person table.");
	    r = select("select ixPerson, sFullName, sEmail "
		       + "from Person "
		       + "order by fDeleted, ixPerson ");
	    while (r.Read())
	    {
		int ix = r.GetInt32(0);
		string fullname = r.IsDBNull(1) ? null : r.GetString(1);
		string email = r.IsDBNull(2) ? "--" : r.GetString(2);
		
		string name = email;
		if (name.IndexOf('@') >= 0)
		    name = name.Substring(0, name.IndexOf('@'));
		
		Person p = s.persons.Add(name, fullname);
		fogpersons.Add(ix, p);
		if (!fogpersons_byname.Contains(name))
		    fogpersons_byname.Add(name, ix);
	    }
	    
	    log.log("Reading Project table.");
	    r = select("select ixProject, sProject from Project");
	    while (r.Read())
	    {
		int ix = r.GetInt32(0);
		string name = r.IsDBNull(1) ? null : r.GetString(1);
		
		Project p = s.projects.Add(name);
		fogprojects.Add(ix, p);
	    }
	    
	    log.log("Reading FixFor table.");
	    r = select("select ixFixFor, ixProject, sFixFor, dt from FixFor");
	    while (r.Read())
	    {
		int ix = r.GetInt32(0);
		int projix = r.GetInt32(1);
		string name = r.IsDBNull(2) ? null : r.GetString(2);
		DateTime date = 
		    r.IsDBNull(3) ? DateTime.MinValue : r.GetDateTime(3);
		
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
		      bool done, DateTime donedate)
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
	    return t;
	}

	public override Task[] make_tasks()
	{
	    IDataReader r;
	    Hashtable abugs = new Hashtable(); // active
	    Hashtable vbugs = new Hashtable(); // needs-verify
	    Hashtable rbugs = new Hashtable(); // resolved-by-me
	    Hashtable sbugs = new Hashtable(); // stolen by someone else
	    
	    if (!fogpersons_byname.Contains(user))
	    {
		log.log("No user '{0}' exists!", user);
		return null;
	    }
	    
	    int userix = (int)fogpersons_byname[user];
	    
	    log.log("Listing active bugs.");
	    r = select(String.Format
		       ("select ixBug, ixStatus "
			+ "from Bug "
			+ "where ixPersonAssignedTo={0} ", userix));
	    while (r.Read())
	    {
		int ix = r.GetInt32(0);
		int status = r.GetInt32(1);
		if (status > 1)
		    vbugs.Add(ix, ix.ToString());
		else
		    abugs.Add(ix, ix.ToString());
	    }
	    
	    log.log("Reading BugEvent table (1).");
	    r = select(String.Format
		       ("select distinct ixBug "
			+ "   from BugEvent "
			+ "   where ixPerson={0} "
			+ "     and sVerb like 'Resolved %' ", userix));
	    while (r.Read())
		rbugs.Add(r.GetInt32(0), r.GetInt32(0).ToString());
	    log.log("  {0} bugs to check.", rbugs.Count);
	    string rbugs_str = bug_str(rbugs.Values);
	    //log.log("rbugs: {0}", rbugs_str);
	    
	    log.log("Reading BugEvent table (2).");
	    rbugs.Clear();
	    r = select(String.Format
		       ("select ixBug, ixPerson, sVerb "
			+ "   from BugEvent "
			+ "   where sVerb like 'Resolved %' "
			+ "     and ixBug in ({1}) "
			+ "   order by ixBug, ixBugEvent ",
			userix, rbugs_str));
	    int last_bug = -1;
	    bool resolved_by_me_once = false, resolved_away = false;
	    while (r.Read())
	    {
		int bug = r.GetInt32(0);
		int ixperson = r.GetInt32(1);
		string verb = r.GetString(2);
		
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
	    
	    log.log("{0} rbugs and {1} sbugs.", rbugs.Count, sbugs.Count);
	    
	    log.log("Reading Bug details.");
	    ArrayList all_bugs = new ArrayList();
	    add_list(all_bugs, abugs.Values);
	    add_list(all_bugs, vbugs.Values);
	    add_list(all_bugs, rbugs.Values);
	    add_list(all_bugs, sbugs.Values);
	    string all_str = bug_str(all_bugs);
	    r = select(String.Format
		       ("select ixBug, sTitle, ixFixFor, ixPriority, "
			+ "   dtResolved, "
			+ "   hrsOrigEst, hrsCurrEst, hrsElapsed "
			+ "from Bug "
			+ "where ixBug in ({0})", all_str));
	    while (r.Read())
	    {
		int ix = r.GetInt32(0);
		string ixstr = ix.ToString();
		string title = r.IsDBNull(1) ? "--" : r.GetString(1);
		int ixfixfor = r.GetInt32(2);
		int pri = r.GetInt32(3);
		DateTime resolvedate = 
		    r.IsDBNull(4) ? DateTime.MinValue : r.GetDateTime(4);
		double origest = r.GetFloat(5);
		double currest = r.GetFloat(6);
		double elapsed = r.GetFloat(7);
		
		FixFor fixfor = (FixFor)fogfixfors[ixfixfor];
		
		if (abugs.Contains(ix))
		{
		    Task t = add_task(ixstr, title, fixfor, pri,
				      false, resolvedate);
			
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
		    add_task(ixstr, title, fixfor, pri, true, resolvedate);
		else if (sbugs.Contains(ix))
		    add_task(ixstr, title, fixfor, pri, true, resolvedate);
		
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
			     false, resolvedate);
		}
	    }
	    
	    return null;
	}
    }
}
