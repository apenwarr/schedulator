using System;
using System.Collections;
using Wv;
using Wv.Obsolete;

namespace Wv.Schedulator
{
    public class FixFor : IComparable
    {
	public Project project;
	public string name;
	SortedHash release_dates = new SortedHash();
	public DateSlider default_habits;
	
	public FixFor(Project project, string name)
	{
	    if (project != null)
		this.project = project;
	    this.name = name;
	}
	
	public static string date_string(DateTime date)
	{
	    return String.Format("{0:d4}-{1:d2}-{2:d2}",
				 date.Year, date.Month, date.Day);
	}
	
	public void add_release(DateTime release_date)
	{
	    release_date = release_date.Date; // round to nearest day
	    if (!release_dates.Contains(date_string(release_date)))
	    {
		release_dates.Add(date_string(release_date), release_date);
		release_dates.Sort(); // fast enough...
	    }
	}
	
	public DateTime release_after(DateTime now)
	{
	    foreach (DateTime d in release_dates)
	    {
		if (d >= now)
		    return d;
	    }
	    return DateTime.MaxValue;
	}
	
	public DateTime final_release
	{
	    get
	    {
		if (release_dates.Count > 0)
		    return (DateTime)release_dates[release_dates.Count-1];
		else
		    return DateTime.MinValue; // undefined: rise to the top
	    }
	}
	
	public IList releases
	{
	    get
	    {
		return (ArrayList)release_dates;
	    }
	}
	
	public int CompareTo(object _y)
	{
	    FixFor y = (FixFor)_y;
	    if (y == null)
		return 1; // null fixfors come first in the list
	    else if (final_release != y.final_release)
		return DateTime.Compare(final_release, y.final_release);
	    else if (project != y.project)
		return project.CompareTo(y.project);
	    else if (name != y.name)
		return name.CompareTo(y.name);
	    else
		return 0; // I give up!  They're the same!
	}
    }
    

    public class FixForList : SortedHash
    {
	protected string make_key(Project project, string name)
	{
	    return project.name + "\0" + name;
	}
	
	public FixFor Add(Project project, string name)
	{
	    FixFor f = Find(project, name);
	    if (f == null)
	    {
		f = new FixFor(project, name);
		base.Add(make_key(project, name), f);
	    }
	    return f;
	}
	
	public FixFor Add(Project project, string name, DateTime date)
	{
	    FixFor f = Add(project, name);
	    f.add_release(date);
	    return f;
	}
	
#pragma warning disable 0109 // appease mono 1.1.13.6
	public new virtual FixFor this[int index]
	{
	    get { return (FixFor)base[index]; }
	}
#pragma warning restore 0109
	
	public FixFor Find(Project project, string name)
	{
	    return (FixFor)base.Find(make_key(project, name));
	}
    }
}
