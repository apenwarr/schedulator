/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;
using Wv;
using Wv.Extensions;

namespace Wv.Web
{
    public class CgiTraceListener : TraceListener
    {
	StringBuilder all = new StringBuilder();
	
        public CgiTraceListener() : base()
	{
	    Trace.Listeners.Add(this);
	}
	
	public override void Write(string message)
	{
	    all.Append(message);
	}
	
	public override void WriteLine(string message)
	{
	    all.Append(message + "\n");
	}
	
	public string get()
	{
	    return all.ToString();
	}
	
	public void dump(Exception e, Stream s)
	{
	    using (TextWriter w = new StreamWriter(s))
	    {
		w.Write("\n\n\n</html><pre>\n");
		w.Write(get());
		w.Write("\n\n");
		if (e != null)
		    w.Write(e.ToString());
	    }
	}
	
	public void flush()
	{
	    all = new StringBuilder();
	}
    }
	
    public class Cgi
    {
	public NameValueCollection cgivars = new NameValueCollection();
	public NameValueCollection request = new NameValueCollection();
	
	public string query_string
	{
	    get { return cgivars["QUERY_STRING"]; }
	}
	
	public string script_name
	{
	    get { return cgivars["SCRIPT_NAME"]; }
	}
	
	public enum Method { Unknown, Get, Post };
	public Method method
	{
	    get
	    {
		switch (cgivars["REQUEST_METHOD"])
		{
		case "GET":  return Method.Get;
		case "POST": return Method.Post;
		default:     return Method.Unknown;
		}
	    }
	}
	
	public Cgi()
	    : this(Environment.GetEnvironmentVariables())
	{
	}
	
	public Cgi(IDictionary env)
	{
	    string[] wanted = {
		    "REQUEST_URI",
		    "SCRIPT_NAME",
		    "QUERY_STRING",
		    "HTTP_HOST",
		    
		    "REQUEST_METHOD",
		    "CONTENT_LENGTH",
		    
		    "SCRIPT_FILENAME",
		    "DOCUMENT_ROOT",
		    
		    "HTTP_COOKIE",
		    "HTTP_USER_AGENT", 
		    "HTTP_REFERER",
		    "HTTP_CACHE_CONTROL",
		    "HTTP_CONNECTION",
		    "HTTP_ACCEPT",
		    "HTTP_ACCEPT_LANGUAGE",
		    "HTTP_ACCEPT_ENCODING",
		    "HTTP_ACCEPT_CHARSET",
                    "HTTP_X_REQUESTED_WITH", 
		    
		    "REMOTE_ADDR",
		    "REMOTE_PORT",
		    
		    "SERVER_NAME",
		    "SERVER_ADDR",
		    "SERVER_PORT",
		    "SERVER_SOFTWARE",
		    "SERVER_PROTOCOL",
		    "GATEWAY_INTERFACE",
		    "SERVER_ADMIN",
	    };
	    
	    // IDictionary env = Environment.GetEnvironmentVariables();
	    foreach (string key in wanted)
	    {
		if (env.Contains(key))
		    cgivars.Add(key, (string)env[key]);
		else
		    cgivars.Add(key, "");
	    }
	    
	    // GET variables
	    split_settings(query_string);
	    
	    // POST variables
	    if (cgivars["REQUEST_METHOD"] == "POST")
	    {
#if false
		string postdata = Console.In.ReadToEnd();
#else
		string postdata = "";
		string line;
		while ((line = Console.In.ReadLine()) != null)
		    postdata += line;
#endif
		split_settings(postdata);
	    }
	}
	
	void split_settings(string query)
	{
	    if (query == "") return;
	    
	    string[] settings = query.Split("&".ToCharArray());
	    foreach (string setting in settings)
	    {
		string[] a = setting.Split("=".ToCharArray(), 2);
		//Console.Write("queryk:" + a[0] + "\n");
		string key = HttpUtility.UrlDecode(a[0]);
		//Console.Write("queryv:" + a[1] + "\n");
		string value 
		    = a.Length > 1 ? HttpUtility.UrlDecode(a[1]) : "1";
		request.Add(key, value);
	    }
	}
	
	public string hostbase()
	{
	    return "http://" + cgivars["HTTP_HOST"];
	}
	
	public string uribase()
	{
	    string s = cgivars["REQUEST_URI"];
	    int idx = s.IndexOf("?");
	    if (idx >= 0)
		return s.Substring(0, idx);
	    else
		return s;
	}
    }
    
    
    public enum Context { Headers, Head, Body, Done };
    
    public class Html
    {
	public Context context;
	string s;
	
	public Html(Context context, string s)
	{
	    this.context = context;
	    this.s = s;
	}
	
