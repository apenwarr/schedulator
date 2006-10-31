using System;
using NUnit.Framework;

namespace Wv.Test
{
    public class WvTest
    {
	public static bool booleanize(long x)
	{
	    return x != 0;
	}
	
	public static bool booleanize(string s)
	{
	    return s != null && s != "";
	}
	
	public static bool booleanize(object o)
	{
	    return o != null;
	}
	
	public static bool test(bool cond, string file, int line, string s)
	{
	    System.Console.Out.WriteLine("! {0}:{1,-5} {2,-40} {3}",
					 file, line, s,
					 cond ? "ok" : "FAIL");
	    System.Console.Out.Flush();
	    Assert.IsTrue(cond, String.Format("{0}:{1} {2}", file, line, s));
	    return cond;
	}
	
	public static bool test_eq(long cond1, long cond2,
				   string file, int line,
				   string s1, string s2)
	{
	    return test(cond1 == cond2, file, line,
		String.Format("[{0}] == [{1}] ({{{2}}} == {{{3}}})",
			      cond1, cond2, s1, s2));
	}
	
	public static bool test_eq(double cond1, double cond2,
				   string file, int line,
				   string s1, string s2)
	{
	    return test(cond1 == cond2, file, line,
		String.Format("[{0}] == [{1}] ({{{2}}} == {{{3}}})",
			      cond1, cond2, s1, s2));
	}
	
	public static bool test_eq(string cond1, string cond2,
				   string file, int line,
				   string s1, string s2)
	{
	    return test(cond1 == cond2, file, line,
		String.Format("[{0}] == [{1}] ({{{2}}} == {{{3}}})",
			      cond1, cond2, s1, s2));
	}

	// some objects can compare themselves to 'null', which is helpful.
	// for example, DateTime.MinValue == null, but only through
	// IComparable, not through IObject.
	public static bool test_eq(IComparable cond1, IComparable cond2,
				   string file, int line,
				   string s1, string s2)
	{
	    return test(cond1.CompareTo(cond2) == 0, file, line,
			String.Format("[{0}] == [{1}]", s1, s2));
	}

	public static bool test_eq(object cond1, object cond2,
				   string file, int line,
				   string s1, string s2)
	{
	    return test(cond1 == cond2, file, line,
		String.Format("[{0}] == [{1}]", s1, s2));
	}

	public static bool test_ne(long cond1, long cond2,
				   string file, int line,
				   string s1, string s2)
	{
	    return test(cond1 != cond2, file, line,
		String.Format("[{0}] != [{1}] ({{{2}}} != {{{3}}})",
			      cond1, cond2, s1, s2));
	}
	
	public static bool test_ne(double cond1, double cond2,
				   string file, int line,
				   string s1, string s2)
	{
	    return test(cond1 != cond2, file, line,
		String.Format("[{0}] != [{1}] ({{{2}}} != {{{3}}})",
			      cond1, cond2, s1, s2));
	}
	
	public static bool test_ne(string cond1, string cond2,
				   string file, int line,
				   string s1, string s2)
	{
	    return test(cond1 != cond2, file, line,
		String.Format("[{0}] != [{1}] ({{{2}}} != {{{3}}})",
			      cond1, cond2, s1, s2));
	}
	
	// See notes for test_eq(IComparable,IComparable)
	public static bool test_ne(IComparable cond1, IComparable cond2,
				   string file, int line,
				   string s1, string s2)
	{
	    return test(cond1.CompareTo(cond2) != 0, file, line,
			String.Format("[{0}] != [{1}]", s1, s2));
	}
	
	public static bool test_ne(object cond1, object cond2,
				   string file, int line,
				   string s1, string s2)
	{
	    return test(cond1 != cond2, file, line,
		String.Format("[{0}] != [{1}]", s1, s2));
	}
    }
}
