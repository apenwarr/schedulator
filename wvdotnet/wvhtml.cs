/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Wv;
using Wv.Extensions;

namespace Wv
{
    public class WvHtml
    {
	public delegate object HtmlFunc();
	//static WvLog log = new WvLog("WvHtml");

	public class Attr
	{
	    public string name, value;

	    public Attr(string name, string value)
	    {
		wv.assert(name != null);
		wv.assert(value != null);
		this.name = name;
		this.value = value;
	    }

	    public Attr()
	    {
		this.name = "";
		this.value = "";
	    }
	}
	public static readonly Attr noattr = null;

	public class Verbatim
	{
	    public string s;

	    public Verbatim(string s)
	    {
		this.s = s;
	    }
	}

	string tagname;
	List<object> content = new List<object>();
	Stack<WvHtml> opens = new Stack<WvHtml>();

	public WvHtml(string tagname, params object[] args)
	{
	    //log.print("Creating '{0}'\n", tagname);
	    this.tagname = tagname; // may be null
	    add(args);
	}
	
	public WvHtml()
	    : this(null)
	    {
	    }

	public string ToString(int indent)
	{
	    //log.print("tostring: {0}\n", tagname);
	    
	    var attrs = new Dictionary<string, string>();
	    StringBuilder ab = new StringBuilder();
	    StringBuilder cb = new StringBuilder();
	    
	    while (opens.Count > 0)
		pop();

	    if (tagname != null)
		ab.Append(wv.fmt("<{0}", tagname));

	    foreach (object _o in content)
	    {
		Action<object> handler;

		// Okay, this is a little funny.  The inner part of this loop
		// can call itself recursively :)  It's a nice way to handle
		// things like IEnumerable.
		handler = delegate(object o)
		{
		    if (o == null)
			return;
		    else if (o is HtmlFunc)
			handler(((HtmlFunc)o)());
		    else if (o is string)
		    {
			// Normal objects get automatically html-encoded.  If
			// you need to supply verbatim html, use v().
			cb.Append(HttpUtility.HtmlEncode(o.ToString()));
		    }
		    else if (o is Attr)
		    {
			// Collect all the attributes together to end up as
			// part of our tag.  If the same attribute name occurs
			// more than once, concatenate all the values with
			// spaces between. (This is useful for attributes like
			// class="list blue")
			wv.assert(tagname != null);
			Attr a = (Attr)o;
			string v = HttpUtility.HtmlAttributeEncode(a.value);
			if (attrs.ContainsKey(a.name))
			    attrs[a.name] = attrs[a.name] + ' ' + v;
			else
			    attrs.Add(a.name, v);
		    }
		    else if (o is Verbatim)
		    {
			cb.Append(((Verbatim)o).s);
		    }
		    else if (o is WvHtml)
		    {
			// WvHtml objects have already been html-encoded.
			string s = ((WvHtml)o).ToString(indent+2);
			cb.Append(wv.fmt("\n{0}", "".PadRight(indent+2)));
			cb.Append(s);
			if (s.Contains("\n"))
			    cb.Append(wv.fmt("\n{0}", "".PadRight(indent)));
		    }
		    else if (o is IEnumerable)
		    {
			// CAREFUL!  This branch catches all sorts of objects.
			// For example, string is IEnumerable.
			foreach (object oo in (IEnumerable)o)
			    handler(oo);
		    }
		    else
			cb.Append(o.ToString());
		};

		handler(_o);
	    }

	    if (tagname != null)
	    {
		foreach (string k in attrs.Keys)
		    ab.Append(wv.fmt(" {0}=\"{1}\"", k, attrs[k]));
		
		string c = cb.ToString().TrimEnd();
		/* if (c.Length == 0)
		    return ab.ToString() + " />";
		else */ if (c.Contains("\n"))
		    return wv.fmt("{0}>{1}\n{2}</{3}>",
				  ab.ToString(), c, "".PadRight(indent),
				  tagname);
		else
		    return wv.fmt("{0}>{1}</{2}>",
				  ab.ToString(), c, tagname);
	    }
	    else
		return cb.ToString();
	}

