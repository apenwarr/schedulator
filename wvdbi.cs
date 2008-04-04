using System;
using System.Data;
using System.Data.Odbc;
using System.Collections.Specialized;
using Wv.Utils;

namespace Wv.Dbi
{
    public class Db
    {
	IDbConnection db;
	static Ini settings = new Ini("wvodbc.ini");
	
	public Db(string odbcstr)
	{
	    string real;
	    
	    if (settings[odbcstr].Count > 0)
	    {
		StringDictionary sect = settings[odbcstr];
		    
		string s = wv.fmt("driver={0};server={1};database={2};"
				  + "uid={3};pwd={4};",
				  sect["driver"], sect["server"],
				  sect["database"],
				  sect["user"], sect["password"]);
		real = s;
	    }
	    else if (String.Compare(odbcstr, 0, "dsn=", 0, 4, true) == 0)
		real = odbcstr;
	    else if (String.Compare(odbcstr, 0, "driver=", 0, 7, true) == 0)
		real = odbcstr;
	    else
		throw new ArgumentException
		   ("unrecognized odbc string '" + odbcstr + "'");
	    db = new OdbcConnection(real);
	    db.Open();
	}
	
	public IDbCommand prepare(string sql, int nparams)
	{
	    IDbCommand cmd = db.CreateCommand();
	    cmd.CommandText = sql;
	    for (int i = 0; i < nparams; i++)
		cmd.Parameters.Add(cmd.CreateParameter());
	    cmd.Prepare();
	    return cmd;
	}
	
	void bind(IDbCommand cmd, params object[] args)
	{
	    int i = 0;
	    foreach (IDataParameter param in cmd.Parameters)
	    {
		object o = args[i++];
		if (o is DateTime)
		    param.DbType = DbType.DateTime;
		else
		    param.DbType = DbType.String; // I sure hope so...
		param.Value = o;
	    }
	}
	
	public IDataReader select(string sql, params object[] args)
	{
	    return select(prepare(sql, args.Length), args);
	}
	
	public IDataReader select(IDbCommand cmd, params object[] args)
	{
	    bind(cmd, args);
	    return cmd.ExecuteReader();
	}
	
	public int execute(string sql, params object[] args)
	{
	    return execute(prepare(sql, args.Length), args);
	}
	
	public int execute(IDbCommand cmd, params object[] args)
	{
	    bind(cmd, args);
	    return cmd.ExecuteNonQuery();
	}
	
	public int try_execute(string sql, params object[] args)
	{
	    try
	    {
		return execute(sql, args);
	    }
	    catch (OdbcException)
	    {
		// well, I guess no rows were affected...
		return 0;
	    }
	}
    }
}

