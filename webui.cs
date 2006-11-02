 using System;
using System.IO;
using System.Web;
using System.Data;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Wv.Utils;
using Wv.Dbi;
using Wv.Web;

namespace Wv.Schedulator
{
    public class WebUI
    {
	Cgi cgi = new Cgi();
	HtmlGen g = new HtmlGen();
	SourceRegistry reg = new SourceRegistry();

	Html render_est(string edit_key, TimeSpan span)
	{
	    string str = span > TimeSpan.Zero 
		? span.TotalHours.ToString() + "h"
		: "";
	    if (edit_key != null)
		return g.editinplace(edit_key, "text",
				     new Attr("size", 5),
				     g.text(str));
	    else
		return g.text(str);
	}
	
	void render_date(DateTime date, bool done, DateTime now,
			 out string datestr, out bool late)
	{
	    bool nodate = (date == DateTime.MinValue);
	    datestr = (nodate
		       ? "(???)"
		       : FixFor.date_string(date));
	    bool past = date < now;
	    late = past && !done && !nodate;
	    if (!late && past && !nodate)
		datestr = "done";
	}
	
	void titlerow(string msname, bool shown, Html text,
		   string datestr, bool late)
	{
	    Attr at1
		= new Attr("href",
		 String.Format("javascript:hideBugs('{0}', 'schedule');",
			       msname));
	    Attr at2
		= new Attr("href",
		 String.Format("javascript:showBugs('{0}', 'schedule');",
			       msname));
	    Html hidestr = g.span(new Attr("id", msname + "_hidelink",
						 "class", "showhidelink",
				 "style", shown ? "" : "display:none"),
				    g.a(at1, g.text("[-]")));
	    Html showstr = g.span(new Attr("id", msname + "_showlink",
						 "class", "showhidelink",
				 "style", shown ? "display:none" : ""),
				  g.a(at2, g.text("[+]")));
	    g.send(g.tr(new Attr("class", "title"),
			g.td(new Attr("colspan", 7),
			     hidestr, showstr, g.text(" "),
			     text),
			g.td(new Attr("class", late ? "late" : "notlate"),
			     datestr)));
	}
	
	void commentrow(string msname, bool shown, params Html[] ha)
	{
	    g.send(g.tr(new Attr("class", "comment",
			       "milestone", msname,
			       "style", shown ? "" : "display:none"),
			g.td(new Attr("colspan", 8), ha)));
	}
	
	void commentrow(string msname, bool shown, string s)
	{
	    commentrow(msname, shown, g.text(s));
	}
	
	void spacerow(string msname, bool shown)
	{
	    commentrow(msname, shown, g.nbsp());
	}
	
	void row(string msname, bool shown, bool done,
		 string pri, string name,
		 Html e1, Html e2, Html e3, Html e4,
		 Html due, bool late)
	{
	    g.send(g.tr(new Attr("class", done ? "done" : "notdone",
				 "milestone", msname,
				 "style", shown ? "" : "display:none"
				 ),
			g.td(""),
			g.td(pri),
			g.td(new Attr("class", "name"), name),
			g.td(e1), g.td(e2), g.td(e3), g.td(e4),
			g.td(new Attr("class", late ? "late" : "notlate"),
			     due)
			)
		   );
	}
	
	void taskrow(string msname, bool shown, bool done, string name,
		     Task t, DateTime end, DateTime now)
	{
	    string id = String.Format("{0}_{1}", t.source.name, t.id);
	    row(msname, shown, done,
		t.priority>0 ? t.priority.ToString() : "",
		name,
		render_est(null, t.origest),
		render_est(!done ? "currest_"+id : null, t.currest),
		render_est(!done ? "elapsed_"+id : null, t.elapsed),
		render_est(null, t.remain),
		g.text(FixFor.date_string(end)), end.Date < now);
	}
	
	void submit_button()
	{
	    Html e = g.text("");
	    row("", true, false, "", "", e, e, e, e,
		g.submit("Save"), false);
	}
	