	public Html(Context context, params object[] ha)
	{
	    this.context = context;
	    this.s = merge(context, ha);
	}
	
	string merge(Context context, object[] ha)
	{
	    string s = "";
	    
	    foreach (object o in ha)
	    {
		if (o is object[])
		    s += merge(context, (object[])o);
		else
		{
		    Html h = (Html)o;
		    wv.assert(h.context == context);
		    s += h.ToString();
		}
	    }
	    return s;
	}
	
	public override string ToString()
	{
	    return s;
	}
    }
    
    public class Attr
    {
	string s = "";
	
	public Attr(Attr parent, params object[] sa)
	{
	    wv.assert((sa.Length % 2) == 0);
	    
	    if (parent != null)
		s = parent.ToString();
	    
	    for (int i = 0; i < sa.Length; i += 2)
		s += String.Format(" {0}=\"{1}\"",
		   HttpUtility.HtmlAttributeEncode(sa[i].ToString()),
		   HttpUtility.HtmlAttributeEncode(sa[i+1].ToString()));
	}
	
	public Attr(params object[] sa)
	    : this(null, sa)
	{
	}
	
	public override string ToString()
	{
	    return s;
	}
	
	public static Attr none = new Attr();
    }
    
    public class HtmlGen
    {
	Stream stream;
	
	Context context = Context.Headers;
	NameValueCollection headers = new NameValueCollection();
	
	public HtmlGen(Stream stream)
	{
	    this.stream = stream;
	    header("Content-Type", "text/html");
	}
	
	public HtmlGen()
	    : this(null)
	{
	}
	
	~HtmlGen()
	{
	    done();
	}
	
	public void header(string key, string value)
	{
	    wv.assert(context <= Context.Headers);
	    headers.Add(key, value);
	}
	
	void go(Context newcontext)
	{
	    wv.assert(newcontext >= context);
	    while (context < newcontext)
	    {
		context++;
		switch (context)
		{
		case Context.Headers:
		    wv.assert(); // it was the first one!
		    break;
		    
		case Context.Head:
		    foreach (string k in headers.Keys)
			_send(fmt("{0}: {1}\n", k, headers[k]));
		    _send("\n");
		    _send("<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.01//EN\" "
			  + "\"http://www.w3.org/TR/html4/strict.dtd\">\n");
		    _send("<html><head>");
		    break;
		    
		case Context.Body:
		    _send("</head>\n<body>");
		    break;
		    
		case Context.Done:
		    _send("</body>\n</html>\n");
		    break;
		}
	    }
	}
	
	public Html done()
	{
	    return new Html(Context.Done, "");
	}
	
	public void _send(string s)
	{
	    stream.write(s.ToUTF8());
	}
	
	public void send(params Html[] ha)
	{
	    foreach (Html h in ha)
	    {
		go(h.context);
		_send(h.ToString());
	    }
	}
	
	public void send(IEnumerable<Html> ha)
	{
	    foreach (Html h in ha)
		send(h);
	}
	
	public Html[] htmlarray(ICollection keys)
	{
	    Html[] a = new Html[keys.Count];
	    int i = 0;
	    foreach (object o in keys)
		a[i++] = (Html)o;
	    return a;
	}
	
	public string fmt(string fmt, params object[] args)
	{
	    return String.Format(fmt, args);
	}
	
	public Html head_v(string s)
	{
	    return new Html(Context.Head, s);
	}
	
	public Html head_v(params object[] ha)
	{
	    return new Html(Context.Head, ha);
	}
	
	public Html head_vfmt(string format, params object[] args)
	{
	    return head_v(fmt(format, args));
	}
	
	public Html v(string s)
	{
	    return new Html(Context.Body, s);
	}
	
	public Html v(params object[] ha)
	{
	    return new Html(Context.Body, ha);
	}
	
	Html vfmt(string format, params object[] args)
	{
	    return v(fmt(format, args));
	}
	
	public Html head_text(string s)
	{
	    return head_v(HttpUtility.HtmlEncode(s));
	}
	
	public Html text(string fmt, params object[] args)
	{
	    return v(HttpUtility.HtmlEncode(String.Format(fmt, args)));
	}
	
	public Html text(string s)
	{
	    return v(HttpUtility.HtmlEncode(s));
	}
	
	public Html pre(string s)
	{
	    return vfmt("<pre>{0}</pre>", text(s));
	}
	
	public Html title(string s)
	{
	    return head_vfmt("<title>{0}</title>", head_text(s));
	}
	
	public Html include_js(string name)
	{
	    return head_vfmt("<script{0}></script>",
			     new Attr("src", name,
				      "type", "text/javascript"));
	}
	