	public override string ToString()
	{
	    return ToString(0);
	}
	
	public WvHtml topmost()
	{
	    if (opens.Count > 0)
		return opens.Peek();
	    else
		return this;
	}

	public void add(params object[] args)
	{
	    if (opens.Count > 0)
		opens.Peek().add(args);
	    else
	    {
		foreach (object o in args)
		{
		    if (o != null)
			content.Add(o);
		}
	    }
	}
	
	// Make subsequent add() calls add to an entirely different WvHtml
	// object.  You can call this, do a bunch of adds, and then
	// take the return value of pop() and add it somewhere too.
	public void push()
	{
	    opens.Push(new WvHtml());
	}
	
	// The reverse of push().  Returns the finished WvHtml subtree,
	// which you can then add().
	public WvHtml pop()
	{
	    return opens.Pop();
	}
	
	// WARNING MAGIC: use insert_contents_here as a marker in a
	// deep hierarchy, then use twist() to make add() point at that
	// part of the hierarchy, then use pop() to start adding back
	// at the parent level.  For example:
	//        add(html(body(div(insert_content_here),
	//                      footer())));
	//           // add()s here come after the html() tag
	//        twist();
	//           // add()s here come inside the div()
	//        add(p("hello world")); // this gets inserted at the i_c_h
	//        pop();
	//           // subsequent add()s come after the html() tag.
	public WvHtml insert_content_here {
	    get {
		WvHtml h = new WvHtml();
		wv.assert(ich == null,
			  "There was no twist() after the last i_c_h!");
		ich = h;
		return h;
	    }
	}
	
	// WARNING MAGIC: 
	WvHtml ich = null;
	public void twist()
	{
	    wv.assert(ich != null, 
		      "twist() only works after insert_content_here");
	    opens.Push(ich);
	    ich = null;
	}
	
	// To make them stand out, we prefix all the attribute-adding functions
	// with _.
	public Attr _attr(string name, string value)
	    { return new Attr(name, value); }
	public Attr _attr(string name, string fmt, params object[] args)
	    { return new Attr(name, wv.fmt(fmt, args)); }
	public Attr _id(string value)
            { return _attr("id", value); }
	public Attr _class(string value)
            { return _attr("class", value); }
	public Attr _style(string value)
            { return _attr("style", value); }
	public Attr _name(string value)
            { return _attr("name", value); }
	public Attr _value(string value)
            { return _attr("value", value); }
	public Attr _type(string value)
            { return _attr("type", value); }
	public Attr _colspan(string value)
            { return _attr("colspan", value); }
	public Attr _rowspan(string value)
            { return _attr("rowspan", value); }
	public Attr _src(string value)
            { return _attr("src", value); }
	public Attr _align(string value)
            { return _attr("align", value); }
	public Attr _size(int size)
	    { return _attr("size", size.ToString()); }
	public Attr _selected()
	    { return _attr("selected", "true"); }
	public Attr _rows(int count)
	    { return _attr("rows", count.ToString()); }
	public Attr _cols(int count)
	    { return _attr("cols", count.ToString()); }

	// Helpers
	public WvHtml v(params string[] args)
	    { return new WvHtml(null, args.map((s) => new Verbatim(s))); }
	public WvHtml v(params object[] args)
	    { return new WvHtml(null, args); }
	public WvHtml tag(string tagname, params object[] args)
            { return new WvHtml(tagname, args); }
	public WvHtml fmt(string f, params object[] args)
            { return new WvHtml(null, wv.fmt(f, args)); }
	public WvHtml func(Func<WvHtml> f)
	    { return new WvHtml(null, new HtmlFunc(f)); }
	
	// Javascripty bits
	public Attr _confirm(string msg)
	{
	    return _attr("onclick",
			 wv.fmt("return confirm(\"{0}\");", msg));
	}

	// Toplevel
	public WvHtml html(params object[] args)
	{
	    return new WvHtml
		(null,
		 v("<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.01//EN\" "
		   + "\"http://www.w3.org/TR/html4/strict.dtd\">\n"),
		 tag("html", args));
	}

