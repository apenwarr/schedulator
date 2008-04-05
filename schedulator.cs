using System;
using System.Collections;
using Wv;

namespace Wv.Schedulator
{
    public class TimeSlot
    {
	public DateTime start, end;
	public bool done;
	public string name;
	
	protected TimeSlot(DateTime start, DateTime end,
			   bool done, string name)
	{
	    this.start = start;
	    this.end = end;
	    this.done = done;
	    this.name = name;
	}
    }
    
    public class TaskTimeSlot : TimeSlot
    {
	public FixFor fixfor;
	public Task task;
	
	public TaskTimeSlot(Task t, DateTime start, DateTime end,
			    string name, bool done)
	    : base(start, end, done, name)
	{
	    this.fixfor = t.fixfor;
	    this.task = t;
	}

	public TaskTimeSlot(Task t, DateTime start, DateTime end)
	    : this(t, start, end, t.name, t.done)
	{
	    this.fixfor = t.fixfor;
	    this.task = t;
	}
    }
    
    public class CommentTimeSlot : TimeSlot
    {
	public CommentTimeSlot(string comment, DateTime point)
	    : base(point, point, true, comment)
	{
	}
    }
    
    public class Schedulator: IDisposable
    {
	public string name;
	
	public DateSlider default_habits = new DateSlider();
	public DateTime now = DateTime.Now;
	public DateTime align = wv.date("1901-01-01");
	
	public PersonList persons = new PersonList();
	public ProjectList projects = new ProjectList();
	public FixForList fixfors = new FixForList();
	public TaskList tasks = new TaskList();
	
	public ArrayList schedule = new ArrayList(); // of TimeSlot
	
	public ArrayList sources = new ArrayList();
	
	public Schedulator(string name)
	{
	    this.name = name;
	}
	
	public void Dispose()
	{
	    while (sources.Count > 0)
	    {
		if (sources[0] is IDisposable)
		    ((IDisposable)sources[0]).Dispose();
		sources.RemoveAt(0);
	    }
	}
	
	public void register_source(Source src)
	{
	    sources.Add(src);
	}
	
	public enum Phase
	{
	    None = 0,
		Basic, Tasks, Sort1, 
		Cleanup, Sort2,
		Schedule, PostSchedule,
		Done
	}
	
	public Phase ran_phase = Phase.None;
	
	public void run_until(Phase phase)
	{
	    while (ran_phase < phase)
	    {
		switch (ran_phase+1)
		{
		case Phase.Basic:
		    foreach (Source s in sources)
			s.make_basic();
		    break;
		    
		case Phase.Tasks:
		    foreach (Source s in sources)
			s.make_tasks();
		    break;
		    
		case Phase.Sort1:
		case Phase.Sort2:
		    persons.Sort();
		    projects.Sort();
		    fixfors.Sort();
		    tasks.Sort();
		    break;
		    
		case Phase.Cleanup:
		    foreach (Source s in sources)
			s.cleanup_tasks();
		    break;
		    
		// Note: Sort2 actually happens HERE, not above!
		    
		case Phase.Schedule:
		    do_schedule();
		    break;
		    
		case Phase.PostSchedule:
		    foreach (Source s in sources)
			s.post_schedule();
		    break;
		    
		case Phase.Done:
		    // nothing to do
		    break;
		}
		
		ran_phase++;
	    }
	}
	
	public void run()
	{
	    run_until(Phase.Done);
	}
	
	DateSlider last_h;
	
	void do_one_task(ref DateTime point, Task t)
	{
	    DateSlider h = t.find_habits();
	    DateTime start = point;
	    DateTime end = h.add(point, t.done ? t.currest : t.remain);
	    
	    if (h != last_h)
	    {
		schedule.Add(new CommentTimeSlot(h.ToString(), end));
		last_h = h;
	    }
	    schedule.Add(new TaskTimeSlot(t, start, end));
	    
	    point = end;
	}
	
