using System;
using System.Collections;
using Wv.Utils;
using Wv.Web;

namespace Wv.Schedulator
{
    public class Source
    {
	public Schedulator s;
	public string name;
	
	public Source(Schedulator s, string name)
	{
	    this.name = name;
	    this.s = s;
	    s.register_source(this);
	}
	
	// When this is called, you have no guarantee that any particular
	// objects already exist except for all Sources.  Create Persons,
	// Projects, FixFors, and (if you want) Tasks here.
	public virtual void make_basic()
	{
	}
	
	// Called when all non-task objects (Sources, Persons, Projects,
	// FixFors) have been created, so you know you can refer to them
	// from your tasks.  Finish creating all your Tasks here.
	// 
	// Unit testing: this can return an array of Task objects that
	// represents the correct ordering of your tasks after Sort().  If
	// you don't care, which is quite possible, just return null.
	public virtual Task[] make_tasks()
	{
	    return null;
	}
	
	// Called when all Tasks have been created, but you might want to
	// fixup some of their attributes.
	// 
	// This is where you can, say, fill in the "parent" field if you
	// want to refer to Tasks that were created by other Sources.
	// 
	// You can also use this phase to override estimates on bugs
	// produced by other Sources.  Be careful with this, since if you
	// get two Sources fighting over estimates, results will be
	// undefined.  (We use this to let you override Fogbugz estimates
	// when FogBugz isn't flexible enough)
	public virtual void cleanup_tasks()
	{
	}
	
	// Called after the scheduling phase has run.  If your plugin wants
	// to look at the finished schedule or task list and record or
	// analyze information, do it here.
	public virtual void post_schedule()
	{
	}
    }
    
    
    public class SourceRegistry
    {
	Log log = new Log("SourceRegistry");
	Hashtable sources = new Hashtable();
	
	public delegate Source Creator(Schedulator s, string name,
				       string prefix, string suffix);
	
	public SourceRegistry()
	{
	    register("test", TestSource.create);
	    register("string", StringSource.create);
	    register("file", StringSource.create_from_file);
	    register("fogbugz", FogBugzSource.create);
	    register("mantis", MantisSource.create);
	    register("logstr", LogSource.create);
	    register("log", LogSource.create_from_file_id);
	    register("results", ResultSource.create);
	    register("result", ResultSource.create);
	}
	
	public void register(string prefix, Creator create)
	{
	    log.log("registering {0}", prefix);
	    sources.Add(prefix, create);
	}
	
	public Source create(Schedulator s, string name, string moniker)
	{
	    char[] splitchars = {':'};
	    string[] list = moniker.Split(splitchars, 2);
	    string prefix = list[0];
	    string suffix = list.Length>1 ? list[1] : "";
	    
	    log.log("create: prefix='{0}', suffix='{1}'", prefix, suffix);
	    
	    if (!sources.Contains(prefix))
		return null;
	    else
	    {
		Creator func = (Creator)sources[prefix];
		return func(s, name, prefix, suffix);
	    }
	}
    }
}
