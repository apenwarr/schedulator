using System;
using System.Data;
using Wv.Utils;
using Wv.Schedulator;

namespace Wv.Schedulator
{
    public class ResultSource : Source
    {
	string user; // get the bugs for this username
	Log log;
	Wv.Dbi.Db db;
	
        public ResultSource(Schedulator s, string name,
			    string odbcstring, string user)
	    : base(s, name)
	{
	    this.user = user;
	    log = new Log(String.Format("Result:{0}", name));
	    log.log("Initializing result plugin '{0}'.", name);
	    log.log("Connecting to: '{0}'", odbcstring);
	    db = new Wv.Dbi.Db (odbcstring);
	}
	
	public static Source create(Schedulator s, string name,
				    string prefix, string suffix)
	{
	    string[] points = suffix.Split(':');
	    string dsn  = points[0];
	    string user = points.Length>1 ? points[1] : "UNKNOWN";
	    return new ResultSource(s, name, dsn, user);
	}
	
	public override void post_schedule()
	{
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
	    
	    db.execute("delete from Schedule where sUser=?", user);
	    
	    IDbCommand cmd = db.prepare
		("insert into Schedule "
		 + "(sUser,sProject,sFixFor,sTaskId,sTask,"
		 + " ixPriority,fDone,fHalfDone,fEstimated,dtStart,dtEnd) "
		 + "values (?,?,?,?,?,?,?,?,?,?,?) ", 11);
	    
	    foreach (TimeSlot _ts in s.schedule)
	    {
		if (_ts is TaskTimeSlot)
		{
		    TaskTimeSlot ts = (TaskTimeSlot)_ts;
		    log.log("Adding {0} - {1}", ts.start, ts.end);
		    FixFor ff = ts.task.fixfor;
		    if (ff == null)
			ff = s.fixfors.Add(s.projects.Add("UNKNOWN"),
					   "-Undecided-");
		    db.execute(cmd, user, ff.project.name, ff.name,
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
