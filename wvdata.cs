/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using SCG = System.Collections.Generic;
using System.Linq;
using Wv.Extensions;
using System.Threading;

namespace Wv
{
    /**
     * A wrapper that will make any object implicitly castable into various
     * basic data types.
     * 
     * This is useful because IDataRecord is pretty terrible at giving you
     * objects in the form you actually want.  If I ask for an integer, I
     * want you to *try really hard* to give me an integer, for example by
     * converting a string to an int.  But IDataRecord simply throws an
     * exception if the object wasn't already an integer.
     * 
     * When converting to bool, we assume any non-zero int is true, just
     * like C/C++ would do. 
     */
    public struct WvAutoCast : IEnumerable<WvAutoCast>
    {
	object v;
	public static readonly WvAutoCast _null = new WvAutoCast(null);

	public WvAutoCast(object v)
	{
	    this.v = v;
	}
	
	public bool IsNull { get { return v == null || v is DBNull; } }
	
	public static implicit operator string(WvAutoCast o)
	{
	    return o.ToString();
	}
	
	public object inner 
	    { get { return v; } }
	
	public override string ToString()
	{
	    if (IsNull)
		return "(nil)"; // shouldn't return null since this != null
            else if (v is Boolean)
                return intify().ToString();
	    else if (v is IEnumerable<WvAutoCast>)
		return "[" + this.join(",") + "]";
	    else
		return v.ToString();
	}
	
	public static implicit operator DateTime(WvAutoCast o)
	{
	    if (o.IsNull)
		return DateTime.MinValue;
	    else if (o.v is DateTime)
		return (DateTime)o.v;
	    else if (o.v is SqlDateTime)
		return ((SqlDateTime)o.v).Value;
	    else
		return wv.date(o.v);
	}

	public static implicit operator SqlDateTime(WvAutoCast o)
	{
	    if (o.IsNull)
		return SqlDateTime.MinValue;
	    else if (o.v is SqlDateTime)
		return (SqlDateTime)o.v;
	    else if (o.v is DateTime)
		return (DateTime)o.v;
	    else
		return wv.date(o.v);
	}

        public static implicit operator byte[](WvAutoCast o)
        {
            return (byte[])o.v;
        }

        public static implicit operator SqlBinary(WvAutoCast o)
        {
	    if (o.IsNull)
		return null;
	    else if (o.v is SqlBinary)
		return (SqlBinary)o.v;
	    else
		return new SqlBinary((byte[])o.v);
        }
	
	bool isint()
	{
	    if (IsNull)
		return false;
	    else if (v is Int64 || v is Int32 || v is Int16
		     || v is UInt64 || v is UInt32 || v is UInt16
		     || v is byte || v is bool)
		return true;
	    else
		return false;
	}
	
	Int64 intify()
	{
	    if (IsNull)
		return 0;
	    else if (v is Int64)
		return (Int64)v;
	    else if (v is Int32)
		return (Int32)v;
	    else if (v is Int16)
		return (Int16)v;
	    else if (v is Byte)
		return (Byte)v;
            else if (v is Boolean)
                return (Boolean)v ? 1 : 0;
	    else
		return wv.atol(v);
	}

	public static implicit operator Int64(WvAutoCast o)
	{
	    return o.intify();
	}

	public static implicit operator Int32(WvAutoCast o)
	{
	    return (Int32)o.intify();
	}

	public static implicit operator Int16(WvAutoCast o)
	{
	    return (Int16)o.intify();
	}

	public static implicit operator UInt64(WvAutoCast o)
	{
	    return (UInt64) o.intify();
	}

	public static implicit operator UInt32(WvAutoCast o)
	{
	    return (UInt32)o.intify();
	}

	public static implicit operator UInt16(WvAutoCast o)
	{
	    return (UInt16)o.intify();
	}

	public static implicit operator Byte(WvAutoCast o)
	{
	    return (Byte)o.intify();
	}

	public static implicit operator bool(WvAutoCast o)
	{
	    return o.intify() != 0;
	}

