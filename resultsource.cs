using System;
using System.Data;
using Wv;
using Wv.Schedulator;

namespace Wv.Schedulator
{
    public class ResultSource : Source
    {
	string user; // get the bugs for this username
	WvLog log;
	public WvDbi db;
	
        public ResultSource(Schedulator s, string name,
			    string odbcstring, string user)
	    : base(s, name)
	{
	    if (!wv.isempty(user))
		this.user = user;
	    else
		this.user = s.name;
	    log = new WvLog(String.Format("Result:{0}", name));
	    log.print("Initializing result plugin '{0}'.\n", name);
	    log.print("Connecting to: '{0}'\n", odbcstring);
	    db = new WvDbi(odbcstring);
	    
	    // db.try_execute("drop table Schedule");
	    db.try_execute("create table Schedule ("
			   + "sUser varchar(40) not null, "
			   + "sProject varchar(40) not null, "
			   + "sFixFor varchar(40) not null, "
			   + "sTaskId varchar(40) not null, "
			   + "sTask varchar(80) not null, "
			   + "ixPriority int not null, "
			   + "fDone boolean not null, "
			   + "fHalfDone boolean not null, "
			   + "fEstimated boolean not null, "
			   + "dtStart datetime not null, "
			   + "dtEnd datetime not null "
			   + ")");
	}
	
	public static Source create(Schedulator s, string name,
				    string prefix, string suffix)
	{
	    string[] points = suffix.Split(':');
	    string dsn  = points[0];
	    string user = points.Length>1 ? points[1] : null;
	    return new ResultSource(s, name, dsn, user);
	}
	
	public override void post_schedule()
	{
	    db.execute("delete from Schedule where sUser=?", user);
	    
	    string q = 
		"insert into Schedule "
		 + "(sUser,sProject,sFixFor,sTaskId,sTask,"
		 + " ixPriority,fDone,fHalfDone,fEstimated,dtStart,dtEnd) "
		 + "values (?,?,?,?,?,?,?,?,?,?,?) ";
	    
	    foreach (TimeSlot _ts in s.schedule)
	    {
		if (_ts is TaskTimeSlot)
		{
		    TaskTimeSlot ts = (TaskTimeSlot)_ts;
		    log.print("Adding {0} - {1}\n", ts.start, ts.end);
		    FixFor ff = ts.task.fixfor;
		    if (ff == null)
			ff = s.fixfors.Add(s.projects.Add("UNKNOWN"),
					   "-Undecided-");
		    db.execute(q, user, ff.project.name, ff.name,
			       ts.task.moniker, ts.name, ts.task.priority,
			       ts.done ? 1 : 0,
			       ts.task.halfdone ? 1 : 0,
			       ts.task.is_estimated() ? 1 : 0,
			       ts.start, ts.end);
		}
	    }
	}
    }
}
