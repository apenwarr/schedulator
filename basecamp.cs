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
	Dictionary<int,Project> projmap = new Dictionary<int,Project>();
	Dictionary<Project,int> projrev = new Dictionary<Project,int>();
	WvLog log;
	Basecamp.BasecampManager bc;
	List<Basecamp.TodoList> todos = new List<Basecamp.TodoList>();
	
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
	    Task t = s.tasks.Find(this, taskid);
	    return wv.fmt("http://{0}.basecamphq.com/projects/{1}"
			  + "/todo_items/{2}/comments",
			  url.host, projrev[t.fixfor.project], t.id);
	}
	
	public override void make_basic()
	{
	    log.print("make_basic\n");
	    
	    var done_companies = new Dictionary<int,bool>();
	    
	    foreach (Basecamp.Project bp in bc.GetProjects())
	    {
		log.print("Project: {0}\n", bp.Name);
		Project p = s.projects.Add(bp.Name);
		projmap[bp.ID] = p;
		projrev[p] = bp.ID;
		
		if (!done_companies.tryget(bp.Company.ID, false))
		{
		    foreach (Basecamp.Person bpp in bc.GetPeople(bp.Company.ID))
		    {
			log.print("Person: {0}-{1}\n", bpp.ID, bpp.Email);
			if (bpp.UserName == user 
			    || bpp.Email.StartsWith(wv.fmt("{0}@", user)))
			{
			    userids[bpp.ID] = true;
			    log.print("  -- match\n");
			}
		    }
		    done_companies[bp.Company.ID] = true;
		}
	    }
	    
	    foreach (int i in userids.Keys)
	    {
		var tdl = bc.GetAllTodoLists(i).ToList();
		log.print("items for {0}: {1}\n", i, tdl.Count);
		todos.AddRange(tdl);
	    }
	    
	    foreach (Basecamp.TodoList bl in todos)
	    {
		log.print("TodoList: {0}\n", bl.Name);
		Project p = projmap[bl.ProjectID];
		s.fixfors.Add(p, bl.Name);
	    }
	}
	
	public override Task[] make_tasks()
	{
	    log.print("make_tasks\n");
	    foreach (Basecamp.TodoList bl in todos)
	    {
		log.print("tasks for {0}\n", bl.Name);
		Project p = projmap[bl.ProjectID];
		FixFor f = s.fixfors.Find(p, bl.Name);
		
		foreach (Basecamp.TodoItem i in bl.TodoItems)
		{
		    log.print("task: '{0}'\n", i.Content);
		    Task t = s.tasks.Add(this, i.ID.ToString(), i.Content);
		    t.fixfor = f;
		    t.done = i.Completed;
		    if (t.done)
			t.donedate = i.DateCompleted;
		}
	    }
	    return null;
	}
    }
}
