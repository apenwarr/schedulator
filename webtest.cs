using System;
using System.IO;
using System.Collections;
using Wv;
using Wv.Web;

public class WebTest
{
    public static void Main()
    {
	HtmlGen g = new HtmlGen();

	g.send(g.title("My Title"),
	       g.include_css("webtest.css"),
	       g.use_editinplace()
	       // g.use_tablesort() // incompatible with editinplace...
	       );

	g.send(g.form_start(new Attr("action", "",
				     "name", "sillyform",
				     "method", "post")));

	g.send(g.h1("My h1"),
	       g.h1("Another h1"),
	       g.h2("My h2"),
	       g.text("This is some text."));
	
	g.send(g.h1("Query variables:"));
	Cgi cgi = new Cgi();
	g.send(g.table_start());
	g.send(g.tr(g.th("Key"), g.th("Value")));
	foreach (string key in cgi.request.Keys)
	    g.send(g.tr(g.td(key), g.td(g.pre(cgi.request[key]))));
	g.send(g.table_end());
	
	g.send(g.p(),
	       g.editinplace("bigtext", "textarea",
			     Attr.none,
			     g.text("some big text\nanother line")));
	g.send(g.p(), 
	       g.editinplace("big2text", "textarea",
			     new Attr("tooltip", g.text("foo <monkeys>")),
			     g.text("big text 2\nanother &lt; <line>!")));

	g.send(g.p(),
	       g.editinplace("hello", "text",
			     new Attr("size", 50),
			     g.text("this is the content")),
	       g.p());

	g.send(g.table_start(new Attr("id", "foo1",
				      "tablesort", "1",
				      "border", "0")),
	       g.tr(g.td("hello"),
		    g.td("foo"),
		    g.td("blah")),
	       g.tr(g.td("1"),
		    g.td("2"),
		    g.td("3")),
	       g.tr(g.td("Big"),
		    g.td("Bad"),
		    g.td(g.editinplace("wolf", "text",
				       new Attr("powersearch",
					      "Dog,Cat,Monkey,Blueberry"),
				       g.text("Wolf")))),
	       g.tr(g.td("9"),
		    g.td("8"),
		    g.td(g.editinplace("7", "text",
				       new Attr("autovalidate",
						      "integer"),
				       g.text("7")))),
	       g.table_end());

	g.send(g.ul_start(new Attr("id", "foo2",
					 "tablesort", "1",
					 "border", "1")),
	       g.li(g.text("title"),
		    g.ul(g.li("hello"),
			 g.li("foo"),
			 g.li("blah"))),
	       g.li(g.ul(g.li("1"),
			 g.li("2"),
			 g.li("3"))),
	       g.li(g.ul(g.li("Big"),
			 g.li("Bad"),
			 g.li("Wolf"))),
	       g.li(g.ul(g.li("9"),
			 g.li("8"),
			 g.li("7 <foo>"))),
	       g.ul_end());
	
	g.send(g.h1("Environment variables:"));
	g.send(g.table_start());
	g.send(g.tr(g.th("Key"), g.th("Value")));
	IDictionary env = Environment.GetEnvironmentVariables();
	foreach (string key in env.Keys)
	    g.send(g.tr(g.td(key), g.td((string)env[key])));
	g.send(g.table_end());
	
	g.send(g.submit("Subm\"it!"),
	       g.form_end());
	g.send(g.done());
    }
}
