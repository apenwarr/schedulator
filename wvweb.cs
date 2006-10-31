using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using Wv.Utils;

namespace Wv.Web
{
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
		    
		    "HTTP_USER_AGENT", 
		    "HTTP_REFERER",
		    "HTTP_CACHE_CONTROL",
		    "HTTP_CONNECTION",
		    "HTTP_ACCEPT",
		    "HTTP_ACCEPT_LANGUAGE",
		    "HTTP_ACCEPT_ENCODING",
		    "HTTP_ACCEPT_CHARSET",
		    
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
	    
	    IDictionary env = Environment.GetEnvironmentVariables();
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
	
	public Attr(params object[] sa)
	{
	    wv.assert((sa.Length % 2) == 0);
	    
	    for (int i = 0; i < sa.Length; i += 2)
		s += String.Format(" {0}=\"{1}\"",
		   HttpUtility.HtmlAttributeEncode(sa[i].ToString()),
		   HttpUtility.HtmlAttributeEncode(sa[i+1].ToString()));
	}
	
	public override string ToString()
	{
	    return s;
	}
	
	public static Attr none = new Attr();
    }
    
    public class HtmlGen
    {
	TextWriter stream = Console.Out;
	
	Context context = Context.Headers;
	NameValueCollection headers = new NameValueCollection();
	
	public HtmlGen()
	{
	    header("Content-Type", "text/html");
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
	    stream.Write(s);
	}
	
	public void send(params Html[] ha)
	{
	    foreach (Html h in ha)
	    {
		go(h.context);
		_send(h.ToString());
	    }
	}
	
	public string fmt(string fmt, params object[] args)
	{
	    return String.Format(fmt, args);
	}
	
	Html head_v(string s)
	{
	    return new Html(Context.Head, s);
	}
	
	Html head_v(params object[] ha)
	{
	    return new Html(Context.Head, ha);
	}
	
	Html head_vfmt(string format, params object[] args)
	{
	    return head_v(fmt(format, args));
	}
	
	Html v(string s)
	{
	    return new Html(Context.Body, s);
	}
	
	Html v(params object[] ha)
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
	
	public Html start_ul(Attr at)
	{
	    return vfmt("<ul{0}>", at);
	}
	
	public Html start_ul()
	{
	    return start_ul(Attr.none);
	}
	    
	public Html end_ul()
	{
	    return v("</ul>");
	}
	
	public Html ul(params Html[] ha)
	{
	    return v(start_ul(), ha, end_ul());
	}
	
	public Html li(params Html[] ha)
	{
	    return v(v("<li>"), ha, v("</li>\n"));
	}
	
	public Html li(string s)
	{
	    return li(text(s));
	}
	
	public Html start_table(Attr at)
	{
	    return vfmt("<table{0}>", at);
	}
	
	public Html start_table()
	{
	    return start_table(Attr.none);
	}
	    
	public Html end_table()
	{
	    return v("</table>");
	}
	
	public Html table(params Html[] ha)
	{
	    return new Html(Context.Body,
			    start_table(),
			    ha,
			    end_table());
	}
	
	public Html tr(Attr at, params Html[] ha)
	{
	    return new Html(Context.Body,
			    vfmt("<tr{0}>", at),
			    ha,
			    v("</tr>\n"));
	}
	
	public Html tr(params Html[] ha)
	{
	    return tr(Attr.none, ha);
	}
	
	public Html td(Attr at, string s)
	{
	    return vfmt("<td{0}>{1}</td>", at, text(s));
	}
	
	public Html td(string s)
	{
	    return td(Attr.none, s);
	}
	
	public Html td(Attr at, params Html[] ha)
	{
	    return v(vfmt("<td{0}>", at), ha, v("</td>"));
	}
	
	public Html td(params Html[] ha)
	{
	    return td(Attr.none, ha);
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
	
	public Html start_form(Attr at, params Html[] ha)
	{
	    return vfmt("<form{0}>", at);
	}
	
	public Html start_form(params Html[] ha)
	{
	    return start_form(Attr.none, ha);
	}
	
	public Html end_form()
	{
	    return v("</form>");
	}
	
	public Html submit(string id)
	{
	    return vfmt("<input type=\"submit\"{0}></input>",
			 new Attr("id", id,
				  "name", id,
				  "value", id));
	}
	
	public Html input(string id)
	{
	    return vfmt("<input{0}></input>", new Attr("name", id));
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
		     v("</p>"));
	}
	
	public Html p(string s)
	{
	    return p(Attr.none, text(s));
	}
	
	public Html p()
	{
	    return p(Attr.none);
	}
	
	public Html nbsp()
	{
	    return v("&nbsp;");
	}
	
	public Html img(string src)
	{
	    return vfmt("<img{0}>", new Attr("src", src));
	}
    }
}
