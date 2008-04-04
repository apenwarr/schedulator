#include "wvtest.cs.h"
using System;
using System.IO;
using System.Collections;
using Wv;
using Wv.Test;
using Wv.Schedulator;

[TestFixture]
public class SchedTests
{
    WvLog log = new WvLog("Tests");
    Schedulator s;
    SourceRegistry reg;
    
    [SetUp] public void Init()
    {
	log.print("Creating schedulator.");
	reg = new SourceRegistry();
	s = new Schedulator("test");
    }
    
    [Test] public void person_project_fixfor_test()
    {
	reg.create(s, "ss", "test:");
	// new TestSource(s, "ss");
	s.run_until(Schedulator.Phase.Sort1);
	// s.dump(log);
	
	Person p0 = s.persons[0];
	WVPASSEQ(p0.name, "apenwarr");
	WVPASSEQ(p0.fullname, "Avery Pennarun");
	
	Person p1 = s.persons[1];
	WVPASSEQ(p1.name, "bob");
	WVPASSEQ(p1.fullname, "Bob McDobb");
	
	Person p2 = s.persons[2];
	WVPASSEQ(p2.name, "chrisk");
	WVPASSEQ(p2.fullname, "chrisk"); // default fullname
	
	Person p3 = s.persons[3];
	WVPASSEQ(p3.name, "dcoombs");
	WVPASSEQ(p3.fullname, "Dave Coombs");
	
	FixFor f1 = s.fixfors[1];
	WVPASSEQ("Wv 1.0", f1.name);
	WVPASSEQ(wv.date("2006-10-15"), f1.final_release);
	WVPASSEQ(2, f1.releases.Count);
	
	WVPASSEQ(wv.date("2006-10-15"),
		 f1.release_after(wv.date("2006-10-11")));
    }
    
    void check_tasks(Task[] correct)
    {
	WVPASSEQ(correct.Length, s.tasks.Count);
	for (int i = 0; i < correct.Length; i++)
	    WVPASSEQ(correct[i].id, s.tasks[i].id);
    }
    
    [Test] [Category("basic")] public void task_test_sensible()
    {
	TestSource src = new TestSource(s, "ts");

	s.run_until(Schedulator.Phase.Basic);
	Task[] correct = src.make_tasks();
	s.ran_phase = Schedulator.Phase.Tasks;
	s.run_until(Schedulator.Phase.Sort2);

	//s.dump(log);
	check_tasks(correct);
    }
    
    // this is actually pretty tricky: we create the tasks *before* the
    // things (eg. fixfors) the tasks refer to, then fill the information
    // into the other objects afterwards.  This has to work so that you can
    // load, say, a text schedule with extra tasks before the "real" bug
    // tracking database with all the extra details.
    [Test] public void task_test_backwards()
    {
	TestSource src = new TestSource(s, "ts");
	
	Task[] correct = src.make_tasks();
	src.make_basic();
	s.ran_phase = Schedulator.Phase.Tasks;
	s.run_until(Schedulator.Phase.Sort2);
	
	//s.dump(log);
	check_tasks(correct);
    }

    void dosplit(string input, string expect)
    {
	string output = String.Join("!", StringSource.word_split(input));
	WVPASSEQ(expect, output);
    }
    
    [Test] [Category("Silly")] public void wordsplit_test()
    {
	dosplit("hello world", "hello!world");
	dosplit("hello world ", "hello!world!");
	dosplit(" hello world ", "!hello!world!");
	dosplit("", "");
	WVPASSEQ(1, StringSource.word_split("").Length);
	
	dosplit("\"hello world\"", "\"hello world\"");
	dosplit("'hello world'", "'hello world'");
	dosplit("[hello world]", "[hello world]");
	dosplit("(hello world)", "(hello world)");
	dosplit("{hello world}", "{hello world}");
	
	dosplit("\"a b\" \t c", "\"a b\"!c");
	dosplit("'a\tb'   c", "'a\tb'!c");
	dosplit("\t'a b [c d]' [e f] (g h) {i j (} k) )} (l m) ",
		  "!'a b [c d]'![e f]!(g h)!{i j (} k) )}!(l m)!");
    }
    
    string[] merge(string[][] lists)
    {
	int max = 0;
	foreach (string[] list in lists)
	    max += list.Length;
	string[] outlist = new string[max];
	int i = 0;
	foreach (string[] list in lists)
	    foreach (string s in list)
		outlist[i++] = s;
	return outlist;
    }
    