	public Html include_css(string name, string media)
	{
	    return head_vfmt("<link{0}>",
			     new Attr("rel", "stylesheet",
				      "href", name,
				      "type", "text/css",
				      "media", media));
	}
	
	public Html include_css(string name)
	{
	    return head_vfmt("<link{0}>",
			     new Attr("rel", "stylesheet",
				      "href", name,
				      "type", "text/css"));
	}
	
	public Html use_tablesort()
	{
	    return head_v(include_js("mesh/shared.js"),
			  include_css("mesh/tablesort/tablesort.css"),
			  include_js("mesh/tablesort/tablesort.js"));
	}
	
	public Html use_editinplace()
	{
	    return head_v(include_js("mesh/shared.js"),
			  include_js("config.js"),
			  
			  include_css("mesh/tooltip/tooltip.css"),
			  include_js("mesh/tooltip/tooltip.js"),
			  
			  include_css("mesh/editinplace/editinplace.css"),
			  include_js("mesh/editinplace/editinplace.js"),
			  
			  include_css("mesh/autovalidate/autovalidate.css"),
			  include_js("mesh/autovalidate/autovalidate.js"),
			  
			  include_css("mesh/powersearch/powersearch.css"),
			  include_js("mesh/powersearch/powersearch.js")
			  );
	}
	
	public Html h1(string s)
	{
	    return vfmt("<h1>{0}</h1>\n", text(s));
	}
	
	public Html h2(string s)
	{
	    return vfmt("<h2>{0}</h2>\n", text(s));
	}
	
	public Html h3(string s)
	{
	    return vfmt("<h3>{0}</h3>\n", text(s));
	}
	
	public Html h4(string s)
	{
	    return vfmt("<h4>{0}</h4>\n", text(s));
	}
	
	public Html div_start(Attr at)
	{
	    return vfmt("<div{0}>", at);
	}
	
	public Html div_start()
	{
	    return ul_start(Attr.none);
	}
	    
	public Html div_end()
	{
	    return v("</div>");
	}
	
	public Html div(Attr at, params Html[] ha)
	{
	    return v(div_start(at), ha, div_end());
	}
	
	public Html div(params Html[] ha)
	{
	    return div(Attr.none, ha);
	}
	
	public Html ul_start(Attr at)
	{
	    return vfmt("<ul{0}>", at);
	}
	
	public Html ul_start()
	{
	    return ul_start(Attr.none);
	}
	    
	public Html ul_end()
	{
	    return v("</ul>");
	}
	
	public Html ul(Attr at, params Html[] ha)
	{
	    return v(ul_start(at), ha, ul_end());
	}
	
	public Html ul(params Html[] ha)
	{
	    return ul(Attr.none, ha);
	}
	
	public Html li_start()
	{
	    return v("<li>");
	}
	
	public Html li_end()
	{
	    return v("</li>\n");
	}
	
	public Html li(params Html[] ha)
	{
	    return v(li_start(),
		     ha,
		     li_end());
	}
	
	public Html li(string s)
	{
	    return li(text(s));
	}
	
	public Html table_start(Attr at)
	{
	    return vfmt("<table{0}>", at);
	}
	
	public Html table_start()
	{
	    return table_start(Attr.none);
	}
	    
	public Html table_end()
	{
	    return v("</table>");
	}
	
	public Html table(params Html[] ha)
	{
	    return v(table_start(),
		     ha,
		     table_end());
	}
	
	public Html tr(Attr at, params Html[] ha)
	{
	    return new Html(Context.Body,
			    tr_start(at),
			    ha,
			    tr_end());
	}
	
	public Html tr(params Html[] ha)
	{
	    return tr(Attr.none, ha);
	}
	
	public Html tr_start(Attr at)
	{
	    return new Html(Context.Body, vfmt("<tr{0}>", at));
	}
	
	public Html tr_start()
	{
	    return tr_start(Attr.none);
	}
	
	public Html tr_end()
	{
	    return new Html(Context.Body, vfmt("</tr>\n"));
	}
	
	public Html td(Attr at, string s)
	{
	    return td(at, text(s));
	}
	
	public Html td(string s)
	{
	    return td(Attr.none, s);
	}
	
	public Html td(Attr at, params Html[] ha)
	{
	    return v(td_start(at),
		     ha,
		     td_end());
	}
	
	public Html td(params Html[] ha)
	{
	    return td(Attr.none, ha);
	}
	
	public Html td_start(Attr at)
	{
	    return vfmt("<td{0}>", at);
	}
	
	public Html td_start()
	{
	    return td_start(Attr.none);
	}
	
	public Html td_end()
	{
	    return v("</td>");
	}
	
	public Html th(Attr at, string s)
	{
	    return vfmt("<th{0}>{1}</th>", at, text(s));
	}
	
