using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wv;
using Wv.Extensions;

namespace Wv.Query
{
    // API (currently very loosely) based on:
    // http://docs.djangoproject.com/en/dev/topics/db/queries/
     
    public class ColumnAttribute : Attribute { }
    public class PrimaryKeyColumnAttribute : ColumnAttribute { }
    
    public class QuerySet<T> : IEnumerable<T> where T: new()
    {
	public struct WhereClause
	{
	    public WhereClause(string name, string op, string value)
	        { this.name = name; this.op = op; this.value = value; }
	    public string name, op, value;
	}
	
	WvDbi dbi;
	List<WhereClause> wheres;
	
	QuerySet(WvDbi dbi, List<WhereClause> wheres)
	{
	    this.dbi = dbi;
	    this.wheres = wheres;
	}
	
	public QuerySet(WvDbi dbi) 
	    : this(dbi, new List<WhereClause>())
	    { }
	
	public QuerySet<T> filter(string name, string op, string value)
	{
	    var wl = new List<WhereClause>(wheres);
	    wl.Add(new WhereClause(name, op, value));
	    return new QuerySet<T>(dbi, wl);
	}
	
	public QuerySet<T> filter(string name, string value)
	{
	    return filter(name, "=", value);
	}
	
	public QuerySet<T> exclude(string name, string value)
	{
	    return filter(name, "<>", value);
	}
	
	IEnumerable<T> runq()
	{
	    FieldInfo[] fi = 
		WvReflection
		  .find_fields(typeof(T), typeof(ColumnAttribute))
		  .ToArray();
	    string[] colnames = (from f in fi select f.Name).ToArray();
	    
	    string q = wv.fmt("select {0} from {1}",
			      colnames.join(", "), typeof(T).Name);
	    
	    var names = from w in wheres select (w.name + "=?");
	    var values = from w in wheres select w.value;
	    
	    if (wheres.Count > 0) 
		q += " where " + names.join(" and ");
	    
	    wv.printerr("QUERY: '{0}' ({1})\n", q, values.join("; "));
	    
	    foreach (var r in dbi.select(q, values.ToArray()))
	    {
		T t = new T();
		
		int i = 0;
		foreach (var col in r)
		{
		    FieldInfo f = fi[i++];
		    f.SetValue(t, col.to(f.FieldType));
		}
		yield return t;
	    }
	}
	
	IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
	    foreach (var r in runq())
		yield return r;
	}
	
	public IEnumerator<T> GetEnumerator()
	{
	    foreach (var r in runq())
		yield return r;
	}
    }
    
    public class Database : IDisposable
    {
	protected WvDbi dbi { get; private set; }
	
	public Database(WvDbi dbi)
	{
	    this.dbi = dbi;
	}
	
	public Database(string dbi_moniker) : this(WvDbi.create(dbi_moniker))
	    { }
	
	public void Dispose()
	{
	    using (dbi)
		;
	}
    }
}