    Task[] lookup_all(TaskList tasks, string[] list)
    {
	Task[] outlist = new Task[list.Length];
	int i = 0;
	foreach (string name in list)
	{
	    Task t = tasks.FindByName(name);
	    if (t == null)
		t = tasks.FindById(name);
	    if (t == null)
		throw new ArgumentException
		     ("can't find task named '" + name + "'");
	    outlist[i++] = t;
	}
	return outlist;
    }
    
    [Test] [Category("Silly2")] public void stringsource_test()
    {
	reg.create(s, "s1", "file:test1.sched");
	s.run_until(Schedulator.Phase.Sort2);
	s.dump(log);
	
	FixFor ff = s.fixfors.Find(s.projects.Find("Weaver"), "Wv 1.0");
	WVPASSEQ(ff.releases.Count, 4);
	
	string[] finished = {
	    "Third",
	};
	string[] undecided = {
	    "t:2",
	    "Bug without fixfor",
	};
	string[] v2_0 = {
	    "Unestimated \"Wv 2.0\" stuff",
	    "Unestimated one",
	    "Unestimated two",
	    "More Unestimated Wv \"2.0\" stuff",
	};
	string[] v1_0 = {
	    "bug:983",
	    "Sub-bug one",
	    "Sub-bug two estimate \"in title [1d]\"",
	    "Sub-bug two and a half",
	    "Sub-bug three",
	    "Do some stuff",
	    "First",
	    "Second",
	    "Two",
	    "Do \"more stuff\"",
	    "One",
	    "Three",
	};
	
	s.dump(log);
	
	string[][] all = {finished, undecided, v2_0, v1_0};
	string[][] active = {undecided, v2_0, v1_0};
	
	Task[] correct = lookup_all(s.tasks, merge(all));
	check_tasks(correct);
	
	foreach (Task t in lookup_all(s.tasks, finished))
	    WVPASSEQ(t.done, true);
	foreach (Task t in lookup_all(s.tasks, merge(active)))
	    WVPASSEQ(t.done, false);
	
	// spot check some estimates
	WVPASSEQ(s.tasks.FindByName("First").currest.TotalHours, 5.5*8);
	WVPASSEQ(s.tasks.FindByName("First").elapsed.TotalHours, 4.33);
	WVPASSEQ(s.tasks.FindByName("Second").currest.TotalHours, 6.0*8);
	WVPASSEQ(s.tasks.FindByName("Second").elapsed.TotalHours, 8.0);
    }


    [Test] [Category("Silly3")] public void stringtestsource_test()
    {
	// new TestSource(s, "t");
	// new StringSource(s, "s1", get_file("test1.sched"));
	reg.create(s, "t", "test:");
	reg.create(s, "s1", "file:test1.sched");
	
	s.run_until(Schedulator.Phase.Sort2);
	s.dump(log);
	
	FixFor ff = s.fixfors.Find(s.projects.Find("Weaver"), "Wv 1.0");
	WVPASSEQ(ff.releases.Count, 5);
	
	string[] finished = {
	    // the ordering of "Third", t5, and t6 is purely because of
	    // the order they were created, so this tests that the sources
	    // are creating tasks in the right phases, and schedulator
	    // runs multiple sources in the right order.
	    "t:5", "t:6",
	    "t:8", "t:7", // done dates come after no done dates
	    "Third",
	};
	string[] undecided = {
	    "t:4", "t:200", "t:100",
	};
	string[] v1_0 = {
	    "t:3", // because it was created first
	    "bug:983",
	    "Sub-bug one",
	    "Sub-bug two estimate \"in title [1d]\"",
	    "Sub-bug two and a half",
	    "Sub-bug three",
	    "Do some stuff", // low priority bug
	    "First",
	    "Second",
	    "Two",
	    "Do \"more stuff\"",
	    "One", // child of a lower-priority bug than t3
	    "Three",
	};
	string[] v2_0 = {
	    "t:2", // because it was created first
	    "Bug without fixfor", // lower priority
	    "Unestimated \"Wv 2.0\" stuff",
	    "Unestimated one",
	    "Unestimated two",
	    "More Unestimated Wv \"2.0\" stuff",
	    "t:1", // because it has nonzero priority
	};
	
	string[][] all = {finished, undecided, v1_0, v2_0};
	Task[] correct = lookup_all(s.tasks, merge(all));
	check_tasks(correct);
    }
    
