using System;
using Wv;
using Wv.Schedulator;

namespace Wv.Schedulator
{
    public class TestSource : Source
    {
        public TestSource(Schedulator s, string name) : base(s, name)
	{
	}
	
	public static Source create(Schedulator s, string name,
				    string prefix, string suffix)
	{
	    return new TestSource(s, name);
	}

	public override void make_basic()
	{
	    s.persons.Add("apenwarr", "Bob");
	    s.persons.Add("apenwarr", "Avery Pennarun");
	    s.persons.Add("dcoombs", "Dave Coombs");
	    s.persons.Add("dcoombs");
	    s.persons.Add("chrisk");
	    s.persons.Add("bob", "Bob McDobb");

	    Project p = s.projects.Add("Weaver");

	    s.fixfors.Add(p, "Wv 1.0", wv.date("2006-10-10 14:43:00"));
	    s.fixfors.Add(p, "Wv 1.0", wv.date("2006-10-15"));
	    s.fixfors.Add(p, "Wv 1.0", wv.date("2006-10-10"));
	    s.fixfors.Add(p, "Wv 1.5");
	    s.fixfors.Add(p, "Wv 2.0", wv.date("2007-10-10"));
	    s.fixfors.Add(p, "Wv 2.0", wv.date("2007-10-12"));
	}

	public override Task[] make_tasks()
	{
	    Task t1  = s.tasks.Add(this, 1.ToString(), "myname 1");
	    Task t2  = s.tasks.Add(this, 2.ToString(), "myname 2");
	    Task t3  = s.tasks.Add(this, 3.ToString(), "myname 3");
	    Task t4  = s.tasks.Add(this, 4.ToString(), "myname 4");
	    Task t5  = s.tasks.Add(this, 5.ToString(), "myname 5");
	    Task t6  = s.tasks.Add(this, 6.ToString(), "myname 6");
	    Task t7  = s.tasks.Add(this, 7.ToString(), "myname 7");
	    Task t8  = s.tasks.Add(this, 8.ToString(), "myname 8");
	    Task t100 = s.tasks.Add(this, 100.ToString(), "myname 100");
	    Task t200 = s.tasks.Add(this, 200.ToString(), "myname 200");

	    t1.fixfor = s.fixfors.Add(s.projects.Add("Weaver"), "Wv 2.0");
	    t2.fixfor = s.fixfors.Add(s.projects.Add("Weaver"), "Wv 2.0");
	    t3.fixfor = s.fixfors.Add(s.projects.Add("Weaver"), "Wv 1.0");
	    t5.fixfor = s.fixfors.Add(s.projects.Add("Weaver"), "Wv 2.0");
	    t6.fixfor = s.fixfors.Add(s.projects.Add("Weaver"), "Wv 2.0");

	    t1.priority = 4;
	    t5.priority = 6;
	    t6.priority = 4;

	    t5.done = t6.done = t7.done = t8.done = true;
	    t7.donedate = wv.date("2006-10-10");
	    t8.donedate = wv.date("2006-10-9");
	    
	    t100.parent = t200;

	    Task[] correct = {t5,t6,t8,t7, t4,t200,t100, t3, t2,t1};
	    return correct;
	}
    }
}
