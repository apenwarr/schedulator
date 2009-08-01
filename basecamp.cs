using System;
using System.IO;
using System.Net;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Wv;
using Wv.Extensions;
using Wv.Schedulator;

namespace Wv.Schedulator
{
    public class BasecampSource : Source
    {
	WvUrl url; // auth information for this project
	string user; // get the bugs for this username
	Dictionary<int,bool> userids = new Dictionary<int,bool>();
	WvLog log;
	Basecamp.BasecampManager bc;
	
	struct TodoInfo {
	    public FixFor fixfor;
	    public Basecamp.TodoList list;
	    
	    public TodoInfo(FixFor f, Basecamp.TodoList l)
	    { 
		fixfor = f;
		list = l;
	    }
	}
	List<TodoInfo> todoinfo = new List<TodoInfo>();
	
        public BasecampSource(Schedulator s, string name, string url)
	    : base(s, name)
	{
	    this.url = new WvUrl(url);
	    if (this.url.path.ne())
		this.user = this.url.path;
	    else
		this.user = s.name;
	    if (this.user.StartsWith("/"))
		this.user = this.user.Substring(1);
	    
	    log = new WvLog(wv.fmt("Basecamp:{0}", name));
	    log.print("Init Basecamp '{0}' (proj={1}, user={2}).\n",
		      name, this.url.host, this.user);
	    
	    bc = new Basecamp.BasecampManager(this.url.host, this.url.user, 
					      this.url.password, false);
	}
	
	public static Source create(Schedulator s, string name,
				    string prefix, string suffix)
	{
	    return new BasecampSource(s, name, suffix);
	}

	public override string view_url(string taskid)
	{
	    // can't show a specific task, just the entire todo list
	    string[] bits = taskid.split("_");
	    return wv.fmt("http://{0}.basecamphq.com/todo_lists/{1}",
			  url.host, bits[0]);
	}
	
	public override void make_basic()
	{
	    log.print("make_basic\n");
	    
	    int companyid = 0;
	    
	    foreach (Basecamp.Project bp in bc.GetProjects())
	    {
		log.print("Project: {0}\n", bp.Name);
		companyid = bp.Company.ID;
		Project p = s.projects.Add(bp.Name);
		
		foreach (Basecamp.TodoList bl in bc.GetToLists(bp.ID))
		{
		    log.print("Todo list: ..{0}-{1}\n", bl.ID, bl.Name);
		    if (bl.UncompletedCount > 0)
		    {
			FixFor f = s.fixfors.Add(p, bl.Name);
			todoinfo.Add(new TodoInfo(f, bc.GetTodoList(bl.ID)));
		    }
		}
	    }
	    
	    log.print("companyid={0}\n", companyid);
	    if (companyid != 0)
	    {
		foreach (Basecamp.Person bp in bc.GetPeople(companyid))
		{
		    log.print("Person: {0}-{1}\n", bp.ID, bp.UserName);
		    if (bp.UserName == user 
			  || bp.Email.StartsWith(wv.fmt("{0}@", user)))
			userids[bp.ID] = true;
		}
	    }
	}
	
	public override Task[] make_tasks()
	{
	    log.print("make_tasks\n");
	    foreach (TodoInfo tdi in todoinfo)
	    {
		log.print("tasks for '{0}'\n", tdi.fixfor.name);
		foreach (Basecamp.TodoItem i in tdi.list.TodoItems)
		{
		    log.print("task: '{0}'\n", i.Content);
		    if (!userids.tryget(i.ResponsiblePartyPersonID, false))
			continue;
		    string tid = wv.fmt("{0}_{1}", tdi.list.ID, i.ID);
		    Task t = s.tasks.Add(this, i.ID.ToString(), i.Content);
		    t.fixfor = tdi.fixfor;
		    t.done = i.Completed;
		    if (t.done)
			t.donedate = i.DateCompleted;
		}
	    }
	    return null;
	}
    }
}