    [Test] [Category("Silly4")] public void stringimport_test()
    {
	reg.create(s, "file2", "file:test2.sched");
	s.run_until(Schedulator.Phase.Sort2);
	s.dump(log);
	
	string[] all = {
	    "t:5", "t:6", "t:8", "t:7",
	    "t:4", "t:200", "t:100",
	    "t:3",
		
	    "child1",
	    "child1a",
	    "child2",
	    "t:1", "t:2",
	    "child1b",
	};
	check_tasks(lookup_all(s.tasks, all));
    }
    
    [Test] [Category("sort")] public void sort_test()
    {
	reg.create(s, "t", "file:test3.sched");
	s.run_until(Schedulator.Phase.Sort2);
	s.dump(log);
	
	string[] all = {
	    "Top1",
		"1.1", "1.1.1", "1.1.2",
		"1.2", "1.2.1", "1.2.2",
	    "Top2",
		"2.1", "2.2",
	};
	check_tasks(lookup_all(s.tasks, all));
    }
    
    [Test] public void fogbugz_test()
    {
	string odbcstr = String.Format
	    ("driver={{MySQL}};" +
	     "server={0};database={1};" +
	     "uid={2};pwd={3};",
	     "localhost", "fogbugz", "root", "scs");
	reg.create(s, "bug", "fogbugz:" + odbcstr + ":apenwarr");
	s.run_until(Schedulator.Phase.Sort2);
	//s.dump(log);
    }
    
    [Test] [Category("mantis")] public void mantis_test()
    {
	string odbcstr = String.Format
	    ("driver={{MySQL}};" +
	     "server={0};database={1};" +
	     "uid={2};pwd={3};",
	     "localhost", "mantis", "root", "scs");
	reg.create(s, "bug", "mantis:" + odbcstr + ":wooi");
	s.run_until(Schedulator.Phase.Sort2);
	s.dump(log);
    }
    
    [Test] [Category("Silly10")] public void schedule_test()
    {
	reg.create(s, "ts", "file:test10.sched");
	s.run();
	s.dump(log);
	s.dump_schedule(log);
    }
    
    [Test] [Category("Silly11")] public void schedule_test2()
    {
	reg.create(s, "ts", "file:test11.sched");
	s.run();
	s.dump(log);
	s.dump_schedule(log);
	
	TaskTimeSlot[] slots = new TaskTimeSlot[s.schedule.Count];
	int total = 0;
	foreach (TimeSlot ts in s.schedule)
	{
	    if (ts is TaskTimeSlot)
		slots[total++] = (TaskTimeSlot)ts;
	}
	
	WVPASSEQ(total, 9+2);
	WVPASSEQ(slots[0].name, "f1");
	WVPASSEQ(slots[0].start, wv.date("2006-10-09 06:00:00"));
	
	// start is exactly equal to aligndate, but f4, while done, was
	// finished after the aligndate, so it comes before the unfinished
	// tasks but after the aligndate.
	WVPASSEQ(slots[3].name, "f5");
	WVPASSEQ(slots[3].start, wv.date("2006-10-10"));
	WVPASSEQ(slots[4].name, "f4");
	
	// correction factor for elapsed time on unfinished bugs
	WVPASSEQ(slots[5].done, true);
	WVPASSEQ(slots[5].task.done, false);
	
	// make sure all the unfinished bugs add up correctly
	WVPASSEQ(slots[total-1].end, wv.date("2006-10-12 18:00:00"));
    }
    
    [Test] [Category("result")] public void result_test()
    {
	reg.create(s, "t", "file:test10.sched");
	reg.create(s, "result",
		   "result:dsn=schedulator;uid=root;pwd=scs:test");
	s.run();
    }
    
    [Test] [Category("googlecode")] public void googlecode_test()
    {
	//reg.create(s, "gc", "googlecode:pixeltoaster:glenn*fiedler");
	reg.create(s, "gc", "googlecode:versabox:apenwarr");
	s.run();
	// s.dump(log);
	s.dump_schedule(log);
	
	// not much of a test, but oh well, it proves *something* :)
	WVPASS(s.tasks.Count > 0);
    }
    
    public static void Main()
    {
	WvTest.DoMain();
    }
}