	public Html th(string s)
	{
	    return th(Attr.none, s);
	}
	
	public Html th(Attr at, params Html[] ha)
	{
	    return v(vfmt("<th{0}>", at), ha, v("</th>"));
	}
	
	public Html th(params Html[] ha)
	{
	    return th(Attr.none, ha);
	}
	
	public Html form_start(Attr at)
	{
	    return vfmt("<form{0}>", at);
	}
	
	public Html form_start()
	{
	    return form_start(Attr.none);
	}
	
	public Html form_end()
	{
	    return v("</form>");
	}
	
	public Html form(Attr at, params Html[] ha)
	{
	    return v(form_start(at), ha, form_end());
	}
	
	public Html form(params Html[] ha)
	{
	    return v(form_start(), ha, form_end());
	}
	
	public Html submit(string id)
	{
	    return vfmt("<input type=\"submit\"{0}></input>",
			 new Attr("id", id,
				  "name", id,
				  "value", id));
	}
	
	public Html input(Attr at, string id, params Html[] ha)
	{
	    return vfmt("<input{0}></input>", new Attr(at, "name", id));
	}
	
	public Html input(string id)
	{
	    return input(Attr.none, id);
	}
	
	public Html editinplace(string id, string type,
				     Attr at, params Html[] ha)
	{
	    if (type == "textarea")
	    {
		return v(vfmt("<textarea{0}{1}>",
			      new Attr("name", id,
				       "editinplace", "textarea"),
			      at),
			 ha,
			 v("</textarea>"));
	    }
	    else
	    {
		return v(vfmt("<span{0}{1}>",
			      new Attr("id", id,
				       "editinplace", type),
			      at),
			 ha,
			 v("</span>"));
	    }
	}
	
	public Html a(Attr at, params Html[] ha)
	{
	    return v(vfmt("<a{0}>", at),
		     ha,
		     v("</a>"));
	}
	
	public Html ahref(Attr at, string url, params Html[] ha)
	{
	    return v(vfmt("<a{0}{1}>", new Attr("href", url), at),
		     ha,
		     v("</a>"));
	}
	
	public Html ahref(string url, params Html[] ha)
	{
	    return ahref(Attr.none, url, ha);
	}
	
	public Html ahref(string url, string s)
	{
	    return ahref(Attr.none, url, text(s));
	}
	
	public Html span(Attr at, params Html[] ha)
	{
	    return v(vfmt("<span{0}>", at),
		     ha,
		     v("</span>"));
	}
	
	public Html p(Attr at, params Html[] ha)
	{
	    return v(vfmt("<p{0}>", at),
		     ha,
		     v("</p>\n"));
	}
	
	public Html p(params Html[] ha)
	{
	    return p(Attr.none, ha);
	}
	
	public Html p(params string[] sa)
	{
	    List<Html> l = new List<Html>();
	    foreach (string s in sa)
		l.Add(text(s));
	    return p(l.ToArray());
	}
	
	public Html p()
	{
	    return p(Attr.none);
	}
	
	public Html br()
	{
	    return v("<br />");
	}
	
	public Html sup(string s)
	{
	    return vfmt("<sup>{0}</sup>", text(s));
	}
	
	public Html nbsp()
	{
	    return v("&nbsp;");
	}
	
	public Html img(string src)
	{
	    return vfmt("<img{0}>", new Attr("src", src));
	}
	
	
	/* bp_* are for the "blueprint css" premade css formatting library */
	
	public Html bp_init()
	{
	    return head_v
		(include_css("css/blueprint/screen.css",
			     "screen, projection"),
		 include_css("css/blueprint/print.css",
			     "print"),
		 head_v("<!--[if IE]><link rel='stylesheet' "
			+ "href='css/blueprint/ie.css' "
			+ "type='text/css' media='screen, projection'>"
			+ "<![endif]-->"));
	}
	
	public Html bp_container_start()
	{
	    return div_start(new Attr("class", "container"));
	}
	
	public Html bp_container_end()
	{
	    return div_end();
	}
	
	public Html bp_container(params Html[] ha)
	{
	    return v(bp_container_start(),
		     ha,
		     bp_container_end());
	}
	
	public Html bp_span_start(int span, bool last)
	{
	    return div_start(new Attr("class",
				      wv.fmt("span-{0}{1}", span,
					     last ? " last" : "")));
	}
	
	public Html bp_span_end()
	{
	    return div_end();
	}
	
	public Html bp_span(int span, bool last, params Html[] ha)
	{
	    if (ha.Length > 0)
		return v(bp_span_start(span, last),
			 ha,
			 bp_span_end());
	    else
		return v(bp_span_start(span, last),
			 v("&nbsp;"),
			 bp_span_end());
	}
    }
}
