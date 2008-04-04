using System;
using System.Collections;
using Wv;
using Wv.Obsolete;

namespace Wv.Schedulator
{
    public class Task : IComparable
    {
	public Task(Source source, string id, string name)
	{
	    this.source = source;
	    this.id = id;
	    this.name = name;
	}
	
	public int array_index; // used by TaskList; don't touch!
	
	public Source source;
	public string id;
	public string name;
	
	public string moniker
	{
	    get { return source.name + ":" + id; }
	}

	public Task parent;
	
	public FixFor fixfor;
	public int priority;

	public Person assignedto;
	
	public bool done;
	public bool halfdone; // eg. "needs verification" bugs
	public DateTime donedate;
	
	// set no more than two of the startdate, duedate, and work
	// estimate, or scheduling results may be undefined!
	public DateTime startdate, duedate;
	
	public TimeSpan origest, currest, elapsed;
	public TimeSpan remain
	{
	    get { return currest - elapsed; }
	}
	
	public bool is_estimated()
	{
	    return !wv.isempty(currest) || !wv.isempty(elapsed);
	}
	
	public DateSlider habits;
	public DateSlider find_habits()
	{
	    if (habits != null)
		return habits;
	    else if (fixfor != null
		     && fixfor.default_habits != null)
		return fixfor.default_habits;
	    else if (fixfor != null && fixfor.project != null
		     && fixfor.project.default_habits != null)
		return fixfor.project.default_habits;
	    else
		return source.s.default_habits;
	}
	
	public int firstpriority()
	{
	    if (parent != null && parent.priority < priority)
		return parent.firstpriority();
	    else
		return priority;
	}
	
	public FixFor firstfixfor()
	{
	    if (fixfor == null)
		return null; // null fixfors always come "first"
	    if (parent != null && fixfor.CompareTo(parent.fixfor) > 0)
		return parent.firstfixfor();
	    else
		return fixfor;
	}
	
	public bool has_ancestor(Task t)
	{
	    for (Task p = parent; p != null; p = p.parent)
		if (p == t)
		    return true;
	    return false;
	}
	
	public int depth()
	{
	    int depth = 0;
	    for (Task t = parent; t != null; t = t.parent)
		depth++;
	    return depth;
	}
	
	public Task up(int n)
	{
	    Task t = this;
	    for (int i = 0; i < n; i++)
		t = t.parent;
	    return t;
	}
	
	// Note: the duedate field doesn't affect bug comparisons, because
	// this is really a "bug priority comparison" algorithm, and simply
	// having a due date doesn't make a bug more important.  That said,
	// it *might* make it get scheduled earlier than a more important bug,
	// but that's a job for the scheduling algorithm, and there are too
	// many variables involved in that to be able to include it in a
	// simple bug-vs-bug comparison like this one.
	// 
	// This prioritized list is useful for the scheduler, but it will have
	// to pull out the due-date bugs and insert them at the appropriate
	// times.
	public int CompareTo(object _y)
	{
	    if (_y == null)
		return 1; // null task?  Oh well, it comes first.
	    
	    Task y = (Task)_y;
	    
	    if (done != y.done)
		return done ? -1 : 1;
	    else if (done) // so also y.done
	    {
		if (donedate != y.donedate)
		    return donedate.CompareTo(y.donedate);
		else // just preserve the original array ordering
		    return array_index - y.array_index;
	    }
	    else if (firstfixfor() != y.firstfixfor()) // & !done & !y.done
	    {
		if (firstfixfor() == null)
		    return -y.firstfixfor().CompareTo(firstfixfor());
		else
		    return firstfixfor().CompareTo(y.firstfixfor());
	    }
	    else if (firstpriority() != y.firstpriority())
	    {
		// default priority 0 should be the highest - it means the
		// task is uncategorized, so it should be at the top to
		// remind you to prioritize it ASAP.
		return firstpriority() - y.firstpriority();
	    }
	    else if (y.has_ancestor(this))
		return -1; // parent before child
	    else if (has_ancestor(y))
		return 1; // child after parent
	    else if (parent != y.parent)
	    {
		// this is a bit complicated.  If two subtasks are mostly
		// equal but have different parents, this will order them
		// in the same order as their parent.  That sort of makes
		// sense, since if one parent is more important than
		// another parent, we might as well do all his subtasks
		// before the less-important-parent's subtasks, but only
		// if the subtasks themselves are otherwise equivalent.
		// Whew!
		// 
		// Anyway, the real reason this is important is that we
		// want to group all of each parent's subtasks together
		// in the much more common case that all the parents, and
		// all the subtasks, have the same priority.
		
		int dx = depth(), dy = y.depth();
		if (dx > dy)
		{
		    int d = -y.CompareTo(up(dx-dy));
		    if (d == 0)
			return 1; // I'm deeper, so I'm later
		    else
			return d;
		}
		else if (dx < dy)
		{
		    int d = CompareTo(y.up(dy-dx));
		    if (d == 0)
			return -1; // y is deeper, so I'm earlier
		    else
			return d;
		}
		else
		    return parent.CompareTo(y.parent);
	    }
	    else // preserve the original array ordering
		return array_index - y.array_index;
	}
    }
    
    
    public class TaskList : SortedHash
    {
	protected string make_key(Source source, string id)
	{
	    return source.name + "\0" + id;
	}
	
	public Task Add(Source source, string id, string name)
	{
	    Task t = new Task(source, id, name);
	    base.Add(make_key(source, id), t);
	    t.array_index = Count-1;
	    return t;
	}
	
#pragma warning disable 0109 // appease mono 1.1.13.6
	public new virtual Task this[int index]
	{
	    get { return (Task)base[index]; }
	}
#pragma warning restore 0109
	
	public Task Find(Source source, string id)
	{
	    return (Task)base.Find(make_key(source, id));
	}
	
	public Task FindByName(string name)
	{
	    foreach (Task t in this)
		if (t.name == name)
		    return t;
	    return null;
	}
	
	public Task FindById(string id)
	{
	    foreach (Task t in this)
		if (id == t.id || id == (t.source.name + ":" + t.id))
		    return t;
	    return null;
	}
	
	public override void Sort()
	{
	    // note: this doesn't update the array_index field, because
	    // it's the *original* array_index that is most useful.
	    base.Sort();
	}
    }
}
