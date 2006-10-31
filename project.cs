using System;
using System.Collections;
using Wv.Utils;

namespace Wv.Schedulator
{
    public class Project : IComparable
    {
	public string name;
	public DateSlider default_habits;
	
	public Project(string name)
	{
	    this.name = name;
	}

    	public int CompareTo(object _y)
	{
	    Project y = (Project)_y;
	    return String.Compare(name, y.name, true);
	}
    }
    
    
    public class ProjectList : SortedHash
    {
	public Project Add(string name)
	{
	    Project p = Find(name);
	    if (p == null)
	    {
		p = new Project(name);
		base.Add(name, p);
	    }
	    return p;
	}
	
#pragma warning disable 0109 // appease mono 1.1.13.6
	public new virtual Project this[int index]
	{
	    get { return (Project)base[index]; }
	}
#pragma warning restore 0109
	
	public new Project Find(string name)
	{
	    return (Project)base.Find(name);
	}
    }
}