	public static implicit operator double(WvAutoCast o)
	{
	    if (o.IsNull)
		return 0;
	    else if (o.v is double)
		return (double)o.v;
	    else if (o.v is Int64)
		return (Int64)o.v;
	    else if (o.v is Int32)
		return (Int32)o.v;
	    else if (o.v is Int16)
		return (Int16)o.v;
	    else if (o.v is Byte)
		return (Byte)o.v;
            else if (o.v is Boolean)
                return (Boolean)o.v ? 1.0 : 0.0;
	    else
		return wv.atod(o.v);
	}

	public static implicit operator float(WvAutoCast o)
	{
	    if (o.IsNull)
		return 0;
	    else if (o.v is float)
		return (float)o.v;
	    else
		return (float)(double)o;
	}

	public static implicit operator char(WvAutoCast o)
	{
	    if (o.IsNull)
		return Char.MinValue;
	    else if (o.v is char || o.isint())
		return (char)o.intify();
	    else
		return Char.MinValue;
	}

	public static implicit operator Decimal(WvAutoCast o)
	{
	    decimal d;
	    if (o.v is Decimal)
		return (Decimal)o.v;
	    else if (o.v is UInt64)
		return new Decimal((UInt64)o.v);
	    else if (o.isint())
		return new Decimal(o.intify());
	    else if (o.v is double || o.v is float)
		return new Decimal((double)o);
	    else if (o.v is string && Decimal.TryParse((string)o.v, out d))
		return d;
	    else
		return 0;
	}
	
	public static implicit operator SqlDecimal(WvAutoCast o)
	{
	    if (o.v is SqlDecimal)
		return (SqlDecimal)o.v;
	    else
		return (Decimal)o;
	}
	
	public static implicit operator Guid(WvAutoCast o)
	{
	    if (o.v is Guid)
		return (Guid)o.v;
	    else if (o.v is SqlGuid)
		return ((SqlGuid)o.v).Value;
	    else
		return Guid.Empty;
	}
	
	public static implicit operator SqlGuid(WvAutoCast o)
	{
	    if (o.v is SqlGuid)
		return (SqlGuid)o.v;
	    else if (o.v is Guid)
		return (Guid)o.v;
	    else
		return SqlGuid.Null;
	}
	
	public object to(Type t)
	{
	    if (t == typeof(string))
		return (string)this;
	    else if (t == typeof(DateTime))
		return (DateTime)this;
	    else if (t == typeof(SqlDateTime))
		return (SqlDateTime)this;
	    else if (t == typeof(byte[]))
		return (byte[])this;
	    else if (t == typeof(SqlBinary))
		return (SqlBinary)this;
	    else if (t == typeof(Int64))
		return (Int64)this;
	    else if (t == typeof(UInt64))
		return (UInt64)this;
	    else if (t == typeof(Int32))
		return (Int32)this;
	    else if (t == typeof(UInt32))
		return (UInt32)this;
	    else if (t == typeof(Int16))
		return (Int16)this;
	    else if (t == typeof(UInt16))
		return (UInt16)this;
	    else if (t == typeof(byte))
		return (byte)this;
	    else if (t == typeof(bool))
		return (bool)this;
	    else if (t == typeof(double))
		return (double)this;
	    else if (t == typeof(float))
		return (float)this;
	    else if (t == typeof(char))
		return (char)this;
	    else if (t == typeof(Decimal))
		return (Decimal)this;
	    else if (t == typeof(SqlDecimal))
		return (SqlDecimal)this;
	    else if (t == typeof(Guid))
		return (Guid)this;
	    else if (t == typeof(SqlGuid))
		return (SqlGuid)this;
	    else
		return v;
	}
	
	IEnumerable<object> _iter()
	{
	    if (!IsNull && v is IEnumerable)
	    {
		foreach (object i in (IEnumerable)v)
		{
		    if (i is WvAutoCast)
			yield return ((WvAutoCast)i).inner;
		    else
			yield return i;
		}
	    }
	}
	
	IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
	    foreach (var i in _iter())
		yield return i;
	}
	