	public void do_schedule()
	{
	    now = now.Date;
	    align = align.Date;
	    
	    // DateTime point = wv.date("1997-01-01");
	    last_h = null;
	    
	    ArrayList past = new ArrayList();
	    Task last_past = null;
	    foreach (Task t in tasks)
	    {
		// all incomplete tasks are in the future, of course
		if (!t.done)
		    break;
		
		// completed tasks know if they're in the future or not
		if (t.donedate >= align)
		    break;
		
		past.Add(t);
		last_past = t;
	    }
	    
	    // now we count time backwards from the alignment date,
	    // handling all bugs that happened before it, in order to
	    // produce the actual schedule starting date.
	    past.Reverse();
	    DateTime point = align;
	    foreach (Task t in past)
		point = t.find_habits().add(point, -t.currest);
	    
	    schedule.Add(new CommentTimeSlot(
			  "(STARTDATE " + FixFor.date_string(point) + ")",
			  point));
	    
	    bool last_done = true;
	    foreach (Task t in tasks)
	    {
		if (!t.done && last_done == true)
		{
		    // count all elapsed time as "done", because we won't
		    // be counting it later.
		    foreach (Task tt in tasks)
		    {
			if (!tt.done && tt.elapsed != TimeSpan.Zero)
			{
			    DateTime end = 
				t.find_habits().add(point, tt.elapsed);
			    schedule.Add(new TaskTimeSlot(tt, point, end,
				  "ELAPSED: " + tt.name, true));
			    point = end;
			}
		    }
		    
		    last_done = t.done;
		}
		
		do_one_task(ref point, t);
		
		if (last_past == t)
		{
		    schedule.Add(new CommentTimeSlot(
			  "(ALIGNDATE " + FixFor.date_string(align) + ")",
			  point));
		    
		    // self-check: the algorithm is definitely broken if
		    // this doesn't line up
		    wv.assert(point == align);
		}
	    }
	}
	
	public void dump(WvLog log)
	{
	    log.print("\nPERSONS\n");
	    foreach (Person p in persons)
		log.print("  {0,-20} {1}\n", p.name, p.fullname);
	    
	    log.print("\nPROJECTS\n");
	    foreach (Project p in projects)
		log.print("  '{0}'\n", p.name);
	    
	    log.print("\nFIXFORS\n");
	    foreach (FixFor f in fixfors)
	    {
		log.print("'{0}'.'{1}' has {2} release date(s). (final={3})\n",
			f.project.name, f.name, f.releases.Count,
			FixFor.date_string(f.final_release));
		foreach (DateTime d in f.releases)
		    log.print("  {0,-30} {1}\n",
			    String.Format("'{0}'.'{1}'",
					  f.project.name, f.name),
			    FixFor.date_string(d));
	    }
	    
	    log.print("\nTASKS\n");
	    FixFor last_ff = null;
	    bool was_done = true;
	    foreach (Task t in tasks)
	    {
		if (was_done && !t.done)
		{
		    log.print("END FINISHED TASKS --\n");
		    log.print("\n");
		}
		else if (!was_done && last_ff != t.fixfor)
		{
		    if (last_ff == null)
			log.print("END UNDECIDED TASKS --\n");
		    else
			log.print("END MILESTONE '{0}.{1}' --\n",
				last_ff.project.name, last_ff.name);
		    log.print("\n");
		}
		
		log.print("   {0} [P{1}/{8}] {2}:{3} '{4}' ({5}/{6}/{7})\n",
			t.done ? "X" : ".",
			t.priority,
			t.source.name, t.id,
			t.name.Length>40 ? t.name.Substring(0,40) : t.name,
			t.origest.TotalHours, t.currest.TotalHours,
			t.elapsed.TotalHours, t.firstpriority());
		
		was_done = t.done;
		last_ff = t.fixfor;
	    }
	    
	    if (last_ff != null)
		log.print("END MILESTONE '{0}.{1}' --\n",
			last_ff.project.name, last_ff.name);
	}
	
	public void dump_schedule(WvLog log)
	{
	    log.print("\nSCHEDULE\n");
	    FixFor last_fixfor = null;
	    
	    foreach (TimeSlot _ts in schedule)
	    {
		if (_ts is CommentTimeSlot)
		{
		    CommentTimeSlot ts = (CommentTimeSlot)_ts;
		    log.print("{0}\n", ts.name);
		}
		
		if (_ts is TaskTimeSlot)
		{
		    TaskTimeSlot ts = (TaskTimeSlot)_ts;
		    
		    if (!ts.done && ts.fixfor != last_fixfor)
		    {
			log.print("Release {0}:{1}:\n",
				ts.fixfor.project.name, ts.fixfor.name);
			last_fixfor = ts.fixfor;
		    }
		    
		    TimeSpan realest =
			ts.task.done ? ts.task.currest : ts.task.remain;
		    string eststr = 
			(realest == TimeSpan.Zero 
			 ? "" 
			 : realest.TotalHours.ToString() + "h");
		    int daypercent =
			(int)Math.Round(ts.end.TimeOfDay.TotalHours/24 * 100);
		    string dayadd 
			= (daypercent==0 
			     ? "" 
			     : "." + daypercent.ToString("d2"));
		    log.print("  {0} {1}{2,3} {3,4} {4} ({5})\n",
			    ts.done ? "X" : ".",
			    FixFor.date_string(ts.end),
			    dayadd,
			    eststr,
			    ts.name, ts.start);
		}
	    }
	}
    }
}
