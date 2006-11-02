#include "wvtest.cs.h"

using System;
using System.Collections;
using NUnit.Framework;
using Wv.Test;
using Wv.Utils;

[TestFixture]
public class WvTests
{
    [Test] [Category("shift")] public void shift_test()
    {
	string[] x = {"a", null, "c", "", "e", "f"};
	
	WVPASSEQ(wv.shift(ref x, 0), "a");
	WVPASSEQ(wv.shift(ref x, 0), null);
	WVPASSEQ(wv.shift(ref x, 1), "");
	WVPASSEQ(wv.shift(ref x, 2), "f");
	WVPASSEQ(x.Length, 2);
	WVPASSEQ(wv.shift(ref x, 0), "c");
	WVPASSEQ(wv.shift(ref x, 0), "e");
	WVPASSEQ(x.Length, 0);
    }
    
    [Test] [Category("ini")] public void ini_test()
    {
	Ini ini = new Ini("test.ini");
	WVPASSEQ(ini[""].Count, 2);
	WVPASSEQ(ini[""]["global item"], "i");
	WVPASSEQ(ini[""]["global 2"], "i2");
	WVPASSEQ(ini["subsEction"].Count, 3);
	WVPASSEQ(ini["subseCtion"]["2"], "3");
	WVPASSEQ(ini["nonexistent"].Count, 0);
    }
}
