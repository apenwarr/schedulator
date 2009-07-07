/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Web;
using Wv;
using Wv.Extensions;

namespace Wv
{
    public class WvHttpSession : Dictionary<string,string>
    {
	WvLog log = new WvLog("session");
	WvDbi dbi;
	string _sessid;
	public string sessid { get { return _sessid; } }
	
	public WvHttpSession(WvDbi dbi, string sessid)
	{
	    this.dbi = dbi;
	    this._sessid = sessid;
	    load();
	}
	
        public WvHttpSession(WvDbi dbi)
	{
	    this.dbi = dbi;
	    this._sessid = new_sessid();
	    dbi.execute("insert into Session (ixSession, dtSaved, bData) "
			+ "values (?, ?, ?) ", sessid, DateTime.Now, "");
	}
	
	static string new_sessid()
	{
	    return wv.randombytes(20).ToHex();
	}
	
	void load()
	{
	    log.print("Loading session '{0}'\n", sessid);
	    
	    // There should actually be only one row
	    foreach (var r in 
		     dbi.select("select dtSaved, bData from Session "
				+ "where ixSession=? ",
				sessid))
	    {
		DateTime dt = r[0];
		if (dt + TimeSpan.FromSeconds(60*60) >= DateTime.Now)
		    wv.urlsplit(this, r[1]);
	    }
	}
	
	public void save()
	{
	    List<string> l = new List<string>();
	    foreach (string k in Keys)
		l.Add(HttpUtility.UrlEncode(k)
		      + "="
		      + HttpUtility.UrlEncode(this[k]));
	    string data = l.join("&");
	    
	    try
	    {
		dbi.execute("insert into Session (ixSession, dtSaved, bData) "
			    + "values (?, ?, ?)",
			    sessid, DateTime.Now, "");
	    }
	    catch { }
	    
	    dbi.execute("update Session set bData=?, dtSaved=? "
			+ "where ixSession=?", data, DateTime.Now, sessid);
	}
    }

    public interface IWvHttpRequest
    {
        string request_uri { get; }
        Web.Cgi.Method request_method { get; }
        string path { get; }
        string query_string { get; }
        Dictionary<string,string> headers { get; }
        // FIXME: This sucks, it should really be a Dictionary<string,string>
        // to match the headers, but Wv.Cgi already has this as a
        // NameValueCollection 
        NameValueCollection request { get; }
    }

    public class WvHttpRequest : IWvHttpRequest
    {
	public string _request_uri;
        public string request_uri 
        { 
            get { return _request_uri; } 
            set { _request_uri = value; } 
        }

        public Web.Cgi.Method request_method
        {
            get { return Web.Cgi.Method.Get; }
        }

	public string path
	{
	    get { return request_uri.Split(new char[] {'?'}, 2)[0]; }
	}

	public string query_string
	{
	    get
	    {
		string[] parts = request_uri.Split(new char[] {'?'}, 2);
		if (parts.Length >= 2)
		    return parts[1];
		else
		    return "";
	    }
	}

	public Dictionary<string,string> _headers
	    = new Dictionary<string,string>();
        public Dictionary<string,string> headers
        {
            get { return _headers; }
        }

        public NameValueCollection request
        {
            get { return null; }
        }

	public WvHttpRequest() { }
	
	public WvHttpRequest(string s)
	{
	    parse_request(wv.fmt("GET {0} HTTP/1.0", s));
	}

	public void parse_request(string s)
	{
	    string[] parts = s.Split(new char[] {' '}, 3);
	    if (parts.Length < 3)
		throw new Exception("Not enough words in request!");
	    if (parts[0] != "GET")
		throw new Exception("Request should start with GET");
	    request_uri = parts[1];
	}

	public void parse_header(string s)
	{
	    if (s == "") return;

	    string[] parts = s.Split(new char[] {':'}, 2,
				     StringSplitOptions.None);
            string key = parts[0].ToLower();
	    headers.Remove(key);
	    if (parts.Length < 2)
		headers.Add(key, "");
	    else
		headers.Add(key, parts[1].Trim());
	}
    }

    public class WvHttpCgiRequest : IWvHttpRequest
    {
        Web.Cgi cgi;

        public string request_uri
        {
            get { return cgi.cgivars["REQUEST_URI"]; }
        }

        public Web.Cgi.Method request_method
        {
            get { return cgi.method; }
        }

        public string path
        {
            get { return cgi.script_name; }
        }

        public string query_string
        {
            get { return cgi.query_string; }
        }

        public Dictionary<string,string> _headers 
            = new Dictionary<string,string>();
        public Dictionary<string,string> headers
        {
            get { return _headers; }
        }

        public NameValueCollection request
        {
            get { return cgi.request; }
        }

        public WvHttpCgiRequest() 
        {
            cgi = new Web.Cgi();

            foreach (string key in cgi.cgivars)
            {
                if (key.StartsWith("HTTP_"))
                {
                    // Turn "HTTP_HEADER_NAME" into "header-name"
                    headers.Add(
                        key.Substring(5).ToLower().Replace('_', '-'), 
                        cgi.cgivars[key]);
                }
            }
        }
    }

    public class WvHttpServer
    {
	TcpListener server;
	WvLog log = new WvLog("HTTP Server", WvLog.L.Info);

	public delegate void Responder(WvHttpRequest req, Stream s);
	Responder responder;

	public WvHttpServer(int port, Responder responder)
	{
	    this.responder = responder;
	    log.print("World's dumbest http server initializing. (and how!)\n");
	    log.print("Trying port {0}.\n", port);
	    server = new TcpListener(port);
	    server.Start();
	    log.print("Listening on {0}\n", server.LocalEndpoint);
	}

	public void runonce()
	{
	    TcpClient client = server.AcceptTcpClient();
	    log.print("Incoming connection.\n");

	    NetworkStream stream = client.GetStream();
	    StreamReader r = new StreamReader(stream);

	    WvHttpRequest req = new WvHttpRequest();
	    string s = r.ReadLine();
	    log.print("Got request line: '{0}'\n", s);
	    req.parse_request(s);
	    log.print("Path is: '{0}'\n", req.request_uri);
	    do
	    {
		s = r.ReadLine();
		log.print("Got header line: '{0}'\n", s);
		req.parse_header(s);
	    }
	    while (s != "");

	    responder(req, stream);

	    log.print("Closing connection.\n");
	    client.Close();
	}
    }
}