	string base_url()
	{
	    return cgi.cgivars["SCRIPT_NAME"];
	}
	
	string person_url(string sid)
	{
	    if (!wv.isempty(sid))
		return base_url() + "?sid=" + sid;
	    else
		return base_url();
	}
	
	string person_url()
	{
	    return person_url(cgi.request["sid"]);
	}
	
	string summary_url(string project, string fixfor)
	{
	    return wv.fmt("{0}?summary={1}",
			  base_url(),
			  HttpUtility.UrlEncode(project + ":" + fixfor));
	}
	
	string get_file(string filename)
	{
	    if (filename == null) return "";
	    
	    try
	    {
		StreamReader r = File.OpenText(filename);
		return r.ReadToEnd();
	    }
	    catch (IOException)
	    {
		// ignore
	    }
	    
	    return "";
	}
	
	void put_file(string filename, string content)
	{
	    if (filename == null) return;
	    
	    try
	    {
		StreamWriter w = File.CreateText(filename);
		w.Write(content);
		w.Close();
	    }
	    catch (IOException)
	    {
		// ignore
	    }
	}
	
	void append_file(string filename, string content)
	{
	    if (filename == null || content == null || content == "")
		return;
	    
	    try
	    {
		StreamWriter w = File.AppendText(filename);
		w.Write(content);
		w.Close();
	    }
	    catch (IOException)
	    {
		// ignore
	    }
	}
	
