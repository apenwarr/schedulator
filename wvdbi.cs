using System;
using System.Data;
using System.Data.Odbc;

namespace Wv.Dbi
{
    public class Db
    {
	IDbConnection db;
	
	public Db(string odbcstring)
	{
	    db = new OdbcConnection(odbcstring);
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

