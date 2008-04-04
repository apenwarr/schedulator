using System;
using System.Collections;
using Wv;
using Wv.Obsolete;

namespace Wv.Schedulator
{
    public class Person : IComparable
    {
	public string name;
	
	string _fullname;
	public string fullname
	{
	    get { return _fullname != null ? _fullname : name; }
	    set { _fullname = value; }
	}
	
	public Person(string name)
	{
	    this.name = name;
	}
	
	public int CompareTo(object _y)
	{
	    Person y = (Person)_y;
	    return String.Compare(name, y.name, true);
	}
    }

    
    public class PersonList : SortedHash
    {
	public Person Add(string name)
	{
	    Person p = Find(name);
	    if (p == null)
	    {
		p = new Person(name);
		base.Add(name, p);
	    }
	    return p;
	}
	
	public Person Add(string name, string fullname)
	{
	    Person p = Add(name);
	    p.fullname = fullname;
	    return p;
	}
	
#pragma warning disable 0109 // appease mono 1.1.13.6
	public new virtual Person this[int index]
	{
	    get { return (Person)base[index]; }
	}
#pragma warning restore 0109
	
	public new Person Find(string name)
	{
	    return (Person)base.Find(name);
	}
    }
}