	string schedname()
	{
	    if (!wv.isempty(cgi.request["sid"]))
	    {
		string s = cgi.request["sid"];
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
	    else
		return null;
	}
	
	string schedfile()
	{
	    string name = schedname();
	    if (name != null)
		return String.Format("schedules/{0}.sched", schedname());
	    else
		return null;
	}
	
	void handle_save_schedule()
	{
	    if (cgi.request["schedinput"] != null)
		put_file(schedfile(), cgi.request["schedinput"]);
	}
	
	void handle_save_estimates()
	{
	    string appendwhat = "";
	    foreach (string est in cgi.request.Keys)
	    {
		if (est.Length < 8) continue;
		string prefix = est.Substring(0, 8);
		if (prefix == "currest_" || prefix == "elapsed_")
		{
		    appendwhat
		       += String.Format("{0} {1}\n",
				HttpUtility.UrlEncode(est),
				HttpUtility.UrlEncode(cgi.request[est]));
		}
	    }
	    if (schedname() != null)
		append_file(schedfile() + ".log", appendwhat);
	}
	
	void http_redirect(string url)
	{
	    g.header("Location", url);
	    g.header("Status", "301 Moved Permanently");
	}
	
	string[] all_schedules()
	{
	    string[] files = Directory.GetFiles("schedules/", "*.sched");
	    string[] names = new string[files.Length];
	    int i = 0;
	    foreach (string file in files)
	    {
		wv.assert(file.Substring(0, 10) == "schedules/");
		wv.assert(file.Substring(file.Length-6) == ".sched");
		names[i++] = file.Substring(10, file.Length-6-10);
	    }
	    return names;
	}
	
	void page_list_schedules()
	{
	    g.send(g.title("Available Schedules - Schedulator"));
	    g.send(g.start_form(new Attr("action", base_url(),
					 "method", "GET")));
	    g.send(g.h1("Schedulator: Available Schedules"));
	    
	    g.send(g.start_ul());
	    foreach (string name in all_schedules())
	    {
		g.send(g.li(g.a(new Attr("href", person_url(name)),
				g.text(name))));
	    }
	    g.send(g.li(g.input("sid"), g.submit("Create New")));
	    g.send(g.end_ul());
	    
	    g.send(g.end_form());
	    
	    g.send(g.start_form(new Attr("action", base_url(),
					 "method", "POST")));
	    g.send(g.submit("Update All"));
	    g.send(g.end_form());
	    
	    Db db = new Db("schedulator");
	    g.send(g.h1("Schedulator: Available Summaries"));
	    g.send(g.start_ul());
	    IDataReader r = db.select
		("select distinct sProject, sFixfor "
		 + " from Schedule "
		 + " where fDone=0 "
		 + " order by sProject, sFixFor ");
	    string last_proj = "";
	    while (r.Read())
	    {
		string proj = r.GetString(0);
		string fixfor = r.GetString(1);
		if (last_proj != proj)
		{
		    if (last_proj != null)
			g.send(g.end_li());
		    g.send(g.start_li(), g.text(wv.fmt("{0}: ", proj)));
		    last_proj = proj;
		}
		else
		    g.send(g.text(", "));
		g.send(g.a(new Attr("href", summary_url(proj, fixfor)),
				g.text(fixfor)));
	    }
	    g.send(g.end_ul());
	}
	
	void page_update_all()
	{
	    g.send(g.title("Update All - Schedulator"));
	    g.send(g.start_form(new Attr("action", base_url(),
					 "method", "GET")));
	    g.send(g.h1("Schedulator: Updating All Schedules"));
	    
	    g.send(g.start_ul());
	    foreach (string name in all_schedules())
	    {
		g.send(g.li(g.text(name)));
		
		Schedulator s = new Schedulator(name);
		reg.create(s, "system",
			   "string:" + get_file("system.sched"));
		reg.create(s, "init", "file:schedules/" + name + ".sched");
		s.run();
	    }
	    g.send(g.end_ul());
	    
	    g.send(g.h2("Done."));
	    g.send(g.a(new Attr("href", base_url()), g.text("<<back")));
	    
	    g.send(g.end_form());
	}
	
	void page_show_schedule()
	{
	    Schedulator s = new Schedulator(schedname());
	    
	    wv.assert(schedname() != null);
	    g.send(g.title(schedname() + " - Schedulator"));
	    
	    g.send(g.start_form(new Attr("action", person_url(),
					 "name", "mainform",
					 "method", "POST")));
	    
	    g.send(g.a(new Attr("href", base_url()), g.text("<<back")));
	    g.send(g.h1(schedname() + "'s Schedulator"));
	    
	    string schedtext = get_file(schedfile());
	    g.send(g.editinplace("schedinput", "textarea",
			 new Attr("tooltip", g.text("(Edit Schedule)"),
				  "rows", 20,
				  "cols", 80,
				  "style", "display:none"),
			 g.text(schedtext)),
		   g.p());

	    
	    reg.create(s, "system", "string:" + get_file("system.sched"));
	    reg.create(s, "init", "string:" + schedtext);
	    
	    s.run();
	    //s.dump(new Log("S"));
	    
	    g.send(g.start_table(new Attr("id", "schedule",
					  "class", "schedule",
					  "border", "0",
					  "width", "95%")));
	    submit_button();
	    
	    bool was_done = true;
	    FixFor last_fixfor = null;
	    int msnum = 0;
	    string msname;
	    bool shown;
	    int shown_so_far = 0;
	    
	    g.send(g.tr(g.th(g.nbsp()),
			g.th("Pri"),
			g.th(new Attr("width", "100%"),
			     "Task"),
			g.th("Orig"),
			g.th("Curr"),
			g.th("Done"),
			g.th("Left"),
			g.th("Due")));
	    
	    msname = "ms_" + (++msnum).ToString();
	    shown = false;
	    titlerow(msname, shown, g.text("Finished tasks"), "", false);
	    
	    foreach (TimeSlot _ts in s.schedule)
	    {
		if (_ts is CommentTimeSlot)
		{
		    CommentTimeSlot ts = (CommentTimeSlot)_ts;
		    commentrow(msname, shown, ts.name);
		}
		
		if (_ts is TaskTimeSlot)
		{
		    TaskTimeSlot ts = (TaskTimeSlot)_ts;
		    FixFor ff = ts.fixfor;
		    if (ff == null)
			ff = s.fixfors.Add(s.projects.Add("UNKNOWN"),
					   "Undecided");
		    
		    if (!ts.done
			&& (was_done || ts.fixfor != last_fixfor))
		    {
			if (last_fixfor != null)
			{
			    string datestr2;
			    bool late2;
			    render_date(last_fixfor.final_release,
					was_done, s.now,
					out datestr2, out late2);
			    Html h = g.text("");
			    row(msname, shown, was_done,
				"", "RELEASE",
				h, h, h, h,
				g.text(datestr2), late2);
			}
			spacerow(msname, shown);
			msname = "ms_" + (++msnum).ToString();
			shown = (shown_so_far < 25);
			
			string datestr;
			bool late;
			render_date(ff.final_release,
				    false, s.now,
				    out datestr, out late);
			titlerow(msname, shown,
				 g.v(g.text("MILESTONE: "),
				     g.a(new Attr("href", summary_url(ff.project.name, ff.name)),
					 g.text(wv.fmt("{0} - {1}", ff.project.name, ff.name)))),
				 datestr, false);
			
			last_fixfor = ts.fixfor;
			was_done = false;
		    }
		    
		    taskrow(msname, shown, ts.done, ts.name,
			    ts.task, ts.end, s.now);
		    if (shown)
			shown_so_far++;
		}
	    }
	    
	    submit_button();
	    g.send(g.end_table());
	}
	
	struct MiniTask
	{
	    public string user, id, task;
	    public int priority;
	    public bool done, halfdone, estimated;
	}
	
	string[] monthnames = {
	    "Jan", "Feb", "Mar", "Apr", "May", "June",
	    "Jul", "Aug", "Sept", "Oct", "Nov", "Dec"
	};
	    
	public void page_show_summary()
	{
	    string pff = cgi.request["summary"];
	    string[] splits = pff.Split(new char[] {':'}, 2);
	    string projname = splits[0];
	    string fixforname = (splits.Length > 1) ? splits[1] : "";
	    
	    g.send(g.title(wv.fmt("Summary of {0} - Schedulator", pff)));
	    g.send(g.a(new Attr("href", base_url()), g.text("<<back")));
	    g.send(g.h1(wv.fmt("Schedulator: Summary of {0}", pff)));
	    
	    Db db = new Db("dsn=schedulator;uid=root;pwd=scs");
	    IDataReader r = db.select
		("select sUser, sTaskId, sTask, ixPriority, "
		 + "    fDone, fHalfDone, fEstimated, dtEnd "
		 + "  from Schedule "
		 + "  where sProject=? and sFixFor=? "
		 + "  order by dtEnd, ixPriority, sTaskId ",
		 projname, fixforname);
	    
	    Hashtable tasks = new Hashtable();
	    Hashtable people = new Hashtable();
	    DateTime firstdate = DateTime.MaxValue;
	    DateTime lastdate = DateTime.MinValue;
	    ObjectCounter ycounts = new ObjectCounter();
	    ObjectCounter mcounts = new ObjectCounter();
	    ObjectCounter dcounts = new ObjectCounter();
	    
	    while (r.Read())
	    {
		string user = r.GetString(0);
		string id = r.GetString(1);
		string task = r.GetString(2);
		int priority = r.GetInt32(3);
		bool done = r.GetByte(4)!=0 ? true : false;
		bool halfdone = r.GetByte(5)!=0 ? true : false;
		bool estimated = r.GetByte(6)!=0 ? true : false;
		DateTime end = r.GetDateTime(7).Date;
		
		if (done) continue;
		
		dcounts[end.Date]++;
		if (firstdate > end) firstdate = end;
		if (lastdate < end)  lastdate = end;
		if (!people.Contains(user))
		    people.Add(user, true);
		
		if (!tasks.Contains(end))
		    tasks.Add(end, new ArrayList());
		ArrayList day = (ArrayList)tasks[end];
		
		MiniTask t = new MiniTask();
		t.user = user;
		t.id = id;
		t.task = task;
		t.priority = priority;
		t.done = done;
		t.halfdone = halfdone;
		t.estimated = estimated;
		day.Add(t);
	    }
	    
	    foreach (DateTime date in dcounts.Keys)
	    {
		ycounts[date.Year]++;
		mcounts[new DateTime(date.Year, date.Month, 1)]++;
	    }
	    
	    g.send(g.start_table(new Attr("class", "summary")));
	    
	    g.send(g.start_tr(new Attr("class", "yearheader")), g.td());
	    for (int year = firstdate.Year; year <= lastdate.Year; year++)
	    {
		if (ycounts[year] == 0) 
		    continue;
		g.send(g.th(new Attr("colspan", ycounts[year]),
			    g.text(year.ToString())));
	    }
	    g.send(g.end_tr());
	    
	    g.send(g.start_tr(new Attr("class", "monthheader")), g.td());
	    foreach (DateTime date in wv.sort(mcounts.Keys))
	    {
		g.send(g.th(new Attr("colspan", mcounts[date]),
			    g.text(monthnames[date.Month-1])));
	    }
	    g.send(g.end_tr());
	    
	    g.send(g.start_tr(new Attr("class", "dayheader")), g.td());
	    foreach (DateTime date in wv.sort(dcounts.Keys))
		g.send(g.th(g.text(date.Day.ToString())));
	    g.send(g.end_tr());
	    
	    foreach (string user in wv.sort(people.Keys))
	    {
		g.send(g.start_tr(new Attr("class", "tasks")));
		g.send(g.th(new Attr("class", "username"),
			    g.a(new Attr("href", person_url(user)),
				g.text(user))));
		foreach (DateTime date in wv.sort(tasks.Keys))
		{
		    ArrayList day = (ArrayList)tasks[date];
		    ArrayList html = new ArrayList();
		    foreach (MiniTask t in day)
		    {
			if (t.user != user) continue;
			
			string name = t.priority.ToString();
			
			Attr at = new Attr
			    ("title", wv.fmt("{0}: {1}", t.id, t.task),
			     "class",
			      wv.fmt("{0} p{1}",
				     t.done ? "done" 
				       : (t.halfdone
					  ? "halfdone" : "notdone"),
				     t.priority)
			     );
			html.Add(g.v(g.span(at, g.text(name)),
			     t.estimated ? g.text(" ") : g.sup("? ")));
		    }
		    g.send(g.td(g.htmlarray(html)));
		}
		g.send(g.end_tr());
	    }
		
	    g.send(g.end_table());
	    
	    g.send(g.h2("Done."));
	}
	
        public class StringTraceListener : TraceListener
	{
	    StringBuilder all = new StringBuilder();
	    
	    StringTraceListener() : base()
	    {
	    }
	    
	    public override void Write(string message)
	    {
		all.Append(message);
	    }
	    
	    public override void WriteLine(string message)
	    {
		all.Append(message + "\n");
	    }
	    
	    public string get()
	    {
		return all.ToString();
	    }
	}
	
	static StringTraceListener stl;
	
	public void run()
	{
	    try {
		handle_save_schedule();
		handle_save_estimates();
		
		// HTTP redirect if it was a POST, so pressing the back
		// button doesn't automatically repost the form
		if (!wv.isempty(cgi.request["Update All"]))
		    page_update_all();
		else if (cgi.method == Cgi.Method.Post)
		    http_redirect(person_url());
		else // normal page
		{
		    g.send(g.include_css("schedulator.css"),
			   g.include_js("schedulator.js"),
			   g.use_editinplace());
		    
		    if (!wv.isempty(cgi.request["summary"]))
			page_show_summary();
		    else if (schedname() != null)
			page_show_schedule();
		    else
			page_list_schedules();
		}
		
		g.send(g.done());
	    
		// throw new Exception();
	    }
	    catch (Exception e)
	    {
		Console.Write("\n\n\n</html><pre>\n");
		Console.Write(stl.get());
		Console.Write("\n\n");
		Console.Write(e.ToString());
	    }
	}
	
	public static void Main()
	{
	    stl = new StringTraceListener();
	    Trace.Listeners.Add(stl);
	    Log.no_default_listener();
	    
	    (new WebUI()).run();
	}
    }
}