	public IEnumerator<WvAutoCast> GetEnumerator()
	{
	    foreach (object i in _iter())
		yield return new WvAutoCast(i);
	}
    }
    
    
    public struct WvColInfo
    {
	public string name;
	public Type type;
	public bool nullable;
	public int size;
	public short precision;
	public short scale;
	
	public static IEnumerable<WvColInfo> FromDataTable(DataTable schema)
	{
	    foreach (DataRow col in schema.Rows)
		yield return new WvColInfo(col);
	}
	
	public WvColInfo(string name, Type type, bool nullable,
			 int size, short precision, short scale)
	{
	    this.name = name;
	    this.type = type;
	    this.nullable = nullable;
	    this.size = size;
	    this.precision = precision;
	    this.scale = scale;
	}
	
	WvColInfo(DataRow data)
	{
	    name      = (string)data["ColumnName"];
	    type      = (Type)  data["DataType"];
	    nullable  = (bool)  data["AllowDBNull"];
	    size      = (int)   wv.atoi(data["ColumnSize"]);
	    precision = (short) wv.atoi(data["NumericPrecision"]);
	    scale     = (short) wv.atoi(data["NumericScale"]);
	}
    }
    
    
    public class WvSqlRow : IEnumerable<WvAutoCast>
    {
	object[] _data;
	WvColInfo[] schema;
	Dictionary<string,int> colnames = null;
	
	public WvSqlRow(object[] data, IEnumerable<WvColInfo> schema)
	{
	    this._data = data;
	    this.schema = schema.ToArray();
	    
	    // This improves behaviour with IronPython, and doesn't seem to
	    // hurt anything else.  WvAutoCast knows how to deal with 'real'
	    // nulls anyway.  I don't really know what DBNull is even good
	    // for.
	    for (int i = 0; i < _data.Length; i++)
		if (_data[i] != null && _data[i] is DBNull)
		    _data[i] = null;
	}

	public WvAutoCast this[int i]
	    { get { return new WvAutoCast(_data[i]); } }
	
	void init_colnames()
	{
	    if (colnames != null)
		return;
	    colnames = new Dictionary<string,int>();
	    for (int i = 0; i < schema.Length; i++)
		colnames.Add(schema[i].name, i);
	}
	
	public WvAutoCast this[string s]
	{
	    get
	    {
		init_colnames();
		return this[colnames[s]];
	    }
	}

	public int Length
	    { get { return _data.Length; } }

	public IEnumerator<WvAutoCast> GetEnumerator()
	{
	    foreach (object colval in _data)
		yield return new WvAutoCast(colval);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
	    foreach (object colval in _data)
		yield return colval;
	}
	
	public object[] data
	    { get { return _data; } }
	
	public IEnumerable<WvColInfo> columns
	    { get { return schema; } }
    }
    
    
    public abstract class WvSqlRows : IDisposable, IEnumerable<WvSqlRow>
    {
	public abstract IEnumerable<WvColInfo> columns { get; }
	
	public virtual void Dispose()
	{
	    // nothing to do here
	}
	
	public abstract IEnumerator<WvSqlRow> GetEnumerator();
	
	IEnumerator IEnumerable.GetEnumerator()
	{
	    IEnumerator<WvSqlRow> e = GetEnumerator();
	    return e;
	}
    }
    
    public class WvSqlRows_IDataReader : WvSqlRows, IEnumerable<WvSqlRow>
    {
	IDataReader reader;
	WvColInfo[] schema;

	// FIXME:  This design obviously won't work for multiple simultaneous
	// DB accesses.
	public static Object cmdmutex = new Object();
	static IDbCommand curcmd = null;

	static readonly decimal smallmoneymax =  214748.3647m;
	static readonly decimal smallmoneymin = -214748.3648m;
	
	public WvSqlRows_IDataReader(IDbCommand cmd)
	{
	    wv.assert(cmd != null);
	    lock (cmdmutex)
	    {
		curcmd = cmd;
	    }
	    this.reader = cmd.ExecuteReader();
	    wv.assert(this.reader != null);
	    var st = reader.GetSchemaTable();
	    if (st != null)
		this.schema = WvColInfo.FromDataTable(st).ToArray();
	    else
		this.schema = new WvColInfo[0];
	}
	
	public override void Dispose()
	{
	    if (reader != null)
		reader.Dispose();
	    reader = null;

	    lock (cmdmutex)
	    {
		if (curcmd != null)
		    curcmd.Dispose();
		curcmd = null;
	    }
	    
	    base.Dispose();
	}
	
	public override IEnumerable<WvColInfo> columns
	    { get { return schema; } }
	
	// This function exists solely to work around bugs in mono's
	// Decimal and SqlDecimal data types.  (Tested in mono 1.9.1.0.)
	// 
	// This function shouldn't be used at all unless there was an
	// unexpected conversion error in the first place in GetValues().
	// 
	// (Tested with mono 1.9.1.0; failing unit tests are in
	//  versaplex: verifydata.t.cs/VerifyDecimal, VerifyMoney, VerifyChar)
	object fixdecimal(IDataRecord rec, int col)
	{
	    if (rec is SqlDataReader)
	    {
		SqlDataReader r = (SqlDataReader)rec;
		
		try
		{
		    // mono gets an OverflowException when trying to use
		    // GetDecimal() on a very large decimal(38,38) field.
		    // But GetSqlDecimal works... in that particular case.
		    SqlDecimal sd = r.GetSqlDecimal(col);
		    try
		    {
			return (decimal)sd;
		    }
		    catch (OverflowException)
		    {
			// Unfortunately, if the SqlDecimal is actually really
			// huge, it won't fit at all in the (decimal) type
			// and we get an exception.  We'll have to just
			// return it in the only type that works, a string.
			return sd.ToString();
		    }
		}
		catch (OverflowException)
		{
		    // fall through to plain GetDecimal
		}
		catch (InvalidCastException)
		{
		    // fall through to plain GetDecimal
		}
	    }
	    
	    // Mono doesn't crash when doing GetDecimal on reasonably-sized
	    // *negative* numbers, but it has another bug: it turns them
	    // into a huge number > MaxValue instead.  Unfortunately,
	    // MaxValue depends on the size of the decimal data type in
	    // use, so we have to check that before anything else.
	    // 
	    // Hopefully this translation is always harmless, since we
	    // only kick it in when the returned value is larger than the
	    // supposedly largest possible value of the provided data type.
	    decimal d = rec.GetDecimal(col);
	    decimal dmin, dmax;
	    switch (schema[col].size)
	    {
	    case 4:
		dmin = smallmoneymin;
		dmax = smallmoneymax;
		break;
	    case 8:
		dmin = (decimal)SqlMoney.MinValue;
		dmax = (decimal)SqlMoney.MaxValue;
		break;
	    default:
		dmin = Decimal.MinValue;
		dmax = Decimal.MaxValue;
		break;
	    }
	    
	    if (d > dmax)
		return dmin*2 + d;
	    else
		return d;
	}
	
	public override IEnumerator<WvSqlRow> GetEnumerator()
	{
	    int max = reader.FieldCount;
	    
	    using(this) // handle being called inside a foreach()
	    while (reader.Read())
	    {
		object[] oa = new object[max];
		
		// This ought to be enough, but in mono, unfortunately isn't.
		// To make testing more consistent, we'll disable it and do
		// it the slow way on *all* systems, not just if wv.IsMono().
		// Ugh.
		//reader.GetValues(oa);
		    
		// The slow way.
		for (int i = 0; i < max; i++)
		{
		    if (!reader.IsDBNull(i) 
			&& reader.GetFieldType(i) == typeof(Decimal))
			oa[i] = fixdecimal(reader, i);
		    else
			oa[i] = reader.GetValue(i);
		}
		
		yield return new WvSqlRow(oa, schema);
	    }
	}

	public static void Cancel()
	{
	    //FIXME:  Obviously needs changing in the long run, since curcmd is
	    //a static variable at this point.  It shouldn't be static, and
	    //Cancel should be an abstract method of WvSqlRows which we
	    //implement per-instance.
	    lock (cmdmutex)
	    {
		if (curcmd != null)
		   curcmd.Cancel();
		curcmd = null;
	    }
	}
    }
}
