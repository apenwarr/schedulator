 using System;
using System.IO;
using System.Web;
using System.Collections;
using Wv.Utils;
using Wv.Web;

namespace Wv.Schedulator
{
    public class WebUI
    {
	Cgi cgi = new Cgi();
	HtmlGen g = new HtmlGen();
	SourceRegistry reg = new SourceRegistry();
	Schedulator s = new Schedulator();

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
	    bool past = date < s.now;
	    late = past && !done && !nodate;
	    if (!late && past && !nodate)
		datestr = "done";
	}
	
	void titlerow(string msname, bool shown, string s,
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
			     g.text(s)),
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
		     Task t, DateTime end)
	{
	    string id = String.Format("{0}_{1}", t.source.name, t.id);
	    row(msname, shown, done,
		t.priority>0 ? t.priority.ToString() : "",
		name,
		render_est(null, t.origest),
		render_est(!done ? "currest_"+id : null, t.currest),
		render_est(!done ? "elapsed_"+id : null, t.elapsed),
		render_est(null, t.remain),
		g.text(FixFor.date_string(end)), end.Date < s.now);
	}
	
	void submit_button()
	{
	    Html e = g.text("");
	    row("", true, false, "", "", e, e, e, e,
		g.submit("Save"), false);
	}
	
	string base_url(string sid)
	{
	    if (!wv.isempty(sid))
		return cgi.cgivars["SCRIPT_NAME"] + "?sid=" + sid;
	    else
		return cgi.cgivars["SCRIPT_NAME"];
	}
	
	string base_url()
	{
	    return base_url(cgi.request["sid"]);
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
	    if (cgi.request["sid"] != null && cgi.request["sid"] != "")
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
	
	void page_list_schedules()
	{
	    g.send(g.title("Available Schedules - Schedulator"));
	    
	    g.send(g.h1("Schedulator: Available Schedules"));
	    
	    g.send(g.start_ul());
	    foreach (string file
		     in Directory.GetFiles("schedules/", "*.sched"))
	    {
		wv.assert(file.Substring(0, 10) == "schedules/");
		wv.assert(file.Substring(file.Length-6) == ".sched");
		string id = file.Substring(10, file.Length-6-10);
		g.send(g.li(g.a(new Attr("href",
					 base_url() + "?sid=" + id),
				g.text(id))));
	    }
	    g.send(g.end_ul());
	    
	    g.send(g.start_form(new Attr("action", base_url(),
					 "method", "GET")),
		   g.input("sid"),
		   g.submit("Create New"),
		   g.end_form());
	}
	
	void page_show_schedule()
	{
	    wv.assert(schedname() != null);
	    g.send(g.title(schedname() + " - Schedulator"));
	    
	    g.send(g.start_form(new Attr("action", base_url(),
					       "name", "mainform",
					       "method", "POST")));
	    
	    g.send(g.a(new Attr("href", base_url("")),
		       g.text("<<back")));
	    g.send(g.h1(schedname() + "'s Schedulator"));
	    
	    string schedtext = get_file(schedfile());
	    g.send(g.editinplace("schedinput", "textarea",
			 new Attr("tooltip", g.text("(Edit Schedule)"),
				  "rows", 20,
				  "cols", 80,
				  "style", "display:none"),
			 g.text(schedtext)),
		   g.p());

	    
	    reg.create(s, "main", "string:" + schedtext);
	    
	    s.run();
	    //s.dump(new Log("S"));
	    
	    g.send(g.start_table(new Attr("id", "schedule",
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
	    titlerow(msname, shown, "Finished tasks", "", false);
	    
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
				 String.Format("MILESTONE: {0} - {1}",
					       ff.project.name,
					       ff.name),
				 datestr, false);
			
			last_fixfor = ts.fixfor;
			was_done = false;
		    }
		    
		    taskrow(msname, shown, ts.done, ts.name,
			    ts.task, ts.end);
		    if (shown)
			shown_so_far++;
		}
	    }
	    
	    submit_button();
	    g.send(g.end_table());
	}
	
	public void run()
	{
	    handle_save_schedule();
	    handle_save_estimates();
	    
	    // HTTP redirect if it was a POST, so pressing the back
	    // button doesn't automatically repost the form
	    if (cgi.method == Cgi.Method.Post)
		http_redirect(base_url());
	    else // normal page
	    {
		g.send(g.include_css("schedulator.css"),
		       g.include_js("schedulator.js"),
		       g.use_editinplace());
		
		if (schedname() == null)
		    page_list_schedules();
		else
		    page_show_schedule();
	    }
	    
	    g.send(g.done());
	}
	
	public static void Main()
	{
	    (new WebUI()).run();
	}
    }
}