	// Header tags
	public WvHtml head(params object[] args)
            { return tag("head", args); }
	public WvHtml title(params object[] args)
            { return tag("title", args); }
	public WvHtml meta(params object[] args)
            { return tag("meta", args); }
	public WvHtml link(params object[] args)
            { return tag("link", args); }
	
	// FIXME: script tags get confused by IE if you use <script... />
	// syntax, so we have to force (badly!) <script>...</script> syntax
	// here.
	public WvHtml script(params object[] args)
            { return tag("script", args); }

	// Scripts
	public WvHtml javascript_file(string src, params object[] args)
            { return script(_attr("type", "text/javascript"),
			    _src(src),
			    args); }
	public WvHtml js_file(string src, params object[] args)
	    { return javascript_file(src, args); }

	// Stylesheets
	public WvHtml css_file(string src, params object[] args)
            { return link(_attr("rel", "stylesheet"),
			  _attr("href", src),
			  _attr("type", "text/css"),
			  args); }

	// Body tags
	public WvHtml body(params object[] args)
            { return tag("body", args); }

	// Images
	public WvHtml img(string src, params object[] args)
            { return tag("img", _src(src), args); }

	// Anchors
	public WvHtml a(params object[] args)
            { return tag("a", args); }
	public WvHtml ahref(string href, params object[] args)
            { return a(_attr("href", href), args); }

	// Sections
	public WvHtml p(params object[] args)
            { return tag("p", args); }
	public WvHtml div(params object[] args)
            { return tag("div", args); }
	public WvHtml span(params object[] args)
            { return tag("span", args); }
	public WvHtml br {
	    // This looks weird, but works better with IE.
            get { return new WvHtml(null, v("<br />")); }
	}
	
	// Text
	public WvHtml b(params object[] args)
            { return tag("b", args); }
	public WvHtml i(params object[] args)
            { return tag("i", args); }
	public WvHtml sup(params object[] args)
            { return tag("sup", args); }
	public WvHtml nbsp {
	    get { return v("&nbsp;"); }
	}

	// Headings
	public WvHtml h1(params object[] args)
            { return tag("h1", args); }
	public WvHtml h2(params object[] args)
            { return tag("h2", args); }
	public WvHtml h3(params object[] args)
            { return tag("h3", args); }
	public WvHtml h4(params object[] args)
            { return tag("h4", args); }
	public WvHtml h5(params object[] args)
            { return tag("h5", args); }
	public WvHtml h6(params object[] args)
            { return tag("h6", args); }

	// Lists
	public WvHtml ol(params object[] args)
            { return tag("ol", args); }
	public WvHtml ul(params object[] args)
            { return tag("ul", args); }
	public WvHtml li(params object[] args)
            { return tag("li", args); }

	// Tables
	public WvHtml table(params object[] args)
            { return tag("table", args); }
	public WvHtml tr(params object[] args)
            { return tag("tr", args); }
	public WvHtml tr_empty 
	    { get { return tr(td(nbsp)); } }
	public WvHtml td(params object[] args)
            { return tag("td", args); }
	public WvHtml th(params object[] args)
            { return tag("th", args); }
	
	// Forms
	public WvHtml form(string method, string url,
			   params object[] args)
            { return tag("form",
			 _attr("method", method),
			 _attr("action", url),
			 args); }
	public WvHtml submit(string id, params object[] args)
            { return tag("input", _attr("type", "submit"),
			 _id(id), _name(id),
			 _attr("value", id),
			 args); }
	public WvHtml input(string id, int length, string defval,
			    params object[] args)
            { return tag("input", _name(id), _size(length),
			 _value(defval), args); }
	public WvHtml input(string id, int length)
            { return input(id, length, ""); }
	public WvHtml hidden(string id, object value)
            { return tag("input", _type("hidden"),
			 _name(id), _value(value.ToString())); }
	public WvHtml select(string id, params object[] args)
	    { return tag("select", _name(id), args); }
	public WvHtml option(string name, string display, 
			     params object[] args)
	    { return tag("option", _value(name), display, args,
			 nbsp, nbsp, nbsp); }
	public WvHtml textarea(string name, params object[] args)
	    { return tag("textarea", _name(name), args); }
    }
    
} // namespace
