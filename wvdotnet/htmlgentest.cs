/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System.Collections.Generic;
using Wv;

class MyPage : WvHtml
{
    WvHtml foo()
    {
	return li("hello");
    }
    
    IEnumerable<WvHtml> lister(params object[] args)
    {
	foreach (object o in args)
	    yield return li(o);
    }
    
    public MyPage(WvStream s)
    {
	add(html(insert_content_here));
	twist();
	add(head(title("hello title")));
	add(body(insert_content_here));
	twist();
	add(div(_id("foo\" ''"),
		ul(li(_class("bonk boo"), "hello", _class("chicken")), 
		   li("boo"), li("<snoot>")),
		_class("blue")
		));
	add(ol(new HtmlFunc(foo)));
	add(ol(lister("one", "two", "three",
		      ol(lister("four", "five")))));
	
	s.print(ToString());
	s.print("\n");
    }
}

class HtmlGenTest
{
    public static void Main()
    {
	WvLog log = new WvLog("test");
	new MyPage(log);
    }
}
